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
    /// Recommended minimum specific GPS/Transit data
    /// </summary>
    public class GPRMC
    {
        /// <summary>
        /// Enum for the Receiver Status information.
        /// </summary>
        public enum StatusEnum
        {
            /// <summary>
            /// Fix warning
            /// </summary>
            Warning,
            /// <summary>
            /// Fix OK
            /// </summary>
            Ok,
            /// <summary>
            /// Bad fix
            /// </summary>
            BadFix,
            /// <summary>
            /// GPS fix
            /// </summary>
            GPS,
            /// <summary>
            /// Differential GPS fix
            /// </summary>
            DGPS
        }

        /// <summary>
        /// Initializes the NMEA Recommended minimum specific GPS/Transit data
        /// </summary>
        public GPRMC()
        {
            _position = new Coordinate();
        }

        /// <summary>
        /// Initializes the NMEA Recommended minimum specific GPS/Transit data and parses an NMEA sentence
        /// </summary>
        /// <param name="nmeaSentence"></param>
        public GPRMC(string nmeaSentence)
        {
            try
            {
                //Split into an array of strings.
                string[] split = nmeaSentence.Split(new[] { ',' });

                //Extract date/time
                try
                {
                    string[] dateTimeFormats = { "ddMMyyHHmmss", "ddMMyy", "ddMMyyHHmmss.FFFFFF" };
                    if (split[9].Length >= 6) { //Require at least the date to be present 
                        string time = split[9] + split[1]; // +" 0";
                        _timeOfFix = DateTime.ParseExact(time, dateTimeFormats, GpsHandler.NumberFormatEnUs, System.Globalization.DateTimeStyles.AssumeUniversal);
                    }
                    else
                        _timeOfFix = new DateTime();
                }
                catch { _timeOfFix = new DateTime(); }

                _status = split[2] == "A" ? StatusEnum.Ok : StatusEnum.Warning;

                _position = new Coordinate(	GpsHandler.GPSToDecimalDegrees(split[5], split[6]),
                                           	GpsHandler.GPSToDecimalDegrees(split[3], split[4]));

                GpsHandler.DblTryParse(split[7], out _speed);
                GpsHandler.DblTryParse(split[8], out _course);
                GpsHandler.DblTryParse(split[10], out _magneticVariation);
            }
            catch { }
        }


        private readonly Coordinate _position;
        private readonly StatusEnum _status;
        private readonly DateTime _timeOfFix;
        private readonly double _speed;
        private readonly double _course;
        private readonly double _magneticVariation;

        /// <summary>
        /// Indicates the current status of the GPS receiver.
        /// </summary>
        public StatusEnum Status
        {
            get { return _status; }
        }

        /// <summary>
        /// Coordinate of recieved position
        /// </summary>
        public Coordinate Position
        {
            get { return _position; }
        }
		
        /// <summary>
        /// Groundspeed in knots.
        /// </summary>
        public double Speed
        {
            get { return _speed; }
        }
	
        /// <summary>
        /// Course (true, not magnetic) in decimal degrees.
        /// </summary>
        public double Course
        {
            get { return _course; }
        }

        /// <summary>
        /// MagneticVariation in decimal degrees.
        /// </summary>
        public double MagneticVariation
        {
            get { return _magneticVariation; }
        }

        /// <summary>
        /// Date and Time of fix - Greenwich mean time.
        /// </summary>
        public DateTime TimeOfFix
        {
            get { return _timeOfFix; }
        }
    }
}