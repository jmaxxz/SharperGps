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
using System.Collections.Generic;

namespace Ares.SharperGps.NMEA
{
    /// <summary>
    /// GPS DOP and active satellites
    /// </summary>
    public class GPGSA
    {

        /// <summary>
        /// Enum for the GSA Fix mode
        /// </summary>
        public enum GSAFixModeEnum
        {
            /// <summary>
            /// No fix available
            /// </summary>
            FixNotAvailable = 0,
            /// <summary>
            /// Horisontal fix only
            /// </summary>
            _2D = 2,
            /// <summary>
            /// 3D fix
            /// </summary>
            _3D = 3
        }

        /// <summary>
        /// Initializes the NMEA GPS DOP and active satellites
        /// </summary>
        public GPGSA()
        {
            _prnInSolution = new List<string>();
        }

        /// <summary>
        ///  GPS DOP and active satellites and parses an NMEA sentence
        /// </summary>
        /// <param name="nmeaSentence"></param>
        public GPGSA(string nmeaSentence)
        {
            _prnInSolution = new List<string>(); 
            try
            {
                if (nmeaSentence.IndexOf('*') > 0)
                    nmeaSentence = nmeaSentence.Substring(0, nmeaSentence.IndexOf('*'));
                //Split into an array of strings.
                string[] split = nmeaSentence.Split(new[] { ',' });
                _mode = split[1].Length > 0 ? split[1][0] : ' ';
                if (split[2].Length > 0)
                {
                    switch (split[2])
                    {
                        case "2": _fixMode = GSAFixModeEnum._2D; break;
                        case "3": _fixMode = GSAFixModeEnum._3D; break;
                        default: _fixMode = GSAFixModeEnum.FixNotAvailable; break;
                    }
                }
                _prnInSolution.Clear();
                for (int i = 0; i <= 11; i++)
                    if(split[i + 3]!="")
                        _prnInSolution.Add(split[i + 3]);
                double.TryParse(split[15], out _pdop);
                double.TryParse(split[16], out _hdop);
                double.TryParse(split[17], out _vdop);
            }
            catch { }
        }


        private readonly char _mode;
        private readonly GSAFixModeEnum _fixMode;
        private readonly List<string> _prnInSolution;
        private readonly double _pdop;
        private readonly double _hdop;
        private readonly double _vdop;

        /// <summary>
        /// Mode. M=Manuel, A=Auto (forced/not forced to operate in 2D or 3D mode)
        /// </summary>
        public char Mode
        {
            get { return _mode; }
        }

        /// <summary>
        /// Fix not available / 2D / 3D
        /// </summary>
        public GSAFixModeEnum FixMode
        {
            get { return _fixMode; }
        }

        /// <summary>
        /// PRN Numbers used in solution
        /// </summary>
        public List<string> PRNInSolution
        {
            get { return _prnInSolution; }
        }

        /// <summary>
        /// Point Dilution of Precision
        /// </summary>
        public double PDOP
        {
            get { return _pdop; }
        }

        /// <summary>
        /// Horisontal Dilution of Precision
        /// </summary>
        public double HDOP
        {
            get { return _hdop; }
        }

        /// <summary>
        /// Vertical Dilution of Precision
        /// </summary>
        public double VDOP
        {
            get { return _vdop; }
        }
    }
}