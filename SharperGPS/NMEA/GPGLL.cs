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
    /// Geographic position, Latitude and Longitude
    /// </summary>
    public class GPGLL
    {
        /// <summary>
        /// Initializes the NMEA Geographic position, Latitude and Longitude
        /// </summary>
        public GPGLL()
        {
        }

        /// <summary>
        /// Initializes the NMEA Geographic position, Latitude and Longitude and parses an NMEA sentence
        /// </summary>
        /// <param name="nmeaSentence"></param>
        public GPGLL(string nmeaSentence)
        {
            try
            {
                //Split into an array of strings.
                string[] split = nmeaSentence.Split(new[] { ',' });

                try
                {
                    _position = new Coordinate(GpsHandler.GPSToDecimalDegrees(split[3], split[4]),
                                               GpsHandler.GPSToDecimalDegrees(split[1], split[2]));
                }
                catch { _position = null; }

                try
                {
                    _timeOfSolution = new TimeSpan(int.Parse(split[5].Substring(0, 2)),
                                                   int.Parse(split[5].Substring(2, 2)),
                                                   int.Parse(split[5].Substring(4)));	
                }
                catch
                {
                    _timeOfSolution = null; // TimeSpan.Zero;
                }
                _dataValid = (split[6] == "A");
            }
            catch { }
        }


        private readonly Coordinate _position ;
        private readonly TimeSpan? _timeOfSolution;
        private readonly bool _dataValid;

        /// <summary>
        /// Current position
        /// </summary>
        public Coordinate Position
        {
            get { return _position; }
        }

        /// <summary>
        /// UTC Of Position Solution
        /// </summary>
        public TimeSpan? TimeOfSolution
        {
            get { return _timeOfSolution; }
        }

        /// <summary>
        /// Data valid (true for valid or false for data invalid).
        /// </summary>
        public bool DataValid
        {
            get { return _dataValid; }
            //set { _dataValid = value; }
        }

    }
}