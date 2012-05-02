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
    using System.Collections.Generic;
    using System;


    /// <summary>
    /// Satellites in view
    /// </summary>
    public class GPGSV
    {
        /// <summary>
        /// Initializes NMEA "Satellites in view"
        /// </summary>
        public GPGSV()
        {
            _satellites = new List<Satellite>();
        }

        private bool _firstMessageParsed;

        /// <summary>
        /// Adds a GPGSV sentence, and parses it. 
        /// </summary>
        /// <param name="nmeaSentence">NMEA string</param>
        /// <returns>Returns true if this is the last message in GSV nmea sentences</returns>
        public bool AddSentence(string nmeaSentence)
        {
            bool lastmsg = false;
            try
            {
                //Split into an array of strings.
                string[] split = nmeaSentence.Split(',');

                int satsInView = 0;
                int.TryParse(split[3], out satsInView);

                int msgCount =  0;
                int.TryParse(split[1], out msgCount); //Number of GPGSV messages

                int msgno =  0;
                int.TryParse(split[2], out msgno); //Current messagenumber

                if (msgCount < msgno || msgno < 1) //check for invalid data (could be zero if parse failed)
                    return false;

                if (msgno == 1)
                {
                    _satellites.Clear(); //First message. Let's clear the satellite list
                    _firstMessageParsed = true;
                }
                else if (!_firstMessageParsed) //If we haven't received the first GSV message, return
                    return false;

                lastmsg = (msgCount == msgno); //Is this the last GSV message in the GSV messages?
                int satsInMsg;
                if (!lastmsg)
                    satsInMsg = 4; //If this isn't the last message, the message will hold info for 4 satellites
                else
                    satsInMsg = satsInView - 4 * (msgno - 1); //calculate number of satellites in last message
                for (int i = 0; i < satsInMsg; i++)
                {
                    Satellite sat = new Satellite
                                        {
                                            PRN = split[i*4 + 4],
                                            Elevation = Convert.ToByte(split[i*4 + 5]),
                                            Azimuth = Convert.ToInt16(split[i*4 + 6]),
                                            SNR = Convert.ToByte(split[i*4 + 7])
                                        };
                    _satellites.Add(sat);
                }
            }
            catch { }
            return lastmsg;
        }

        #region Properties

        //private int _satsInView;

        /// <summary>
        /// Number of satellites visible
        /// </summary>
        public int SatsInView
        {
            get	{ return _satellites.Count; }
        }

        private List<Satellite> _satellites;

        /// <summary>
        /// List of visible satellites
        /// </summary>
        public List<Satellite> Satellites
        {
            get { return _satellites; }
            set { _satellites = value; }
        }

        /// <summary>
        /// Returns 
        /// </summary>
        /// <param name="prn"></param>
        /// <returns></returns>
        public Satellite GetSatelliteByPRN(string prn)
        {
            foreach(Satellite sat in _satellites)
            {
                if (sat.PRN == prn)
                    return sat;
            }
            return null;
        }
		
        /// <summary>
        /// Space Vehicle (SV/Satellite) info structure
        /// </summary>
        public class Satellite
        {
            /// <summary>
            /// Pseudo-Random Number ID
            /// </summary>
            public string PRN;
            /// <summary>
            /// Elevation above horizon in degrees (0-90)
            /// </summary>
            public byte Elevation;
            /// <summary>
            /// Azimuth	in degrees (0-359)
            /// </summary>
            public short Azimuth;
            /// <summary>
            /// Signal-to-noise ratio in dBHZ (0-99)
            /// </summary>
            public byte SNR;
        }
        #endregion
    }
}