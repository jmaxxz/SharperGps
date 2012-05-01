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
namespace Ares.SharperGps
{
    /// <summary>
    /// GPSEventType
    /// </summary>
    public enum GpsEventType 
    {
        /// <summary>
        /// Recommended minimum specific GPS/Transit data
        /// </summary>
        GPRMC,
        /// <summary>
        /// Global Positioning System Fix Data
        /// </summary>
        GPGGA,
        /// <summary>
        /// Satellites in view
        /// </summary>
        GPGSV,
        /// <summary>
        /// GPS DOP and active satellites
        /// </summary>
        GPGSA,
        /// <summary>
        /// Geographic position, Latitude and Longitude
        /// </summary>
        GPGLL,
        /// <summary>
        /// Estimated Position Error - Garmin proprietary sentence(!)
        /// </summary>
        PGRME,
        /// <summary>
        /// Data timeout event fired when data haven't been received from GPS device for a while
        /// </summary>
        TimeOut,
        /// <summary>
        /// GPS sentence not recognized, unknown or not implemented
        /// </summary>
        Unknown,
        /// <summary>
        /// Fired when Fix is lost (to be implemented)
        /// </summary>
        FixLost,
        /// <summary>
        /// Fired when valid Fix is acquired (to be implemented)
        /// </summary>
        FixAquired
    }
}