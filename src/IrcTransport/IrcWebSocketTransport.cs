/*
 * $Id$
 * $URL$
 * $Rev$
 * $Author$
 * $Date$
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 *
 * Copyright (c) 2003-2009 Mirco Bauer <meebey@meebey.net> <http://www.meebey.net>
 * Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 * Copyright (c) 2015 Katy Coe <djkaty@start.no> <http://www.djkaty.com>
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
using WebSocketSharp;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    /// WebSocket-based IRC protocol using websocket-sharp library
    /// </summary>
    public class IrcWebSocketTransport : IIrcTransportManager
    {
        private bool _IsConnected;
        private bool _IsConnectionError;

        /// <summary>
        /// Event which fires when a line of text is received from the connection
        /// </summary>
        public event ReadLineEventHandler OnMessageReceived;

        /// <summary>
        /// The underlying WebSocket of the connection
        /// </summary>
        [CLSCompliant(false)]
        public WebSocket Socket {
            get; set;
        }

        public bool IsConnected {
            get {
                return _IsConnected;
            }
        }

        public bool IsConnectionError {
            get {
                return _IsConnectionError;
            }
        }

        public string Address {
            get {
                return Socket.Url.OriginalString;
            }

            set {
                throw new NotImplementedException();
            }
        }

        public int Port {
            get {
                return 0;
            }

            set {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// WebSocket text frames always use UTF-8 encoding as defined in RFC 6455
        /// </summary>
        public System.Text.Encoding Encoding {
            get {
                return System.Text.Encoding.UTF8;
            }

            set {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Create a new WebSocket instance with the specified host address
        /// </summary>
        /// <param name="address"></param>
        public IrcWebSocketTransport(string address)
        {
            Socket = new WebSocket(address);
            Socket.OnOpen += Socket_OnOpen;
            Socket.OnMessage += Socket_OnMessage;
            Socket.OnClose += Socket_OnClose;
            Socket.OnError += Socket_OnError;
        }

        public void Connect()
        {
            _IsConnected = _IsConnectionError = false;

            // Blocks until connection is established or fails
            Socket.Connect();

            if (!Socket.IsAlive)
                throw new CouldNotConnectException("could not connect to WebSocket");
        }

        public void Disconnect()
        {
            Socket.Close();

            _IsConnected = _IsConnectionError = false;
        }

        public bool WriteLine(string data)
        {
            Socket.Send(data + "\r\n");

            return _IsConnected && !_IsConnectionError;
        }

        /// <summary>
        /// Received when the WebSocket connection is opened
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Socket_OnOpen(object sender, EventArgs e)
        {
            _IsConnected = true;
        }

        /// <summary>
        /// Received when the WebSocket connection is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Socket_OnClose(object sender, CloseEventArgs e)
        {
            _IsConnected = _IsConnectionError = false;
        }

        /// <summary>
        /// Received when data is received from the WebSocket connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Socket_OnMessage(object sender, MessageEventArgs e)
        {
            if (OnMessageReceived != null)
                OnMessageReceived(sender, new ReadLineEventArgs(e.Data.Substring(0, e.Data.Length - 2)));
        }

        /// <summary>
        /// Received when a WebSocket connection error occurs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Socket_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            _IsConnectionError = true;
        }
    }
}
