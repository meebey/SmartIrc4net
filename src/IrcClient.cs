/**
 * $Id: IrcClient.cs,v 1.1 2003/11/16 16:58:42 meebey Exp $
 * $Revision: 1.1 $
 * $Author: meebey $
 * $Date: 2003/11/16 16:58:42 $
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

namespace SmartIRC
{
    public delegate void PingEventHandler(string data);

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

        public event PingEventHandler   OnPing;

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

        public IrcClient()
        {
            _Commands = new IrcCommands(_Connection);
            _Connection.OnReadLine += new ReadLineEventHandler(_Parser);

            Logger.Init();
            Logger.Main.Debug("IrcClient created");
        }

        ~IrcClient()
        {
            Logger.Main.Debug("IrcClient destroyed");
            log4net.LogManager.Shutdown();
        }

        public void Connect(string address, int port)
        {
            _Address = address;
            _Port = port;

            _Connection.Connect(_Address, _Port);
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
            ircdata.rawmessage = rawline;
            ircdata.rawmessageex = rawlineex;
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
                    ircdata.nick = from.Substring(0, exclamationpos);
                }
                if ((atpos != -1) &&
                    (exclamationpos != -1)) {
                    ircdata.ident = from.Substring(exclamationpos+1, (atpos - exclamationpos)-1);
                }
                if (atpos != -1) {
                    ircdata.host = from.Substring(atpos+1);
                }
                //ircdata.type = this._gettype(rawline);
                ircdata.from = from;
                if (colonpos != -1) {
                    ircdata.message = line.Substring(colonpos+1);
                    ircdata.messageex = ircdata.message.Split(new Char[] {' '});
                }

                /*
                if ($ircdata->type & (SMARTIRC_TYPE_CHANNEL|
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

                if ($ircdata->channel !== null) {
                    if (substr($ircdata->channel, 0, 1) == ':') {
                        $ircdata->channel = substr($ircdata->channel, 1);
                    }
                }
                */
                Logger.MessageParser.Debug("ircdata nick: \""+ircdata.nick+
                                           "\" ident: \""+ircdata.ident+
                                           "\" host: \""+ircdata.host+
                                           "\" type: \""+ircdata.type+
                                           "\" from: \""+ircdata.from+
                                           "\" channel: \""+ircdata.channel+
                                           "\" message: \""+ircdata.message+
                                           "\"");
            }

            // lets see if we have events or internal messagehandler for it
            _HandleEvents(messagecode, ircdata);

            if (validmessage == true) {
                // now the actionhandlers are comming
               // $this->_handleactionhandler($ircdata);
            }
        }

        private void _HandleEvents(string messagecode, Data ircdata)
        {
            switch(messagecode) {
                case "PING":
                    _rpl_ping(ircdata);
                    if (OnPing != null) {
                        string pongdata = ircdata.rawmessageex[1].Substring(1);
                        OnPing(pongdata);
                    }
                break;
            }
        }

// <internal messagehandler>

        private void _rpl_ping(Data ircdata)
        {
            Logger.Connection.Debug("Ping? Pong!");
            string pongdata = ircdata.rawmessageex[1].Substring(1);
            Commands.Pong(pongdata);
        }

// </internal messagehandler>
    }
}

