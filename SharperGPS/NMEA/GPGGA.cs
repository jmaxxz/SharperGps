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
namespace Ares.SharperGps.NMEA
{
    using System;

    /// <summary>
    /// Global Positioning System Fix Data
    /// </summary>
    public class GPGGA
    {
        /// <summary>
        /// Initializes the NMEA Global Positioning System Fix Data
        /// </summary>
        public GPGGA()
        {
            _position = new Coordinate();
        }

        /// <summary>
        /// Initializes the NMEA Global Positioning System Fix Data and parses an NMEA sentence
        /// </summary>
        /// <param name="nmeaSentence"></param>
        public GPGGA(string nmeaSentence)
        {
            try
            {
                if (nmeaSentence.IndexOf('*') > 0)
                    nmeaSentence = nmeaSentence.Substring(0, nmeaSentence.IndexOf('*'));
                //Split into an array of strings.
                string[] split = nmeaSentence.Split(new[] { ',' });
                if (split[1].Length >= 6)
                {
                    TimeSpan t = new TimeSpan(GpsHandler.IntTryParse(split[1].Substring(0, 2)),
                                              GpsHandler.IntTryParse(split[1].Substring(2, 2)), GpsHandler.IntTryParse(split[1].Substring(4, 2)));
                    DateTime nowutc = DateTime.UtcNow;
                    nowutc = nowutc.Add(-nowutc.TimeOfDay);
                    _timeOfFix = nowutc.Add(t);

                }

                _position = new Coordinate(GpsHandler.GPSToDecimalDegrees(split[4], split[5]),
                                           GpsHandler.GPSToDecimalDegrees(split[2], split[3]));
                if (split[6] == "1")
                    FixQuality = FixQualityEnum.GPS;
                else if (split[6] == "2")
                    FixQuality = FixQualityEnum.DGPS;
                else
                    FixQuality = FixQualityEnum.Invalid;
                _noOfSats = Convert.ToByte(split[7]);
                GpsHandler.DblTryParse(split[8], out _dilution);
                GpsHandler.DblTryParse(split[9], out _altitude);
                _altitudeUnits = split[10][0];
                GpsHandler.DblTryParse(split[11], out _heightOfGeoid);
                GpsHandler.IntTryParse(split[13], out _dGPSUpdate);
                _dGPSStationID = split[14];
            }
            catch { }
        }

        /// <summary>
        /// Enum for the GGA Fix Quality.
        /// </summary>
        public enum FixQualityEnum
        {
            /// <summary>
            /// Invalid fix
            /// </summary>
            Invalid = 0,
            /// <summary>
            /// GPS fix
            /// </summary>
            GPS = 1,
            /// <summary>
            /// DGPS fix
            /// </summary>
            DGPS = 2
        }


        private readonly DateTime _timeOfFix;
        private readonly Coordinate _position;
        private readonly byte _noOfSats;
        private readonly double _altitude;
        private readonly char _altitudeUnits;
        private readonly double _dilution;
        private readonly double _heightOfGeoid;
        private readonly int _dGPSUpdate;
        private readonly string _dGPSStationID;

        /// <summary>
        /// time of fix (hhmmss).
        /// </summary>
        public DateTime TimeOfFix
        {
            get { return _timeOfFix; }
        }

        /// <summary>
        /// Coordinate of recieved position
        /// </summary>
        public Coordinate Position
        {
            get { return _position; }
        }

        /// <summary>
        /// Fix quality (0=invalid, 1=GPS fix, 2=DGPS fix)
        /// </summary>
        public FixQualityEnum FixQuality { get; internal set; }

        /// <summary>
        /// number of satellites being tracked.
        /// </summary>
        public byte NoOfSats
        {
            get { return _noOfSats; }
        }

        /// <summary>
        /// Altitude above sea level.
        /// </summary>
        public double Altitude
        {
            get { return _altitude; }
        }

        /// <summary>
        /// Altitude Units - M (meters).
        /// </summary>
        public char AltitudeUnits
        {
            get { return _altitudeUnits; }
        }

        /// <summary>
        /// Horizontal dilution of position (HDOP).
        /// </summary>
        public double Dilution
        {
            get { return _dilution; }
        }

        /// <summary>
        /// Height of geoid (mean sea level) above WGS84 ellipsoid.
        /// </summary>
        public double HeightOfGeoid
        {
            get { return _heightOfGeoid; }
        }

        /// <summary>
        /// Time in seconds since last DGPS update.
        /// </summary>
        public int DGPSUpdate
        {
            get { return _dGPSUpdate; }
        }

        /// <summary>
        /// DGPS station ID number.
        /// </summary>
        public string DGPSStationID
        {
            get { return _dGPSStationID; }
        }
    }
}