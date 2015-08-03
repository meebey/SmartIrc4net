/*
 * $Id$
 * $URL$
 * $Rev$
 * $Author$
 * $Date$
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 *
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
using System.Collections.Generic;
using System.Linq;
using WebSocketSharp;
using WebSocketSharp.Net;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    /// WebSocket-based IRC protocol using websocket-sharp library
    /// </summary>
    public class IrcWebSocketTransport : IIrcTransportManager
    {
        private bool _IsConnected;
        private bool _IsConnectionError;
        private bool _IsDisconnecting;
        private bool _IsConnecting;

        /// <summary>
        /// Event which fires when a line of text is received from the connection
        /// </summary>
        public event ReadLineEventHandler OnMessageReceived;

        /// <summary>
        /// Event which fires when a connection error occurs
        /// </summary>
        public event Action OnConnectionError;

        /// <summary>
        /// The underlying WebSocket of the connection
        /// </summary>
        [CLSCompliant(false)]
        public WebSocket Socket { get; set; }

        /// <summary>
        /// Return true if the connection is connected, false otherwise.
        /// </summary>
        public bool IsConnected {
            get {
                return _IsConnected;
            }
        }

        /// <summary>
        /// Return true on connection error, false otherwise.
        /// </summary>
        public bool IsConnectionError {
            get {
                return _IsConnectionError;
            }
        }

        /// <summary>
        /// The address of the connection in the format "ws://hostname-or-ip" or "wss://hostname-or-ip" for SSL/TLS connections
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Not used in WebSockets. Inferred from ws:// or wss:// Address protocol prefix.
        /// </summary>
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
        /// Array of Sec-WebSocket-Protocol items to accept in the server handshake response (default = none)
        /// </summary>
        public IEnumerable<string> Protocols { private get; set; }

        /// <summary>
        /// The compression method to use (default = none)
        /// </summary>
        [CLSCompliant(false)]
        public CompressionMethod Compression { private get; set; } = CompressionMethod.None;

        /// <summary>
        /// Generate an OnMessageReceived event in response to ping messages from the server (default = false)
        /// </summary>
        public bool EmitOnPing { private get; set; }

        /// <summary>
        /// Follow HTTP Location header redirects on connection (default = true)
        /// </summary>
        public bool EnableRedirection { private get; set; } = true;

        /// <summary>
        /// HTTP Origin header to transmit during connection upgrade handshake (default = none)
        /// </summary>
        public string Origin { private get; set; }

        /// <summary>
        /// HTTP cookies to send during connection handshake
        /// </summary>
        [CLSCompliant(false)]
        public IEnumerable<WebSocketSharp.Net.Cookie> Cookies { private get; set; }

        /// <summary>
        /// HTTP Basic Auth username
        /// </summary>
        public string HttpUsername { private get; set; }

        /// <summary>
        /// HTTP Basic auth password
        /// </summary>
        public string HttpPassword { private get; set; }

        /// <summary>
        /// Use HTTP pre-authorization
        /// </summary>
        public bool HttpPreAuth { private get; set; }

        /// <summary>
        /// Proxy URL to connect to
        /// </summary>
        public string ProxyUrl { private get; set; }

        /// <summary>
        /// Proxy username
        /// </summary>
        public string ProxyUsername { private get; set; }

        /// <summary>
        /// Proxy password
        /// </summary>
        public string ProxyPassword { private get; set; }

        /// <summary>
        /// SSL certificate and other configuration to use
        /// </summary>
        [CLSCompliant(false)]
        public ClientSslConfiguration SslConfiguration { private get; set; }

        /// <summary>
        /// Length of time to wait for a ping response before timing out the connection (default = 5 seconds)
        /// </summary>
        public TimeSpan WaitTime { private get; set; } = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Create a new WebSocket instan.ce with the specified host address
        /// </summary>
        /// <param name="address">Optional address to connect to. If not supplied, set with Address before connecting.</param>
        public IrcWebSocketTransport(string address = null)
        {
            Address = address ?? "";
        }

        public void Connect()
        {
            _IsConnected = _IsDisconnecting = _IsConnectionError = false;

            // Configure new WebSocket client
            // Unfortunately we have to do this on every connect
            // because dirty disconnects leave the WebSocket instance in an unusable state

            if (Protocols == null) {
                Socket = new WebSocket(Address);
            } else {
                Socket = new WebSocket(Address, Protocols.ToArray());
            }

            Socket.Compression = Compression;
            Socket.EmitOnPing = EmitOnPing;
            Socket.EnableRedirection = EnableRedirection;
            Socket.Origin = Origin;

            if (Cookies != null) {
                foreach (Cookie cookie in Cookies)
                    Socket.SetCookie(cookie);
            }

            if (HttpUsername != null && HttpUsername.Length > 0) {
                Socket.SetCredentials(HttpUsername, HttpPassword, HttpPreAuth);
            }

            if (ProxyUrl != null && ProxyUrl.Length > 0) {
                Socket.SetProxy(ProxyUrl, ProxyUsername, ProxyPassword);
            }

            Socket.SslConfiguration = SslConfiguration;

            Socket.WaitTime = WaitTime;

            // Hook events
            Socket.OnOpen += Socket_OnOpen;
            Socket.OnMessage += Socket_OnMessage;
            Socket.OnClose += Socket_OnClose;
            Socket.OnError += Socket_OnError;

            // Blocks until connection is established or fails
            _IsConnecting = true;

            // Default timeout on Windows 7's TCP/IP stack seems to be 5 seconds, must fix with threading
            Socket.Connect(); // Might raise OnError, which we don't actually want

            if (!_IsConnecting) {
                throw new CouldNotConnectException("Could not connect to WebSocket: " + Address + ": Error during connection");
            }

            // I don't trust Socket.IsAlive
            if (!Socket.IsAlive) {
                throw new CouldNotConnectException("Could not connect to WebSocket: " + Address + ": Socket dead");
            }

            // Set this here rather than in Socket_OnOpen to make sure no errors occurred during connect/negotiation
            // before we signal that we are connected
            _IsConnected = true;
            _IsConnecting = false;
        }

        public void Disconnect()
        {
            // If there's a connection error, websocket-sharp will have already closed the connection
            // Calling Close() again will cause a hang while the system figures out the socket is closed
            if (!_IsConnectionError) {
                _IsDisconnecting = true;

                // Unhook events
                Socket.OnOpen -= Socket_OnOpen;
                Socket.OnMessage -= Socket_OnMessage;
                Socket.OnClose -= Socket_OnClose;
                Socket.OnError -= Socket_OnError;

                Socket.Close();

                _IsDisconnecting = false;
            }

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
            // Don't set _IsConnected here. Negotiation might fail before the connection finishes being established.
        }

        /// <summary>
        /// Received when the WebSocket connection is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Socket_OnClose(object sender, CloseEventArgs e)
        {
            // Don't set _IsConnected here. We might arrive here via thread lag after we have re-connected.
        }

        /// <summary>
        /// Received when data is received from the WebSocket connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Socket_OnMessage(object sender, MessageEventArgs e)
        {
            // It is possible to receive multiple lines
            string[] lines = e.Data.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (OnMessageReceived != null) {
                foreach (string line in lines)
                    OnMessageReceived(sender, new ReadLineEventArgs(line));
            }
        }

        /// <summary>
        /// Received when a WebSocket connection error occurs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Socket_OnError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            // Error while trying to connect?
            if (_IsConnecting) {
                _IsConnecting = false;
                return;
            }

            // Don't send errors if we are connecting (!_IsConnected) or disconnecting (IsDisconnecting) or already disconnected (!_IsConnected)
            // (the latter can happen when websocket-sharp is a bit lagged about noticing the connection has been closed)
            if (_IsDisconnecting || !_IsConnected)
                return;

            bool _Previous = _IsConnectionError;
            _IsConnectionError = true;

            // Only send a connection error once
            if (OnConnectionError != null && !_Previous) {
                OnConnectionError();
            }
        }
    }
}
