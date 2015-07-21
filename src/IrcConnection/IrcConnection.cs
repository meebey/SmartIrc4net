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
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Threading;

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
        private IIrcTransportManager _Transport;
        private string[]         _AddressList = {"localhost"};
        private int              _CurrentAddress;
        private int              _Port;
        private ConcurrentQueue<string> _ReadQueue;
        private AutoResetEvent   _ReadQueueEvent;
        private WriteManager  _WriteManager;
        private IdleWorkerThread _IdleWorkerThread;
        private int              _SendDelay = 200;
        private bool             _IsDisconnecting;
        private bool             _IsRegistered;
        private int              _AutoRetryAttempt;
        private bool             _AutoRetry;
        private int              _AutoRetryDelay = 30;
        private int              _AutoRetryLimit = 3;
        private bool             _AutoReconnect;
        private int              _IdleWorkerInterval = 5;
        private int              _PingInterval = 60;
        private int              _PingTimeout = 90;
        private DateTime         _LastPingSent;
        private DateTime         _LastPongReceived;
        private TimeSpan         _Lag;

        
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
        /// Raised on successful re-connect
        /// </event>
        public event EventHandler           OnReconnected;
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
        /// Get the transport being used for the connection
        /// </summary>
        public IIrcTransportManager Transport {
            get {
                return _Transport;
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
        /// Gets the used port of the connection
        /// </summary>
        public int Port {
            get {
                return _Port;
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
        /// When a connection error is detected this property will return true
        /// </summary>
        public bool IsConnectionError {
            get {
                if (_Transport == null)
                    return false;

                return _Transport.IsConnectionError;
            }
        }

        /// <summary>
        /// On successful connect to the IRC server, this is set to true.
        /// </summary>
        public bool IsConnected {
            get {
                if (_Transport == null)
                    return false;

                return _Transport.IsConnected;
            }
        }

        /// <summary>
        /// Returns true if the client is currently disconnecting
        /// </summary>
        public bool IsDisconnecting {
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
        /// On successful registration on the IRC network, this is set to true.
        /// </summary>
        public bool IsRegistered {
            get {
                return _IsRegistered;
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
        /// Returns true if the connection is established and working properly, no errors have been reported and we are not disconnecting
        /// Internal use only.
        /// </summary>
        private bool _ConnectionEstablished {
            get {
                return IsConnected && !IsDisconnecting && !IsConnectionError;
            }
        }

        /// <summary>
        /// Initializes the message queues and write thread
        /// </summary>
        public IrcConnection()
        {
#if LOG4NET
            Logger.Main.Debug("IrcConnection created");
#endif
            // setup own callbacks
            OnReadLine        += new ReadLineEventHandler(_SimpleParser);

            // use default TCP transport for backwards compatibility if none supplied to Connect()
            _Transport = new IrcTcpTransport();

            _ReadQueueEvent = new AutoResetEvent(false);
            _WriteManager = new WriteManager(this);
            _IdleWorkerThread = new IdleWorkerThread(this);

            _WriteManager.OnWriteLine += new WriteLineEventHandler(_OnWriteLine);

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

        /// <summary>
        /// Connects to the specified server and port using the current transport, or default TCP if none pre-specified.
        /// When the connection fails the next server in the list will be used.
        /// </summary>
        /// <param name="addresslist">List of servers to connect to</param>
        /// <param name="port">Portnumber to connect to</param>
        /// <exception cref="CouldNotConnectException">The connection failed</exception>
        /// <exception cref="AlreadyConnectedException">If there is already an active connection</exception>
        public virtual void Connect(string[] addresslist, int port = 0)
        {
            Connect(_Transport, addresslist, port);
        }

        /// <summary>
        /// Connects to the specified server and port using the current transport, or default TCP if none pre-specified
        /// </summary>
        /// <param name="address">Server address to connect to</param>
        /// <param name="port">Port number to connect to</param>
        /// <exception cref="CouldNotConnectException">The connection failed</exception>
        /// <exception cref="AlreadyConnectedException">If there is already an active connection</exception>
        public virtual void Connect(string address, int port = 0)
        {
            Connect(_Transport, new string[] { address }, port);
        }

        /// <summary>
        /// Connects to the specified transport. The transport must be pre-populated with the host address
        /// </summary>
        /// <param name="transport">Transport protocol to use</param>
        /// <exception cref="CouldNotConnectException">The connection failed</exception>
        /// <exception cref="AlreadyConnectedException">If there is already an active connection</exception>
        public virtual void Connect(IIrcTransportManager transport)
        {
            Connect(transport, new string[] { transport.Address }, transport.Port);
        }

        /// <overloads>this method has 3 overloads</overloads>
        /// <summary>
        /// Connects to the specified server and port using the specified transport, when the connection fails
        /// the next server in the list will be used.
        /// </summary>
        /// <param name="transport">Transport protocol to use</param>
        /// <param name="addresslist">List of servers to connect to</param>
        /// <param name="port">Portnumber to connect to</param>
        /// <exception cref="CouldNotConnectException">The connection failed</exception>
        /// <exception cref="AlreadyConnectedException">If there is already an active connection</exception>
        public virtual void Connect(IIrcTransportManager transport, string[] addresslist, int port = 0)
        {
            if (IsConnected) {
                throw new AlreadyConnectedException("Already connected to: " + Address + ":" + Port);
            }

            if (_IdleWorkerInterval >= _PingTimeout || _IdleWorkerInterval >= _PingInterval) {
                throw new CouldNotConnectException("Idle worker polling interval (IdleWorkerInterval) MUST be less than PingTimeout and PingInterval!");
            }
#if LOG4NET
            Logger.Connection.Info(String.Format("connecting... (attempt: {0})",
                                                 _AutoRetryAttempt));
#endif
            _Transport = transport;
            _AddressList = (string[])addresslist.Clone();
            _Port = port;

            // always start from the beginning of the address list
            _CurrentAddress = 0;

            // no retries yet
            _AutoRetryAttempt = 0;

            // repeat up to the maximum number of retries
            do {
                _AutoRetryAttempt++;

                if (OnConnecting != null) {
                    OnConnecting(this, EventArgs.Empty);
                }

                try {
                    // Connect with transport
                    _Transport.OnMessageReceived += Transport_OnMessageReceived;
                    _Transport.OnConnectionError += Transport_OnConnectionError;

                    // Not all transports support changing the address or port once created
                    // Those that don't should throw NotImplementedException
                    try {
                        _Transport.Address = Address;
                        _Transport.Port = Port;
                    } catch (NotImplementedException) {
                    }

                    // throws on error
                    _Transport.Connect();

                    // lets power up our queues
                    _ReadQueue = new ConcurrentQueue<string>();
                    _WriteManager.Start();
                    _IdleWorkerThread.Start();

#if LOG4NET
                    Logger.Connection.Info("connected");
#endif
                    if (OnConnected != null) {
                        OnConnected(this, EventArgs.Empty);
                    }
                }
                catch (Exception e) {
#if LOG4NET
                    Logger.Connection.Info("connection failed: " + e.Message, e);
#endif
                    _Transport.OnMessageReceived -= Transport_OnMessageReceived;
                    _Transport.OnConnectionError -= Transport_OnConnectionError;

                    // fatal error
                    if (e is System.Security.Authentication.AuthenticationException)
                        throw;

                    if (_AutoRetry &&
                        (_AutoRetryLimit == -1 ||
                         _AutoRetryLimit == 0 ||
                         _AutoRetryAttempt < _AutoRetryLimit)) {
                        if (OnAutoConnectError != null) {
                            OnAutoConnectError(this, new AutoConnectErrorEventArgs(Address, Port, e));
                        }
#if LOG4NET
                        Logger.Connection.Debug("delaying new connect attempt for " + _AutoRetryDelay + " sec");
#endif
                        Thread.Sleep(_AutoRetryDelay * 1000);

                        _NextAddress();
                    } else if (!_AutoRetry) {
                        throw new CouldNotConnectException(e.Message, e);
                    }
                }
            } while (!IsConnected && _AutoRetry &&
                    (_AutoRetryLimit == -1 ||
                    _AutoRetryLimit == 0 ||
                    _AutoRetryAttempt < _AutoRetryLimit));

            if (!IsConnected && _AutoRetry && _AutoRetryAttempt == _AutoRetryLimit)
                throw new CouldNotConnectException("Maximum number of connection retries exceeded");
        }

        /// <summary>
        /// Reconnects to the server using the current transport
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
            Disconnect(true);
            Connect(_Transport, _AddressList, _Port);

            if (OnReconnected != null)
                OnReconnected(this, EventArgs.Empty);
        }
        
        /// <summary>
        /// Disconnects from the server
        /// </summary>
        /// <param name="reconnecting">Set to true if you intend to call Connect() again so that Listen() continues blocking</param>
        /// <exception cref="NotConnectedException">
        /// If there was no active connection
        /// </exception>
        public void Disconnect(bool reconnecting = false)
        {
            if (!IsConnected) {
                throw new NotConnectedException("The connection could not be disconnected because there is no active connection");
            }

            // Disconnect() is not re-entrant.
            // If a call is made to Disconnect() while already disconnecting, it returns immediately
            if (IsDisconnecting) {
                return;
            }
#if LOG4NET
            Logger.Connection.Info("disconnecting...");
#endif
            IsDisconnecting = true;

            if (OnDisconnecting != null) {
                OnDisconnecting(this, EventArgs.Empty);
            }

            // NOTE: In the event of a connection error, these threads will have stopped themselves cleanly
            // but there is no harm in making sure. If there is no connection error, they will need to be stopped.
            _IdleWorkerThread.Stop();
            _WriteManager.Stop();

            // make sure we do this after the threads are stopped otherwise we could write to a non-existent connection
            _Transport.Disconnect();
            _Transport.OnMessageReceived -= Transport_OnMessageReceived;
            _Transport.OnConnectionError -= Transport_OnConnectionError;

            _IsRegistered = false;

            IsDisconnecting = false;
            
            if (OnDisconnected != null) {
                OnDisconnected(this, EventArgs.Empty);
            }

            // Release Listen() from blocking - this MUST be called after _Transport.Disconnect() to ensure IsConnected == false
            if (!reconnecting) {
                _ReadQueueEvent.Set();
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
                // We don't use _ConnectionEstablished here because we don't want Listen() to return until
                // all disconnection steps are finished, so instead we raise _ReadQueueEvent once
                // IsConnected == false
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
        /// Read one line of text from the read queue; returns null if the connection has been closed or errors,
        /// or an empty string if there is no new data to read (when non-blocking)
        /// </summary>
        /// <param name="blocking"></param>
        /// <returns></returns>
        public string ReadLine(bool blocking)
        {
            string data = "";

            if (blocking) {
                // block till the queue has data, but bail out on connection error
                // We don't use _ConnectionEstablished here because we don't want Listen() to return until
                // all disconnection steps are finished, so instead we raise _ReadQueueEvent once
                // IsConnected == false
                while (IsConnected &&
                       _ReadQueue.Count == 0) {
                    _ReadQueueEvent.WaitOne();
                }
            }

            if (_ConnectionEstablished &&
                _ReadQueue.Count > 0) {
                _ReadQueue.TryDequeue(out data);
            }

            if (data != null && data.Length > 0) {
                if (OnReadLine != null) {
                    OnReadLine(this, new ReadLineEventArgs(data));
                }
            }

            return data;
        }

        /// <summary>
        /// Write a line of text to the connection with the specified priority
        /// </summary>
        /// <param name="data"></param>
        /// <param name="priority"></param>
        public void WriteLine(string data, Priority priority)
        {
            _WriteManager.WriteLine(data, priority);
        }

        /// <summary>
        /// Write a line of text to the connection with default (medium) priority
        /// </summary>
        /// <param name="data"></param>
        public void WriteLine(string data)
        {
            WriteLine(data, Priority.Medium);
        }

        /// <summary>
        /// Received from the write thread when a line is successfully written to the connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _OnWriteLine(object sender, WriteLineEventArgs e)
        {
            if (OnWriteLine != null) {
                OnWriteLine(sender, e);
            }
        }

        /// <summary>
        /// Received from the transport when a line is successfully received from the connection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Transport_OnMessageReceived(object sender, ReadLineEventArgs e)
        {
            // Ignore incoming messages if we are disconnecting or there is a connection error
            // because we don't want to raise _ReadQueueEvent and cause an exit from Listen()
            // if AutoReconnect is enabled and we are in the process of re-connecting
            if (_ConnectionEstablished) {

                // Add it to the read queue
                _ReadQueue.Enqueue(e.Line);
                _ReadQueueEvent.Set();
            }
        }

        /// <summary>
        /// Received from the transport when a connection error occurs
        /// </summary>
        private void Transport_OnConnectionError()
        {
            // We will now always funnel all connection errors into this method so fire the event here
            // Apart from centralizing it, this also means we can raise OnConnectionError when an error
            // occurs in this class (eg. ping timeout) that will not raise _Transport.IsConnectionError.
            if (OnConnectionError != null) {
                OnConnectionError(this, EventArgs.Empty);
            }

            try {
                if (AutoReconnect) {
                    // prevent connect -> exception -> connect flood loop
                    // TODO: But not on first disconnect
                    // TODO: We really need to clean up all the threads and stuff before sleeping
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

        /// <summary>
        /// A write thread and helper functions to write data to the connection
        /// </summary>
        private class WriteManager
        {
            private IrcConnection  _Connection;
            private Thread         _Thread;
            private Hashtable _SendBuffer = Hashtable.Synchronized(new Hashtable());
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

            private AutoResetEvent _QueuedEvent;

            /// <summary>
            /// Fires when a line has been successfully sent to the network stream
            /// </summary>
            public event WriteLineEventHandler OnWriteLine;

            /// <summary>
            /// Set up using the specified connection
            /// </summary>
            /// <param name="connection"></param>
            public WriteManager(IrcConnection connection)
            {
                _Connection = connection;
                _QueuedEvent = new AutoResetEvent(false);

                _SendBuffer[Priority.High] = Queue.Synchronized(new Queue());
                _SendBuffer[Priority.AboveMedium] = Queue.Synchronized(new Queue());
                _SendBuffer[Priority.Medium] = Queue.Synchronized(new Queue());
                _SendBuffer[Priority.BelowMedium] = Queue.Synchronized(new Queue());
                _SendBuffer[Priority.Low] = Queue.Synchronized(new Queue());
            }

            /// <summary>
            /// 
            /// </summary>
            public void Start()
            {
                _QueuedEvent.Reset();

                ((Queue)_SendBuffer[Priority.High]).Clear();
                ((Queue)_SendBuffer[Priority.AboveMedium]).Clear();
                ((Queue)_SendBuffer[Priority.Medium]).Clear();
                ((Queue)_SendBuffer[Priority.BelowMedium]).Clear();
                ((Queue)_SendBuffer[Priority.Low]).Clear();

                _Thread = new Thread(new ThreadStart(_Worker));
                _Thread.Name = "WriteThread (" + _Connection.Address + ") [" + DateTime.Now + "]";
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
                _QueuedEvent.Set();

                // We have to check _Thread here in case the connection is disconnected before the thread starts
                if (_Thread != null) {
                    _Thread.Join();
                }
            }

            /// <summary>
            /// Write a line to the stream if priority is Critical, otherwise queue to one of the write buffers
            /// </summary>
            /// <param name="data">String to send</param>
            /// <param name="priority">Priority level</param>
            public bool WriteLine(string data, Priority priority)
            {
                if (priority == Priority.Critical) {
                    // We don't use _ConnectionEstablished here because we want it to fall through without exceptions
                    // if there's a connection error
                    if (!_Connection.IsConnected) {
                        return true; // the data was successfully sent... into a void (so that WriteManager doesn't re-queue the messages); clients should check IsConnected
                    }

                    if (_Connection.Transport.WriteLine(data)) {
                        if (OnWriteLine != null)
                            OnWriteLine(this, new WriteLineEventArgs(data));

                        return true;
                    } else {
                        // Transport.ConnectionError will have been set automatically if we reach this point: CONFIRMED by Unit test 'ClientDirtyDisconnect'
                        return false;
                    }
                } else {
                    ((Queue) _SendBuffer[priority]).Enqueue(data);
                    _QueuedEvent.Set();
                    return true;
                }
            }

            private bool _WriteLineCritical(string data)
            {
                return WriteLine(data, Priority.Critical);
            }

            private void _Worker()
            {
#if LOG4NET
                Logger.Socket.Debug("WriteThread started");
#endif
                try {
                    try {
                        do {
                            _QueuedEvent.WaitOne();

                            if (_Connection._ConnectionEstablished) {
                                bool isBufferEmpty = false;
                                do {
                                    isBufferEmpty = _CheckBuffer() == 0;
                                    Thread.Sleep(_Connection._SendDelay);
                                    
                                    // We must check _ConnectionEstablished here because when the write fails,
                                    // it will re-queue and the buffer will never empty, causing this loop
                                    // to repeat endlessly
                                } while (!isBufferEmpty && _Connection._ConnectionEstablished);
                            }
                        } while (_Connection._ConnectionEstablished);
                    } catch (IOException e) {
#if LOG4NET
                        Logger.Socket.Warn("IOException: " + e.Message);
#endif
                    } finally {
#if LOG4NET
                        Logger.Socket.Warn("connection lost");
#endif
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
                _HighCount        = ((Queue)_SendBuffer[Priority.High]).Count;
                _AboveMediumCount = ((Queue)_SendBuffer[Priority.AboveMedium]).Count;
                _MediumCount      = ((Queue)_SendBuffer[Priority.Medium]).Count;
                _BelowMediumCount = ((Queue)_SendBuffer[Priority.BelowMedium]).Count;
                _LowCount         = ((Queue)_SendBuffer[Priority.Low]).Count;

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
                    string data = (string)((Queue)_SendBuffer[Priority.High]).Dequeue();
                    if (_WriteLineCritical(data) == false) {
#if LOG4NET
                        Logger.Queue.Warn("Sending data was not sucessful, data is requeued!");
#endif
                        ((Queue)_SendBuffer[Priority.High]).Enqueue(data);
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
                    string data = (string)((Queue)_SendBuffer[Priority.AboveMedium]).Dequeue();
                    if (_WriteLineCritical(data) == false) {
#if LOG4NET
                        Logger.Queue.Warn("Sending data was not sucessful, data is requeued!");
#endif
                        ((Queue)_SendBuffer[Priority.AboveMedium]).Enqueue(data);
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
                    string data = (string)((Queue)_SendBuffer[Priority.Medium]).Dequeue();
                    if (_WriteLineCritical(data) == false) {
#if LOG4NET
                        Logger.Queue.Warn("Sending data was not sucessful, data is requeued!");
#endif
                        ((Queue)_SendBuffer[Priority.Medium]).Enqueue(data);
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
                    string data = (string)((Queue)_SendBuffer[Priority.BelowMedium]).Dequeue();
                    if (_WriteLineCritical(data) == false) {
#if LOG4NET
                        Logger.Queue.Warn("Sending data was not sucessful, data is requeued!");
#endif
                        ((Queue)_SendBuffer[Priority.BelowMedium]).Enqueue(data);
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

                    string data = (string)((Queue)_SendBuffer[Priority.Low]).Dequeue();
                    if (_WriteLineCritical(data) == false) {
#if LOG4NET
                        Logger.Queue.Warn("Sending data was not sucessful, data is requeued!");
#endif
                        ((Queue)_SendBuffer[Priority.Low]).Enqueue(data);
                        return false;
                    }

                    if (_LowCount > 1) {
                        return false;
                    }
                }

                return true;
            }
            // END OF WARNING, below this you can read/change again ;)
            // - sorry, I changed it all :P Love, Katy.
#endregion
        }

        /// <summary>
        /// Manage pings and pongs
        /// </summary>
        private class IdleWorkerThread
        {
            private IrcConnection   _Connection;
            private Thread          _Thread;

            private volatile bool _ThreadErrorRaised;

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
                // Using lock for thread-safe access to _Thread
                lock (this) {
                    _ThreadErrorRaised = false;
                    _Thread = new Thread(new ThreadStart(_Worker));
                    _Thread.Name = "IdleWorkerThread (" + _Connection.Address + ") [" + DateTime.Now + "]";
                    _Thread.IsBackground = true;
                    _Thread.Start();
                }
            }

            /// <summary>
            /// 
            /// </summary>
            public void Stop()
            {
                // We have to check _Thread here in case the connection is disconnected before the thread starts
                lock (this) {
                    if (!_ThreadErrorRaised && _Thread != null) {
                        _Thread.Abort();
                        _Thread.Join();
                    }
                }
            }

            private void _Worker()
            {
                bool wasRegistered = false;

                // Strip protocol from outbound PING messages
                string pingAddress = "://" + _Connection.Address;
                pingAddress = pingAddress.Substring(pingAddress.LastIndexOf("://") + 3);
#if LOG4NET
                Logger.Socket.Debug("IdleWorkerThread started");
#endif
                try {
                    while (_Connection._ConnectionEstablished) {
                        // TODO: We need to deal with this so we can get rid of _Thread.Abort()
                        Thread.Sleep(_Connection._IdleWorkerInterval * 1000);
                        
                        // only send active pings if we are registered
                        if (!_Connection.IsRegistered) {
                            continue;
                        }

                        DateTime now = DateTime.Now;

                        // Don't start counting time or sending ping requests until we are registered
                        if (!wasRegistered) {
                            _Connection._LastPingSent = now - TimeSpan.FromSeconds(1);
                            _Connection._LastPongReceived = now;
                            wasRegistered = true;
                        }

                        bool timeout = false;

                        // We haven't received a pong since our last ping
                        // The "=" is critical here.
                        if (_Connection._LastPongReceived <= _Connection._LastPingSent) {

                            // Has it been long enough to trigger a ping timeout?
                            if (now.Subtract(_Connection._LastPingSent).TotalSeconds >= _Connection._PingTimeout) {
                                timeout = true;
                            }

                        // We have received a pong since our last ping
                        } else {

                            // Is it time to send a new ping?
                            if (now.Subtract(_Connection._LastPongReceived).TotalSeconds >= _Connection.PingInterval) {
                                _Connection.WriteLine(Rfc2812.Ping(pingAddress), Priority.Critical);
                                _Connection._LastPingSent = now;
                            }
                        }

                        // Ping timeout
                        if (timeout) {
#if LOG4NET
                            Logger.Connection.Warn("Ping timeout");
                            Logger.Connection.Warn("Last ping: " + _Connection._LastPingSent + "; Last pong: " + _Connection._LastPongReceived);
#endif
                            if (_Connection.IsDisconnecting) {
                                break;
                            }
#if LOG4NET
                            Logger.Socket.Warn("ping timeout, connection lost");
#endif
                            // Transport_OnConnectionError() will block here until either the disconnect or a re-connect completes
                            // In the event of a re-connect, a 2nd worker thread will be created before this one terminates,
                            // however the termination will still happen cleanly.
                            _ThreadErrorRaised = true;
                            _Connection.Transport_OnConnectionError();
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
