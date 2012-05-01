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
namespace Ares.SharperGps.NTRIP
{
    using System.Net.Sockets;
    using System.Text;
    using System.Net;
    using System;
    

    /// <summary>
    /// Networked Transport of RTCM via Internet Protocol (Ntrip) is an application-level protocol that
    /// supports streaming Global Navigation Satellite System (GNSS) data over the Internet. Ntrip is a
    /// generic, stateless protocol based on the Hypertext Transfer Protocol HTTP/1.1. The HTTP
    /// objects are extended to GNSS data streams.
    /// </summary>
    /// <remarks>
    /// <para>Ntrip is designed to disseminate differential correction data or other kinds of GNSS streaming
    /// data to stationary or mobile users over the Internet, allowing simultaneous PC, Laptop, PDA, or
    /// receiver connections to a broadcasting host. Ntrip supports wireless Internet access through
    /// Mobile IP Networks like GSM, GPRS, EDGE, or UMTS.</para>
    /// <para>Ntrip is meant to be an open non-proprietary protocol. Major characteristics of Ntrip’s
    /// dissemination technique are the following:</para>
    /// <para>• It is based on the popular HTTP streaming standard; it is comparatively easy to implement
    /// when limited client and server platform resources are available.<br/>
    /// • Its application is not limited to one particular plain or coded stream content; it has the ability
    /// to distribute any kind of GNSS data.<br/>
    /// • It has the potential to support mass usage; it can disseminate hundreds of streams
    /// simultaneously for up to a thousand users when applying modified Internet Radio
    /// broadcasting software.<br/>
    /// • Regarding security needs, stream providers and users are not necessarily in direct contact,
    /// and streams are usually not blocked by firewalls or proxy servers protecting Local Area
    /// Networks.<br/>
    /// • It enables streaming over any mobile IP network because it uses TCP/IP.</para>
    /// </remarks>
	
    public class NTRIPClient : IDisposable
    {
        private Socket _socket;
        private string _username;
        private string _password;
        readonly byte[] _rtcmDataBuffer = new byte[128];
        //IAsyncResult m_asynResult;
        public AsyncCallback pfnCallBack;

        /// <summary>
        /// NTRIP server Username
        /// </summary>
        public string UserName
        {
            get { return _username; }
            set { _username = value; }
        }

        /// <summary>
        /// NTRIP server password
        /// </summary>
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>
        /// NTRIP Server
        /// </summary>
        public IPEndPoint BroadCaster { get; set; }

        public NTRIPClient(IPEndPoint server)
        {
            //Initialization...
            BroadCaster = server;
            //InitializeSocket();
        }

        public NTRIPClient(IPEndPoint server, string strUserName, string strPassword)
        {
            BroadCaster = server;
            _username = strUserName;
            _password = strPassword;
            //InitializeSocket();
        }

        private void InitializeSocket()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

		

        /// <summary>
        /// Creates request to NTRIP server
        /// </summary>
        /// <param name="strRequest">Resource to request. Leave blank to get NTRIP service data</param>
        private byte[] CreateRequest(string strRequest)
        {
            //Build request message
            string msg = "GET /" + strRequest + " HTTP/1.1\r\n";
            msg += "User-Agent: SharperGps iter.dk\r\n";
            //If password/username is specified, send login details
            if (!string.IsNullOrEmpty(_username))
            {
                string auth = ToBase64(_username + ":" + _password);
                msg += "Authorization: Basic " + auth + "\r\n";
            }
            msg += "Accept: */*\r\nConnection: close\r\n";
            msg += "\r\n";

            return Encoding.ASCII.GetBytes(msg);	
        }

        private void Connect()
        {
            //Connect to server	   
            if (!_socket.Connected)
                _socket.Connect(BroadCaster);
        }

        private void Close()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
        }

        public SourceTable GetSourceTable()
        {
            InitializeSocket();
            _socket.Blocking = true;
            Connect();
            _socket.Send(CreateRequest(""));
            string responseData = "";
            System.Threading.Thread.Sleep(1000); //Wait for response
            while (_socket.Available>0)
            {
                byte[] returndata = new byte[_socket.Available];
                _socket.Receive(returndata); //Get response
                responseData += Encoding.ASCII.GetString(returndata, 0, returndata.Length);
                System.Threading.Thread.Sleep(100); //Wait for response
            }
            Close();

            if (!responseData.StartsWith("SOURCETABLE 200 OK"))
                return null;
			
            SourceTable result = new SourceTable();
            foreach (string line in responseData.Split('\n'))
            {
                if(line.StartsWith("STR"))
                    result.DataStreams.Add(SourceTable.NTRIPDataStream.ParseFromString(line));
                else if (line.StartsWith("CAS"))
                    result.Casters.Add(SourceTable.NTRIPCaster.ParseFromString(line));
                else if (line.StartsWith("NET"))
                    result.Networks.Add(SourceTable.NTRIPNetwork.ParseFromString(line));
            }
            return result;
        }

        /// <summary>
        /// Opens the connection to the NTRIP server and starts receiving
        /// </summary>
        /// <param name="mountPoint">ID of Stream</param>
        public void StartNTRIP(string mountPoint)
        {
            InitializeSocket();
            _socket.Blocking = true;
            Connect();
            _socket.Send(CreateRequest(mountPoint));
            _socket.Blocking = false;
            WaitForData(_socket);
        }

        public void WaitForData(Socket sock)
        {
            //if (pfnCallBack == null)
            //	pfnCallBack = new AsyncCallback(OnDataReceived);
            AsyncCallback recieveData = OnDataReceived;
            sock.BeginReceive(_rtcmDataBuffer, 0, _rtcmDataBuffer.Length, SocketFlags.None,
                              recieveData, sock);
            //m_asynResult = sckt.BeginReceive(rtcmDataBuffer, 0, rtcmDataBuffer.Length, SocketFlags.None, pfnCallBack, null);
        }

        private void OnDataReceived(IAsyncResult ar)
        {
            Socket sock = (Socket)ar.AsyncState;
            int iRx = sock.EndReceive(ar);
            if (iRx > 0) //if we received at least one byte
            {
                try
                {
                    if (GpsHandler.GpsPort.IsPortOpen)
                        //Send RTCM data to GPS. We assume the data is valid RTCM
                        GpsHandler.GpsPort.Write(_rtcmDataBuffer);
                }
                catch (Exception ex)
                {
                    Close();
                    throw (new Exception("Error sending RTCM data to device:" + ex.Message));
                }
            }
            WaitForData(sock);
        }

        /// <summary>
        /// Stops receiving data from the NTRIP server
        /// </summary>
        private void StopNTRIP()
        {
            Close();
        }

        /// <summary>
        /// Apply AsciiEncoding to user name and password to obtain it as an array of bytes
        /// </summary>
        /// <param name="str">username:password</param>
        /// <returns>Base64 encoded username/password</returns>
        private static string ToBase64(string str)
        {
            Encoding asciiEncoding = Encoding.ASCII;
            byte[] byteArray = asciiEncoding.GetBytes(str);
            return Convert.ToBase64String(byteArray, 0, byteArray.Length);
        }

        #region IDisposable Members

        public void Dispose()
        {
            Close();
        }

        #endregion
    }
}