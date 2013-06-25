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
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Security.Authentication;
using System.Threading;
using Starksoft.Net.Proxy;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    /// 
    /// </summary>
    /// <threadsafety static="true" instance="true" />
    public class IrcConnection
    {
        private string           _VersionNumber;
        private string           _VersionString;
        private string[]         _AddressList = {"localhost"};
        private int              _CurrentAddress;
        private int              _Port;
        private bool             _UseSsl;
        private bool             _ValidateServerCertificate;
        private X509Certificate  _SslClientCertificate;
        private StreamReader     _Reader;
        private StreamWriter     _Writer;
        private ReadThread       _ReadThread;
        private WriteThread      _WriteThread;
        private IdleWorkerThread _IdleWorkerThread;
        private TcpClient        _TcpClient;
        private Hashtable        _SendBuffer = Hashtable.Synchronized(new Hashtable());
        private int              _SendDelay = 200;
        private bool             _IsRegistered;
        private bool             _IsConnected;
        private bool             _IsConnectionError;
        private bool             _IsDisconnecting;
        private int              _AutoRetryAttempt;
        private bool             _AutoRetry;
        private int              _AutoRetryDelay = 30;
        private int              _AutoRetryLimit = 3;
        private bool             _AutoReconnect;
        private Encoding         _Encoding = Encoding.Default;
        public bool EnableUTF8Recode { get; set; }
        private int              _SocketReceiveTimeout  = 600;
        private int              _SocketSendTimeout = 600;
        private int              _IdleWorkerInterval = 60;
        private int              _PingInterval = 60;
        private int              _PingTimeout = 300;
        private DateTime         _LastPingSent;
        private DateTime         _LastPongReceived;
        private TimeSpan         _Lag;
        private string           _ProxyHost;
        private int              _ProxyPort;
        private ProxyType        _ProxyType = ProxyType.None;
        private string           _ProxyUsername;
        private string           _ProxyPassword;
        
        /// <event cref="OnReadLine">
        /// Raised when a \r\n terminated line is read from the socket
        /// </event>
        public event ReadLineEventHandler   OnReadLine;
        /// <event cref="OnWriteLine">
        /// Raised when a \r\n terminated line is written to the socket
        /// </event>
        public event WriteLineEventHandler  OnWriteLine;
        /// <event cref="OnConnect">
        /// Raised before the connect attempt
        /// </event>
        public event EventHandler           OnConnecting;
        /// <event cref="OnConnect">
        /// Raised on successful connect
        /// </event>
        public event EventHandler           OnConnected;
        /// <event cref="OnConnect">
        /// Raised before the connection is closed
        /// </event>
        public event EventHandler           OnDisconnecting;
        /// <event cref="OnConnect">
        /// Raised when the connection is closed
        /// </event>
        public event EventHandler           OnDisconnected;
        /// <event cref="OnConnectionError">
        /// Raised when the connection got into an error state
        /// </event>
        public event EventHandler           OnConnectionError;
        /// <event cref="AutoConnectErrorEventHandler">
        /// Raised when the connection got into an error state during auto connect loop
        /// </event>
        public event AutoConnectErrorEventHandler   OnAutoConnectError;
        
        /// <summary>
        /// When a connection error is detected this property will return true
        /// </summary>
        protected bool IsConnectionError {
            get {
                lock (this) {
                    return _IsConnectionError;
                }
            }
            set {
                lock (this) {
                    _IsConnectionError = value;
                }
                if (value) {
                    // signal ReadLine() to check IsConnectionError state
                    _ReadThread.QueuedEvent.Set();
                }
            }
        }

        protected bool IsDisconnecting {
            get {
                lock (this) {
                    return _IsDisconnecting;
                }
            }
            set {
                lock (this) {
                    _IsDisconnecting = value;
                }
            }
        }
        
        /// <summary>
        /// Gets the current address of the connection
        /// </summary>
        public string Address {
            get {
                return _AddressList[_CurrentAddress];
            }
        }

        /// <summary>
        /// Gets the address list of the connection
        /// </summary>
        public string[] AddressList {
            get {
                return _AddressList;
            }
        }

        /// <summary>
        /// Gets the used port of the connection
        /// </summary>
        public int Port {
            get {
                return _Port;
            }
        }

        /// <summary>
        /// By default nothing is done when the library looses the connection
        /// to the server.
        /// Default: false
        /// </summary>
        /// <value>
        /// true, if the library should reconnect on lost connections
        /// false, if the library should not take care of it
        /// </value>
        public bool AutoReconnect {
            get {
                return _AutoReconnect;
            }
            set {
#if LOG4NET
                if (value) {
                    Logger.Connection.Info("AutoReconnect enabled");
                } else {
                    Logger.Connection.Info("AutoReconnect disabled");
                }
#endif
                _AutoReconnect = value;
            }
        }

        /// <summary>
        /// If the library should retry to connect when the connection fails.
        /// Default: false
        /// </summary>
        /// <value>
        /// true, if the library should retry to connect
        /// false, if the library should not retry
        /// </value>
        public bool AutoRetry {
            get {
                return _AutoRetry;
            }
            set {
#if LOG4NET
                if (value) {
                    Logger.Connection.Info("AutoRetry enabled");
                } else {
                    Logger.Connection.Info("AutoRetry disabled");
                }
#endif
                _AutoRetry = value;
            }
        }

        /// <summary>
        /// Delay between retry attempts in Connect() in seconds.
        /// Default: 30
        /// </summary>
        public int AutoRetryDelay {
            get {
                return _AutoRetryDelay;
            }
            set {
                _AutoRetryDelay = value;
            }
        }

        /// <summary>
        /// Maximum number of retries to connect to the server
        /// Default: 3
        /// </summary>
        public int AutoRetryLimit {
            get {
                return _AutoRetryLimit;
            }
            set {
                _AutoRetryLimit = value;
            }
        }

        /// <summary>
        /// Returns the current amount of reconnect attempts
        /// Default: 3
        /// </summary>
        public int AutoRetryAttempt {
            get {
                return _AutoRetryAttempt;
            }
        }

        /// <summary>
        /// To prevent flooding the IRC server, it's required to delay each
        /// message, given in milliseconds.
        /// Default: 200
        /// </summary>
        public int SendDelay {
            get {
                return _SendDelay;
            }
            set {
                _SendDelay = value;
            }
        }

        /// <summary>
        /// On successful registration on the IRC network, this is set to true.
        /// </summary>
        public bool IsRegistered {
            get {
                return _IsRegistered;
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
        /// Gets the SmartIrc4net version number
        /// </summary>
        public string VersionNumber {
            get {
                return _VersionNumber;
            }
        }

        /// <summary>
        /// Gets the full SmartIrc4net version string
        /// </summary>
        public string VersionString {
            get {
                return _VersionString;
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
        
        /// <summary>
        /// Interval in seconds to run the idle worker
        /// Default: 60
        /// </summary>
        public int IdleWorkerInterval {
            get {
                return _IdleWorkerInterval;
            }
            set {
                _IdleWorkerInterval = value;
            }
        }

        /// <summary>
        /// Interval in seconds to send a PING
        /// Default: 60
        /// </summary>
        public int PingInterval {
            get {
                return _PingInterval;
            }
            set {
                _PingInterval = value;
            }
        }
        
        /// <summary>
        /// Timeout in seconds for server response to a PING
        /// Default: 600
        /// </summary>
        public int PingTimeout {
            get {
                return _PingTimeout;
            }
            set {
                _PingTimeout = value;
            }
        }

        /// <summary>
        /// Latency between client and the server
        /// </summary>
        public TimeSpan Lag {
            get {
                if (_LastPingSent > _LastPongReceived) {
                    // there is an outstanding ping, thus we don't have a current lag value
                    return DateTime.Now - _LastPingSent;
                }
                
                return _Lag;
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
        /// Initializes the message queues, read and write thread
        /// </summary>
        public IrcConnection()
        {
#if LOG4NET
            Logger.Init();
            Logger.Main.Debug("IrcConnection created");
#endif
            _SendBuffer[Priority.High]        = Queue.Synchronized(new Queue());
            _SendBuffer[Priority.AboveMedium] = Queue.Synchronized(new Queue());
            _SendBuffer[Priority.Medium]      = Queue.Synchronized(new Queue());
            _SendBuffer[Priority.BelowMedium] = Queue.Synchronized(new Queue());
            _SendBuffer[Priority.Low]         = Queue.Synchronized(new Queue());

            // setup own callbacks
            OnReadLine        += new ReadLineEventHandler(_SimpleParser);
            OnConnectionError += new EventHandler(_OnConnectionError);

            _ReadThread  = new ReadThread(this);
            _WriteThread = new WriteThread(this);
            _IdleWorkerThread = new IdleWorkerThread(this);

            Assembly assm = Assembly.GetAssembly(this.GetType());
            AssemblyName assm_name = assm.GetName(false);

            AssemblyProductAttribute pr = (AssemblyProductAttribute)assm.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0];

            _VersionNumber = assm_name.Version.ToString();
            _VersionString = pr.Product+" "+_VersionNumber;
        }
        
#if LOG4NET
        ~IrcConnection()
        {
            Logger.Main.Debug("IrcConnection destroyed");
        }
#endif
        
        /// <overloads>this method has 2 overloads</overloads>
        /// <summary>
        /// Connects to the specified server and port, when the connection fails
        /// the next server in the list will be used.
        /// </summary>
        /// <param name="addresslist">List of servers to connect to</param>
        /// <param name="port">Portnumber to connect to</param>
        /// <exception cref="CouldNotConnectException">The connection failed</exception>
        /// <exception cref="AlreadyConnectedException">If there is already an active connection</exception>
        public void Connect(string[] addresslist, int port)
        {
            if (_IsConnected) {
                throw new AlreadyConnectedException("Already connected to: " + Address + ":" + Port);
            }

            _AutoRetryAttempt++;
#if LOG4NET
            Logger.Connection.Info(String.Format("connecting... (attempt: {0})",
                                                 _AutoRetryAttempt));
#endif

            _AddressList = (string[])addresslist.Clone();
            _Port = port;

            if (OnConnecting != null) {
                OnConnecting(this, EventArgs.Empty);
            }
            try {
                _TcpClient = new TcpClient();
                _TcpClient.NoDelay = true;
                _TcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);
                // set timeout, after this the connection will be aborted
                _TcpClient.ReceiveTimeout = _SocketReceiveTimeout * 1000;
                _TcpClient.SendTimeout = _SocketSendTimeout * 1000;
                
                if (_ProxyType != ProxyType.None) {
                    IProxyClient proxyClient = null;
                    ProxyClientFactory proxyFactory = new ProxyClientFactory();
                    // HACK: map our ProxyType to Starksoft's ProxyType
                    Starksoft.Net.Proxy.ProxyType proxyType = 
                        (Starksoft.Net.Proxy.ProxyType) Enum.Parse(
                            typeof(ProxyType), _ProxyType.ToString(), true
                        );
                    
                    if (_ProxyUsername == null && _ProxyPassword == null) {
                        proxyClient = proxyFactory.CreateProxyClient(
                            proxyType
                        );
                    } else {
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
                    proxyClient.CreateConnection(Address, port);
                } else {
                    _TcpClient.Connect(Address, port);
                }
                
                Stream stream = _TcpClient.GetStream();
                if (_UseSsl) {
                    RemoteCertificateValidationCallback certValidation;
                    if (_ValidateServerCertificate) {
                        certValidation = delegate(object sender,
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
                    } else {
                        certValidation = delegate { return true; };
                    }
                    SslStream sslStream = new SslStream(stream, false,
                                                        certValidation);
                    try {
                        if (_SslClientCertificate != null) {
                            var certs = new X509Certificate2Collection();
                            certs.Add(_SslClientCertificate);
                            sslStream.AuthenticateAsClient(Address, certs,
                                                           SslProtocols.Default,
                                                           false);
                        } else {
                            sslStream.AuthenticateAsClient(Address);
                        }
                    } catch (IOException ex) {
#if LOG4NET
                        Logger.Connection.Error(
                            "Connect(): AuthenticateAsClient() failed!"
                        );
#endif
                        throw new CouldNotConnectException("Could not connect to: " + Address + ":" + Port + " " + ex.Message, ex);
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

                // Connection was succeful, reseting the connect counter
                _AutoRetryAttempt = 0;

                // updating the connection error state, so connecting is possible again
                IsConnectionError = false;
                _IsConnected = true;

                // lets power up our threads
                _ReadThread.Start();
                _WriteThread.Start();
                _IdleWorkerThread.Start();
                
#if LOG4NET
                Logger.Connection.Info("connected");
#endif
                if (OnConnected != null) {
                    OnConnected(this, EventArgs.Empty);
                }
            } catch (AuthenticationException ex) {
#if LOG4NET
                Logger.Connection.Error("Connect(): Exception", ex);
#endif
                throw new CouldNotConnectException("Could not connect to: " + Address + ":" + Port + " " + ex.Message, ex);
            } catch (Exception e) {
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
                IsConnectionError = true;
                
#if LOG4NET
                Logger.Connection.Info("connection failed: "+e.Message, e);
#endif
                if (e is CouldNotConnectException) {
                    // error was fatal, bail out
                    throw;
                }

                if (_AutoRetry &&
                    (_AutoRetryLimit == -1 ||
                     _AutoRetryLimit == 0 ||
                     _AutoRetryLimit <= _AutoRetryAttempt)) {
                    if (OnAutoConnectError != null) {
                        OnAutoConnectError(this, new AutoConnectErrorEventArgs(Address, Port, e));
                    }
#if LOG4NET
                    Logger.Connection.Debug("delaying new connect attempt for "+_AutoRetryDelay+" sec");
#endif
                    Thread.Sleep(_AutoRetryDelay * 1000);
                    _NextAddress();
                    // FIXME: this is recursion
                    Connect(_AddressList, _Port);
                } else {
                    throw new CouldNotConnectException("Could not connect to: "+Address+":"+Port+" "+e.Message, e);
                }
            }
        }

        /// <summary>
        /// Connects to the specified server and port.
        /// </summary>
        /// <param name="address">Server address to connect to</param>
        /// <param name="port">Port number to connect to</param>
        public void Connect(string address, int port)
        {
            Connect(new string[] { address }, port);
        }

        /// <summary>
        /// Reconnects to the server
        /// </summary>
        /// <exception cref="NotConnectedException">
        /// If there was no active connection
        /// </exception>
        /// <exception cref="CouldNotConnectException">
        /// The connection failed
        /// </exception>
        /// <exception cref="AlreadyConnectedException">
        /// If there is already an active connection
        /// </exception>
        public void Reconnect()
        {
#if LOG4NET
            Logger.Connection.Info("reconnecting...");
#endif
            Disconnect();
            Connect(_AddressList, _Port);
        }
        
        /// <summary>
        /// Disconnects from the server
        /// </summary>
        /// <exception cref="NotConnectedException">
        /// If there was no active connection
        /// </exception>
        public void Disconnect()
        {
            if (!IsConnected) {
                throw new NotConnectedException("The connection could not be disconnected because there is no active connection");
            }
            
#if LOG4NET
            Logger.Connection.Info("disconnecting...");
#endif
            if (OnDisconnecting != null) {
                OnDisconnecting(this, EventArgs.Empty);
            }
            
            IsDisconnecting = true;
            
            _ReadThread.Stop();
            _WriteThread.Stop();
            _TcpClient.Close();
            _IsConnected = false;
            _IsRegistered = false;
            
            IsDisconnecting = false;
            
            if (OnDisconnected != null) {
                OnDisconnected(this, EventArgs.Empty);
            }

#if LOG4NET
            Logger.Connection.Info("disconnected");
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blocking"></param>
        public void Listen(bool blocking)
        {
            if (blocking) {
                while (IsConnected) {
                    ReadLine(true);
                }
            } else {
                while (ReadLine(false).Length > 0) {
                    // loop as long as we receive messages
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Listen()
        {
            Listen(true);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="blocking"></param>
        public void ListenOnce(bool blocking)
        {
            ReadLine(blocking);
        }

        /// <summary>
        /// 
        /// </summary>
        public void ListenOnce()
        {
            ListenOnce(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="blocking"></param>
        /// <returns></returns>
        public string ReadLine(bool blocking)
        {
            string data = "";
            if (blocking) {
                // block till the queue has data, but bail out on connection error
                while (IsConnected &&
                       !IsConnectionError &&
                       _ReadThread.Queue.Count == 0) {
                    _ReadThread.QueuedEvent.WaitOne();
                }
            }

            if (IsConnected &&
                _ReadThread.Queue.Count > 0) {
                data = (string)(_ReadThread.Queue.Dequeue());
            }

            if (data != null && data.Length > 0) {
#if LOG4NET
                Logger.Queue.Debug("read: \""+data+"\"");
#endif
                if (OnReadLine != null) {
                    OnReadLine(this, new ReadLineEventArgs(data));
                }
            }

            if (IsConnectionError &&
                !IsDisconnecting &&
                OnConnectionError != null) {
                OnConnectionError(this, EventArgs.Empty);
            }
            
            return data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="priority"></param>
        public void WriteLine(string data, Priority priority)
        {
            if (priority == Priority.Critical) {
                if (!IsConnected) {
                    throw new NotConnectedException();
                }
                
                _WriteLine(data);
            } else {
                ((Queue)_SendBuffer[priority]).Enqueue(data);
                _WriteThread.QueuedEvent.Set();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void WriteLine(string data)
        {
            WriteLine(data, Priority.Medium);
        }

        private bool _WriteLine(string data)
        {
            if (IsConnected) {
                try {
                    lock (_Writer) {
                        _Writer.Write(data + "\r\n");
                        _Writer.Flush();
                    }
                } catch (IOException) {
#if LOG4NET
                    Logger.Socket.Warn("sending data failed, connection lost");
#endif
                    IsConnectionError = true;
                    return false;
                } catch (ObjectDisposedException) {
#if LOG4NET
                    Logger.Socket.Warn("sending data failed (stream error), connection lost");
#endif
                    IsConnectionError = true;
                    return false;
                }

#if LOG4NET
                Logger.Socket.Debug("sent: \""+data+"\"");
#endif
                if (OnWriteLine != null) {
                    OnWriteLine(this, new WriteLineEventArgs(data));
                }
                return true;
            }

            return false;
        }

        private void _NextAddress()
        {
            _CurrentAddress++;
            if (_CurrentAddress >= _AddressList.Length) {
                _CurrentAddress = 0;
            }
#if LOG4NET
            Logger.Connection.Info("set server to: "+Address);
#endif
        }

        private void _SimpleParser(object sender, ReadLineEventArgs args)
        {
            string   rawline = args.Line;
            string[] rawlineex = rawline.Split(new char[] {' '});
            string   line = null;
            string   prefix = null;
            string   command = null;

            if (rawline[0] == ':') {
                prefix = rawlineex[0].Substring(1);
                line = rawline.Substring(prefix.Length + 2);
            } else {
                line = rawline;
            }
            string[] lineex = line.Split(new char[] {' '});

            command = lineex[0];
            ReplyCode replycode = ReplyCode.Null;
            int intReplycode;
            if (Int32.TryParse(command, out intReplycode)) {
                replycode = (ReplyCode) intReplycode;
            }
            if (replycode != ReplyCode.Null) {
                switch (replycode) {
                    case ReplyCode.Welcome:
                        _IsRegistered = true;
#if LOG4NET
                        Logger.Connection.Info("logged in");
#endif
                        break;
                }
            } else {
                switch (command) {
                    case "ERROR":
                        // FIXME: handle server errors differently than connection errors!
                        //IsConnectionError = true;
                        break;
                    case "PONG":
                        DateTime now = DateTime.Now;
                        _LastPongReceived = now;
                        _Lag = now - _LastPingSent;

#if LOG4NET
                        Logger.Connection.Debug("PONG received, took: " + _Lag.TotalMilliseconds + " ms");
#endif
                        break;
                }
            }
        }

        private void _OnConnectionError(object sender, EventArgs e)
        {
            try {
                if (AutoReconnect) {
                    // prevent connect -> exception -> connect flood loop
                    Thread.Sleep(AutoRetryDelay * 1000);
                    // lets try to recover the connection
                    Reconnect();
                } else {
                    // make sure we clean up
                    Disconnect();
                }
            } catch (ConnectionException) {
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        private class ReadThread
        {
#if LOG4NET
            private static readonly log4net.ILog _Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#endif
            private IrcConnection  _Connection;
            private Thread         _Thread;
            private Queue          _Queue = Queue.Synchronized(new Queue());

            public AutoResetEvent  QueuedEvent;

            public Queue Queue {
                get {
                    return _Queue;
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="connection"></param>
            public ReadThread(IrcConnection connection)
            {
                _Connection = connection;
                QueuedEvent = new AutoResetEvent(false);
            }

            /// <summary>
            /// 
            /// </summary>
            public void Start()
            {
                _Thread = new Thread(new ThreadStart(_Worker));
                _Thread.Name = "ReadThread ("+_Connection.Address+":"+_Connection.Port+")";
                _Thread.IsBackground = true;
                _Thread.Start();
            }

            /// <summary>
            /// 
            /// </summary>
            public void Stop()
            {
#if LOG4NET
                _Logger.Debug("Stop()");
#endif
                
#if LOG4NET
                _Logger.Debug("Stop(): aborting thread...");
#endif
                _Thread.Abort();
                // make sure we close the stream after the thread is gone, else
                // the thread will think the connection is broken!
#if LOG4NET
                _Logger.Debug("Stop(): joining thread...");
#endif
                _Thread.Join();
                
#if LOG4NET
                _Logger.Debug("Stop(): closing reader...");
#endif
                try {
                    _Connection._Reader.Close();
                } catch (ObjectDisposedException) {
                }
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
                            _Queue.Enqueue(data);
                            QueuedEvent.Set();
#if LOG4NET
                            Logger.Socket.Debug("received: \""+data+"\"");
#endif
                        }
                    } catch (IOException e) {
#if LOG4NET
                        Logger.Socket.Warn("IOException: "+e.Message);
#endif
                    } finally {
#if LOG4NET
                        Logger.Socket.Warn("connection lost");
#endif
                        // only flag this as connection error if we are not
                        // cleanly disconnecting
                        if (!_Connection.IsDisconnecting) {
                            _Connection.IsConnectionError = true;
                        }
                    }
                } catch (ThreadAbortException) {
                    Thread.ResetAbort();
#if LOG4NET
                    Logger.Socket.Debug("ReadThread aborted");
#endif
                } catch (Exception ex) {
#if LOG4NET
                    Logger.Socket.Error(ex);
#endif
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class WriteThread
        {
            private IrcConnection  _Connection;
            private Thread         _Thread;
            private int            _HighCount;
            private int            _AboveMediumCount;
            private int            _MediumCount;
            private int            _BelowMediumCount;
            private int            _LowCount;
            private int            _AboveMediumSentCount;
            private int            _MediumSentCount;
            private int            _BelowMediumSentCount;
            private int            _AboveMediumThresholdCount = 4;
            private int            _MediumThresholdCount      = 2;
            private int            _BelowMediumThresholdCount = 1;
            private int            _BurstCount;

            public AutoResetEvent QueuedEvent;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="connection"></param>
            public WriteThread(IrcConnection connection)
            {
                _Connection = connection;
                QueuedEvent = new AutoResetEvent(false);
            }

            /// <summary>
            /// 
            /// </summary>
            public void Start()
            {
                _Thread = new Thread(new ThreadStart(_Worker));
                _Thread.Name = "WriteThread ("+_Connection.Address+":"+_Connection.Port+")";
                _Thread.IsBackground = true;
                _Thread.Start();
            }

            /// <summary>
            /// 
            /// </summary>
            public void Stop()
            {
#if LOG4NET
                Logger.Connection.Debug("Stopping WriteThread...");
#endif
                
                _Thread.Abort();
                // make sure we close the stream after the thread is gone, else
                // the thread will think the connection is broken!
                _Thread.Join();
                
                try {
                    _Connection._Writer.Close();
                } catch (ObjectDisposedException) {
                }
            }

            private void _Worker()
            {
#if LOG4NET
                Logger.Socket.Debug("WriteThread started");
#endif
                try {
                    try {
                        while (_Connection.IsConnected) {
                            QueuedEvent.WaitOne();
                            var isBufferEmpty = false;
                            do {
                                isBufferEmpty = _CheckBuffer() == 0;
                                Thread.Sleep(_Connection._SendDelay);
                            } while (!isBufferEmpty);
                        }
                    } catch (IOException e) {
#if LOG4NET
                        Logger.Socket.Warn("IOException: " + e.Message);
#endif
                    } finally {
#if LOG4NET
                        Logger.Socket.Warn("connection lost");
#endif
                        // only flag this as connection error if we are not
                        // cleanly disconnecting
                        if (!_Connection.IsDisconnecting) {
                            _Connection.IsConnectionError = true;
                        }
                    }
                } catch (ThreadAbortException) {
                    Thread.ResetAbort();
#if LOG4NET
                    Logger.Socket.Debug("WriteThread aborted");
#endif
                } catch (Exception ex) {
#if LOG4NET
                    Logger.Socket.Error(ex);
#endif
                }
            }

#region WARNING: complex scheduler, don't even think about changing it!
            // WARNING: complex scheduler, don't even think about changing it!
            private int _CheckBuffer()
            {
                _HighCount        = ((Queue)_Connection._SendBuffer[Priority.High]).Count;
                _AboveMediumCount = ((Queue)_Connection._SendBuffer[Priority.AboveMedium]).Count;
                _MediumCount      = ((Queue)_Connection._SendBuffer[Priority.Medium]).Count;
                _BelowMediumCount = ((Queue)_Connection._SendBuffer[Priority.BelowMedium]).Count;
                _LowCount         = ((Queue)_Connection._SendBuffer[Priority.Low]).Count;

                var msgCount = _HighCount +
                               _AboveMediumCount +
                               _MediumCount +
                               _BelowMediumCount +
                               _LowCount;

                // only send data if we are succefully registered on the IRC network
                if (!_Connection._IsRegistered) {
                    return msgCount;
                }

                if (_CheckHighBuffer() &&
                    _CheckAboveMediumBuffer() &&
                    _CheckMediumBuffer() &&
                    _CheckBelowMediumBuffer() &&
                    _CheckLowBuffer()) {
                    // everything is sent, resetting all counters
                    _AboveMediumSentCount = 0;
                    _MediumSentCount      = 0;
                    _BelowMediumSentCount = 0;
                    _BurstCount = 0;
                }

                if (_BurstCount < 3) {
                    _BurstCount++;
                    //_CheckBuffer();
                }

                return msgCount;
            }

            private bool _CheckHighBuffer()
            {
                if (_HighCount > 0) {
                    string data = (string)((Queue)_Connection._SendBuffer[Priority.High]).Dequeue();
                    if (_Connection._WriteLine(data) == false) {
#if LOG4NET
                        Logger.Queue.Warn("Sending data was not sucessful, data is requeued!");
#endif
                        ((Queue)_Connection._SendBuffer[Priority.High]).Enqueue(data);
                        return false;
                    }

                    if (_HighCount > 1) {
                        // there is more data to send
                        return false;
                    }
                }

                return true;
            }

            private bool _CheckAboveMediumBuffer()
            {
                if ((_AboveMediumCount > 0) &&
                    (_AboveMediumSentCount < _AboveMediumThresholdCount)) {
                    string data = (string)((Queue)_Connection._SendBuffer[Priority.AboveMedium]).Dequeue();
                    if (_Connection._WriteLine(data) == false) {
#if LOG4NET
                        Logger.Queue.Warn("Sending data was not sucessful, data is requeued!");
#endif
                        ((Queue)_Connection._SendBuffer[Priority.AboveMedium]).Enqueue(data);
                        return false;
                    }
                    _AboveMediumSentCount++;

                    if (_AboveMediumSentCount < _AboveMediumThresholdCount) {
                        return false;
                    }
                }

                return true;
            }

            private bool _CheckMediumBuffer()
            {
                if ((_MediumCount > 0) &&
                    (_MediumSentCount < _MediumThresholdCount)) {
                    string data = (string)((Queue)_Connection._SendBuffer[Priority.Medium]).Dequeue();
                    if (_Connection._WriteLine(data) == false) {
#if LOG4NET
                        Logger.Queue.Warn("Sending data was not sucessful, data is requeued!");
#endif
                        ((Queue)_Connection._SendBuffer[Priority.Medium]).Enqueue(data);
                        return false;
                    }
                    _MediumSentCount++;

                    if (_MediumSentCount < _MediumThresholdCount) {
                        return false;
                    }
                }

                return true;
            }

            private bool _CheckBelowMediumBuffer()
            {
                if ((_BelowMediumCount > 0) &&
                    (_BelowMediumSentCount < _BelowMediumThresholdCount)) {
                    string data = (string)((Queue)_Connection._SendBuffer[Priority.BelowMedium]).Dequeue();
                    if (_Connection._WriteLine(data) == false) {
#if LOG4NET
                        Logger.Queue.Warn("Sending data was not sucessful, data is requeued!");
#endif
                        ((Queue)_Connection._SendBuffer[Priority.BelowMedium]).Enqueue(data);
                        return false;
                    }
                    _BelowMediumSentCount++;

                    if (_BelowMediumSentCount < _BelowMediumThresholdCount) {
                        return false;
                    }
                }

                return true;
            }

            private bool _CheckLowBuffer()
            {
                if (_LowCount > 0) {
                    if ((_HighCount > 0) ||
                        (_AboveMediumCount > 0) ||
                        (_MediumCount > 0) ||
                        (_BelowMediumCount > 0)) {
                        return true;
                    }

                    string data = (string)((Queue)_Connection._SendBuffer[Priority.Low]).Dequeue();
                    if (_Connection._WriteLine(data) == false) {
#if LOG4NET
                        Logger.Queue.Warn("Sending data was not sucessful, data is requeued!");
#endif
                        ((Queue)_Connection._SendBuffer[Priority.Low]).Enqueue(data);
                        return false;
                    }

                    if (_LowCount > 1) {
                        return false;
                    }
                }

                return true;
            }
            // END OF WARNING, below this you can read/change again ;)
#endregion
        }

        /// <summary>
        /// 
        /// </summary>
        private class IdleWorkerThread
        {
            private IrcConnection   _Connection;
            private Thread          _Thread;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="connection"></param>
            public IdleWorkerThread(IrcConnection connection)
            {
                _Connection = connection;
            }

            /// <summary>
            /// 
            /// </summary>
            public void Start()
            {
                DateTime now = DateTime.Now;
                _Connection._LastPingSent = now;
                _Connection._LastPongReceived = now;
                
                _Thread = new Thread(new ThreadStart(_Worker));
                _Thread.Name = "IdleWorkerThread ("+_Connection.Address+":"+_Connection.Port+")";
                _Thread.IsBackground = true;
                _Thread.Start();
            }

            /// <summary>
            /// 
            /// </summary>
            public void Stop()
            {
                _Thread.Abort();
            }

            private void _Worker()
            {
#if LOG4NET
                Logger.Socket.Debug("IdleWorkerThread started");
#endif
                try {
                    while (_Connection.IsConnected ) {
                        Thread.Sleep(_Connection._IdleWorkerInterval * 1000);
                        
                        // only send active pings if we are registered
                        if (!_Connection.IsRegistered) {
                            continue;
                        }
                        
                        DateTime now = DateTime.Now;
                        int last_ping_sent = (int)(now - _Connection._LastPingSent).TotalSeconds;
                        int last_pong_rcvd = (int)(now - _Connection._LastPongReceived).TotalSeconds;
                        // determins if the resoponse time is ok
                        if (last_ping_sent < _Connection._PingTimeout) {
                            if (_Connection._LastPingSent > _Connection._LastPongReceived) {
                                // there is a pending ping request, we have to wait
                                continue;
                            }
                            
                            // determines if it need to send another ping yet
                            if (last_pong_rcvd > _Connection._PingInterval) {
                                _Connection.WriteLine(Rfc2812.Ping(_Connection.Address), Priority.Critical);
                                _Connection._LastPingSent = now;
                                //_Connection._LastPongReceived = now;
                            } // else connection is fine, just continue
                        } else {
                            if (_Connection.IsDisconnecting) {
                                break;
                            }
#if LOG4NET
                            Logger.Socket.Warn("ping timeout, connection lost");
#endif
                            // only flag this as connection error if we are not
                            // cleanly disconnecting
                            _Connection.IsConnectionError = true;
                            break;
                        }
                    }
                } catch (ThreadAbortException) {
                    Thread.ResetAbort();
#if LOG4NET
                    Logger.Socket.Debug("IdleWorkerThread aborted");
#endif
                } catch (Exception ex) {
#if LOG4NET
                    Logger.Socket.Error(ex);
#endif
                }
            }
        }
    }
}
