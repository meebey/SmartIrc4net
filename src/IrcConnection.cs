/**
 * $Id: IrcConnection.cs,v 1.10 2004/07/15 20:51:03 meebey Exp $
 * $Revision: 1.10 $
 * $Author: meebey $
 * $Date: 2004/07/15 20:51:03 $
 *
 * Copyright (c) 2003-2004 Mirco 'meebey' Bauer <mail@meebey.net> <http://www.meebey.net>
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
using Meebey.SmartIrc4net.Delegates;
namespace Meebey.SmartIrc4net
{
    /// <summary>
    ///
    /// </summary>
    public class IrcConnection
    {
        private string          _Version;
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
        private bool            _Registered = false;
        private bool            _Connected  = false;
        private int             _ConnectTries = 0;
        private bool            _AutoRetry     = false;
        private bool            _AutoReconnect = false;
        private bool            _ConnectionError = false;
        private Encoding        _Encoding = Encoding.GetEncoding("ISO-8859-15");
        //private Encoding        _Encoding = Encoding.GetEncoding(1252);
        //private Encoding        _Encoding = Encoding.UTF8;

        public event ReadLineEventHandler   OnReadLine;
        public event WriteLineEventHandler  OnWriteLine;
        public event SimpleEventHandler     OnConnect;
        public event SimpleEventHandler     OnConnected;
        public event SimpleEventHandler     OnDisconnect;
        public event SimpleEventHandler     OnDisconnected;

        protected bool ConnectionError
        {
            get {
                lock (this) {
                    return _ConnectionError;
                }
            }
            set {
                lock (this) {
                    _ConnectionError = value;
                }
            }
        }

        public string Address
        {
            get {
                return _AddressList[_CurrentAddress];
            }
        }

        public string[] AddressList
        {
            get {
                return _AddressList;
            }
        }

        public int Port
        {
            get {
                return _Port;
            }
        }

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

        public int SendDelay
        {
            get {
                return _SendDelay;
            }
            set {
                _SendDelay = value;
            }
        }

        public bool Registered
        {
            get {
                return _Registered;
            }
        }

        public bool Connected
        {
            get {
                return _Connected;
            }
        }

        public string Version
        {
            get {
                return _Version;
            }
        }

        public string VersionString
        {
            get {
                return _VersionString;
            }
        }

        public Encoding Encoding
        {
            get {
                return _Encoding;
            }
            set {
                _Encoding = value;
            }
        }

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

            Assembly assembly = Assembly.GetAssembly(this.GetType());
            AssemblyName assembly_name = assembly.GetName(false);

            AssemblyProductAttribute pr = (AssemblyProductAttribute)assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false)[0];

            _Version = assembly_name.Version.ToString();
            _VersionString = pr.Product+" "+_Version;
        }

        public bool Connect(string[] addresslist, int port)
        {
            if (_Connected != false) {
                throw new Exception("already connected");
            }

#if LOG4NET
            Logger.Connection.Info("connecting...");
#endif
            _ConnectTries++;
            _AddressList = (string[])addresslist.Clone();
            _Port = port;

            if (OnConnect != null) {
                OnConnect();
            }
            try {
                System.Net.IPAddress ip = System.Net.Dns.Resolve(Address).AddressList[0];
                _TcpClient = new IrcTcpClient();
                _TcpClient.Connect(ip, port);
                if (OnConnected != null) {
                    OnConnected();
                }

                _Reader = new StreamReader(_TcpClient.GetStream(), Encoding);
                _Writer = new StreamWriter(_TcpClient.GetStream(), Encoding);

                // Connection was succeful, reseting the connect counter
                _ConnectTries = 0;

                // updating the connection id, so connecting is possible again
                lock(this) {
                    ConnectionError = false;
                }
                _Connected = true;
#if LOG4NET
                Logger.Connection.Info("connected");
#endif

                // lets power up our threads
                _ReadThread.Start();
                _WriteThread.Start();
                return true;
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
                _Connected = false;
                ConnectionError = true;
#if LOG4NET
                Logger.Connection.Info("connection failed: "+e.Message);
#endif
                if (_AutoRetry == true &&
                    _ConnectTries <= 3) {
                    _NextAddress();
                    if (Reconnect(false)) {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool Connect(string address, int port)
        {
            return Connect(new string[] {address}, port);
        }

        // login parameter only for IrcClient needed
        public virtual bool Reconnect(bool login)
        {
#if LOG4NET
            Logger.Connection.Info("reconnecting...");
#endif
            Disconnect();
            return Connect(_AddressList, _Port);
        }

        public bool Reconnect()
        {
            return Reconnect(true);
        }

        public bool Disconnect()
        {
            if (Connected == true) {
#if LOG4NET
                Logger.Connection.Info("disconnecting...");
#endif
                if (OnDisconnect != null) {
                    OnDisconnect();
                }

                _ReadThread.Stop();
                _WriteThread.Stop();
                _TcpClient.Close();
                _Connected = false;
                _Registered = false;

                if (OnDisconnected != null) {
                    OnDisconnected();
                }

#if LOG4NET
                Logger.Connection.Info("disconnected");
#endif
                return true;
            } else {
                return false;
            }
        }

        public void Listen(bool blocking)
        {
            if (blocking) {
                while(Connected == true) {
                    ReadLine(true);
                    if (ConnectionError == true) {
                        if (AutoReconnect == true) {
                            Reconnect();
                        } else {
                            Disconnect();
                        }
                    }
                }
            } else {
                while (ReadLine(false) != String.Empty) {
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
            bool received = false;
            
            if (blocking) {
                // block till the queue has data
                while ((Connected == true) &&
                    (_ReadThread.Queue.Count == 0)) {
                    Thread.Sleep(10);
                }
            }

            if ((Connected == true) &&
                (_ReadThread.Queue.Count > 0)) {
                data = (string)(_ReadThread.Queue.Dequeue());
            }

            if (data != "" && data != null) {
#if LOG4NET
                Logger.Queue.Debug("read: \""+data+"\"");
#endif
                if (OnReadLine != null) {
                    OnReadLine(data);
                }
            }

            return data;
        }

        public void WriteLine(string data, Priority priority)
        {
            if (priority == Priority.Critical) {
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
            if (Connected == true) {
                try {
                    _Writer.Write(data+"\r\n");
                    _Writer.Flush();
                } catch (IOException) {
#if LOG4NET
                    Logger.Socket.Warn("sending data failed, connection lost");
#endif
                    ConnectionError = true;
                    return false;
                }

#if LOG4NET
                Logger.Socket.Debug("sent: \""+data+"\"");
#endif
                if (OnWriteLine != null) {
                    OnWriteLine(data);
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

        private void _SimpleParser(string rawline)
        {
            string messagecode = "";
            string[] rawlineex = rawline.Split(new Char[] {' '});

            if (rawline.Substring(0, 1) == ":") {
                messagecode = rawlineex[1];
                try {
                    ReplyCode replycode = (ReplyCode)int.Parse(messagecode);
                    switch(replycode) {
                        case ReplyCode.RPL_WELCOME:
                            _Registered = true;
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
                        ConnectionError = true;
                    break;
                }
            }
        }

        private class ReadThread
        {
            private IrcConnection  _Connection;
            private Thread         _Thread;
            // syncronized queue (thread safe)
            private Queue       _Queue = Queue.Synchronized(new Queue());

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
                        while ((_Connection.Connected == true) &&
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
                    }

#if LOG4NET
                    Logger.Socket.Warn("connection lost");
#endif
                    _Connection.ConnectionError = true;
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
                    while (_Connection.Connected == true) {
                        _CheckBuffer();
                        Thread.Sleep(_Connection._SendDelay);
                    }

#if LOG4NET
                    Logger.Socket.Warn("connection lost");
#endif
                    _Connection.ConnectionError = true;
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
                if (!_Connection._Registered) {
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
                        // putting the message back into the queue if sending was not successful
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

