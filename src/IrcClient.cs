/**
 * $Id: IrcClient.cs,v 1.10 2004/07/31 22:56:22 meebey Exp $
 * $Revision: 1.10 $
 * $Author: meebey $
 * $Date: 2004/07/31 22:56:22 $
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
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Meebey.SmartIrc4net.Delegates;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    ///
    /// </summary>
    public class IrcClient: IrcCommands
    {
        private string              _Nickname = "";
        private string              _Realname = "";
        private string              _Usermode = "";
        private int                 _IUsermode = 0;
        private string              _Username = "";
        private string              _Password = "";
        private string              _CtcpVersion;
        private bool                _ChannelSyncing = false;
        private bool                _AutoRejoin     = false;
        private Array               _ReplyCodes     = Enum.GetValues(typeof(ReplyCode));
        private StringCollection    _JoinedChannels = new StringCollection();
        private Hashtable           _Channels       = Hashtable.Synchronized(new Hashtable());
        private Hashtable           _IrcUsers       = Hashtable.Synchronized(new Hashtable());

        public event SimpleEventHandler         OnRegistered;
        public event PingEventHandler           OnPing;
        public event MessageEventHandler        OnRawMessage;
        public event ErrorEventHandler          OnError;
        public event JoinEventHandler           OnJoin;
        public event NameReplyEventHandler      OnNameReply;
        public event PartEventHandler           OnPart;
        public event QuitEventHandler           OnQuit;
        public event KickEventHandler           OnKick;
        public event InviteEventHandler         OnInvite;
        public event BanEventHandler            OnBan;
        public event UnbanEventHandler          OnUnban;
        public event OpEventHandler             OnOp;
        public event DeopEventHandler           OnDeop;
        public event VoiceEventHandler          OnVoice;
        public event DevoiceEventHandler        OnDevoice;
        public event WhoEventHandler            OnWho;
        public event TopicEventHandler          OnTopic;
        public event TopicChangeEventHandler    OnTopicChange;
        public event NickChangeEventHandler     OnNickChange;
        public event MessageEventHandler        OnModeChange;
        public event MessageEventHandler        OnUserModeChange;
        public event MessageEventHandler        OnChannelModeChange;
        public event MessageEventHandler        OnChannelMessage;
        public event ActionEventHandler         OnChannelAction;
        public event MessageEventHandler        OnChannelNotice;
        public event MessageEventHandler        OnQueryMessage;
        public event ActionEventHandler         OnQueryAction;
        public event MessageEventHandler        OnQueryNotice;
        public event MessageEventHandler        OnCtcpRequest;
        public event MessageEventHandler        OnCtcpReply;

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
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

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string CtcpVersion
        {
            get {
                return _CtcpVersion;
            }
            set {
                _CtcpVersion = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public bool AutoRejoin
        {
            get {
                return _AutoRejoin;
            }
            set {
#if LOG4NET
                if (value == true) {
                    Logger.ChannelSyncing.Info("AutoRejoin enabled");
                } else {
                    Logger.ChannelSyncing.Info("AutoRejoin disabled");
                }
#endif
                _AutoRejoin = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string Nickname
        {
            get {
                return _Nickname;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string Realname
        {
            get {
                return _Realname;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string Username
        {
            get {
                return _Username;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string Usermode
        {
            get {
                return _Usermode;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public int IUsermode
        {
            get {
                return _IUsermode;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string Password
        {
            get {
                return _Password;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string[] JoinedChannels
        {
            get {
                string[] result;
                result = new string[_JoinedChannels.Count];
                _JoinedChannels.CopyTo(result, 0);
                return result;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        public IrcClient()
        {
#if LOG4NET
            Logger.Main.Debug("IrcClient created");
#endif
            OnReadLine        += new ReadLineEventHandler(_Worker);
        }

        ~IrcClient()
        {
#if LOG4NET
            Logger.Main.Debug("IrcClient destroyed");
            log4net.LogManager.Shutdown();
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"> </param>
        public override bool Reconnect(bool login)
        {
            if(base.Reconnect(true) == true) {
                if (login) {
                    Login(Nickname, Realname, IUsermode, Username, Password);
                }
#if LOG4NET
                Logger.Connection.Info("Rejoining channels...");
#endif
                foreach(string channel in _JoinedChannels) {
                    Join(channel, Priority.High);
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nick"> </param>
        /// <param name="usermode"> </param>        
        /// <param name="username"> </param>        
        /// <param name="password"> </param>        
        public void Login(string nick, string realname, int usermode, string username, string password)
        {
#if LOG4NET
            Logger.Connection.Info("logging in");
#endif

            _Nickname = nick.Replace(" ", "");
            _Realname = realname;
            _IUsermode = usermode;

            if (username != String.Empty) {
                _Username = username.Replace(" ", "");
            } else {
                _Username = Environment.UserName.Replace(" ", "");
            }

            if (password != String.Empty) {
                _Password = password;
                Pass(Password, Priority.Critical);
            }

            Nick(Nickname, Priority.Critical);
            User(Username, IUsermode, Realname, Priority.Critical);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nick"> </param>
        /// <param name="realname"> </param>
        /// <param name="usermode"> </param>
        /// <param name="username"> </param>
        public void Login(string nick, string realname, int usermode, string username)
        {
            Login(nick, realname, usermode, username, "");
        }

        public void Login(string nick, string realname, int usermode)
        {
            Login(nick, realname, usermode, "", "");
        }

        public void Login(string nick, string realname)
        {
            Login(nick, realname, 0, "", "");
        }

        public bool IsMe(string nickname)
        {
            return (Nickname == nickname);
        }

        public bool IsJoined(string channelname)
        {
            return IsJoined(channelname, Nickname);
        }

        public bool IsJoined(string channelname, string nickname)
        {
            if (channelname == null) {
                throw new System.ArgumentNullException("channelname");
            }

            if (nickname == null) {
                throw new System.ArgumentNullException("nickname");
            }
            
            Channel channel = GetChannel(channelname);
            if (channel != null &&
                channel.UnsafeUsers != null &&
                channel.UnsafeUsers.ContainsKey(nickname.ToLower())) {
                return true;
            }
            
            return false;
        }

        public IrcUser GetIrcUser(string nickname)
        {
            if (nickname == null) {
                throw new System.ArgumentNullException("nickname");
            }

            return (IrcUser)_IrcUsers[nickname.ToLower()];
        }

        public ChannelUser GetChannelUser(string channel, string nickname)
        {
            if (channel == null) {
                throw new System.ArgumentNullException("channel");
            }

            if (nickname == null) {
                throw new System.ArgumentNullException("nickname");
            }
            
            Channel ircchannel = GetChannel(channel);
            if (ircchannel != null) {
                return (ChannelUser)ircchannel.UnsafeUsers[nickname.ToLower()];
            } else {
                return null;
            } 
        }

        public Channel GetChannel(string channel)
        {
            if (channel == null) {
                throw new System.ArgumentNullException("channel");
            }
            
            return (Channel)_Channels[channel.ToLower()];
        }

        public string[] GetChannels()
        {
            string[] channels = new string[_Channels.Values.Count];
            int i = 0;
            foreach (Channel channel in _Channels.Values) {
                channels[i++] = channel.Name;
            }

            return channels;
        }
        
        public Data MessageParser(string rawline)
        {
            Data     ircdata;
            string   line;
            string[] lineex;
            string[] rawlineex;
            string   messagecode;
            string   from;
            int      exclamationpos;
            int      atpos;
            int      colonpos;

            rawlineex = rawline.Split(new Char[] {' '});
            ircdata = new Data();
            ircdata.Irc = this;
            ircdata.RawMessage = rawline;
            ircdata.RawMessageEx = rawlineex;
            
            if (rawline.Substring(0, 1) == ":") {
                line = rawline.Substring(1);
            } else {
                line = rawline;
            }

            lineex = line.Split(new char[] {' '});

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

            try {
                ircdata.ReplyCode = (ReplyCode)int.Parse(messagecode);
            } catch (FormatException) {
                ircdata.ReplyCode = ReplyCode.NULL;
            }
            ircdata.Type      = _GetMessageType(rawline);
            ircdata.From      = from;
            if (colonpos != -1) {
                ircdata.Message = line.Substring(colonpos+1);
                ircdata.MessageEx = ircdata.Message.Split(new char[] {' '});
            }

            switch(ircdata.Type) {
                case ReceiveType.Join:
                case ReceiveType.Kick:
                case ReceiveType.Part:
                case ReceiveType.TopicChange:
                case ReceiveType.ChannelModeChange:
                case ReceiveType.ChannelMessage:
                case ReceiveType.ChannelAction:
                case ReceiveType.ChannelNotice:
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
            Logger.MessageParser.Debug("ircdata "+
                                       "nick: '"+ircdata.Nick+"' "+
                                       "ident: '"+ircdata.Ident+"' "+
                                       "host: '"+ircdata.Host+"' "+
                                       "type: '"+ircdata.Type.ToString()+"' "+
                                       "from: '"+ircdata.From+"' "+
                                       "channel: '"+ircdata.Channel+"' "+
                                       "message: '"+ircdata.Message+"' "
                                       );
#endif
            return ircdata;
        }
        
        private void _Worker(string rawline)
        {
            // lets see if we have events or internal messagehandler for it
            _HandleEvents(MessageParser(rawline));
        }

        private ReceiveType _GetMessageType(string rawline)
        {
            Match found;

            found = new Regex("^:[^ ]+? ([0-9]{3}) .+$").Match(rawline);
            if (found.Success) {
                string code = found.Groups[1].Value;
                ReplyCode replycode = (ReplyCode)int.Parse(code);

                // check if this replycode is known in the RFC
                if (Array.IndexOf(_ReplyCodes, replycode) == -1) {
#if LOG4NET
                    Logger.MessageTypes.Warn("This IRC server ("+Address+") doesn't conform to the RFC 2812! ignoring unrecongzied replycode '"+replycode+"'");
#endif
                    return ReceiveType.Unknown;
                }

                switch (replycode) {
                    case ReplyCode.RPL_WELCOME:
                    case ReplyCode.RPL_YOURHOST:
                    case ReplyCode.RPL_CREATED:
                    case ReplyCode.RPL_MYINFO:
                    case ReplyCode.RPL_BOUNCE:
                        return ReceiveType.Login;
                    case ReplyCode.RPL_LUSERCLIENT:
                    case ReplyCode.RPL_LUSEROP:
                    case ReplyCode.RPL_LUSERUNKNOWN:
                    case ReplyCode.RPL_LUSERME:
                    case ReplyCode.RPL_LUSERCHANNELS:
                        return ReceiveType.Info;
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
                    case ReplyCode.RPL_NOTOPIC:
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

            found = new Regex("^PING :.*").Match(rawline);
            if (found.Success) {
                return ReceiveType.Unknown;
            }

            found = new Regex("^ERROR :.*").Match(rawline);
            if (found.Success) {
                return ReceiveType.Error;
            }

            found = new Regex("^:.*? PRIVMSG (.).* :"+(char)1+"ACTION .*"+(char)1+"$").Match(rawline);
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

            found = new Regex("^:.*? PRIVMSG .* :"+(char)1+".*"+(char)1+"$").Match(rawline);
            if (found.Success) {
                return ReceiveType.CtcpRequest;
            }

            found = new Regex("^:.*? PRIVMSG (.).* :.*$").Match(rawline);
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

            found = new Regex("^:.*? NOTICE .* :"+(char)1+".*"+(char)1+"$").Match(rawline);
            if (found.Success) {
                return ReceiveType.CtcpReply;
            }

            found = new Regex("^:.*? NOTICE (.).* :.*$").Match(rawline);
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

            found = new Regex("^:.*? INVITE .* .*$").Match(rawline);
            if (found.Success) {
                return ReceiveType.Invite;
            }

            found = new Regex("^:.*? JOIN .*$").Match(rawline);
            if (found.Success) {
                return ReceiveType.Join;
            }

            found = new Regex("^:.*? TOPIC .* :.*$").Match(rawline);
            if (found.Success) {
                return ReceiveType.TopicChange;
            }

            found = new Regex("^:.*? NICK .*$").Match(rawline);
            if (found.Success) {
                return ReceiveType.NickChange;
            }

            found = new Regex("^:.*? KICK .* .*$").Match(rawline);
            if (found.Success) {
                return ReceiveType.Kick;
            }

            found = new Regex("^:.*? PART .*$").Match(rawline);
            if (found.Success) {
                return ReceiveType.Part;
            }

            found = new Regex("^:.*? MODE (.*) .*$").Match(rawline);
            if (found.Success) {
                if (found.Groups[1].Value == _Nickname) {
                    return ReceiveType.UserModeChange;
                } else {
                    return ReceiveType.ChannelModeChange;
                }
            }

            found = new Regex("^:.*? QUIT :.*$").Match(rawline);
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

            // special IRC messages
            code = ircdata.RawMessageEx[0];
            switch (code) {
                case "PING":
                    _Event_PING(ircdata);
                break;
                case "ERROR":
                    _Event_ERROR(ircdata);
                break;
            }

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

             bool validreplycode = false;
             ReplyCode replycode = ReplyCode.NULL;
             try {
                replycode  = (ReplyCode)int.Parse(code);
                validreplycode = true;
             } catch (FormatException) {
                // nothing, if it's not a number then just skip it
             }

             if (validreplycode) {
                switch (replycode) {
                    case ReplyCode.RPL_WELCOME:
                        _Event_RPL_WELCOME(ircdata);
                    break;
                    case ReplyCode.RPL_TOPIC:
                        _Event_RPL_TOPIC(ircdata);
                    break;
                    case ReplyCode.RPL_NOTOPIC:
                        _Event_RPL_NOTOPIC(ircdata);
                    break;
                    case ReplyCode.RPL_NAMREPLY:
                        _Event_RPL_NAMREPLY(ircdata);
                    break;
                    case ReplyCode.RPL_WHOREPLY:
                        _Event_RPL_WHOREPLY(ircdata);
                    break;
                    case ReplyCode.RPL_CHANNELMODEIS:
                        _Event_RPL_CHANNELMODEIS(ircdata);
                    break;
                    case ReplyCode.ERR_NICKNAMEINUSE:
                        _Event_ERR_NICKNAMEINUSE(ircdata);
                    break;
                }
             }
        }

        private bool _RemoveIrcUser(string nickname)
        {
            if (GetIrcUser(nickname).JoinedChannels.Length == 0) {
                // he is nowhere else, lets kill him
                _IrcUsers.Remove(nickname.ToLower());
                return true;
            }

            return false;
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
            string message = ircdata.Message;
#if LOG4NET
            Logger.Connection.Info("received ERROR from IRC server");
#endif

            if (OnError != null) {
                OnError(message, ircdata);
            }
        }

        private void _Event_JOIN(Data ircdata)
        {
            string who = ircdata.Nick;
            string channelname = ircdata.Channel;

            if (IsMe(who)) {
                _JoinedChannels.Add(channelname);
            }

            if (ChannelSyncing) {
                Channel channel;
                if (IsMe(who)) {
                    // we joined the channel
#if LOG4NET
                    Logger.ChannelSyncing.Debug("joining channel: "+channelname);
#endif
                    channel = new Channel(channelname);
                    _Channels.Add(channelname.ToLower(), channel);
                    Mode(channelname);
                    Who(channelname);
                    Ban(channelname);
                } else {
                    // someone else did
                    Who(who);
                }

#if LOG4NET
                Logger.ChannelSyncing.Debug(who+" joins channel: "+channelname);
#endif
                channel = GetChannel(channelname);
                IrcUser ircuser = GetIrcUser(who);

                if (ircuser == null) {
                    ircuser = new IrcUser(who, this);
                    ircuser.Ident = ircdata.Ident;
                    ircuser.Host  = ircdata.Host;
                    _IrcUsers.Add(who.ToLower(), ircuser);
                }

                ChannelUser channeluser = new ChannelUser(channelname, ircuser);
                channel.UnsafeUsers.Add(who.ToLower(), channeluser);
            }

            if (OnJoin != null) {
                OnJoin(channelname, who, ircdata);
            }
        }

        private void _Event_PART(Data ircdata)
        {
            string who = ircdata.Nick;
            string channel = ircdata.Channel;
            string partmessage = ircdata.Message;

            if (IsMe(who)) {
                _JoinedChannels.Remove(channel);
            }

            if (ChannelSyncing) {
                if (IsMe(who)) {
#if LOG4NET
                    Logger.ChannelSyncing.Debug("parting channel: "+channel);
#endif
                    _Channels.Remove(channel.ToLower());
                } else {
#if LOG4NET
                    Logger.ChannelSyncing.Debug(who+" parts channel: "+channel);
#endif
                    GetChannel(channel).UnsafeUsers.Remove(who.ToLower());
                    _RemoveIrcUser(who);
                }
            }

            if (OnPart != null) {
                OnPart(channel, who, partmessage, ircdata);
            }
        }

        private void _Event_KICK(Data ircdata)
        {
            string channel = ircdata.Channel;
            string victim = ircdata.RawMessageEx[3];
            string who = ircdata.Nick;
            string reason = ircdata.Message;

            if (IsMe(victim)) {
                _JoinedChannels.Remove(channel);
            }

            if (ChannelSyncing) {
                if (IsMe(victim)) {
                    _Channels.Remove(channel.ToLower());
                } else {
                    GetChannel(channel).UnsafeUsers.Remove(victim.ToLower());
                    _RemoveIrcUser(who);
                }
            }

            if (OnKick != null) {
                OnKick(channel, victim, who, reason, ircdata);
            }
        }

        private void _Event_QUIT(Data ircdata)
        {
            string who = ircdata.Nick;
            string reason = ircdata.Message;

            if (IsMe(ircdata.Nick)) {
                foreach (string channel in _JoinedChannels) {
                    _JoinedChannels.Remove(channel);
                }
            }

            if (ChannelSyncing) {
                foreach (string channel in GetIrcUser(who).JoinedChannels) {
                    GetChannel(channel).UnsafeUsers.Remove(who.ToLower());
                }
                _RemoveIrcUser(who);
            }

            if (OnQuit != null) {
                OnQuit(who, reason, ircdata);
            }
        }

        private void _Event_PRIVMSG(Data ircdata)
        {
            if (ircdata.Type == ReceiveType.CtcpRequest) {
                // Substring must be 1,4 because of \001 in CTCP messages
                if (ircdata.Message.Substring(1, 4) == "PING") {
                    Message(SendType.CtcpReply, ircdata.Nick, "PING "+ircdata.Message.Substring(6, (ircdata.Message.Length-7)));
                } else if (ircdata.Message.Substring(1, 7) == "VERSION") {
                    string versionstring;
                    if (_CtcpVersion == null) {
                        versionstring = VersionString;
                    } else {
                        versionstring = _CtcpVersion+" | using "+VersionString;
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
                        string action = ircdata.Message.Substring(7, ircdata.Message.Length-8);
                        OnChannelAction(action, ircdata);
                    }
                break;
                case ReceiveType.QueryMessage:
                    if (OnQueryMessage != null) {
                        OnQueryMessage(ircdata);
                    }
                break;
                case ReceiveType.QueryAction:
                    if (OnQueryAction != null) {
                        string action = ircdata.Message.Substring(7, ircdata.Message.Length-8);
                        OnQueryAction(action, ircdata);
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
            string who = ircdata.Nick;
            string channel = ircdata.Channel;
            string newtopic = ircdata.Message;

            if (ChannelSyncing &&
                IsJoined(channel)) {
                GetChannel(channel).Topic = newtopic;
#if LOG4NET
                Logger.ChannelSyncing.Debug("stored topic for channel: "+channel);
#endif
            }

            if (OnTopicChange != null) {
                OnTopicChange(channel, who, newtopic, ircdata);
            }
        }

        private void _Event_NICK(Data ircdata)
        {
            string oldnickname = ircdata.Nick;
            string newnickname = ircdata.Message;

            if (IsMe(ircdata.Nick)) {
                _Nickname = newnickname;
            }

            if (ChannelSyncing) {
                IrcUser ircuser = GetIrcUser(oldnickname);
                
                // if we don't have any info about him, don't update him!
                // (only queries or ourself in no channels)
                if (ircuser != null) {
                    string[] joinedchannels = ircuser.JoinedChannels;

                    // update his nickname
                    ircuser.Nick = newnickname;
                    // remove the old entry 
                    // remove first to avoid duplication, Foo -> foo
                    _IrcUsers.Remove(oldnickname.ToLower());
                    // add him as new entry and new nickname as key
                    _IrcUsers.Add(newnickname.ToLower(), ircuser);
#if LOG4NET
                    Logger.ChannelSyncing.Debug("updated nickname of: "+oldnickname+" to: "+newnickname);
#endif
                    // now the same for all channels he is joined
                    Channel     channel;
                    ChannelUser channeluser;
                    foreach (string channelname in joinedchannels) {
                        channel     = GetChannel(channelname);
                        channeluser = GetChannelUser(channelname, oldnickname);
                        // remove first to avoid duplication, Foo -> foo
                        channel.UnsafeUsers.Remove(oldnickname.ToLower());
                        channel.UnsafeUsers.Add(newnickname.ToLower(), channeluser);
                    }
                }
            }
            
            if (OnNickChange != null) {
                OnNickChange(oldnickname, newnickname, ircdata);
            }
        }

        private void _Event_INVITE(Data ircdata)
        {
            string channel = ircdata.Channel;
            string inviter = ircdata.Nick;

            if (OnInvite != null) {
                OnInvite(inviter, channel, ircdata);
            }
        }

        private void _Event_MODE(Data ircdata)
        {
            if (IsMe(ircdata.RawMessageEx[2])) {
                // my mode changed
                _Usermode = ircdata.RawMessageEx[3].Substring(1);
            } else {
                string   mode = ircdata.RawMessageEx[3];
                string   parameter = String.Join(" ", ircdata.RawMessageEx, 4, ircdata.RawMessageEx.Length-4);
                string[] parameters = parameter.Split(new Char[] {' '});

                bool add       = false;
                bool remove    = false;
                int modelength = mode.Length;
                string temp;
                Channel channel = null;
                if (ChannelSyncing) {
                    channel = GetChannel(ircdata.Channel);
                }
                IEnumerator parametersEnumerator = parameters.GetEnumerator();
                // bring the enumerator to the 1. element
                parametersEnumerator.MoveNext();
                for (int i = 0; i < modelength; i++) {
                    switch(mode[i]) {
                        case '-':
                            add = false;
                            remove = true;
                        break;
                        case '+':
                            add = true;
                            remove = false;
                        break;
                        case 'o':
                            temp = (string)parametersEnumerator.Current;
                            parametersEnumerator.MoveNext();
                            if (add) {
                                if (ChannelSyncing) {
                                    // update the op list
                                    channel.Ops.Add(temp.ToLower(), GetIrcUser(temp));
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("added op: "+temp+" to: "+ircdata.Channel);
#endif
                                    
                                    // update the user op status
                                    GetChannelUser(ircdata.Channel, temp).IsOp = true;
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("set op status: "+temp+" for: "+ircdata.Channel);
#endif
                                }
                                if (OnOp != null) {
                                    OnOp(ircdata.Channel, ircdata.Nick, temp, ircdata);
                                }
                            }
                            if (remove) {
                                if (ChannelSyncing) {
                                    // update the op list
                                    channel.Ops.Remove(temp.ToLower());
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("removed op: "+temp+" from: "+ircdata.Channel);
#endif
                                    // update the user op status
                                    GetChannelUser(ircdata.Channel, temp).IsOp = false;
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("unset op status: "+temp+" for: "+ircdata.Channel);
#endif
                                }
                                if (OnDeop != null) {
                                    OnDeop(ircdata.Channel, ircdata.Nick, temp, ircdata);
                                }
                            }
                        break;
                        case 'v':
                            temp = (string)parametersEnumerator.Current;
                            parametersEnumerator.MoveNext();
                            if (add) {
                                if (ChannelSyncing) {
                                    // update the voice list
                                    channel.Voices.Add(temp.ToLower(), GetIrcUser(temp));
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("added voice: "+temp+" to: "+ircdata.Channel);
#endif
                                    // update the user voice status
                                    GetChannelUser(ircdata.Channel, temp).IsVoice = true;
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("set voice status: "+temp+" for: "+ircdata.Channel);
#endif
                                }
                                if (OnVoice != null) {
                                    OnVoice(ircdata.Channel, ircdata.Nick, temp, ircdata);
                                }
                            }
                            if (remove) {
                                if (ChannelSyncing) {
                                    // update the voice list
                                    channel.Voices.Remove(temp.ToLower());
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("removed voice: "+temp+" from: "+ircdata.Channel);
#endif
                                    // update the user voice status
                                    GetChannelUser(ircdata.Channel, temp).IsVoice = false;
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("unset voice status: "+temp+" for: "+ircdata.Channel);
#endif
                                }
                                if (OnDevoice != null) {
                                    OnDevoice(ircdata.Channel, ircdata.Nick, temp, ircdata);
                                }
                            }
                        break;
                        case 'b':
                            temp = (string)parametersEnumerator.Current;
                            parametersEnumerator.MoveNext();
                            if (add) {
                                if (ChannelSyncing) {
                                    channel.Bans.Add(temp);
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("added ban: "+temp+" to: "+ircdata.Channel);
#endif
                                }
                                if (OnBan != null) {
                                    OnBan(ircdata.Channel, ircdata.Nick, temp, ircdata);
                                }
                            }
                            if (remove) {
                                if (ChannelSyncing) {
                                    channel.Bans.Remove(temp);
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("removed ban: "+temp+" from: "+ircdata.Channel);
#endif
                                }
                                if (OnUnban != null) {
                                    OnUnban(ircdata.Channel, ircdata.Nick, temp, ircdata);
                                }
                            }
                        break;
                        case 'l':
                            temp = (string)parametersEnumerator.Current;
                            parametersEnumerator.MoveNext();
                            if (add) {
                                if (ChannelSyncing) {
                                    channel.UserLimit = int.Parse(temp);
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("stored user limit for: "+ircdata.Channel);
#endif
                                }
                            }
                            if (remove) {
                                if (ChannelSyncing) {
                                    channel.UserLimit = 0;
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("removed user limit for: "+ircdata.Channel);
#endif
                                }
                            }
                        break;
                        case 'k':
                            temp = (string)parametersEnumerator.Current;
                            parametersEnumerator.MoveNext();
                            if (add) {
                                if (ChannelSyncing) {
                                    channel.Key = temp;
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("stored channel key for: "+ircdata.Channel);
#endif
                                }
                            }
                            if (remove) {
                                if (ChannelSyncing) {
                                    channel.Key = "";
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("removed channel key for: "+ircdata.Channel);
#endif
                                }
                            }
                        break;
                        default:
                            if (add) {
                                if (ChannelSyncing) {
                                    channel.Mode += mode[i];
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("added channel mode ("+mode[i]+") for: "+ircdata.Channel);
#endif
                                }
                            }
                            if (remove) {
                                if (ChannelSyncing) {
                                    channel.Mode = channel.Mode.Replace(mode[i], new char());
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("removed channel mode ("+mode[i]+") for: "+ircdata.Channel);
#endif
                                }
                            }
                        break;
                    }
                }
            }

            
            if ((ircdata.Type == ReceiveType.UserModeChange) &&
                (OnUserModeChange != null)) {
                OnUserModeChange(ircdata);
            }

            if ((ircdata.Type == ReceiveType.ChannelModeChange) &&
                (OnChannelModeChange != null)) {
                OnChannelModeChange(ircdata);
            }

            if (OnModeChange != null) {
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

        private void _Event_RPL_TOPIC(Data ircdata)
        {
            string topic   = ircdata.Message;
            string channel = ircdata.Channel;

            if (ChannelSyncing &&
                IsJoined(channel)) {
                GetChannel(channel).Topic = topic;
#if LOG4NET
                Logger.ChannelSyncing.Debug("stored topic for channel: "+channel);
#endif
            }

            if (OnTopic != null) {
                OnTopic(channel, topic, ircdata);
            }
        }

        private void _Event_RPL_NOTOPIC(Data ircdata)
        {
            string channel = ircdata.Channel;

            if (ChannelSyncing &&
                IsJoined(channel)) {
                GetChannel(channel).Topic = "";
#if LOG4NET
                Logger.ChannelSyncing.Debug("stored empty topic for channel: "+channel);
#endif
            }

            if (OnTopic != null) {
                OnTopic(channel, "", ircdata);
            }
        }

        private void _Event_RPL_NAMREPLY(Data ircdata)
        {
            string   channel  = ircdata.Channel;
            string[] userlist = ircdata.MessageEx;
            if (ChannelSyncing &&
                IsJoined(channel)) {
                string nickname;
                bool   op;
                bool   voice;
                foreach (string user in userlist) {
                    if (user.Length <= 0) {
                        continue;
                    }

                    op = false;
                    voice = false;
                    switch (user[0]) {
                        case '@':
                            op = true;
                            nickname = user.Substring(1);
                        break;
                        case '+':
                            voice = true;
                            nickname = user.Substring(1);
                        break;
                        // RFC VIOLATION
                        // some IRC network do this and break our channel sync...
                        case '&':
                            nickname = user.Substring(1);
                        break;
                        case '%':
                            nickname = user.Substring(1);
                        break;
                        default:
                            nickname = user;
                        break;
                    }

                    IrcUser     ircuser     = GetIrcUser(nickname);
                    ChannelUser channeluser = GetChannelUser(channel, nickname);

                    if (ircuser == null) {
#if LOG4NET
                        Logger.ChannelSyncing.Debug("creating IrcUser: "+nickname+" because he doesn't exist yet");
#endif
                        ircuser = new IrcUser(nickname, this);
                        _IrcUsers.Add(nickname.ToLower(), ircuser);
                    }

                    if (channeluser == null) {
#if LOG4NET
                        Logger.ChannelSyncing.Debug("creating ChannelUser: "+nickname+" for Channel: "+channel+" because he doesn't exist yet");
#endif
                        channeluser = new ChannelUser(channel, ircuser);
                        GetChannel(channel).UnsafeUsers.Add(nickname.ToLower(), channeluser);
                    }

                    channeluser.IsOp    = op;
                    channeluser.IsVoice = voice;
                }
            }
            
            if (OnNameReply != null) {
                OnNameReply(channel, userlist, ircdata);
            }
        }

        private void _Event_RPL_WHOREPLY(Data ircdata)
        {
            string channel  = ircdata.Channel;
            string ident    = ircdata.RawMessageEx[4];
            string host     = ircdata.RawMessageEx[5];
            string server   = ircdata.RawMessageEx[6];
            string nick     = ircdata.RawMessageEx[7];
            string usermode = ircdata.RawMessageEx[8];
            string realname = ircdata.Message.Substring(2);
            int    hopcount = 0;
            string temp     = ircdata.RawMessageEx[9].Substring(1);
            try {
                hopcount = int.Parse(temp);
            } catch (FormatException) {
#if LOG4NET
                Logger.MessageParser.Warn("couldn't parse (as int): '"+temp+"'");
#endif
            }

            bool op = false;
            bool voice = false;
            bool ircop = false;
            bool away = false;
            int usermodelength = usermode.Length;
            for (int i = 0; i < usermodelength; i++) {
                switch (usermode[i]) {
                    case 'H':
                        away = false;
                    break;
                    case 'G':
                        away = true;
                    break;
                    case '@':
                        op = true;
                    break;
                    case '+':
                        voice = true;
                    break;
                    case '*':
                        ircop = true;
                    break;
                }
            }

            if (ChannelSyncing) {
#if LOG4NET
                Logger.ChannelSyncing.Debug("updating userinfo (from whoreply) for user: "+nick+" channel: "+channel);
#endif
                IrcUser ircuser  = GetIrcUser(nick);
                ircuser.Ident    = ident;
                ircuser.Host     = host;
                ircuser.Server   = server;
                ircuser.Nick     = nick;
                ircuser.HopCount = hopcount;
                ircuser.Realname = realname;
                ircuser.IsAway   = away;
                ircuser.IsIrcOp  = ircop;
                
                switch (channel[0]) {
                    case '#':
                    case '!':
                    case '&':
                    case '+':
                        // this channel may not be where we are joined!
                        // see RFC 1459 and RFC 2812, it must return a channelname
                        // we use this channel info when possible...
                        ChannelUser channeluser = GetChannelUser(channel, nick);
                        if (channeluser != null) {
                            channeluser.IsOp    = op;
                            channeluser.IsVoice = voice;
                        }
                    break;
                }
            }

            if (OnWho != null) {
                OnWho(channel, nick, ident, host, realname, away, op, voice, ircop, server, hopcount, ircdata);
            }
        }

        private void _Event_RPL_CHANNELMODEIS(Data ircdata)
        {
            if (ChannelSyncing &&
                IsJoined(ircdata.Channel)) {
                GetChannel(ircdata.Channel).Mode = ircdata.RawMessageEx[4];
            }
        }

        private void _Event_ERR_NICKNAMEINUSE(Data ircdata)
        {
#if LOG4NET
            Logger.Connection.Warn("nickname collision detected, changing nickname");
#endif
            string nickname;
            Random rand = new Random();
            int number = rand.Next();
            if (Nickname.Length > 5) {
                nickname = Nickname.Substring(0, 5)+number;
            } else {
                nickname = Nickname.Substring(0, Nickname.Length-1)+number;
            }

            Nick(nickname, Priority.Critical);
        }

        // </internal messagehandler>
    }
}

