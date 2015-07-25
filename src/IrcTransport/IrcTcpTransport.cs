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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Text;
using System.Threading;

using Starksoft.Net.Proxy;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    /// The default TCP-based IRC protocol with optional proxy and SSL support
    /// </summary>
    public class IrcTcpTransport : IIrcTransportManager
    {
        private TcpClient _TcpClient;
        private int _SocketReceiveTimeout = 600;
        private int _SocketSendTimeout = 600;

        private string _Address;
        private int _Port;

        private string _ProxyHost;
        private int _ProxyPort;
        private ProxyType _ProxyType = ProxyType.None;
        private string _ProxyUsername;
        private string _ProxyPassword;

        private bool _UseSsl;
        private bool _ValidateServerCertificate;
        private X509Certificate _SslClientCertificate;

        private Encoding _Encoding = Encoding.Default;

        private bool _IsConnected;
        private bool _IsConnectionError;

        private StreamReader _Reader;
        private StreamWriter _Writer;

        private ReadThread _ReadThread;

        /// <summary>
        /// Event which fires when a message is received on the connection
        /// </summary>
        public event ReadLineEventHandler OnMessageReceived;

        /// <summary>
        /// Timeout in seconds for receiving data from the socket
        /// Default: 600
        /// </summary>
        public int SocketReceiveTimeout {
            get {
                return _SocketReceiveTimeout;
            }
            set {
                _SocketReceiveTimeout = value;
            }
        }

        /// <summary>
        /// Timeout in seconds for sending data to the socket
        /// Default: 600
        /// </summary>
        public int SocketSendTimeout {
            get {
                return _SocketSendTimeout;
            }
            set {
                _SocketSendTimeout = value;
            }
        }

        public string Address {
            get {
                return _Address;
            }
            set {
                _Address = value;
            }
        }

        public int Port {
            get {
                return _Port;
            }
            set {
                _Port = value;
            }
        }

        /// <summary>
        /// If you want to use a Proxy, set the ProxyHost to Host of the Proxy you want to use.
        /// </summary>
        public string ProxyHost {
            get {
                return _ProxyHost;
            }
            set {
                _ProxyHost = value;
            }
        }

        /// <summary>
        /// If you want to use a Proxy, set the ProxyPort to Port of the Proxy you want to use.
        /// </summary>
        public int ProxyPort {
            get {
                return _ProxyPort;
            }
            set {
                _ProxyPort = value;
            }
        }

        /// <summary>
        /// Standard Setting is to use no Proxy Server, if you Set this to any other value,
        /// you have to set the ProxyHost and ProxyPort aswell (and give credentials if needed)
        /// Default: ProxyType.None
        /// </summary>
        public ProxyType ProxyType {
            get {
                return _ProxyType;
            }
            set {
                _ProxyType = value;
            }
        }

        /// <summary>
        /// Username to your Proxy Server
        /// </summary>
        public string ProxyUsername {
            get {
                return _ProxyUsername;
            }
            set {
                _ProxyUsername = value;
            }
        }

        /// <summary>
        /// Password to your Proxy Server
        /// </summary>
        public string ProxyPassword {
            get {
                return _ProxyPassword;
            }
            set {
                _ProxyPassword = value;
            }
        }

        /// <summary>
        /// Enables/disables using SSL for the connection
        /// Default: false
        /// </summary>
        public bool UseSsl {
            get {
                return _UseSsl;
            }
            set {
                _UseSsl = value;
            }
        }

        /// <summary>
        /// Specifies if the certificate of the server is validated
        /// Default: true
        /// </summary>
        public bool ValidateServerCertificate {
            get {
                return _ValidateServerCertificate;
            }
            set {
                _ValidateServerCertificate = value;
            }
        }

        /// <summary>
        /// Specifies the client certificate used for the SSL connection
        /// Default: null
        /// </summary>
        public X509Certificate SslClientCertificate {
            get {
                return _SslClientCertificate;
            }
            set {
                _SslClientCertificate = value;
            }
        }

        /// <summary>
        /// The encoding to use to write to and read from the socket.
        ///
        /// If EnableUTF8Recode is true, reading and writing will always happen
        /// using UTF-8; this encoding is only used to decode incoming messages
        /// that cannot be successfully decoded using UTF-8.
        ///
        /// Default: encoding of the system
        /// </summary>
        public Encoding Encoding {
            get {
                return _Encoding;
            }
            set {
                _Encoding = value;
            }
        }

        /// <summary>
        /// Enable UTF8 re-encoding
        /// </summary>
        public bool EnableUTF8Recode { get; set; }

        /// <summary>
        /// When a connection error is detected this property will return true
        /// </summary>
        public bool IsConnectionError {
            get {
                lock (this) {
                    return _IsConnectionError;
                }
            }
        }

        private void _SetConnectionError(bool value)
        {
            lock (this) {
                _IsConnectionError = value;
            }
            if (value) {
                // signal ReadLine() to check IsConnectionError state - probably don't need this anymore
                //_ReadThread.QueuedEvent.Set();
            }
        }

        /// <summary>
        /// On successful connect to the IRC server, this is set to true.
        /// </summary>
        public bool IsConnected {
            get {
                return _IsConnected;
            }
        }

        /// <summary>
        /// Create a new TCP transport
        /// </summary>
        public IrcTcpTransport()
        {
            _ReadThread = new ReadThread(this);
        }

        /// <summary>
        /// Create a new TCP transport with the specified address and port
        /// </summary>
        public IrcTcpTransport(string address, int port) : this()
        {
            _Address = address;
            _Port = port;
        }

        /// <summary>
        /// Connect to the IRC server via TCP, optionally using SSL and/or a proxy
        /// </summary>
        /// <param name="address">Address of the IRC server</param>
        /// <param name="port">Port of the IRC server</param>
        /// <exception cref="AuthenticationException">Throws if SSL authentication fails</exception>
        /// <exception cref="CouldNotConnectException">Throws if the TCP connection failed</exception>
        public void Connect()
        {
            try
            {
                _TcpClient = new TcpClient();
                _TcpClient.NoDelay = true;
                _TcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);
                // set timeout, after this the connection will be aborted
                _TcpClient.ReceiveTimeout = _SocketReceiveTimeout * 1000;
                _TcpClient.SendTimeout = _SocketSendTimeout * 1000;

                if (_ProxyType != ProxyType.None)
                {
                    IProxyClient proxyClient = null;
                    ProxyClientFactory proxyFactory = new ProxyClientFactory();
                    // HACK: map our ProxyType to Starksoft's ProxyType
                    Starksoft.Net.Proxy.ProxyType proxyType =
                        (Starksoft.Net.Proxy.ProxyType)Enum.Parse(
                            typeof(ProxyType), _ProxyType.ToString(), true
                        );

                    if (_ProxyUsername == null && _ProxyPassword == null)
                    {
                        proxyClient = proxyFactory.CreateProxyClient(
                            proxyType
                        );
                    }
                    else
                    {
                        proxyClient = proxyFactory.CreateProxyClient(
                            proxyType,
                            _ProxyHost,
                            _ProxyPort,
                            _ProxyUsername,
                            _ProxyPassword
                        );
                    }

                    _TcpClient.Connect(_ProxyHost, _ProxyPort);
                    proxyClient.TcpClient = _TcpClient;
                    proxyClient.CreateConnection(_Address, _Port);
                } else {
                    _TcpClient.Connect(_Address, _Port);
                }

                Stream stream = _TcpClient.GetStream();
                if (_UseSsl)
                {
                    RemoteCertificateValidationCallback certValidation;
                    if (_ValidateServerCertificate) {
                        certValidation = ServicePointManager.ServerCertificateValidationCallback;
                        if (certValidation == null) {
                            certValidation = delegate (object sender,
                                X509Certificate certificate,
                                X509Chain chain,
                                SslPolicyErrors sslPolicyErrors) {
                                    if (sslPolicyErrors == SslPolicyErrors.None) {
                                        return true;
                                    }

#if LOG4NET
                                    Logger.Connection.Error(
                                            "Connect(): Certificate error: " +
                                            sslPolicyErrors
                                        );
#endif
                                    return false;
                                };
                        }
                    } else {
                        certValidation = delegate { return true; };
                    }
                    RemoteCertificateValidationCallback certValidationWithIrcAsSender =
                        delegate (object sender, X509Certificate certificate,
                                    X509Chain chain, SslPolicyErrors sslPolicyErrors)
                        {
                            return certValidation(this, certificate, chain, sslPolicyErrors);
                        };
                    SslStream sslStream = new SslStream(stream, false,
                                                        certValidationWithIrcAsSender);
                    try {
                        if (_SslClientCertificate != null)
                        {
                            var certs = new X509Certificate2Collection();
                            certs.Add(_SslClientCertificate);
                            sslStream.AuthenticateAsClient(_Address, certs,
                                                            SslProtocols.Default,
                                                            false);
                        } else {
                            sslStream.AuthenticateAsClient(_Address);
                        }
                    }
                    catch (IOException)
                    {
#if LOG4NET
                        Logger.Connection.Error(
                            "Connect(): AuthenticateAsClient() failed!"
                        );
#endif
                        throw;
                    }
                    stream = sslStream;
                }
                if (EnableUTF8Recode) {
                    _Reader = new StreamReader(stream, new PrimaryOrFallbackEncoding(new UTF8Encoding(false, true), _Encoding));
                    _Writer = new StreamWriter(stream, new UTF8Encoding(false, false));
                } else {
                    _Reader = new StreamReader(stream, _Encoding);
                    _Writer = new StreamWriter(stream, _Encoding);

                    if (_Encoding.GetPreamble().Length > 0) {
                        // HACK: we have an encoding that has some kind of preamble
                        // like UTF-8 has a BOM, this will confuse the IRCd!
                        // Thus we send a \r\n so the IRCd can safely ignore that
                        // garbage.
                        _Writer.WriteLine();
                        // make sure we flush the BOM+CRLF correctly
                        _Writer.Flush();
                    }
                }
            } catch (Exception ex) {

                if (_Reader != null) {
                    try {
                        _Reader.Close();
                    } catch (ObjectDisposedException) {
                    }
                }
                if (_Writer != null) {
                    try {
                        _Writer.Close();
                    } catch (ObjectDisposedException) {
                    }
                }
                if (_TcpClient != null) {
                    _TcpClient.Close();
                }

                _IsConnected = false;
                _SetConnectionError(true);

                throw new CouldNotConnectException("Could not connect to: " + _Address + ":" + _Port + ": " + ex.Message, ex);
            }

            // updating the connection error state, so connecting is possible again
            _SetConnectionError(false);
            _IsConnected = true;

            _ReadThread.Start();
        }

        public void Disconnect()
        {
            _ReadThread.Stop();

            try {
                if (_Writer != null)
                    _Writer.Close();
            } catch (ObjectDisposedException) {
            }

            if (_TcpClient != null)
                _TcpClient.Close();

            // This is important so IrcConnection doesn't call OnConnectionError() on a clean disconnect
            _SetConnectionError(false);

            _IsConnected = false;
        }

        public bool WriteLine(string data)
        {
            if (IsConnected && !IsConnectionError) {
                try {
                    lock (_Writer) {
                        _Writer.Write(data + "\r\n");
                        _Writer.Flush();
                    }

                } catch (IOException) {
#if LOG4NET
                    Logger.Socket.Warn("sending data failed, connection lost");
#endif
                    _SetConnectionError(true);
                    return false;

                } catch (ObjectDisposedException) {
#if LOG4NET
                    Logger.Socket.Warn("sending data failed (stream error), connection lost");
#endif
                    _SetConnectionError(true);
                    return false;
                }

#if LOG4NET
                Logger.Socket.Debug("sent: \"" + data + "\"");
#endif
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        private class ReadThread
        {
#if LOG4NET
            private static readonly log4net.ILog _Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#endif
            private IrcTcpTransport _Connection;
            private Thread _Thread;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="connection"></param>
            public ReadThread(IrcTcpTransport connection)
            {
                _Connection = connection;
            }

            /// <summary>
            /// 
            /// </summary>
            public void Start()
            {
                _Thread = new Thread(new ThreadStart(_Worker));
                _Thread.Name = "ReadThread (" + _Connection._Address + ":" + _Connection._Port + ")";
                _Thread.IsBackground = true;
                _Thread.Start();
            }

            /// <summary>
            /// 
            /// </summary>
            public void Stop()
            {
#if LOG4NET
                _Logger.Debug("Stop(): closing reader...");
#endif
                try {
                    _Connection._Reader.Close();
                } catch (ObjectDisposedException) {
                }
#if LOG4NET
                _Logger.Debug("Stop(): joining thread...");
#endif
                _Thread.Join();
            }

            private void _Worker()
            {
#if LOG4NET
                Logger.Socket.Debug("ReadThread started");
#endif
                try {
                    string data = "";
                    try {
                        while (_Connection.IsConnected &&
                               ((data = _Connection._Reader.ReadLine()) != null)) {
#if LOG4NET
                            Logger.Socket.Debug("received: \"" + data + "\"");
#endif
                            if (_Connection.OnMessageReceived != null)
                                _Connection.OnMessageReceived(_Connection, new ReadLineEventArgs(data));
                        }
                    } catch (IOException e) {
#if LOG4NET
                        Logger.Socket.Warn("IOException: " + e.Message);
#endif
                    } finally {
#if LOG4NET
                        Logger.Socket.Warn("connection lost");
#endif
                        _Connection._SetConnectionError(true);
                    }
                } catch (Exception ex) {
#if LOG4NET
                    Logger.Socket.Error(ex);
#endif
                }
            }
        }
    }
}
