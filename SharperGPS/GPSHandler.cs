/* Copyright 2007 - Morten Nielsen
 * Copyright (C) 2009  John Schmitt, Mike McBride, and Kevin Curtis
 * 
 * This file is part of SharperGps.
 * SharperGps is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * 
 * SharperGps is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with SharperGps; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 * Change log
 * 
 * Date    | Author | Reason for change                | Description of change
 * -------- -------- ---------------------------------- ------------------------------------------------------
 * 12/9/09  jschmitt initial development                Fully implemented every method
 */

// History:
// 2005-05-22:	Several bugs in NMEA parsing corrected.
//				DateTime properties is now in UTC.
// 2005-05-14:	Now using .NET 2.0 SerialPort classes
//				Time- and Date values are changed from String to DateTime and Timespan types
// 2005-02-16:	Corrected a problem with ToDouble conversion. (some countries
//				use ',' as decimal seperator) Thanks to Arne Madsen for pointing
//				this out.
// 2005-01-22:	First official OpenSource release
namespace Ares.SharperGps
{
    using System.Globalization;
    using System.Threading;
    using System.IO;
    using System;
    using NMEA;


    /// <summary>
    /// GPS Handler - GPS Library for Pocket PC
    /// Released under GNU Lesser General Public License
    /// </summary>
    public class GpsHandler : IDisposable
    {
        internal static SerialPort GpsPort = new SerialPort();
        private ThreadStart _clThreadStart;
        private Thread _clThread;

        private bool _disposed;
        //Ensure that we are interpreting '.' as decimal seperator
        internal static NumberFormatInfo NumberFormatEnUs = new CultureInfo( "en-US", false ).NumberFormat;

        /// <summary>
        /// Recommended minimum specific GPS/Transit data
        /// </summary>
        public GPRMC GPRMC;
        /// <summary>
        /// Global Positioning System Fix Data
        /// </summary>
        public GPGGA GPGGA;
        /// <summary>
        /// Satellites in view
        /// </summary>
        public GPGSV GPGSV;
        /// <summary>
        /// GPS DOP and active satellites
        /// </summary>
        public GPGSA GPGSA;
        /// <summary>
        /// Geographic position, Latitude and Longitude
        /// </summary>
        public GPGLL GPGLL;
        /// <summary>
        /// Estimated Position Error - Garmin proprietary sentence(!)
        /// </summary>
        public GPRME PGRME;

        /// <summary>
        /// A delegate type for hooking up change notifications.
        /// </summary>
        public delegate void NewGpsFixHandler(object sender, GpsEventArgs e);
		
        /// <summary>
        /// Overridden. Fires when the GpsHandler has received data from the GPS device.
        /// </summary>
        public event NewGpsFixHandler NewGpsFix;
		
        /// <summary>
        /// Event fired whenever new GPS data has been processed. Runs in GPS thread
        /// </summary>
        private event NewGpsFixHandler NewProcessedGpsFix;

        /// <summary>
        /// Initializes a GpsHandler for communication with GPS receiver.
        /// The GpsHandler is used for communication with the GPS device and process information from the GPS revice.
        /// </summary>
        public GpsHandler()
        {
            _disposed = false;
            NewProcessedGpsFix += GpsEventHandler;
			
            //Link event from GPS receiver to process data function
            GpsPort.NewGpsData += GpsDataEventHandler;
            GPRMC = new GPRMC();
            GPGGA = new GPGGA();
            GPGSA = new GPGSA();
            GPRMC = new GPRMC();
            PGRME = new GPRME();
            GPGSV = new GPGSV();
        }
		
        /// <summary>
        /// Gets a boolean stating whether the port to the GPS device is open.
        /// </summary>
        public bool IsPortOpen 
        {
            get { return GpsPort.IsPortOpen; }
        }
		
        /// <summary>
        /// Get a boolean stating whether the GPS device has a fix or not.
        /// </summary>
        public bool HasGpsFix
        {
            get 
            {
                return (GPGGA.FixQuality != GPGGA.FixQualityEnum.Invalid);
            }
        }

        /// <summary>
        /// Parse event from GPS thread to parent thread
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">GPSEventArgs</param>
        private void GpsEventHandler(object sender, GpsEventArgs e)
        {
            NewGpsFix(this, e);
        }

        /// <summary>
        /// Eventtype invoked when a new message is received from the GPS.
        /// String GPSEventArgs.TypeOfEvent specifies eventtype.
        /// </summary>
        public class GpsEventArgs:EventArgs
        {
            /// <summary>
            /// Type of event
            /// </summary>
            public GpsEventType TypeOfEvent;
            /// <summary>
            /// Full NMEA sentence
            /// </summary>
            public string Sentence;
        }

        /// <summary>
        /// Returns Garmin estimated horisontal error. This is Garmin proprietary message and may not function with all GPS devices.
        /// </summary>
        public double GpsAccuracy 
        {
            get 
            { 
                return PGRME.EstHorisontalError;
            }
        }

        /// <summary>
        /// Indicates whether NMEA input is emulated from file
        /// </summary>
        /// <returns>true of emulate is on</returns>
        public bool Emulate
        {
            get { return _emulate; }
            set
            {
                if(value)
                    if (!File.Exists(NMEAInputFile))
                        throw(new Exception("Error. File not set or not found"));
                _emulate = value;
            }
        }
        private bool _emulate;
        internal static string NMEAInputFile;
		
        /// <summary>
        /// Turns on NMEA emulation
        /// </summary>
        /// <param name="fileName">File to read NMEA sentences from</param>
        public void EnableEmulate(string fileName)
        {
            _emulate = true;
            if(File.Exists(fileName))
                NMEAInputFile = fileName;
            else
                throw(new Exception("Error. File not found"));
        }

        /// <summary>
        /// Starts the GPS thread and opens the port.
        /// </summary>
        /// <param name="baudRate">Baudrate (usually 4800).</param>
        /// <param name="serialPort">Serialport number where GPS receiver is connected (ie. "COM1").</param>
        public void Start(string serialPort, int baudRate)
        {
            GpsPort.Port = serialPort;
            GpsPort.BaudRate = baudRate;
            _clThreadStart = !_emulate ? GpsPort.Start: new ThreadStart(new NMEAEmulator().Emulator);
            _clThread = new Thread(_clThreadStart);

            _clThread.Start();
        }

        /// <summary>
        /// Writes data to the GPS device. For instance RTCM data for Differential GPS.
        /// </summary>
        /// <param name="buffer">RTCM or control data to send to GPS</param>
        public void WriteToGps(byte[] buffer) 
        {
            GpsPort.Write(buffer);
        }

        /// <summary>
        /// Method called when a GPS event occured.
        /// This is where we call the methods that parses each kind of NMEA sentence
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GpsDataEventHandler(object sender, SerialPort.GpsEventArgs e) 
        {
            switch (e.TypeOfEvent)
            {
                case GpsEventType.GPRMC: 
                    ParseRMC(e.Sentence);
                    break;
                case GpsEventType.GPGGA:
                    ParseGGA(e.Sentence);
                    break;
                case GpsEventType.GPGLL:
                    ParseGLL(e.Sentence);
                    break;
                case GpsEventType.GPGSA:
                    ParseGSA(e.Sentence);
                    break;
                case GpsEventType.GPGSV:
                    ParseGSV(e.Sentence);
                    break;
                case GpsEventType.PGRME:
                    ParseRME(e.Sentence);
                    break;
                case GpsEventType.TimeOut:
                    FireTimeOut();
                    break;
                case GpsEventType.Unknown:
                    GpsEventArgs e2 = new GpsEventArgs
                                          {
                                              TypeOfEvent = e.TypeOfEvent, 
                                              Sentence = e.Sentence
                                          };
                    NewProcessedGpsFix(this, e2);
                    break;
                default: break;
            }
        }

        /// <summary>
        /// Stops the GPS thread and closes the port.
        /// </summary>
        public void Stop()
        {
            GPGGA.FixQuality = GPGGA.FixQualityEnum.Invalid;
            if(_clThread!=null)
                _clThread.Abort();
            GpsPort.Stop();
            _clThread = null;
            _clThreadStart = null;
        }

        /// <summary>
        /// Disposes the GpsHandler and if nessesary calls Stop()
        /// </summary>
        public void Dispose() 
        {
            if (!_disposed)
            {
                Stop();
                GpsPort.Dispose();
                GPGGA = null;
                GPGLL = null;
                GPGSA = null;
                GPRMC = null;
                PGRME = null;
                GpsPort = null;
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Finalizer
        /// </summary>
        ~GpsHandler()
        {
            Dispose();
        }

        /// <summary>
        /// Gets or sets the GpsHandler TimeOut (default: 5 seconds).
        /// </summary>
        public int TimeOut 
        {
            get { return GpsPort.TimeOut; }
            set { GpsPort.TimeOut = value; }
        }

        /// <summary>
        /// Private method for Firing a serialport timeout event
        /// </summary>
        private void FireTimeOut() 
        {
            GPGGA.FixQuality = GPGGA.FixQualityEnum.Invalid;
            GpsEventArgs e = new GpsEventArgs
                                 {
                                     TypeOfEvent = GpsEventType.TimeOut
                                 };
            NewProcessedGpsFix(this, e);
        }

        /// <summary>
        /// Private method for parsing the GPGLL NMEA sentence
        /// </summary>
        /// <param name="strGLL">GPGLL sentence</param>
        private void ParseGLL(string strGLL)
        {
            GPGLL = new GPGLL(strGLL);
            GpsEventArgs e = new GpsEventArgs
                                 {
                                     TypeOfEvent = GpsEventType.GPGLL, 
                                     Sentence = strGLL
                                 };
            NewProcessedGpsFix(this, e);
        }

        /// <summary>
        /// Private method for parsing the GPGSV NMEA sentence
        /// GPGSV is a bit different, since it if usually made from several NMEA sentences
        /// </summary>
        /// <param name="strGSV">GPGSV sentence</param>
        private void ParseGSV(string strGSV)
        {
            //fire the event if last GSV message.
            if(GPGSV.AddSentence(strGSV)) 
            {
                GpsEventArgs e = new GpsEventArgs
                                     {
                                         TypeOfEvent = GpsEventType.GPGSV, 
                                         Sentence = strGSV
                                     };
                NewProcessedGpsFix(this, e);
            }
        }

        /// <summary>
        /// Private method for parsing the GPGSA NMEA sentence
        /// </summary>
        /// <param name="strGSA">GPGSA sentence</param>
        private void ParseGSA(string strGSA)
        {
            GPGSA = new GPGSA(strGSA);
            //fire the event.
            GpsEventArgs e = new GpsEventArgs
                                 {
                                     TypeOfEvent = GpsEventType.GPGSA, 
                                     Sentence = strGSA
                                 };
            NewProcessedGpsFix(this, e);
        }

        /// <summary>
        /// Private method for parsing the GPGGA NMEA sentence
        /// </summary>
        /// <param name="strGGA">GPGGA sentence</param>
        private void ParseGGA(string strGGA)
        {
            GPGGA = new GPGGA(strGGA);
            //fire the event.
            GpsEventArgs e = new GpsEventArgs
                                 {
                                     TypeOfEvent = GpsEventType.GPGGA, 
                                     Sentence = strGGA
                                 };
            NewProcessedGpsFix(this, e);
        }
							
        /// <summary>
        /// Private method for parsing the GPRMC NMEA sentence
        /// </summary>
        /// <param name="strRMC">GPRMC sentence</param>
        private void ParseRMC(string strRMC)
        {
            GPRMC = new GPRMC(strRMC);

            //fire the event.
            GpsEventArgs e = new GpsEventArgs
                                 {
                                     TypeOfEvent = GpsEventType.GPRMC, 
                                     Sentence = strRMC
                                 };
            NewProcessedGpsFix(this, e);
        }

        /// <summary>
        /// Private method for parsing the PGRME NMEA sentence
        /// </summary>
        /// <param name="strRME">GPRMC sentence</param>
        private void ParseRME(string strRME)
        {
            PGRME = new GPRME(strRME);
            //fire the event.
            GpsEventArgs e = new GpsEventArgs
                                 {
                                     TypeOfEvent = GpsEventType.PGRME, 
                                     Sentence = strRME
                                 };
            NewProcessedGpsFix(this, e);
        }
		
        /// <summary>
        /// Converts GPS position in d"dd.ddd' to decimal degrees ddd.ddddd
        /// </summary>
        /// <param name="dm"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        internal static double GPSToDecimalDegrees(string dm, string dir)
        {
            try
            {
                if (dm == "" || dir == "")
                {
                    return 0.0;
                }
                //Get the fractional part of minutes
                //DM = '5512.45',  Dir='N'
                //DM = '12311.12', Dir='E'

                double fm = double.Parse(dm.Substring(dm.IndexOf(".")),NumberFormatEnUs);

                //Get the minutes.
                double min = double.Parse(dm.Substring(dm.IndexOf(".") - 2, 2), NumberFormatEnUs);

                //Degrees
                double deg = double.Parse(dm.Substring(0, dm.IndexOf(".") - 2), NumberFormatEnUs);
				
                if (dir == "S" || dir == "W")
                    deg = -(deg + (min + fm) / 60);
                else
                    deg = deg + (min + fm) / 60;
                return deg;
            }
            catch
            {
                return 0.0;
            }
        }



        internal class NMEAEmulator : IDisposable
        {
            public event SerialPort.NewGpsDataHandler NewGpsData;
            private StreamReader _file;
            public void Emulator()
            {
                _file = new StreamReader(NMEAInputFile);
                while (true)
                {
                    if (_file.EndOfStream)
                    {
                        //Start from beginning of file
                        _file.Close();
                        _file = new StreamReader(NMEAInputFile);
                    }
                    string line = _file.ReadLine();
                    SerialPort.GpsEventArgs e = new SerialPort.GpsEventArgs
                                                    {
                                                        TypeOfEvent = String2Eventtype(line), 
                                                        Sentence = line
                                                    };
                    NewGpsData(this, e);
                    Thread.Sleep(50);
                }
            }
            public void Dispose()
            {
                _file.Close();
                _file = null;
            }
        }

        /// <summary>
        /// Analyzes a NMEA sentence and returns the corresponding NMEA sentence type
        /// </summary>
        /// <param name="strData">NMEA Sentence</param>
        /// <returns>Sentence type</returns>
        internal static GpsEventType String2Eventtype(string strData)
        {
            if (strData.StartsWith("$" + GpsEventType.GPGGA))
                return GpsEventType.GPGGA;
            if (strData.StartsWith("$" + GpsEventType.GPGLL))
                return GpsEventType.GPGLL;
            if (strData.StartsWith("$" + GpsEventType.GPGSA))
                return GpsEventType.GPGSA;
            if (strData.StartsWith("$" + GpsEventType.GPGSV))
                return GpsEventType.GPGSV;
            if (strData.StartsWith("$" + GpsEventType.GPRMC))
                return GpsEventType.GPRMC;
            return strData.StartsWith("$" + GpsEventType.PGRME) ? GpsEventType.PGRME : GpsEventType.Unknown;
        }
    }
}