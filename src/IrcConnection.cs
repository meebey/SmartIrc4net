/**
 * $Id: IrcConnection.cs,v 1.5 2003/11/27 23:21:01 meebey Exp $
 * $Revision: 1.5 $
 * $Author: meebey $
 * $Date: 2003/11/27 23:21:01 $
 *
 * Copyright (c) 2003 Mirco 'meebey' Bauer <mail@meebey.net> <http://www.meebey.net>
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
using Meebey.SmartIrc4net.Delegates;

namespace Meebey.SmartIrc4net
{
    public class IrcConnection
    {
        private string              _Address = "localhost";
        private int                 _Port = 6667;
        private StreamReader        _Reader;
        private StreamWriter        _Writer;
        private ReadThread          _ReadThread;
        private WriteThread         _WriteThread;
        private IrcTcpClient        _TcpClient;
        private Hashtable           _SendBuffer = new Hashtable();
        private int                 _SendDelay = 200;
        private bool                _Registered = false;
        private bool                _Connected = false;
        private int                 _ConnectTries = 0;
        private bool                _AutoRetry = false;
        private bool                _AutoReconnect = false;
        private int                 _CurrentConnectionId = 1;
        private int                 _ConnectionId = 0;
        public  Encoding            Encoding = Encoding.GetEncoding(1250);

        public event ReadLineEventHandler   OnReadLine;
        public event WriteLineEventHandler  OnWriteLine;
        public event SimpleEventHandler     OnConnect;
        public event SimpleEventHandler     OnConnected;
        public event SimpleEventHandler     OnDisconnect;
        public event SimpleEventHandler     OnDisconnected;

        public bool Connected
        {
            get {
                return _Connected;
            }
        }

        public bool AutoReconnect
        {
            get {
                return _AutoReconnect;
            }
            set {
                if (value == true) {
                    Logger.Connection.Info("AutoReconnect enabled");
                } else {
                    Logger.Connection.Info("AutoReconnect disabled");
                }
                _AutoReconnect = value;
            }
        }

        public bool AutoRetry
        {
            get {
                return _AutoRetry;
            }
            set {
                if (value == true) {
                    Logger.Connection.Info("AutoRetry enabled");
                } else {
                    Logger.Connection.Info("AutoRetry disabled");
                }
                _AutoRetry = value;
            }
        }

        public int CurrentConnectionId
        {
            get {
                lock(this) {
                    return _CurrentConnectionId;
                }
            }
            set {
                lock(this) {
                    _CurrentConnectionId = value;
                }
            }
        }

        public int ConnectionId
        {
            get {
                lock(this) {
                    return _ConnectionId;
                }
            }
            set {
                lock(this) {
                    _ConnectionId = value;
                }
            }
        }

        public IrcConnection()
        {
            Thread.CurrentThread.Name = "Main";
            _SendBuffer[Priority.Low]     = Queue.Synchronized(new Queue());
            _SendBuffer[Priority.Medium]  = Queue.Synchronized(new Queue());
            _SendBuffer[Priority.High]    = Queue.Synchronized(new Queue());

            OnReadLine += new ReadLineEventHandler(_SimpleParser);

            _ReadThread  = new ReadThread(this);
            _WriteThread = new WriteThread(this);
        }

        public bool Connect(string address, int port)
        {
            if (_Connected != false) {
                throw new Exception("already connected");
            }

            Logger.Connection.Info("connecting...");
            _ConnectTries++;
            _Address = address;
            _Port = port;

            if (OnConnect != null) {
                OnConnect();
            }
            try {
                System.Net.IPAddress ip = System.Net.Dns.Resolve(address).AddressList[0];
                _TcpClient = new IrcTcpClient();
                _TcpClient.Connect(ip, port);
                if (OnConnected != null) {
                    OnConnected();
                }

                _Reader = new StreamReader(_TcpClient.GetStream(), Encoding);
                _Writer = new StreamWriter(_TcpClient.GetStream(), Encoding);

                _ReadThread.Start();
                _WriteThread.Start();

                // Connection was succeful, reseting the connect counter
                _ConnectTries = 0;

                // updating the connection id, so connecting is possible again
                lock(this) {
                    ConnectionId = _CurrentConnectionId;
                    _ReadThread.ConnectionId = _CurrentConnectionId;
                    _WriteThread.ConnectionId = _CurrentConnectionId;
                }
                _Connected = true;
                Logger.Connection.Info("connected");
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
                Logger.Connection.Info("connection failed", e);
                if (_AutoRetry == true &&
                    _ConnectTries <= 3) {
                    Reconnect();
                }
                return false;
            }
        }

        public virtual bool Reconnect()
        {
            Logger.Connection.Info("reconnecting...");
            Disconnect();
            return Connect(_Address, _Port);
        }

        public bool Disconnect()
        {
            if (Connected == true) {
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

                Logger.Connection.Info("disconnected");
                return true;
            } else {
                return false;
            }
        }

        public void Listen()
        {
            while((Connected == true) &&
                  (ReadLine() != null)) {
                  // ReadLine does the work...
            }
        }

        public void ListenOnce()
        {
            ReadLine();
        }

        public string ReadLine()
        {
            string data = "";

            // block till the queue has data
            while ((Connected == true) &&
                   (_ReadThread.Queue.Count == 0)) {
                Thread.Sleep(10);
            }

            if (Connected == true) {
                data = (string)(_ReadThread.Queue.Dequeue());
            }

            if (data != "" && data != null) {
                Logger.Queue.Debug("read: \""+data+"\"");
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
                    _Writer.WriteLine(data);
                    _Writer.Flush();
                } catch (IOException) {
                    Logger.Socket.Warn("sending data failed, connection lost");
                    if ((_AutoReconnect == true) &&
                        (ConnectionId == CurrentConnectionId)) {
                        CurrentConnectionId++;
                        Reconnect();
                    }
                    return false;
                }

                Logger.Socket.Debug("sent: \""+data+"\"");
                if (OnWriteLine != null) {
                    OnWriteLine(data);
                }
                return true;
            }

            return false;
        }

        private void _SimpleParser(string rawline)
        {
            string messagecode = "";
            string[] rawlineex = rawline.Split(new Char[] {' '});

            if (rawline.Substring(0, 1) == ":") {
                messagecode = rawlineex[1];
                switch(messagecode) {
                    case "001":
                        _Registered = true;
                        Logger.Connection.Info("logged in");
                    break;
                }
            } else {
                messagecode = rawlineex[0];
                switch(messagecode) {
                    case "ERROR":
                        if (_AutoReconnect == true) {
                            if (ConnectionId == CurrentConnectionId) {
                                CurrentConnectionId++;
                                Reconnect();
                            }
                        } else {
                            if (ConnectionId == CurrentConnectionId) {
                                CurrentConnectionId++;
                                Disconnect();
                            }
                        }
                    break;
                }
            }
        }

        private class ReadThread
        {
            private IrcConnection  _Connection;
            private int            _ConnectionId = 0;
            private Thread         _Thread;
            // syncronized queue (thread safe)
            private Queue       _Queue = Queue.Synchronized(new Queue());

            public Queue Queue
            {
                get {
                    return _Queue;
                }
            }

            public int ConnectionId
            {
                get {
                    lock(this) {
                        return _ConnectionId;
                    }
                }
                set {
                    lock(this) {
                        _ConnectionId = value;
                    }
                }
            }

            public ReadThread(IrcConnection connection)
            {
                _Connection = connection;
            }

            public void Start()
            {
                _Thread = new Thread(new ThreadStart(_Worker));
                _Thread.Name = "ReadThread";
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
                try {
                    string data = "";
                    while ((_Connection.Connected == true) &&
                           ((data = _Connection._Reader.ReadLine()) != null)) {
                        _Queue.Enqueue(data);
                        Logger.Socket.Debug("received: \""+data+"\"");
                    }

                    Logger.Socket.Warn("connection lost");
                    if(_Connection.AutoReconnect == true) {
                        if (ConnectionId == _Connection.CurrentConnectionId) {
                            _Connection.CurrentConnectionId++;
                            _Connection.Reconnect();
                        }
                    } else {
                        if (ConnectionId == _Connection.CurrentConnectionId) {
                            _Connection.CurrentConnectionId++;
                            _Connection.Disconnect();
                        }
                    }
                } catch (ThreadAbortException) {
                    Thread.ResetAbort();
                    Logger.Socket.Debug("ReadThread aborted");
                }
            }
        }

        private class WriteThread
        {
            private IrcConnection  _Connection;
            private int            _ConnectionId = 0;
            private Thread         _Thread;
            private int            _BurstCount = 0;
            private int            _HighCount   = 0;
            private int            _MediumCount = 0;
            private int            _LowCount    = 0;
            private int            _HighSentCount   = 0;
            private int            _MediumSentCount = 0;
            private int            _LowSentCount    = 0;
            private int            _HighThresholdCount   = 2;
            private int            _MediumThresholdCount = 1;
            private int            _LowThresholdCount    = 0;

            public int ConnectionId
            {
                get {
                    lock(this) {
                        return _ConnectionId;
                    }
                }
                set {
                    lock(this) {
                        _ConnectionId = value;
                    }
                }
            }

            public WriteThread(IrcConnection connection)
            {
                _Connection = connection;
            }

            public void Start()
            {
                _Thread = new Thread(new ThreadStart(_Worker));
                _Thread.Name = "WriteThread";
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
                try {
                    while (_Connection.Connected == true) {
                        _CheckBuffer();
                        Thread.Sleep(_Connection._SendDelay);
                    }

                    Logger.Socket.Warn("connection lost");
                    if (_Connection.AutoReconnect == true) {
                        // check if someone (a thread) is already doing a new connection
                        if (ConnectionId == _Connection.CurrentConnectionId) {
                            _Connection.CurrentConnectionId++;
                            _Connection.Reconnect();
                        }
                    } else {
                        if (ConnectionId == _Connection.CurrentConnectionId) {
                            _Connection.CurrentConnectionId++;
                            _Connection.Disconnect();
                        }
                    }
                } catch (ThreadAbortException) {
                    Thread.ResetAbort();
                    Logger.Socket.Debug("WriteThread aborted");
                }
            }

            // WARNING: complex scheduler, don't even think about changing it!
            private void _CheckBuffer()
            {
                // only send data if we are succefully registered on the IRC network
                if (!_Connection._Registered) {
                    return;
                }

                _HighCount   = ((Queue)_Connection._SendBuffer[Priority.High]).Count;
                _MediumCount = ((Queue)_Connection._SendBuffer[Priority.Medium]).Count;
                _LowCount    = ((Queue)_Connection._SendBuffer[Priority.Low]).Count;

                if ((_CheckHighBuffer() == true) &&
                    (_CheckMediumBuffer() == true) &&
                    (_CheckLowBuffer() == true)) {
                    // everything is sent, resetting all counters
                    _HighSentCount = 0;
                    _MediumSentCount = 0;
                    _LowSentCount = 0;
                    _BurstCount = 0;
                }

                if (_BurstCount < 3) {
                    _BurstCount++;
                    //_CheckBuffer();
                }
            }

            private bool _CheckHighBuffer()
            {
                if ((_HighCount > 0) &&
                    (_HighSentCount < _HighThresholdCount)) {
                    string data = (string)((Queue)_Connection._SendBuffer[Priority.High]).Dequeue();
                    if (_Connection._WriteLine(data) == false) {
                        ((Queue)_Connection._SendBuffer[Priority.High]).Enqueue(data);
                    }
                    _HighSentCount++;

                    if (_HighSentCount < _HighThresholdCount) {
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

            private bool _CheckLowBuffer()
            {
                if ((_LowCount > 0) &&
                    (_LowSentCount <= _LowThresholdCount)) {
                    if ((_LowThresholdCount == 0) &&
                         ((_HighCount > 0) ||
                          (_MediumCount > 0))) {
                            return true;
                    }

                    string data = (string)((Queue)_Connection._SendBuffer[Priority.Low]).Dequeue();
                    if (_Connection._WriteLine(data) == false) {
                        ((Queue)_Connection._SendBuffer[Priority.Low]).Enqueue(data);
                    }
                    _LowSentCount++;

                    if (_LowSentCount < _LowThresholdCount) {
                        return false;
                    }
                }

                return true;
            }
            // END OF WARNING, below this you can read/change again ;)
        }
    }
}

