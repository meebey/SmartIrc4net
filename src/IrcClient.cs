/**
 * $Id: IrcClient.cs,v 1.5 2003/12/14 12:41:46 meebey Exp $
 * $Revision: 1.5 $
 * $Author: meebey $
 * $Date: 2003/12/14 12:41:46 $
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
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Meebey.SmartIrc4net.Delegates;

namespace Meebey.SmartIrc4net
{
    public class IrcClient: IrcCommands
    {
        private string              _Nickname = "";
        private string              _Realname = "";
        private string              _Usermode = "";
        private int                 _IUsermode = 0;
        private string              _Username = "";
        private string              _Password = "";
        private bool                _ChannelSyncing;
        private string              _CtcpVersion;
        private bool                _AutoRejoin;
        private StringCollection    _JoinedChannels = new StringCollection();
        private StringDictionary    _Channels       = new StringDictionary();
        private StringDictionary    _IrcUsers       = new StringDictionary();

        public event SimpleEventHandler     OnRegistered;
        public event PingEventHandler       OnPing;
        public event MessageEventHandler    OnRawMessage;
        public event MessageEventHandler    OnError;
        public event JoinEventHandler       OnJoin;
        public event MessageEventHandler    OnPart;
        public event MessageEventHandler    OnQuit;
        public event KickEventHandler       OnKick;
        public event MessageEventHandler    OnInvite;
        public event MessageEventHandler    OnBan;
        public event MessageEventHandler    OnUnban;
        public event MessageEventHandler    OnOp;
        public event MessageEventHandler    OnDeop;
        public event MessageEventHandler    OnVoice;
        public event MessageEventHandler    OnDevoice;
        public event MessageEventHandler    OnWho;
        public event MessageEventHandler    OnWhois;
        public event MessageEventHandler    OnWhowas;
        public event MessageEventHandler    OnTopic;
        public event MessageEventHandler    OnTopicChange;
        public event MessageEventHandler    OnNickChange;
        public event MessageEventHandler    OnModeChange;
        public event MessageEventHandler    OnChannelMessage;
        public event MessageEventHandler    OnChannelAction;
        public event MessageEventHandler    OnChannelNotice;
        public event MessageEventHandler    OnQueryMessage;
        public event MessageEventHandler    OnQueryAction;
        public event MessageEventHandler    OnQueryNotice;
        public event MessageEventHandler    OnCtcpRequest;
        public event MessageEventHandler    OnCtcpReply;

        public bool ChannelSyncing
        {
            get {
                return _ChannelSyncing;
            }
            set {
#if LOG4NET
                if (value == true) {
                    Logger.ChannelSyncing.Info("Channel syncing enabled");
                } else {
                    Logger.ChannelSyncing.Info("Channel syncing disabled");
                }
#endif
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

        public string Nickname
        {
            get {
                return _Nickname;
            }
        }

        public string Realname
        {
            get {
                return _Realname;
            }
        }

        public string Username
        {
            get {
                return _Username;
            }
        }

        public string Password
        {
            get {
                return _Password;
            }
        }

        public IrcClient()
        {
            OnReadLine        += new ReadLineEventHandler(_Parser);

#if LOG4NET
            Logger.Init();
            Logger.Main.Debug("IrcClient created");
#endif
        }

        ~IrcClient()
        {
#if LOG4NET
            Logger.Main.Debug("IrcClient destroyed");
            log4net.LogManager.Shutdown();
#endif
        }

        public override bool Reconnect()
        {
            if(base.Reconnect() == true) {
                Login(_Nickname, _Realname, _IUsermode, _Username, _Password);
                foreach(string Channel in _JoinedChannels) {
                    Join(Channel, Priority.High);
                }
                return true;
            }

            return false;
        }

        public void Login(string nick, string realname, int usermode, string username, string password)
        {
#if LOG4NET
            Logger.Connection.Info("logging in");
#endif

            _Nickname = nick.Replace(" ", "");
            _Realname = realname;

            if (username != "") {
                _Username = username.Replace(" ", "");
            } else {
                _Username = Environment.UserName.Replace(" ", "");
            }

            if (password != "") {
                _Password = password;
                Pass(_Password, Priority.Critical);
            }

            Nick(_Nickname, Priority.Critical);
            User(_Username, _IUsermode, _Realname, Priority.Critical);
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

        public bool isMe(string nickname)
        {
            return (_Nickname == nickname);
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
                colonpos = line.IndexOf(" :");
                if (colonpos != -1) {
                    // we want the exact position of ":" not beginning from the space
                    colonpos += 1;
                }

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

#if LOG4NET
                Logger.MessageParser.Debug("ircdata nick: \""+ircdata.Nick+
                                           "\" ident: \""+ircdata.Ident+
                                           "\" host: \""+ircdata.Host+
                                           "\" type: \""+ircdata.Type.ToString()+
                                           "\" from: \""+ircdata.From+
                                           "\" channel: \""+ircdata.Channel+
                                           "\" message: \""+ircdata.Message+
                                           "\"");
#endif
            }

            // lets see if we have events or internal messagehandler for it
            _HandleEvents(ircdata);
        }

        private ReceiveType _GetMessageType(string rawline)
        {
            Match found;

            found = new Regex("^:.* ([0-9]{3}) .*$/").Match(rawline);
            if (found.Success) {
                string code = found.Groups[1].Value;
                ReplyCode replycode = (ReplyCode)int.Parse(code);

                switch (replycode) {
                    case ReplyCode.RPL_WELCOME:
                    case ReplyCode.RPL_YOURHOST:
                    case ReplyCode.RPL_CREATED:
                    case ReplyCode.RPL_MYINFO:
                    case ReplyCode.RPL_BOUNCE:
                        return ReceiveType.Login;
                    case ReplyCode.RPL_MOTDSTART:
                    case ReplyCode.RPL_MOTD:
                    case ReplyCode.RPL_ENDOFMOTD:
                        return ReceiveType.Motd;
                    case ReplyCode.RPL_NAMREPLY:
                    case ReplyCode.RPL_ENDOFNAMES:
                        return ReceiveType.Name;
                    case ReplyCode.RPL_WHOREPLY:
                    case ReplyCode.RPL_ENDOFWHO:
                        return ReceiveType.Who;
                    case ReplyCode.RPL_LISTSTART:
                    case ReplyCode.RPL_LIST:
                    case ReplyCode.RPL_LISTEND:
                        return ReceiveType.List;
                    case ReplyCode.RPL_BANLIST:
                    case ReplyCode.RPL_ENDOFBANLIST:
                        return ReceiveType.BanList;
                    case ReplyCode.RPL_TOPIC:
                        return ReceiveType.Topic;
                    case ReplyCode.RPL_WHOISUSER:
                    case ReplyCode.RPL_WHOISSERVER:
                    case ReplyCode.RPL_WHOISOPERATOR:
                    case ReplyCode.RPL_WHOISIDLE:
                    case ReplyCode.RPL_ENDOFWHOIS:
                    case ReplyCode.RPL_WHOISCHANNELS:
                        return ReceiveType.Whois;
                    case ReplyCode.RPL_WHOWASUSER:
                    case ReplyCode.RPL_ENDOFWHOWAS:
                        return ReceiveType.Whowas;
                    case ReplyCode.RPL_UMODEIS:
                        return ReceiveType.UserMode;
                    case ReplyCode.RPL_CHANNELMODEIS:
                        return ReceiveType.ChannelMode;
                    case ReplyCode.ERR_NICKNAMEINUSE:
                    case ReplyCode.ERR_NOTREGISTERED:
                        return ReceiveType.Error;
                    default:
#if LOG4NET
                        Logger.MessageTypes.Warn("replycode unknown ("+code+"): \""+rawline+"\"");
#endif
                        return ReceiveType.Unknown;
                }
            }

            found = new Regex("^:.* PRIVMSG (.).* :"+(char)1+"ACTION .*"+(char)1+"$").Match(rawline);
            if (found.Success) {
                switch (found.Groups[1].Value) {
                    case "#":
                    case "!":
                    case "&":
                    case "+":
                        return ReceiveType.ChannelAction;
                    default:
                        return ReceiveType.QueryAction;
                }
            }

            found = new Regex("^:.* PRIVMSG .* :"+(char)1+".*"+(char)1+"$").Match(rawline);
            if (found.Success) {
                return ReceiveType.CtcpRequest;
            }

            found = new Regex("^:.* PRIVMSG (.).* :.*$").Match(rawline);
            if (found.Success) {
                switch (found.Groups[1].Value) {
                    case "#":
                    case "!":
                    case "&":
                    case "+":
                        return ReceiveType.ChannelMessage;
                    default:
                        return ReceiveType.QueryMessage;
                }
            }

            found = new Regex("^:.* NOTICE .* :"+(char)1+".*"+(char)1+"$").Match(rawline);
            if (found.Success) {
                return ReceiveType.CtcpReply;
            }

            found = new Regex("^:.* NOTICE (.).* :.*$").Match(rawline);
            if (found.Success) {
                switch (found.Groups[1].Value) {
                    case "#":
                    case "!":
                    case "&":
                    case "+":
                        return ReceiveType.ChannelNotice;
                    default:
                        return ReceiveType.QueryNotice;
                }
            }

            found = new Regex("^:.* INVITE .* .*$").Match(rawline);
            if (found.Success) {
                return ReceiveType.Invite;
            }

            found = new Regex("^:.* JOIN .*$").Match(rawline);
            if (found.Success) {
                return ReceiveType.Join;
            }

            found = new Regex("^:.* TOPIC .* :.*$").Match(rawline);
            if (found.Success) {
                return ReceiveType.TopicChange;
            }

            found = new Regex("^:.* NICK .*$").Match(rawline);
            if (found.Success) {
                return ReceiveType.NickChange;
            }

            found = new Regex("^:.* KICK .* .*$").Match(rawline);
            if (found.Success) {
                return ReceiveType.Kick;
            }

            found = new Regex("^:.* PART .*$").Match(rawline);
            if (found.Success) {
                return ReceiveType.Part;
            }

            found = new Regex("^:.* MODE .* .*$").Match(rawline);
            if (found.Success) {
                return ReceiveType.ModeChange;
            }

            found = new Regex("^:.* QUIT :.*$").Match(rawline);
            if (found.Success) {
                return ReceiveType.Quit;
            }

#if LOG4NET
            Logger.MessageTypes.Warn("messagetype unknown: \""+rawline+"\"");
#endif
            return ReceiveType.Unknown;
        }

        private void _HandleEvents(Data ircdata)
        {
            string code;
            if (OnRawMessage != null) {
                OnRawMessage(ircdata);
            }

            if (ircdata.RawMessage.Substring(0, 1) == ":") {
                code = ircdata.RawMessageEx[1];
                switch (code) {
                    case "PRIVMSG":
                        _Event_PRIVMSG(ircdata);
                    break;
                    case "NOTICE":
                        _Event_NOTICE(ircdata);
                    break;
                    case "JOIN":
                        _Event_JOIN(ircdata);
                    break;
                    case "PART":
                        _Event_PART(ircdata);
                    break;
                    case "KICK":
                        _Event_KICK(ircdata);
                    break;
                    case "QUIT":
                        _Event_QUIT(ircdata);
                    break;
                    case "TOPIC":
                        _Event_TOPIC(ircdata);
                    break;
                    case "NICK":
                        _Event_NICK(ircdata);
                    break;
                    case "INVITE":
                        _Event_INVITE(ircdata);
                    break;
                    case "MODE":
                        _Event_MODE(ircdata);
                    break;
                }

                try {
                    ReplyCode replycode = (ReplyCode)int.Parse(code);
                    switch (replycode) {
                        case ReplyCode.RPL_WELCOME:
                            _Event_RPL_WELCOME(ircdata);
                        break;
                    }
                } catch {
                    // nothing
                }
            } else {
                // it's not a normal IRC message
                code = ircdata.RawMessageEx[0];
                switch (code) {
                    case "PING":
                        _Event_PING(ircdata);
                    break;
                    case "ERROR":
                        _Event_ERROR(ircdata);
                    break;
                }
            }
        }

// <internal messagehandler>

        private void _Event_PING(Data ircdata)
        {
            string pongdata = ircdata.RawMessageEx[1].Substring(1);
#if LOG4NET
            Logger.Connection.Debug("Ping? Pong!");
#endif
            Pong(pongdata);

            if (OnPing != null) {
                OnPing(pongdata);
            }
        }

        private void _Event_ERROR(Data ircdata)
        {
            if (OnError != null) {
                OnError(ircdata);
            }
        }

        private void _Event_JOIN(Data ircdata)
        {
            string who = ircdata.Nick;
            string channel = ircdata.Channel;

            if (isMe(who)) {
                _JoinedChannels.Add(channel);
            }

            if (_ChannelSyncing) {
                /*
                if (isMe(who)) {
                    // we joined the channel
                    _Channels[channel] = Channel();
                } else {
                    // someone else did
                }
                */
            }

            if (OnJoin != null) {
                OnJoin(channel, who, ircdata);
            }
        }

        private void _Event_PART(Data ircdata)
        {
            if (isMe(ircdata.Nick)) {
                _JoinedChannels.Remove(ircdata.Channel);
            }

            if (OnPart != null) {
                OnPart(ircdata);
            }
        }

        private void _Event_KICK(Data ircdata)
        {
            string channel = ircdata.Channel;
            string victim = ircdata.RawMessageEx[3];
            string who = ircdata.Nick;
            string reason = ircdata.Message;

            if (isMe(victim)) {
                _JoinedChannels.Remove(channel);
            }

            if (OnKick != null) {
                OnKick(channel, victim, who, reason, ircdata);
            }
        }

        private void _Event_QUIT(Data ircdata)
        {
            if (isMe(ircdata.Nick)) {
                _JoinedChannels.Remove(ircdata.Channel);
            }

            if (OnQuit != null) {
                OnQuit(ircdata);
            }
        }

        private void _Event_PRIVMSG(Data ircdata)
        {
            if (ircdata.Type == ReceiveType.CtcpRequest) {
                // Substring must be 1,4 because of \001 in CTCP messages
                if (ircdata.Message.Substring(1, 4) == "PING") {
                    Message(SendType.CtcpReply, ircdata.Nick, "PING "+ircdata.Message.Substring(6, (ircdata.Message.Length-7)));
                } else if (ircdata.Message.Substring(1, 7) == "VERSION") {
                    string versionstring = Info.VersionString;
                    if (_CtcpVersion != null) {
                        versionstring = _CtcpVersion+" | using "+Info.VersionString;
                    }
                    Message(SendType.CtcpReply, ircdata.Nick, "VERSION "+versionstring);
                } else if (ircdata.Message.Substring(1, 10) == "CLIENTINFO") {
                    Message(SendType.CtcpReply, ircdata.Nick, "CLIENTINFO PING VERSION CLIENTINFO");
                }
            }

            switch (ircdata.Type) {
                case ReceiveType.ChannelMessage:
                    if (OnChannelMessage != null) {
                        OnChannelMessage(ircdata);
                    }
                break;
                case ReceiveType.ChannelAction:
                    if (OnChannelAction != null) {
                        OnChannelAction(ircdata);
                    }
                break;
                case ReceiveType.QueryMessage:
                    if (OnQueryMessage != null) {
                        OnQueryMessage(ircdata);
                    }
                break;
                case ReceiveType.QueryAction:
                    if (OnQueryAction != null) {
                        OnQueryAction(ircdata);
                    }
                break;
                case ReceiveType.CtcpRequest:
                    if (OnCtcpRequest != null) {
                        OnCtcpRequest(ircdata);
                    }
                break;
            }
        }

        private void _Event_NOTICE(Data ircdata)
        {
            switch (ircdata.Type) {
                case ReceiveType.ChannelNotice:
                    if (OnChannelNotice != null) {
                        OnChannelNotice(ircdata);
                    }
                break;
                case ReceiveType.QueryNotice:
                    if (OnQueryNotice != null) {
                        OnQueryNotice(ircdata);
                    }
                break;
                case ReceiveType.CtcpReply:
                    if (OnCtcpReply != null) {
                        OnCtcpReply(ircdata);
                    }
                break;
            }
        }

        private void _Event_TOPIC(Data ircdata)
        {
            if (OnTopicChange != null) {
                OnTopicChange(ircdata);
            }
        }

        private void _Event_NICK(Data ircdata)
        {
            if (OnNickChange != null) {
                OnNickChange(ircdata);
            }
        }

        private void _Event_INVITE(Data ircdata)
        {
            if (OnInvite != null) {
                OnInvite(ircdata);
            }
        }

        private void _Event_MODE(Data ircdata)
        {
            if (isMe(ircdata.RawMessageEx[2])) {
                _Usermode = ircdata.RawMessageEx[3].Substring(1);
            } else {
                string   mode = ircdata.RawMessageEx[3];
                string   parameter = String.Join(" ", ircdata.RawMessageEx, 4, ircdata.RawMessageEx.Length-4);
                string[] parameters = parameter.Split(new Char[] {' '});

                bool add       = false;
                bool remove    = false;
                int modelength = mode.Length;
                string temp;
                IEnumerator parametersEnumerator = parameters.GetEnumerator();
                // bring the enumerator to the 1. element
                parametersEnumerator.MoveNext();

                for (int i = 0; i < modelength; i++) {
                    switch(mode[i]) {
                        case '-':
                            remove = true;
                            add = false;
                        break;
                        case '+':
                            add = true;
                            remove = false;
                        break;
                        case 'o':
                            temp = (string)parametersEnumerator.Current;
                            parametersEnumerator.MoveNext();
                            if (add) {
                                if (OnOp != null) {
                                    OnOp(ircdata);
                                }
                            }
                            if (remove) {
                                if (OnDeop != null) {
                                    OnDeop(ircdata);
                                }
                            }
                        break;
                        case 'v':
                            temp = (string)parametersEnumerator.Current;
                            parametersEnumerator.MoveNext();
                            if (add) {
                                if (OnVoice != null) {
                                    OnVoice(ircdata);
                                }
                            }
                            if (remove) {
                                if (OnDevoice != null) {
                                    OnDevoice(ircdata);
                                }
                            }
                        break;
                        case 'b':
                            temp = (string)parametersEnumerator.Current;
                            parametersEnumerator.MoveNext();
                            if (add) {
                                if (OnBan != null) {
                                    OnBan(ircdata);
                                }
                            }
                            if (remove) {
                                if (OnUnban != null) {
                                    OnUnban(ircdata);
                                }
                            }
                        break;
                        case 'l':
                            parametersEnumerator.MoveNext();
                        break;
                        case 'k':
                            parametersEnumerator.MoveNext();
                        break;
                    }
                }
            }

            if (OnModeChange!= null) {
                OnModeChange(ircdata);
            }
        }

        private void _Event_RPL_WELCOME(Data ircdata)
        {
            // updating our nickname, that we got (maybe cutted...)
            _Nickname = ircdata.RawMessageEx[2];

            if (OnRegistered != null) {
                OnRegistered();
            }
        }

// </internal messagehandler>
    }
}

