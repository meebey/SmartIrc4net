/**
 * $Id: IrcClient.cs,v 1.4 2003/11/27 23:23:55 meebey Exp $
 * $Revision: 1.4 $
 * $Author: meebey $
 * $Date: 2003/11/27 23:23:55 $
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
using System.Collections.Specialized;
using Meebey.SmartIrc4net.Delegates;

namespace Meebey.SmartIrc4net
{
    public class IrcClient: IrcCommands
    {
        private string              _Nick = "";
        private string              _Realname = "";
        private int                 _Usermode = 0;
        private string              _Username = "";
        private string              _Password = "";
        private bool                _ChannelSyncing;
        private string              _CtcpVersion;
        private bool                _AutoRejoin;
        private bool                _Registered = false;
        private StringCollection    _JoinedChannels = new StringCollection();

        public event SimpleEventHandler     OnRegistered;
        public event PingEventHandler       OnPing;
        public event MessageEventHandler    OnMessage;
        public event MessageEventHandler    OnError;
        public event MessageEventHandler    OnJoin;
        public event MessageEventHandler    OnPart;
        public event MessageEventHandler    OnQuit;
        public event MessageEventHandler    OnKick;
        public event MessageEventHandler    OnBan;
        public event MessageEventHandler    OnChannelMessage;
        public event MessageEventHandler    OnChannelAction;
        public event MessageEventHandler    OnQueryMessage;
        public event MessageEventHandler    OnQueryAction;

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
            OnReadLine        += new ReadLineEventHandler(_Parser);

            Logger.Init();
            Logger.Main.Debug("IrcClient created");
        }

        ~IrcClient()
        {
            Logger.Main.Debug("IrcClient destroyed");
            log4net.LogManager.Shutdown();
        }

        public override bool Reconnect()
        {
            if(base.Reconnect() == true) {
                Login(_Nick, _Realname, _Usermode, _Username, _Password);
                foreach(string Channel in _JoinedChannels) {
                    Join(Channel);
                }
                return true;
            }

            return false;
        }

        public void Login(string nick, string realname, int usermode, string username, string password)
        {
            Logger.Connection.Info("logging in");

            _Nick = nick.Replace(" ", "");
            _Realname = realname;
            _Usermode = usermode;

            if (username != "") {
                _Username = username.Replace(" ", "");
            } else {
                _Username = Environment.UserName.Replace(" ", "");
            }

            if (password != "") {
                _Password = password;
                Pass(_Password, Priority.Critical);
            }

            Nick(_Nick, Priority.Critical);
            User(_Username, _Usermode, _Realname, Priority.Critical);
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
            WriteLine(data, priority);
        }

        public void Send(string data)
        {
            WriteLine(data);
        }

        private void _Parser(string rawline)
        {
            string   line;
            string[] lineex;
            string[] rawlineex;
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
                    case ReceiveType.Join:
                    case ReceiveType.Kick:
                    case ReceiveType.Part:
                    case ReceiveType.ModeChange:
                    case ReceiveType.TopicChange:
                    case ReceiveType.ChannelMessage:
                    case ReceiveType.ChannelAction:
                        ircdata.Channel = lineex[2];
                    break;
                    case ReceiveType.Who:
                    case ReceiveType.Topic:
                    case ReceiveType.BanList:
                    case ReceiveType.ChannelMode:
                        ircdata.Channel = lineex[3];
                    break;
                    case ReceiveType.Name:
                        ircdata.Channel = lineex[4];
                    break;
                }

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
                    _rpl_join(ircdata);
                    if (OnJoin != null) {
                        OnJoin(ircdata);
                    }
                break;
                case "PART":
                    _rpl_part(ircdata);
                    if (OnPart != null) {
                        OnPart(ircdata);
                    }
                break;
                case "KICK":
                    _rpl_kick(ircdata);
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

        private ReceiveType _GetMessageType(string rawline)
        {
            return ReceiveType.ChannelMessage;
        }

// <internal messagehandler>

        private void _rpl_welcome(Data ircdata)
        {
            _Registered = true;
            // updating our nickname, that we got (maybe cutted...)
            _Nick = ircdata.RawMessageEx[2];
        }

        private void _rpl_ping(Data ircdata)
        {
            Logger.Connection.Debug("Ping? Pong!");
            string pongdata = ircdata.RawMessageEx[1].Substring(1);
            Pong(pongdata);
        }

        private void _rpl_join(Data ircdata)
        {
            _JoinedChannels.Add(ircdata.Channel);
        }

        private void _rpl_part(Data ircdata)
        {
            _JoinedChannels.Remove(ircdata.Channel);
        }

        private void _rpl_kick(Data ircdata)
        {
            _JoinedChannels.Remove(ircdata.Channel);
        }

// </internal messagehandler>
    }
}

