/**
 * $Id: IrcConnection.cs,v 1.3 2003/11/20 23:08:26 meebey Exp $
 * $Revision: 1.3 $
 * $Author: meebey $
 * $Date: 2003/11/20 23:08:26 $
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
using System.Net.Sockets;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;

namespace SmartIRC
{
    public delegate void ReadLineEventHandler(string data);
    public delegate void WriteLineEventHandler(string data);
    public delegate void ConnectEventHandler();
    public delegate void ConnectingEventHandler();
    public delegate void DisconnectEventHandler();

    public class Connection
    {
        private IrcTcpClient        _TcpClient = new IrcTcpClient();
        private StreamReader        _Reader;
        private StreamWriter        _Writer;
        private string              _Address;
        private int                 _Port;
        private Hashtable           _SendBuffer = new Hashtable();
        private ReadThread          _ReadThread;
        private WriteThread         _WriteThread;
        private int                 _SendDelay = 200;
        private bool                _Registered = false;
        public  Encoding            Encoding = Encoding.GetEncoding(1250);

        public event ReadLineEventHandler   OnReadLine;
        public event WriteLineEventHandler  OnWriteLine;
        public event ConnectingEventHandler OnConnecting;
        public event ConnectEventHandler    OnConnect;
        public event DisconnectEventHandler OnDisconnect;

        public bool Connected
        {
            get {
                return _TcpClient.Socket.Connected;
            }
        }

        public Connection()
        {
            _SendBuffer[Priority.Low]     = Queue.Synchronized(new Queue());
            _SendBuffer[Priority.Medium]  = Queue.Synchronized(new Queue());
            _SendBuffer[Priority.High]    = Queue.Synchronized(new Queue());

            OnReadLine += new ReadLineEventHandler(_SimpleParser);

            _ReadThread  = new ReadThread(this);
            _WriteThread = new WriteThread(this);
        }

        public bool Connect(string address, int port)
        {
            Logger.Connection.Info("connecting");
            _Address = address;
            _Port = port;

            if (OnConnecting != null) {
                OnConnecting();
            }
            try {
                System.Net.IPAddress ip = System.Net.Dns.Resolve(address).AddressList[0];
                _TcpClient.Connect(ip, port);
                if (OnConnect != null) {
                    OnConnect();
                }

                _Reader = new StreamReader(_TcpClient.GetStream(), Encoding);
                _Writer = new StreamWriter(_TcpClient.GetStream(), Encoding);

                _WriteThread.Start();
                Logger.Connection.Info("connected");
                return true;
            } catch {
                if (_Reader != null) {
                    _Reader.Close();
                }
                if (_Writer != null) {
                    _Writer.Close();
                }
                if (_TcpClient != null) {
                    _TcpClient.Close();
                }
                Logger.Connection.Info("connection failed");
                return false;
            }
        }

        public bool Disconnect()
        {
            if (Connected == true) {
                _Reader.Close();
                _Writer.Close();
                _TcpClient.Close();
                if (OnDisconnect != null) {
                    OnDisconnect();
                }
                Logger.Connection.Info("disconnected");
                return true;
            } else {
                return false;
            }
        }

        public void Listen()
        {
            _ReadThread.Start();
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

        private void _WriteLine(string data)
        {
            _Writer.WriteLine(data);
            _Writer.Flush();
            Logger.Socket.Debug("sent: \""+data+"\"");
            if (OnWriteLine != null) {
                OnWriteLine(data);
            }
        }

        private void _SimpleParser(string rawline)
        {
            string[] rawlineex = rawline.Split(new Char[] {' '});
            string messagecode = rawlineex[1];

            switch(messagecode) {
                case "001":
                    _Registered = true;
                    OnReadLine -= new ReadLineEventHandler(_SimpleParser);
                break;
            }
        }

        private class ReadThread
        {
            private Connection  _Connection;
            private Thread      _Thread;
            // syncronized queue (thread safe)
            private Queue       _Queue = Queue.Synchronized(new Queue());

            public Queue Queue
            {
                get {
                    return _Queue;
                }
            }

            public ReadThread(Connection connection)
            {
                _Connection = connection;
            }

            public void Start()
            {
                _Thread = new Thread(new ThreadStart(_Worker));
                _Thread.Start();
            }

            private void _Worker()
            {
                string data = "";
                while ((_Connection.Connected == true) &&
                       ((data = _Connection._Reader.ReadLine()) != null)) {
                    _Queue.Enqueue(data);
                    Logger.Socket.Debug("received: \""+data+"\"");
                }

                Logger.Socket.Warn("detected dead connection (socket returned null)");
                _Connection.Disconnect();
            }
        }

        private class WriteThread
        {
            private Connection  _Connection;
            private Thread      _Thread;
            private int         _BurstCount = 0;
            private int         _HighCount   = 0;
            private int         _MediumCount = 0;
            private int         _LowCount    = 0;
            private int         _HighSentCount   = 0;
            private int         _MediumSentCount = 0;
            private int         _LowSentCount    = 0;
            private int         _HighThresholdCount   = 2;
            private int         _MediumThresholdCount = 1;
            private int         _LowThresholdCount    = 0;

            public WriteThread(Connection connection)
            {
                _Connection = connection;
            }

            public void Start()
            {
                _Thread = new Thread(new ThreadStart(_Worker));
                _Thread.Start();
            }

            // WARNING: complex scheduler, don't even think about changing it!
            private void _CheckBuffer()
            {
                if (!_Connection._Registered) {
                    return;
                }
                
                _HighCount   = ((Queue)_Connection._SendBuffer[Priority.High]).Count;
                _MediumCount = ((Queue)_Connection._SendBuffer[Priority.Medium]).Count;
                _LowCount    = ((Queue)_Connection._SendBuffer[Priority.Low]).Count;

                if ((_CheckHighBuffer() == true) &&
                    (_CheckMediumBuffer() == true) &&
                    (_CheckLowBuffer() == true)) {
                    _HighSentCount = 0;
                    _MediumSentCount = 0;
                    _LowSentCount = 0;
                }

                if (_BurstCount < 3) {
                    _BurstCount++;
                    _CheckBuffer();
                } else {
                    _BurstCount = 0;
                }
            }

            private bool _CheckHighBuffer()
            {
                if ((_HighCount > 0) &&
                    (_HighSentCount < _HighThresholdCount)) {
                    _Connection._WriteLine(((string)((Queue)_Connection._SendBuffer[Priority.High]).Dequeue()));
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
                    _Connection._WriteLine(((string)((Queue)_Connection._SendBuffer[Priority.Medium]).Dequeue()));
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

                    _Connection._WriteLine(((string)((Queue)_Connection._SendBuffer[Priority.Low]).Dequeue()));
                    _LowSentCount++;

                    if (_LowSentCount < _LowThresholdCount) {
                        return false;
                    }
                }

                return true;
            }
            // END OF WARNING, below this you can read/change again ;)

            private void _Worker()
            {
                while (_Connection.Connected == true) {
                    _CheckBuffer();
                    Thread.Sleep(_Connection._SendDelay);
                }

                Logger.Socket.Warn("detected dead connection");
                _Connection.Disconnect();
            }
        }
    }
}

