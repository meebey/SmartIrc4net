/*
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 *
 * Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch> <http://www.apophis.ch>
 * 
 * Full LGPL License: <http://www.gnu.org/licenses/lgpl.txt>
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */
 
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;

namespace Meebey.SmartIrc4net
{

    /// <summary>
    /// Dcc Send Connection, Filetransfer
    /// </summary>
    public class DccSend : DccConnection
    {
    
        #region Private Variables
        private Stream _File;
        private long _Filesize;
        private string _Filename;
        private bool _DirectionUp;
        private long _SentBytes;
        private DccSpeed _Speed;
        private byte[] _Buffer = new byte[8192];
        #endregion        
        
        #region Public Properties
        public long SentBytes {
            get {
                return _SentBytes;
            }
        }
        #endregion

        internal DccSend(IrcFeatures irc, string user, IPAddress externalIpAdress, Stream file, string filename, long filesize, DccSpeed speed, bool passive, Priority priority) : base()
        {
            this.Irc = irc;
            _DirectionUp = true;
            _File = file;
            _Filesize = filesize;
            _Filename = filename;
            _Speed = speed;
            User = user;
            
            if(passive) {
                irc.SendMessage(SendType.CtcpRequest, user, "DCC SEND \"" + filename + "\" " + HostToDccInt(externalIpAdress).ToString() + " 0 " + filesize + " " + session, priority);
            } else {
                DccServer = new TcpListener(new IPEndPoint(IPAddress.Any, 0));
                DccServer.Start();
                LocalEndPoint = (IPEndPoint)DccServer.LocalEndpoint;
                irc.SendMessage(SendType.CtcpRequest, user, "DCC SEND \"" + filename + "\" " + HostToDccInt(externalIpAdress).ToString() + " " + LocalEndPoint.Port + " " + filesize, priority);
            }
        }
        
        internal DccSend(IrcFeatures irc, IPAddress externalIpAdress, CtcpEventArgs e) : base()
        {
            /* Remote Request */
            this.Irc = irc;
            _DirectionUp = false;
            User = e.Data.Nick;
            
            if (e.Data.MessageArray.Length > 4) {
                long ip, filesize = 0; int port = 0;
                bool okIP = long.TryParse(e.Data.MessageArray[3], out ip);
                bool okPo = int.TryParse(e.Data.MessageArray[4], out port);  // port 0 = passive
                if (e.Data.MessageArray.Length > 5) {
                    bool okFs = long.TryParse(FilterMarker(e.Data.MessageArray[5]), out filesize);
                    _Filesize = filesize;
                    _Filename = e.Data.MessageArray[2].Trim(new char[] {'\"'});
                }
                if (okIP && okPo) {
                    RemoteEndPoint = new IPEndPoint(IPAddress.Parse(DccIntToHost(ip)), port);
                    DccSendRequestEvent(new DccSendRequestEventArgs(this, e.Data.MessageArray[2], filesize));
                    return;
                } else {
                    irc.SendMessage(SendType.CtcpReply, e.Data.Nick, "ERRMSG DCC Send Parameter Error");
                }
            } else {
                irc.SendMessage(SendType.CtcpReply, e.Data.Nick, "ERRMSG DCC Send not enough parameters");
            }


        }
        
        internal override void InitWork(Object stateInfo)
        {
            if (!Valid)
                return;
            if (DccServer != null) {
                Connection = DccServer.AcceptTcpClient();
                RemoteEndPoint = (IPEndPoint)Connection.Client.RemoteEndPoint;
                DccServer.Stop();
                isConnected = true;
            } else {
                while(!isConnected) {
                    Thread.Sleep(500);    // We wait till Request is Accepted (or jump out when rejected)
                    if (reject) 
                        return;
                }
            }

            DccSendStartEvent(new DccEventArgs(this));
            int bytes;
            
            if(_DirectionUp) {
                do{
                    while (Connection.Available > 0) {
                        switch(_Speed)
                        {
                            case DccSpeed.Rfc:
                                Connection.GetStream().Read(_Buffer, 0, _Buffer.Length);
                                // TODO: only send x not ACKed Bytes ahead / (nobody wants this anyway)
                                break;
                            case DccSpeed.RfcSendAhead:
                                Connection.GetStream().Read(_Buffer, 0, _Buffer.Length);
                                break;
                            case DccSpeed.Turbo: // Available > 0 should not happen
                                break;
                        }                        
                    }
                    
                    bytes = _File.Read(_Buffer, 0, _Buffer.Length);
                    try {
                        Connection.GetStream().Write(_Buffer, 0, (int)bytes);
                    } catch (IOException) {
                        bytes = 0;    // Connection Lost
                    }
                        
                    _SentBytes += bytes;
        
                    if (bytes > 0) {
                        DccSendSentBlockEvent(new DccSendEventArgs(this, _Buffer, bytes));
                        Console.Write(".");
                    }
                } while(bytes > 0);
            } else {
                while((bytes = Connection.GetStream().Read(_Buffer,0,_Buffer.Length))>0) {
                    _File.Write(_Buffer, 0, bytes);
                    _SentBytes += bytes;
                    if (_Speed != DccSpeed.Turbo)
                        Connection.GetStream().Write(getAck(_SentBytes),0,4);
                    
                    DccSendReceiveBlockEvent(new DccSendEventArgs(this, _Buffer, bytes));
                }        
            }
            
            
            isValid = false;
            isConnected = false;
            Console.WriteLine("--> Filetrangsfer Endet / Bytes sent: " + _SentBytes + " of " + _Filesize);
            DccSendStopEvent(new DccEventArgs(this));            
        }

        #region Public Methods for the DCC Send Object
        
        /// <summary>
        /// With this methode you can accept a DCC SEND Request you got from another User
        /// </summary>
        /// <param name="file">Any Stream you want use as a file, if you use offset it should be Seekable</param>
        /// <param name="offset">Offset to start a Resume Request for the rest of a file</param>
        /// <returns></returns>
        public bool AcceptRequest(Stream file, long offset)
        {
            if (isConnected)
                return false;
            try {
                if (file!=null)
                    _File = file;
                if(RemoteEndPoint.Port==0) {
                    DccServer = new TcpListener(new IPEndPoint(IPAddress.Any, 0));
                    DccServer.Start();
                    LocalEndPoint = (IPEndPoint)DccServer.LocalEndpoint;
                    Irc.SendMessage(SendType.CtcpRequest, User, "DCC SEND \"" + _Filename + "\" " + HostToDccInt(ExternalIPAdress).ToString() + " " + LocalEndPoint.Port + " " + _Filesize);
                } else {
                    if(offset==0) {
                        Connection = new TcpClient();
                        Connection.Connect(RemoteEndPoint);
                        isConnected = true;
                    } else {
                        if(_File.CanSeek) {
                            _File.Seek(offset, SeekOrigin.Begin);
                            _SentBytes = offset;
                            Irc.SendMessage(SendType.CtcpRequest, User, "DCC RESUME \"" + _Filename + "\" " + RemoteEndPoint.Port + " " + offset);
                        } else {
                            /* Resume of a file which is not seekable : I dont care, its your filestream! */
                            _SentBytes = offset;
                            Irc.SendMessage(SendType.CtcpRequest, User, "DCC RESUME \"" + _Filename + "\" " + RemoteEndPoint.Port + " " + offset);
                        }
                    }
                }
                return true;
            } catch(Exception) {
                isValid = false;
                isConnected = false;
                return false;
            }
        }
        #endregion
        
        #region Handler for Passive / Resume DCC
        internal bool TryResume(CtcpEventArgs e) {
            if (User == e.Data.Nick) {
                if ((e.Data.MessageArray.Length > 4) && (_Filename == e.Data.MessageArray[2].Trim(new char[] {'\"'}))) {
                    long offset = 0;
                    long.TryParse(FilterMarker(e.Data.MessageArray[4]), out offset);
                    if (_File.CanSeek) {
                        if (e.Data.MessageArray.Length > 5) {
                            Irc.SendMessage(SendType.CtcpRequest, e.Data.Nick, "DCC ACCEPT " + e.Data.MessageArray[2] + " " + e.Data.MessageArray[3] + " " + e.Data.MessageArray[4] + " " + FilterMarker(e.Data.MessageArray[5]));
                        } else {
                            Irc.SendMessage(SendType.CtcpRequest, e.Data.Nick, "DCC ACCEPT " + e.Data.MessageArray[2] + " " + e.Data.MessageArray[3] + " " + FilterMarker(e.Data.MessageArray[4]));
                        }
                    
                        _File.Seek(offset, SeekOrigin.Begin);
                        _SentBytes = offset;
                        return true;
                    } else {
                        Irc.SendMessage(SendType.CtcpRequest, e.Data.Nick, "ERRMSG DCC File not seekable");                            
                    }
                }
            }
            return false;
        }
        
        internal bool TryAccept(CtcpEventArgs e) {
            if (User == e.Data.Nick) {
                if ((e.Data.MessageArray.Length > 4) && (_Filename == e.Data.MessageArray[2].Trim(new char[] {'\"'}))) {
                    return this.AcceptRequest(null, 0);                    
                }
            }
            return false;
        }
        
        internal bool SetRemote(CtcpEventArgs e) {
            long ip; 
            int port = 0;
            bool okIP = long.TryParse(e.Data.MessageArray[3], out ip);
            bool okPo = int.TryParse(e.Data.MessageArray[4], out port);  // port 0 = passive
            if (okIP && okPo) {
                RemoteEndPoint = new IPEndPoint(IPAddress.Parse(DccIntToHost(ip)), port);
                return true;
            }
            return false;
        }
            
        #endregion
        
    }
}
