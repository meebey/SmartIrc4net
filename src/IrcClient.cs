/**
 * $Id: IrcClient.cs,v 1.3 2003/11/21 23:40:31 meebey Exp $
 * $Revision: 1.3 $
 * $Author: meebey $
 * $Date: 2003/11/21 23:40:31 $
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
using System.Text;
using SmartIRC.Delegates;

namespace SmartIRC
{
    public class IrcClient
    {
        private Connection      _Connection = new Connection();
        private string          _Address;
        private int             _Port;
        private string          _Nick;
        private string          _Realname;
        private string          _Username;
        private string          _Password;
        private IrcCommands     _Commands;
        private bool            _ChannelSyncing;
        private string          _CtcpVersion;
        private bool            _AutoRetry;
        private bool            _AutoReconnect;
        private bool            _AutoRejoin;
        private bool            _Registered = false;

        public event SimpleEventHandler     OnConnect;
        public event SimpleEventHandler     OnDisconnect;
        public event SimpleEventHandler     OnRegistered;
        public event SimpleEventHandler     OnQuit;
        public event PingEventHandler       OnPing;
        public event MessageEventHandler    OnMessage;
        public event MessageEventHandler    OnError;
        public event MessageEventHandler    OnJoin;
        public event MessageEventHandler    OnPart;
        public event MessageEventHandler    OnKick;
        public event MessageEventHandler    OnBan;
        public event MessageEventHandler    OnChannelMessage;
        public event MessageEventHandler    OnChannelAction;
        public event MessageEventHandler    OnQueryMessage;
        public event MessageEventHandler    OnQueryAction;

        public IrcCommands Commands
        {
            get {
                return _Commands;
            }
        }

        public bool Connected
        {
            get {
                return _Connection.Connected;
            }
        }

        public bool ChannelSyncing
        {
            get {
                return _ChannelSyncing;
            }
            set {
                if (value == true) {
                    Logger.ChannelSyncing.Info("Channel syncing enabled");
                } else {
                    Logger.ChannelSyncing.Info("Channel syncing disabled");
                }
                _ChannelSyncing = value;
            }
        }

        public string CtcpVersion
        {
            get {
                return _CtcpVersion;
            }
            set {
                _CtcpVersion = value;
            }
        }

        public bool AutoRetry
        {
            get {
                return _AutoRetry;
            }
            set {
                _AutoRetry = value;
            }
        }

        public bool AutoReconnect
        {
            get {
                return _AutoReconnect;
            }
            set {
                _AutoReconnect = value;
            }
        }

        public bool AutoRejoin
        {
            get {
                return _AutoRejoin;
            }
            set {
                _AutoRejoin = value;
            }
        }

        public bool Registered
        {
            get {
                return _Registered;
            }
        }

        public IrcClient()
        {
            _Commands = new IrcCommands(_Connection);

            if (OnConnect != null) {
                _Connection.OnConnect    += new SimpleEventHandler(OnConnect);
            }
            if (OnDisconnect != null) {
                _Connection.OnDisconnect += new SimpleEventHandler(OnDisconnect);
            }
            _Connection.OnReadLine   += new ReadLineEventHandler(_Parser);

            Logger.Init();
            Logger.Main.Debug("IrcClient created");
        }

        ~IrcClient()
        {
            Logger.Main.Debug("IrcClient destroyed");
            log4net.LogManager.Shutdown();
        }

        public bool Connect(string address, int port)
        {
            _Address = address;
            _Port = port;

            return _Connection.Connect(_Address, _Port);
        }

        public void Login(string nick, string realname, int usermode, string username, string password)
        {
            Logger.Connection.Info("logging in");

            _Nick = nick.Replace(" ", "");
            _Realname = realname;

            if (username != "") {
                _Username = username.Replace(" ", "");
            } else {
                _Username = Environment.UserName.Replace(" ", "");
            }

            if (password != "") {
                _Password = password;
                Commands.Pass(_Password, Priority.Critical);
            }

            Commands.Nick(_Nick, Priority.Critical);
            Commands.User(_Username, usermode, _Realname, Priority.Critical);
        }

        public void Login(string nick, string realname, int usermode, string username)
        {
            this.Login(nick, realname, usermode, username, "");
        }

        public void Login(string nick, string realname, int usermode)
        {
            this.Login(nick, realname, usermode, "", "");
        }

        public void Login(string nick, string realname)
        {
            this.Login(nick, realname, 0, "", "");
        }

        public void Send(string data, Priority priority)
        {
            _Connection.WriteLine(data, priority);
        }

        public void Send(string data)
        {
            _Connection.WriteLine(data);
        }

        public void Listen()
        {
            _Connection.Listen();
        }

        public void ListenOnce()
        {
            _Connection.ListenOnce();
        }

        public void Disconnect()
        {
            _Connection.Disconnect();
        }

        private void _Parser(string rawline)
        {
            string   line;
            string[] lineex;
            string[] rawlineex;
            bool     validmessage = false;
            Data     ircdata;
            string   messagecode;
            string   from;
            int      exclamationpos;
            int      atpos;
            int      colonpos;

            rawlineex = rawline.Split(new Char[] {' '});
            ircdata = new Data();
            ircdata.RawMessage = rawline;
            ircdata.RawMessageEx = rawlineex;
            messagecode = rawlineex[0];

            if (rawline.Substring(0, 1) == ":") {
                validmessage = true;
                line = rawline.Substring(1);
                lineex = line.Split(new Char[] {' '});

                // conform to RFC 2812
                from = lineex[0];
                messagecode = lineex[1];
                exclamationpos = from.IndexOf("!");
                atpos = from.IndexOf("@");
                colonpos = line.IndexOf(" :")+1;

                if (exclamationpos != -1) {
                    ircdata.Nick = from.Substring(0, exclamationpos);
                }
                if ((atpos != -1) &&
                    (exclamationpos != -1)) {
                    ircdata.Ident = from.Substring(exclamationpos+1, (atpos - exclamationpos)-1);
                }
                if (atpos != -1) {
                    ircdata.Host = from.Substring(atpos+1);
                }

                ircdata.Type = _GetMessageType(rawline);
                ircdata.From = from;
                if (colonpos != -1) {
                    ircdata.Message = line.Substring(colonpos+1);
                    ircdata.MessageEx = ircdata.Message.Split(new Char[] {' '});
                }

                switch(ircdata.Type) {
                    case MessageType.Channel:
                        ircdata.Channel = lineex[2];
                    break;
                }

                /*
                if (ircdata->type & (SMARTIRC_TYPE_CHANNEL|
                                SMARTIRC_TYPE_ACTION|
                                SMARTIRC_TYPE_MODECHANGE|
                                SMARTIRC_TYPE_TOPICCHANGE|
                                SMARTIRC_TYPE_KICK|
                                SMARTIRC_TYPE_PART|
                                SMARTIRC_TYPE_JOIN)) {
                    $ircdata->channel = $lineex[2];
                } else if ($ircdata->type & (SMARTIRC_TYPE_WHO|
                                    SMARTIRC_TYPE_BANLIST|
                                    SMARTIRC_TYPE_TOPIC|
                                    SMARTIRC_TYPE_CHANNELMODE)) {
                    $ircdata->channel = $lineex[3];
                } else if ($ircdata->type & SMARTIRC_TYPE_NAME) {
                    $ircdata->channel = $lineex[4];
                }
                */

                if (ircdata.Channel != null) {
                    if (ircdata.Channel.Substring(0, 1) == ":") {
                        ircdata.Channel = ircdata.Channel.Substring(1);
                    }
                }

                Logger.MessageParser.Debug("ircdata nick: \""+ircdata.Nick+
                                           "\" ident: \""+ircdata.Ident+
                                           "\" host: \""+ircdata.Host+
                                           "\" type: \""+ircdata.Type.ToString()+
                                           "\" from: \""+ircdata.From+
                                           "\" channel: \""+ircdata.Channel+
                                           "\" message: \""+ircdata.Message+
                                           "\"");
            }

            // lets see if we have events or internal messagehandler for it
            _HandleEvents(messagecode, ircdata);
        }

        private void _HandleEvents(string messagecode, Data ircdata)
        {
            if (OnMessage != null) {
                OnMessage(ircdata);
            }

            switch(messagecode) {
                case "ERROR":
                    if (OnError != null) {
                        OnError(ircdata);
                    }
                break;
                case "PING":
                    _rpl_ping(ircdata);
                    if (OnPing != null) {
                        string pongdata = ircdata.RawMessageEx[1].Substring(1);
                        OnPing(pongdata);
                    }
                break;
                case "JOIN":
                    if (OnJoin != null) {
                        OnJoin(ircdata);
                    }
                break;
                case "PART":
                    if (OnPart != null) {
                        OnPart(ircdata);
                    }
                break;
                case "KICK":
                    if (OnKick != null) {
                        OnKick(ircdata);
                    }
                break;
                case "001":
                    _rpl_welcome(ircdata);
                    if (OnRegistered != null) {
                        OnRegistered();
                    }
                break;
            }
        }

        public MessageType _GetMessageType(string rawline)
        {
            return MessageType.Channel;
        }

// <internal messagehandler>

        private void _rpl_welcome(Data ircdata)
        {
            _Registered = true;
            Logger.Connection.Info("logged in");
            // updating our nickname, that we got (maybe cutted...)
            _Nick = ircdata.RawMessageEx[2];
        }

        private void _rpl_ping(Data ircdata)
        {
            Logger.Connection.Debug("Ping? Pong!");
            string pongdata = ircdata.RawMessageEx[1].Substring(1);
            Commands.Pong(pongdata);
        }

// </internal messagehandler>
    }
}

