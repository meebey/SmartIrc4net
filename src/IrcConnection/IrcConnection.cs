/**
 * $Id$
 * $URL$
 * $Rev$
 * $Author$
 * $Date$
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 *
 * Copyright (c) 2003-2004 Mirco Bauer <meebey@meebey.net> <http://www.meebey.net>
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
using System.Text;
using System.Collections;
using System.Threading;
using System.Reflection;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    ///
    /// </summary>
    public class IrcConnection
    {
        private string          _VersionNumber;
        private string          _VersionString;
        private string[]        _AddressList = {"localhost"};
        private int             _CurrentAddress = 0;
        private int             _Port = 6667;
        private StreamReader    _Reader;
        private StreamWriter    _Writer;
        private ReadThread      _ReadThread;
        private WriteThread     _WriteThread;
        private IrcTcpClient    _TcpClient;
        private Hashtable       _SendBuffer = Hashtable.Synchronized(new Hashtable());
        private int             _SendDelay = 200;
        private bool            _IsRegistered = false;
        private bool            _IsConnected  = false;
        private bool            _IsConnectionError = false;
        private int             _ConnectTries = 0;
        private bool            _AutoRetry     = false;
        private bool            _AutoReconnect = false;
        private Encoding        _Encoding = Encoding.GetEncoding("ISO-8859-1");
        //private Encoding        _Encoding = Encoding.ASCII;
        //private Encoding        _Encoding = Encoding.GetEncoding(1252);
        //private Encoding        _Encoding = Encoding.UTF8;
        private int             _SocketReceiveTimeout  = 600;
        private int             _SocketSendTimeout = 600;

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
        
        /// <summary>
        /// When a connection error is detected this property will return true
        /// </summary>
        /// <remarks>
        /// Thread-safe
        /// </remarks
        protected bool IsConnectionError
        {
            get {
                lock (this) {
                    return _IsConnectionError;
                }
            }
            set {
                lock (this) {
                    _IsConnectionError = value;
                }
            }
        }

        /// <summary>
        /// The current address of the connection
        /// </summary>
        public string Address
        {
            get {
                return _AddressList[_CurrentAddress];
            }
        }

        /// <summary>
        /// The address list of the connection
        /// </summary>
        public string[] AddressList
        {
            get {
                return _AddressList;
            }
        }

        /// <summary>
        /// Which port is used for the connection
        /// </summary>
        public int Port
        {
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
        public bool AutoReconnect
        {
            get {
                return _AutoReconnect;
            }
            set {
#if LOG4NET
                if (value == true) {
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
        public bool AutoRetry
        {
            get {
                return _AutoRetry;
            }
            set {
#if LOG4NET
                if (value == true) {
                    Logger.Connection.Info("AutoRetry enabled");
                } else {
                    Logger.Connection.Info("AutoRetry disabled");
                }
#endif
                _AutoRetry = value;
            }
        }

        /// <summary>
        /// To prevent flooding the IRC server, it's required to delay each
        /// message, given in milliseconds.
        /// Default: 200
        /// </summary>
        public int SendDelay
        {
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
        public bool IsRegistered
        {
            get {
                return _IsRegistered;
            }
        }

        /// <summary>
        /// On successful connect to the IRC server, this is set to true.
        /// </summary>
        public bool IsConnected
        {
            get {
                return _IsConnected;
            }
        }

        /// <summary>
        /// The SmartIrc4net version number
        /// </summary>
        public string VersionNumber
        {
            get {
                return _VersionNumber;
            }
        }

        /// <summary>
        /// The full SmartIrc4net version
        /// </summary>
        public string VersionString
        {
            get {
                return _VersionString;
            }
        }

        /// <summary>
        /// Encoding which is used for reading and writing to the socket
        /// Default: ISO-8859-1
        /// </summary>
        public Encoding Encoding
        {
            get {
                return _Encoding;
            }
            set {
                _Encoding = value;
            }
        }

        /// <summary>
        /// Timeout in seconds for receiving data from the socket
        /// Default: 600
        /// </summary>
        public int SocketReceiveTimeout
        {
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
        public int SocketSendTimeout
        {
            get {
                return _SocketSendTimeout;
            }
            set {
                _SocketSendTimeout = value;
            }
        }
        
        /// <summary>
        /// Initializes the message queues, read and write thread
        /// </summary>
        public IrcConnection()
        {
#if LOG4NET        
            Logger.Init();
#endif
            _SendBuffer[Priority.High]        = Queue.Synchronized(new Queue());
            _SendBuffer[Priority.AboveMedium] = Queue.Synchronized(new Queue());
            _SendBuffer[Priority.Medium]      = Queue.Synchronized(new Queue());
            _SendBuffer[Priority.BelowMedium] = Queue.Synchronized(new Queue());
            _SendBuffer[Priority.Low]         = Queue.Synchronized(new Queue());

            OnReadLine += new ReadLineEventHandler(_SimpleParser);

            _ReadThread  = new ReadThread(this);
            _WriteThread = new WriteThread(this);

            Assembly assm = Assembly.GetAssembly(this.GetType());
            AssemblyName assm_name = assm.GetName(false);

            AssemblyProductAttribute pr = (AssemblyProductAttribute)assm.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0];

            _VersionNumber = assm_name.Version.ToString();
            _VersionString = pr.Product+" "+_VersionNumber;
        }

        /// <overloads>this method has 2 overloads</overloads>
        /// <summary>
        /// Connects to the specified server and port, when the connection fails
        /// the next server in the list will be used.
        /// </summary>
        /// <param name="addresslist">List of servers to connect to</pararm>
        /// <param name="port">Portnumber to connect to</pararm>
        /// <exception cref="CouldNotConnectException">The connection failed</exceptio>
        /// <exception cref="AlreadyConnectedException">If there is already an active connection</exceptio>
        public virtual void Connect(string[] addresslist, int port)
        {
            if (_IsConnected) {
                throw new AlreadyConnectedException("Already connected to: "+Address+":"+Port);
            }

#if LOG4NET
            Logger.Connection.Info("connecting...");
#endif
            _ConnectTries++;
            _AddressList = (string[])addresslist.Clone();
            _Port = port;

            if (OnConnecting != null) {
                OnConnecting(this, EventArgs.Empty);
            }
            try {
                System.Net.IPAddress ip = System.Net.Dns.Resolve(Address).AddressList[0];
                _TcpClient = new IrcTcpClient();
                _TcpClient.NoDelay = true;
                // set timeout, after this the connection will be aborted
                _TcpClient.ReceiveTimeout = _SocketReceiveTimeout*1000;
                _TcpClient.SendTimeout = _SocketSendTimeout*1000;
                _TcpClient.Connect(ip, port);
                if (OnConnected != null) {
                    OnConnected(this, EventArgs.Empty);
                }

                _Reader = new StreamReader(_TcpClient.GetStream(), Encoding);
                _Writer = new StreamWriter(_TcpClient.GetStream(), Encoding);

                // Connection was succeful, reseting the connect counter
                _ConnectTries = 0;

                // updating the connection error state, so connecting is possible again
                IsConnectionError = false;
                _IsConnected = true;
#if LOG4NET
                Logger.Connection.Info("connected");
#endif

                // lets power up our threads
                _ReadThread.Start();
                _WriteThread.Start();
            } catch (Exception e) {
                if (_Reader != null) {
                    _Reader.Close();
                }
                if (_Writer != null) {
                    _Writer.Close();
                }
                if (_TcpClient != null) {
                    _TcpClient.Close();
                }
                _IsConnected = false;
                IsConnectionError = true;
#if LOG4NET
                Logger.Connection.Info("connection failed: "+e.Message);
#endif
                if (_AutoRetry == true &&
                    _ConnectTries <= 3) {
                    _NextAddress();
                    Reconnect(false);
                } else {
                    throw new CouldNotConnectException("Could not connect to: "+Address+":"+Port+" "+e.Message, e);
                }
            }
        }

        /// <summary>
        /// Connects to the specified server and port.
        /// </summary>
        /// <param name="address">Server address to connect to</pararm>
        /// <param name="port">Port number to connect to</pararm>
        public void Connect(string address, int port)
        {
            Connect(new string[] {address}, port);
        }

        /// <overloads>this method has 2 overloads</overloads>
        /// <summary>
        /// Reconnects to the server
        /// </summary>
        /// <param name="login">if the login data should be sent, after successful connect</pararm>
        /// <returns>
        /// Returns true on success and false when the connection failed.
        /// </returns>
        /// <exception cref="NotConnectedException">
        /// If there was no active connection
        /// </exception>
        /// <exception cref="CouldNotConnectException">
        /// The connection failed
        /// </exception>
        /// <exception cref="AlreadyConnectedException">
        /// If there is already an active connection
        /// </exception>
        // login parameter only for IrcClient needed
        public virtual void Reconnect(bool login)
        {
#if LOG4NET
            Logger.Connection.Info("reconnecting...");
#endif
            Disconnect();
            Connect(_AddressList, _Port);
        }

        /// <summary>
        /// Reconnects to the server, and automaticly logs in
        /// </summary>
        public void Reconnect()
        {
            Reconnect(true);
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

            _ReadThread.Stop();
            _WriteThread.Stop();
            _TcpClient.Close();
            _IsConnected = false;
            _IsRegistered = false;
            
            if (OnDisconnected != null) {
                OnDisconnected(this, EventArgs.Empty);
            }

#if LOG4NET
            Logger.Connection.Info("disconnected");
#endif
        }

        public void Listen(bool blocking)
        {
            if (blocking) {
                while(IsConnected == true) {
                    ReadLine(true);
                    if (IsConnectionError == true) {
                        if (AutoReconnect == true) {
                            Reconnect();
                        } else {
                            Disconnect();
                        }
                    }
                }
            } else {
                while (ReadLine(false).Length > 0) {
                    // loop as long as we receive messages
                }
            }
        }

        public void Listen()
        {
            Listen(true);
        }
        
        public void ListenOnce(bool blocking)
        {
            ReadLine(blocking);
        }

        public void ListenOnce()
        {
            ListenOnce(true);
        }
        
        public string ReadLine(bool blocking)
        {
            string data = "";
            
            if (blocking) {
                // block till the queue has data
                while ((IsConnected == true) &&
                       (_ReadThread.Queue.Count == 0)) {
                    Thread.Sleep(10);
                }
            }

            if ((IsConnected == true) &&
                (_ReadThread.Queue.Count > 0)) {
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

            return data;
        }

        public void WriteLine(string data, Priority priority)
        {
            if (priority == Priority.Critical) {
                if (!IsConnected) {
                    throw new NotConnectedException();
                }
                
                _WriteLine(data);
            } else {
                ((Queue)_SendBuffer[priority]).Enqueue(data);
            }
        }

        public void WriteLine(string data)
        {
            WriteLine(data, Priority.Medium);
        }

        private bool _WriteLine(string data)
        {
            if (IsConnected == true) {
                try {
                    _Writer.Write(data+"\r\n");
                    _Writer.Flush();
                } catch (IOException) {
#if LOG4NET
                    Logger.Socket.Warn("sending data failed, connection lost");
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
            if (_CurrentAddress < _AddressList.Length) {
                // nothing
            } else {
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
            string   messagecode = "";

            if (rawline[0] == ':') {
                messagecode = rawlineex[1];
                try {
                    ReplyCode replycode = (ReplyCode)int.Parse(messagecode);
                    switch(replycode) {
                        case ReplyCode.Welcome:
                            _IsRegistered = true;
#if LOG4NET
                            Logger.Connection.Info("logged in");
#endif
                        break;
                    }
                } catch (FormatException) {
                    // nothing
                }
            } else {
                messagecode = rawlineex[0];
                switch(messagecode) {
                    case "ERROR":
                        IsConnectionError = true;
                    break;
                }
            }
        }

        private class ReadThread
        {
            private IrcConnection  _Connection;
            private Thread         _Thread;
            private Queue          _Queue = Queue.Synchronized(new Queue());

            public Queue Queue
            {
                get {
                    return _Queue;
                }
            }

            public ReadThread(IrcConnection connection)
            {
                _Connection = connection;
            }

            public void Start()
            {
                _Thread = new Thread(new ThreadStart(_Worker));
                _Thread.Name = "ReadThread ("+_Connection.Address+":"+_Connection.Port+")";
                _Thread.IsBackground = true;
                _Thread.Start();
            }

            public void Stop()
            {
                _Thread.Abort();
                _Connection._Reader.Close();
            }

            private void _Worker()
            {
#if LOG4NET
                Logger.Socket.Debug("ReadThread started");
#endif
                try {
                    string data = "";
                    try {
                        while ((_Connection.IsConnected == true) &&
                               ((data = _Connection._Reader.ReadLine()) != null)) {
                            _Queue.Enqueue(data);
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
                        _Connection.IsConnectionError = true;
                    }
                } catch (ThreadAbortException) {
                    Thread.ResetAbort();
#if LOG4NET
                    Logger.Socket.Debug("ReadThread aborted");
#endif
                }
            }
        }

        private class WriteThread
        {
            private IrcConnection  _Connection;
            private Thread         _Thread;
            private int            _HighCount        = 0;
            private int            _AboveMediumCount = 0;
            private int            _MediumCount      = 0;
            private int            _BelowMediumCount = 0;
            private int            _LowCount         = 0;
            private int            _AboveMediumSentCount = 0;
            private int            _MediumSentCount      = 0;
            private int            _BelowMediumSentCount = 0;
            private int            _AboveMediumThresholdCount = 4;
            private int            _MediumThresholdCount      = 2;
            private int            _BelowMediumThresholdCount = 1;
            private int            _BurstCount = 0;

            public WriteThread(IrcConnection connection)
            {
                _Connection = connection;
            }

            public void Start()
            {
                _Thread = new Thread(new ThreadStart(_Worker));
                _Thread.Name = "WriteThread ("+_Connection.Address+":"+_Connection.Port+")";
                _Thread.IsBackground = true;
                _Thread.Start();
            }

            public void Stop()
            {
                _Thread.Abort();
                _Connection._Writer.Close();
            }

            private void _Worker()
            {
#if LOG4NET
                Logger.Socket.Debug("WriteThread started");
#endif
                try {
                    try {
                        while (_Connection.IsConnected == true) {
                            _CheckBuffer();
                            Thread.Sleep(_Connection._SendDelay);
                        }
                    } catch (IOException e) {
#if LOG4NET
                        Logger.Socket.Warn("IOException: "+e.Message);
#endif
                    } finally {
#if LOG4NET
                        Logger.Socket.Warn("connection lost");
#endif
                        _Connection.IsConnectionError = true;
                    }
                } catch (ThreadAbortException) {
                    Thread.ResetAbort();
#if LOG4NET
                    Logger.Socket.Debug("WriteThread aborted");
#endif
                }
            }

            // WARNING: complex scheduler, don't even think about changing it!
            private void _CheckBuffer()
            {
                // only send data if we are succefully registered on the IRC network
                if (!_Connection._IsRegistered) {
                    return;
                }

                _HighCount        = ((Queue)_Connection._SendBuffer[Priority.High]).Count;
                _AboveMediumCount = ((Queue)_Connection._SendBuffer[Priority.AboveMedium]).Count;
                _MediumCount      = ((Queue)_Connection._SendBuffer[Priority.Medium]).Count;
                _BelowMediumCount = ((Queue)_Connection._SendBuffer[Priority.BelowMedium]).Count;
                _LowCount         = ((Queue)_Connection._SendBuffer[Priority.Low]).Count;

                if ((_CheckHighBuffer() == true) &&
                    (_CheckAboveMediumBuffer() == true) &&
                    (_CheckMediumBuffer() == true) &&
                    (_CheckBelowMediumBuffer() == true) &&
                    (_CheckLowBuffer() == true)) {
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
                    }

                    if (_LowCount > 1) {
                        return false;
                    }
                }

                return true;
            }
            // END OF WARNING, below this you can read/change again ;)
        }
    }
}

