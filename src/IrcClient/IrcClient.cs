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
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    ///
    /// </summary>
    public class IrcClient : IrcCommands
    {
        private string           _Nickname = "";
        private string           _Realname = "";
        private string           _Usermode = "";
        private int              _IUsermode = 0;
        private string           _Username = "";
        private string           _Password = "";
        private string           _CtcpVersion;
        private bool             _ActiveChannelSyncing  = false;
        private bool             _PassiveChannelSyncing = false;
        private bool             _AutoRejoin         = false;
        private StringCollection _AutoRejoinChannels = new StringCollection();
        private bool             _AutoRelogin    = false;
        private bool             _SupportNonRfc  = false;
        private bool             _SupportNonRfcLocked = false;
        private StringCollection _Motd = new StringCollection();
        private bool             _MotdReceived = false;
        private Array            _ReplyCodes     = Enum.GetValues(typeof(ReplyCode));
        private StringCollection _JoinedChannels = new StringCollection();
        private Hashtable        _Channels       = Hashtable.Synchronized(new Hashtable(new CaseInsensitiveHashCodeProvider(), new CaseInsensitiveComparer()));
        private Hashtable        _IrcUsers       = Hashtable.Synchronized(new Hashtable(new CaseInsensitiveHashCodeProvider(), new CaseInsensitiveComparer()));
        private static Regex     _ReplyCodeRegex   = new Regex("^:[^ ]+? ([0-9]{3}) .+$", RegexOptions.Compiled);
        private static Regex     _PingRegex        = new Regex("^PING :.*", RegexOptions.Compiled);
        private static Regex     _ErrorRegex       = new Regex("^ERROR :.*", RegexOptions.Compiled);
        private static Regex     _ActionRegex      = new Regex("^:.*? PRIVMSG (.).* :"+"\x1"+"ACTION .*"+"\x1"+"$", RegexOptions.Compiled);
        private static Regex     _CtcpRequestRegex = new Regex("^:.*? PRIVMSG .* :"+"\x1"+".*"+"\x1"+"$", RegexOptions.Compiled);
        private static Regex     _MessageRegex     = new Regex("^:.*? PRIVMSG (.).* :.*$", RegexOptions.Compiled);
        private static Regex     _CtcpReplyRegex   = new Regex("^:.*? NOTICE .* :"+"\x1"+".*"+"\x1"+"$", RegexOptions.Compiled);
        private static Regex     _NoticeRegex      = new Regex("^:.*? NOTICE (.).* :.*$", RegexOptions.Compiled);
        private static Regex     _InviteRegex      = new Regex("^:.*? INVITE .* .*$", RegexOptions.Compiled);
        private static Regex     _JoinRegex        = new Regex("^:.*? JOIN .*$", RegexOptions.Compiled);
        private static Regex     _TopicRegex       = new Regex("^:.*? TOPIC .* :.*$", RegexOptions.Compiled);
        private static Regex     _NickRegex        = new Regex("^:.*? NICK .*$", RegexOptions.Compiled);
        private static Regex     _KickRegex        = new Regex("^:.*? KICK .* .*$", RegexOptions.Compiled);
        private static Regex     _PartRegex        = new Regex("^:.*? PART .*$", RegexOptions.Compiled);
        private static Regex     _ModeRegex        = new Regex("^:.*? MODE (.*) .*$", RegexOptions.Compiled);
        private static Regex     _QuitRegex        = new Regex("^:.*? QUIT :.*$", RegexOptions.Compiled);

        public event EventHandler               OnRegistered;
        public event PingEventHandler           OnPing;
        public event IrcEventHandler            OnRawMessage;
        public event ErrorEventHandler          OnError;
        public event IrcEventHandler            OnErrorMessage;
        public event JoinEventHandler           OnJoin;
        public event NamesEventHandler          OnNames;
        public event PartEventHandler           OnPart;
        public event QuitEventHandler           OnQuit;
        public event KickEventHandler           OnKick;
        public event InviteEventHandler         OnInvite;
        public event BanEventHandler            OnBan;
        public event UnbanEventHandler          OnUnban;
        public event OpEventHandler             OnOp;
        public event DeopEventHandler           OnDeop;
        public event HalfopEventHandler         OnHalfop;
        public event DehalfopEventHandler       OnDehalfop;
        public event VoiceEventHandler          OnVoice;
        public event DevoiceEventHandler        OnDevoice;
        public event WhoEventHandler            OnWho;
        public event MotdEventHandler           OnMotd;
        public event TopicEventHandler          OnTopic;
        public event TopicChangeEventHandler    OnTopicChange;
        public event NickChangeEventHandler     OnNickChange;
        public event IrcEventHandler            OnModeChange;
        public event IrcEventHandler            OnUserModeChange;
        public event IrcEventHandler            OnChannelModeChange;
        public event IrcEventHandler            OnChannelMessage;
        public event ActionEventHandler         OnChannelAction;
        public event IrcEventHandler            OnChannelNotice;
        public event IrcEventHandler            OnChannelActiveSynced;
        public event IrcEventHandler            OnChannelPassiveSynced;
        public event IrcEventHandler            OnQueryMessage;
        public event ActionEventHandler         OnQueryAction;
        public event IrcEventHandler            OnQueryNotice;
        public event IrcEventHandler            OnCtcpRequest;
        public event IrcEventHandler            OnCtcpReply;

        /// <summary>
        /// Enables/disables the active channel sync feature
        /// </summary>
        /// <value>true, to enable </value>
        public bool ActiveChannelSyncing
        {
            get {
                return _ActiveChannelSyncing;
            }
            set {
#if LOG4NET
                if (value == true) {
                    Logger.ChannelSyncing.Info("Active channel syncing enabled");
                } else {
                    Logger.ChannelSyncing.Info("Active channel syncing disabled");
                }
#endif
                _ActiveChannelSyncing = value;
            }
        }

        /*
        /// <summary>
        /// Enables/disables the passive channel sync feature
        /// </summary>
        /// <value>true, to enable </value>
        public bool PassiveChannelSyncing
        {
            get {
                return _PassiveChannelSyncing;
            }
            set {
#if LOG4NET
                if (value == true) {
                    Logger.ChannelSyncing.Info("Passive channel syncing enabled");
                } else {
                    Logger.ChannelSyncing.Info("Passive channel syncing disabled");
                }
#endif
                _PassiveChannelSyncing = value;
            }
        }
        */
        
        /// <summary>
        /// Sets the ctcp version that should be replied on ctcp version request
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
        public bool AutoRelogin
        {
            get {
                return _AutoRelogin;
            }
            set {
#if LOG4NET
                if (value == true) {
                    Logger.ChannelSyncing.Info("AutoRelogin enabled");
                } else {
                    Logger.ChannelSyncing.Info("AutoRelogin disabled");
                }
#endif
                _AutoRelogin = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public bool SupportNonRfc
        {
            get {
                return _SupportNonRfc;
            }
            set {
#if LOG4NET
                if (_SupportNonRfcLocked) {
                    return;
                }
                
                if (value == true) {
                    Logger.ChannelSyncing.Info("SupportNonRfc enabled");
                } else {
                    Logger.ChannelSyncing.Info("SupportNonRfc disabled");
                }
#endif
                _SupportNonRfc = value;
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
        public StringCollection JoinedChannels
        {
            get {
                return _JoinedChannels;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public StringCollection Motd
        {
            get {
                return _Motd;
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
            OnDisconnected    += new EventHandler(_OnDisconnected);
            OnConnectionError += new EventHandler(_OnConnectionError);
        }

#if LOG4NET
        ~IrcClient()
        {
            Logger.Main.Debug("IrcClient destroyed");
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        public new void Connect(string[] addresslist, int port)
        {
            _SupportNonRfcLocked = true;
            base.Connect(addresslist, port);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="login">if the login data should be sent, after successful connect</pararm>
        /// <param name="channels">if the channels should be rejoined, after successful connect</param>
        public void Reconnect(bool login, bool channels)
        {
            if (channels) {
                _StoreChannelsToRejoin();
            }
            base.Reconnect();
            if (login) {
                Login(Nickname, Realname, IUsermode, Username, Password);
            }
            if (channels) {
                _RejoinChannels();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="login"> </param>
        public void Reconnect(bool login)
        {
            Reconnect(login, true);
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

            if (username != null && username.Length > 0) {
                _Username = username.Replace(" ", "");
            } else {
                _Username = Environment.UserName.Replace(" ", "");
            }

            if (password != null && password.Length > 0) {
                _Password = password;
                RfcPass(Password, Priority.Critical);
            }

            RfcNick(Nickname, Priority.Critical);
            RfcUser(Username, IUsermode, Realname, Priority.Critical);
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
                channel.UnsafeUsers.ContainsKey(nickname)) {
                return true;
            }
            
            return false;
        }

        public IrcUser GetIrcUser(string nickname)
        {
            if (nickname == null) {
                throw new System.ArgumentNullException("nickname");
            }

            return (IrcUser)_IrcUsers[nickname];
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
                return (ChannelUser)ircchannel.UnsafeUsers[nickname];
            } else {
                return null;
            } 
        }

        public Channel GetChannel(string channel)
        {
            if (channel == null) {
                throw new System.ArgumentNullException("channel");
            }
            
            return (Channel)_Channels[channel];
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
        
        public IrcMessageData MessageParser(string rawline)
        {
            string         line;
            string[]       linear;
            string         messagecode;
            string         from;
            string         nick = null;
            string         ident = null;
            string         host = null;
            string         channel = null;
            string         message = null;
            ReceiveType    type;
            ReplyCode      replycode;
            int            exclamationpos;
            int            atpos;
            int            colonpos;
           
            if (rawline[0] == ':') {
                line = rawline.Substring(1);
            } else {
                line = rawline;
            }

            linear = line.Split(new char[] {' '});

            // conform to RFC 2812
            from = linear[0];
            messagecode = linear[1];
            exclamationpos = from.IndexOf("!");
            atpos = from.IndexOf("@");
            colonpos = line.IndexOf(" :");
            if (colonpos != -1) {
                // we want the exact position of ":" not beginning from the space
                colonpos += 1;
            }
            if (exclamationpos != -1) {
                nick = from.Substring(0, exclamationpos);
            }
            if ((atpos != -1) &&
                (exclamationpos != -1)) {
                ident = from.Substring(exclamationpos+1, (atpos - exclamationpos)-1);
            }
            if (atpos != -1) {
                host = from.Substring(atpos+1);
            }

            try {
                replycode = (ReplyCode)int.Parse(messagecode);
            } catch (FormatException) {
                replycode = ReplyCode.Null;
            }
            type = _GetMessageType(rawline);
            if (colonpos != -1) {
                message = line.Substring(colonpos+1);
            }

            switch (type) {
                case ReceiveType.Join:
                case ReceiveType.Kick:
                case ReceiveType.Part:
                case ReceiveType.TopicChange:
                case ReceiveType.ChannelModeChange:
                case ReceiveType.ChannelMessage:
                case ReceiveType.ChannelAction:
                case ReceiveType.ChannelNotice:
                    channel = linear[2];
                break;
                case ReceiveType.Who:
                case ReceiveType.Topic:
                case ReceiveType.Invite:
                case ReceiveType.BanList:
                case ReceiveType.ChannelMode:
                    channel = linear[3];
                break;
                case ReceiveType.Name:
                    channel = linear[4];
                break;
            }

            if ((channel != null) &&
                (channel[0] == ':')) {
                    channel = channel.Substring(1);
            }

            IrcMessageData data;
            data = new IrcMessageData(this, from, nick, ident, host, channel, message, rawline, type, replycode);
#if LOG4NET
            Logger.MessageParser.Debug("IrcMessageData "+
                                       "nick: '"+data.Nick+"' "+
                                       "ident: '"+data.Ident+"' "+
                                       "host: '"+data.Host+"' "+
                                       "type: '"+data.Type.ToString()+"' "+
                                       "from: '"+data.From+"' "+
                                       "channel: '"+data.Channel+"' "+
                                       "message: '"+data.Message+"' "
                                       );
#endif
            return data;
        }
        
        private void _Worker(object sender, ReadLineEventArgs e)
        {
            // lets see if we have events or internal messagehandler for it
            _HandleEvents(MessageParser(e.Line));
        }

        private void _OnDisconnected(object sender, EventArgs e)
        {
            if (AutoRejoin) {
                _StoreChannelsToRejoin();
            }
            _SyncingCleanup();
        }
        
        private void _OnConnectionError(object sender, EventArgs e)
        {
            // AutoReconnect is handled in IrcConnection._OnConnectionError
            if (AutoRelogin) {
                Login(Nickname, Realname, IUsermode, Username, Password);
            }
            if (AutoRejoin) {
                _RejoinChannels();
            }
        }
        
        private void _StoreChannelsToRejoin()
        {
#if LOG4NET
            Logger.Connection.Info("Storing channels for rejoin...");
#endif
            foreach (string channel in _JoinedChannels) {
                _AutoRejoinChannels.Add(channel);
            }
        }
        
        private void _RejoinChannels()
        {
#if LOG4NET
            Logger.Connection.Info("Rejoining channels...");
#endif
            foreach(string channel in _AutoRejoinChannels) {
                RfcJoin(channel, Priority.High);
            }
            _AutoRejoinChannels.Clear();
        }
        
        private ReceiveType _GetMessageType(string rawline)
        {
            Match found = _ReplyCodeRegex.Match(rawline);
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
                    case ReplyCode.Welcome:
                    case ReplyCode.YourHost:
                    case ReplyCode.Created:
                    case ReplyCode.MyInfo:
                    case ReplyCode.Bounce:
                        return ReceiveType.Login;
                    case ReplyCode.LuserClient:
                    case ReplyCode.LuserOp:
                    case ReplyCode.LuserUnknown:
                    case ReplyCode.LuserMe:
                    case ReplyCode.LuserChannels:
                        return ReceiveType.Info;
                    case ReplyCode.MotdStart:
                    case ReplyCode.Motd:
                    case ReplyCode.EndOfMotd:
                        return ReceiveType.Motd;
                    case ReplyCode.NamesReply:
                    case ReplyCode.EndOfNames:
                        return ReceiveType.Name;
                    case ReplyCode.WhoReply:
                    case ReplyCode.EndOfWho:
                        return ReceiveType.Who;
                    case ReplyCode.ListStart:
                    case ReplyCode.List:
                    case ReplyCode.ListEnd:
                        return ReceiveType.List;
                    case ReplyCode.BanList:
                    case ReplyCode.EndOfBanList:
                        return ReceiveType.BanList;
                    case ReplyCode.Topic:
                    case ReplyCode.NoTopic:
                        return ReceiveType.Topic;
                    case ReplyCode.WhoIsUser:
                    case ReplyCode.WhoIsServer:
                    case ReplyCode.WhoIsOperator:
                    case ReplyCode.WhoIsIdle:
                    case ReplyCode.WhoIsChannels:
                    case ReplyCode.EndOfWhoIs:
                        return ReceiveType.WhoIs;
                    case ReplyCode.WhoWasUser:
                    case ReplyCode.EndOfWhoWas:
                        return ReceiveType.WhoWas;
                    case ReplyCode.UserModeIs:
                        return ReceiveType.UserMode;
                    case ReplyCode.ChannelModeIs:
                        return ReceiveType.ChannelMode;
                    default:
                        if (((int)replycode >= 400) &&
                            ((int)replycode <= 599)) {
                            return ReceiveType.ErrorMessage;
                        } else {
#if LOG4NET
                            Logger.MessageTypes.Warn("replycode unknown ("+code+"): \""+rawline+"\"");
#endif
                            return ReceiveType.Unknown;
                        }                        
                }
            }

            found = _PingRegex.Match(rawline);
            if (found.Success) {
                return ReceiveType.Unknown;
            }

            found = _ErrorRegex.Match(rawline);
            if (found.Success) {
                return ReceiveType.Error;
            }

            found = _ActionRegex.Match(rawline);
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

            found = _CtcpRequestRegex.Match(rawline);
            if (found.Success) {
                return ReceiveType.CtcpRequest;
            }

            found = _MessageRegex.Match(rawline);
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

            found = _CtcpReplyRegex.Match(rawline);
            if (found.Success) {
                return ReceiveType.CtcpReply;
            }

            found = _NoticeRegex.Match(rawline);
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

            found = _InviteRegex.Match(rawline);
            if (found.Success) {
                return ReceiveType.Invite;
            }

            found = _JoinRegex.Match(rawline);
            if (found.Success) {
                return ReceiveType.Join;
            }

            found = _TopicRegex.Match(rawline);
            if (found.Success) {
                return ReceiveType.TopicChange;
            }

            found = _NickRegex.Match(rawline);
            if (found.Success) {
                return ReceiveType.NickChange;
            }

            found = _KickRegex.Match(rawline);
            if (found.Success) {
                return ReceiveType.Kick;
            }

            found = _PartRegex.Match(rawline);
            if (found.Success) {
                return ReceiveType.Part;
            }

            found = _ModeRegex.Match(rawline);
            if (found.Success) {
                if (found.Groups[1].Value == _Nickname) {
                    return ReceiveType.UserModeChange;
                } else {
                    return ReceiveType.ChannelModeChange;
                }
            }

            found = _QuitRegex.Match(rawline);
            if (found.Success) {
                return ReceiveType.Quit;
            }

#if LOG4NET
            Logger.MessageTypes.Warn("messagetype unknown: \""+rawline+"\"");
#endif
            return ReceiveType.Unknown;
        }
        
        private void _SyncingCleanup()
        {
            // lets clean it baby, powered by Mr. Proper
#if LOG4NET
            Logger.ChannelSyncing.Debug("Mr. Proper action, cleaning good...");
#endif
            _JoinedChannels.Clear();
            if (ActiveChannelSyncing) {
                _Channels.Clear();
                _IrcUsers.Clear();
            }
        }

        private void _HandleEvents(IrcMessageData ircdata)
        {
            if (OnRawMessage != null) {
                OnRawMessage(this, new IrcEventArgs(ircdata));
            }

            string code;
            // special IRC messages
            code = ircdata.RawMessageArray[0];
            switch (code) {
                case "PING":
                    _Event_PING(ircdata);
                break;
                case "ERROR":
                    _Event_ERROR(ircdata);
                break;
            }

            code = ircdata.RawMessageArray[1];
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

            if (ircdata.ReplyCode != ReplyCode.Null) {
                switch (ircdata.ReplyCode) {
                    case ReplyCode.Welcome:
                        _Event_RPL_WELCOME(ircdata);
                    break;
                    case ReplyCode.Topic:
                        _Event_RPL_TOPIC(ircdata);
                    break;
                    case ReplyCode.NoTopic:
                        _Event_RPL_NOTOPIC(ircdata);
                    break;
                    case ReplyCode.NamesReply:
                        _Event_RPL_NAMREPLY(ircdata);
                    break;
                    case ReplyCode.EndOfNames:
                        _Event_RPL_ENDOFNAMES(ircdata);
                    break;
                    case ReplyCode.WhoReply:
                        _Event_RPL_WHOREPLY(ircdata);
                    break;
                    case ReplyCode.ChannelModeIs:
                        _Event_RPL_CHANNELMODEIS(ircdata);
                    break;
                    case ReplyCode.BanList:
                        _Event_RPL_BANLIST(ircdata);
                    break;
                    case ReplyCode.EndOfBanList:
                        _Event_RPL_ENDOFBANLIST(ircdata);
                    break;
                    case ReplyCode.Motd:
                        _Event_RPL_MOTD(ircdata);
                    break;
                    case ReplyCode.EndOfMotd:
                        _Event_RPL_ENDOFMOTD(ircdata);
                    break;
                    case ReplyCode.ErrorNicknameInUse:
                        _Event_ERR_NICKNAMEINUSE(ircdata);
                    break;
                }
            }
            
            if (ircdata.Type == ReceiveType.ErrorMessage) {
                _Event_ERR(ircdata);
            }
        }

        private bool _RemoveIrcUser(string nickname)
        {
            if (GetIrcUser(nickname).JoinedChannels.Length == 0) {
                // he is nowhere else, lets kill him
                _IrcUsers.Remove(nickname);
                return true;
            }

            return false;
        }
        
        private void _RemoveChannelUser(string channelname, string nickname)
        {
            Channel chan = GetChannel(channelname);
            chan.UnsafeUsers.Remove(nickname);
            chan.UnsafeOps.Remove(nickname);
            chan.UnsafeVoices.Remove(nickname);
            if (SupportNonRfc) {
                NonRfcChannel nchan = (NonRfcChannel)chan;
                nchan.UnsafeHalfops.Remove(nickname);
            } 
        }

        private void _InterpretChannelMode(IrcMessageData ircdata, string mode, string parameter)
        {
            string[] parameters = parameter.Split(new char[] {' '});
            bool add       = false;
            bool remove    = false;
            int modelength = mode.Length;
            string temp;
            Channel channel = null;
            if (ActiveChannelSyncing) {
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
                            if (ActiveChannelSyncing) {
                                // update the op list
                                try {
                                    channel.UnsafeOps.Add(temp, GetIrcUser(temp));
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("added op: "+temp+" to: "+ircdata.Channel);
#endif
                                } catch (ArgumentException) {
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("duplicate op: "+temp+" in: "+ircdata.Channel+" not added");
#endif
                                }
                                
                                // update the user op status
                                GetChannelUser(ircdata.Channel, temp).IsOp = true;
#if LOG4NET
                                Logger.ChannelSyncing.Debug("set op status: "+temp+" for: "+ircdata.Channel);
#endif
                            }
                            if (OnOp != null) {
                                OnOp(this, new OpEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                            }
                        }
                        if (remove) {
                            if (ActiveChannelSyncing) {
                                // update the op list
                                channel.UnsafeOps.Remove(temp);
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
                                OnDeop(this, new DeopEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                            }
                        }
                    break;
                    case 'h':
                        if (SupportNonRfc) {
                            temp = (string)parametersEnumerator.Current;
                            parametersEnumerator.MoveNext();
                            if (add) {
                                if (ActiveChannelSyncing) {
                                    // update the halfop list
                                    try {
                                        ((NonRfcChannel)channel).UnsafeHalfops.Add(temp, GetIrcUser(temp));
#if LOG4NET
                                        Logger.ChannelSyncing.Debug("added halfop: "+temp+" to: "+ircdata.Channel);
#endif
                                    } catch (ArgumentException) {
#if LOG4NET
                                        Logger.ChannelSyncing.Debug("duplicate halfop: "+temp+" in: "+ircdata.Channel+" not added");
#endif
                                    }
                                    
                                    // update the user halfop status
                                    ((NonRfcChannelUser)GetChannelUser(ircdata.Channel, temp)).IsHalfop = true;
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("set halfop status: "+temp+" for: "+ircdata.Channel);
#endif
                                }
                                if (OnHalfop != null) {
                                    OnHalfop(this, new HalfopEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                                }
                            }
                            if (remove) {
                                if (ActiveChannelSyncing) {
                                    // update the halfop list
                                    ((NonRfcChannel)channel).UnsafeHalfops.Remove(temp);
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("removed halfop: "+temp+" from: "+ircdata.Channel);
#endif
                                    // update the user halfop status
                                    ((NonRfcChannelUser)GetChannelUser(ircdata.Channel, temp)).IsHalfop = false;
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("unset halfop status: "+temp+" for: "+ircdata.Channel);
#endif
                                }
                                if (OnDehalfop != null) {
                                    OnDehalfop(this, new DehalfopEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                                }
                            }
                        }
                    break;
                    case 'v':
                        temp = (string)parametersEnumerator.Current;
                        parametersEnumerator.MoveNext();
                        if (add) {
                            if (ActiveChannelSyncing) {
                                // update the voice list
                                try {
                                    channel.UnsafeVoices.Add(temp, GetIrcUser(temp));
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("added voice: "+temp+" to: "+ircdata.Channel);
#endif
                                } catch (ArgumentException) {
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("duplicate voice: "+temp+" in: "+ircdata.Channel+" not added");
#endif
                                }
                                
                                // update the user voice status
                                GetChannelUser(ircdata.Channel, temp).IsVoice = true;
#if LOG4NET
                                Logger.ChannelSyncing.Debug("set voice status: "+temp+" for: "+ircdata.Channel);
#endif
                            }
                            if (OnVoice != null) {
                                OnVoice(this, new VoiceEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                            }
                        }
                        if (remove) {
                            if (ActiveChannelSyncing) {
                                // update the voice list
                                channel.UnsafeVoices.Remove(temp);
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
                                OnDevoice(this, new DevoiceEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                            }
                        }
                    break;
                    case 'b':
                        temp = (string)parametersEnumerator.Current;
                        parametersEnumerator.MoveNext();
                        if (add) {
                            if (ActiveChannelSyncing) {
                                try {
                                    channel.Bans.Add(temp);
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("added ban: "+temp+" to: "+ircdata.Channel);
#endif
                                } catch (ArgumentException) {
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("duplicate ban: "+temp+" in: "+ircdata.Channel+" not added");
#endif
                                }
                            }
                            if (OnBan != null) {
                               OnBan(this, new BanEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                            }
                        }
                        if (remove) {
                            if (ActiveChannelSyncing) {
                                channel.Bans.Remove(temp);
#if LOG4NET
                                Logger.ChannelSyncing.Debug("removed ban: "+temp+" from: "+ircdata.Channel);
#endif
                            }
                            if (OnUnban != null) {
                                OnUnban(this, new UnbanEventArgs(ircdata, ircdata.Channel, ircdata.Nick, temp));
                            }
                        }
                    break;
                    case 'l':
                        temp = (string)parametersEnumerator.Current;
                        parametersEnumerator.MoveNext();
                        if (add) {
                            if (ActiveChannelSyncing) {
                                channel.UserLimit = int.Parse(temp);
#if LOG4NET
                                Logger.ChannelSyncing.Debug("stored user limit for: "+ircdata.Channel);
#endif
                            }
                        }
                        if (remove) {
                            if (ActiveChannelSyncing) {
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
                                if (ActiveChannelSyncing) {
                                    channel.Key = temp;
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("stored channel key for: "+ircdata.Channel);
#endif
                                }
                            }
                            if (remove) {
                                if (ActiveChannelSyncing) {
                                    channel.Key = "";
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("removed channel key for: "+ircdata.Channel);
#endif
                                }
                            }
                        break;
                        default:
                            if (add) {
                                if (ActiveChannelSyncing) {
                                    channel.Mode += mode[i];
#if LOG4NET
                                    Logger.ChannelSyncing.Debug("added channel mode ("+mode[i]+") for: "+ircdata.Channel);
#endif
                                }
                            }
                            if (remove) {
                                if (ActiveChannelSyncing) {
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
        
        // <internal messagehandler>

        private void _Event_PING(IrcMessageData ircdata)
        {
            string server = ircdata.RawMessageArray[1].Substring(1);
#if LOG4NET
            Logger.Connection.Debug("Ping? Pong!");
#endif
            RfcPong(server, Priority.Critical);

            if (OnPing != null) {
                OnPing(this, new PingEventArgs(ircdata, server));
            }
        }

        private void _Event_ERROR(IrcMessageData ircdata)
        {
            string message = ircdata.Message;
#if LOG4NET
            Logger.Connection.Info("received ERROR from IRC server");
#endif

            if (OnError != null) {
                OnError(this, new ErrorEventArgs(ircdata, message));
            }
        }

        private void _Event_JOIN(IrcMessageData ircdata)
        {
            string who = ircdata.Nick;
            string channelname = ircdata.Channel;

            if (IsMe(who)) {
                _JoinedChannels.Add(channelname);
            }

            if (ActiveChannelSyncing) {
                Channel channel;
                if (IsMe(who)) {
                    // we joined the channel
#if LOG4NET
                    Logger.ChannelSyncing.Debug("joining channel: "+channelname);
#endif
                    if (SupportNonRfc) {
                        channel = new NonRfcChannel(channelname);
                    } else {
                        channel = new Channel(channelname);
                    }
                    _Channels.Add(channelname, channel);
                    // request channel mode
                    RfcMode(channelname);
                    // request wholist
                    RfcWho(channelname);
                    // request banlist
                    Ban(channelname);
                } else {
                    // someone else joined the channel
                    // request the who data
                    RfcWho(who);
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
                    _IrcUsers.Add(who, ircuser);
                }

                ChannelUser channeluser;
                if (SupportNonRfc) {
                    channeluser = new NonRfcChannelUser(channelname, ircuser);
                } else {
                    channeluser = new ChannelUser(channelname, ircuser);
                }
                channel.UnsafeUsers.Add(who, channeluser);
            }

            if (OnJoin != null) {
                OnJoin(this, new JoinEventArgs(ircdata, channelname, who));
            }
        }

        private void _Event_PART(IrcMessageData ircdata)
        {
            string who = ircdata.Nick;
            string channel = ircdata.Channel;
            string partmessage = ircdata.Message;

            if (IsMe(who)) {
                _JoinedChannels.Remove(channel);
            }

            if (ActiveChannelSyncing) {
                if (IsMe(who)) {
#if LOG4NET
                    Logger.ChannelSyncing.Debug("parting channel: "+channel);
#endif
                    _Channels.Remove(channel);
                } else {
#if LOG4NET
                    Logger.ChannelSyncing.Debug(who+" parts channel: "+channel);
#endif
                    _RemoveChannelUser(channel, who);
                    _RemoveIrcUser(who);
                }
            }

            if (OnPart != null) {
                OnPart(this, new PartEventArgs(ircdata, channel, who, partmessage));
            }
        }

        private void _Event_KICK(IrcMessageData ircdata)
        {
            string channel = ircdata.Channel;
            string who = ircdata.Nick;
            string whom = ircdata.RawMessageArray[3];
            string reason = ircdata.Message;

            if (IsMe(whom)) {
                _JoinedChannels.Remove(channel);
            }

            if (ActiveChannelSyncing) {
                if (IsMe(whom)) {
                    _Channels.Remove(channel);
                } else {
                    _RemoveChannelUser(channel, whom);
                    _RemoveIrcUser(whom);
                }
            }

            if (OnKick != null) {
                OnKick(this, new KickEventArgs(ircdata, channel, who, whom, reason));
            }
        }

        private void _Event_QUIT(IrcMessageData ircdata)
        {
            string who = ircdata.Nick;
            string reason = ircdata.Message;
            
            // no need to handle if we quit, disconnect event will take care
            
            if (ActiveChannelSyncing) {
                // sanity checks, freshirc is very broken about RFC
                IrcUser user = GetIrcUser(who);
                if (user != null) {
                    string[] joined_channels = user.JoinedChannels;
                    if (joined_channels != null) {
                        foreach (string channel in joined_channels) {
                            _RemoveChannelUser(channel, who);
                        }
                        _RemoveIrcUser(who);
#if LOG4NET
                    } else {
                        Logger.ChannelSyncing.Warn("received quit message from a user without beeing in any channel?!? ignored");
#endif
                    }
#if LOG4NET
                } else {
                    Logger.ChannelSyncing.Warn("received quit message from a non-existent user?!? ignored");
#endif
                }
            }

            if (OnQuit != null) {
                OnQuit(this, new QuitEventArgs(ircdata, who, reason));
            }
        }

        private void _Event_PRIVMSG(IrcMessageData ircdata)
        {
            if (ircdata.Type == ReceiveType.CtcpRequest) {
                if (ircdata.Message.StartsWith("\x1"+"PING")) {
                    SendMessage(SendType.CtcpReply, ircdata.Nick, "PING "+ircdata.Message.Substring(6, (ircdata.Message.Length-7)));
                } else if (ircdata.Message.StartsWith("\x1"+"VERSION")) {
                    string versionstring;
                    if (_CtcpVersion == null) {
                        versionstring = VersionString;
                    } else {
                        versionstring = _CtcpVersion+" | using "+VersionString;
                    }
                    SendMessage(SendType.CtcpReply, ircdata.Nick, "VERSION "+versionstring);
                } else if (ircdata.Message.StartsWith("\x1"+"CLIENTINFO")) {
                    SendMessage(SendType.CtcpReply, ircdata.Nick, "CLIENTINFO PING VERSION CLIENTINFO");
                }
            }

            switch (ircdata.Type) {
                case ReceiveType.ChannelMessage:
                    if (OnChannelMessage != null) {
                        OnChannelMessage(this, new IrcEventArgs(ircdata));
                    }
                break;
                case ReceiveType.ChannelAction:
                    if (OnChannelAction != null) {
                        string action = ircdata.Message.Substring(7, ircdata.Message.Length-8);
                        OnChannelAction(this, new ActionEventArgs(ircdata, action));
                    }
                break;
                case ReceiveType.QueryMessage:
                    if (OnQueryMessage != null) {
                        OnQueryMessage(this, new IrcEventArgs(ircdata));
                    }
                break;
                case ReceiveType.QueryAction:
                    if (OnQueryAction != null) {
                        string action = ircdata.Message.Substring(7, ircdata.Message.Length-8);
                        OnQueryAction(this, new ActionEventArgs(ircdata, action));
                    }
                break;
                case ReceiveType.CtcpRequest:
                    if (OnCtcpRequest != null) {
                        OnCtcpRequest(this, new IrcEventArgs(ircdata));
                    }
                break;
            }
        }

        private void _Event_NOTICE(IrcMessageData ircdata)
        {
            switch (ircdata.Type) {
                case ReceiveType.ChannelNotice:
                    if (OnChannelNotice != null) {
                        OnChannelNotice(this, new IrcEventArgs(ircdata));
                    }
                break;
                case ReceiveType.QueryNotice:
                    if (OnQueryNotice != null) {
                        OnQueryNotice(this, new IrcEventArgs(ircdata));
                    }
                break;
                case ReceiveType.CtcpReply:
                    if (OnCtcpReply != null) {
                        OnCtcpReply(this, new IrcEventArgs(ircdata));
                    }
                break;
            }
        }

        private void _Event_TOPIC(IrcMessageData ircdata)
        {
            string who = ircdata.Nick;
            string channel = ircdata.Channel;
            string newtopic = ircdata.Message;

            if (ActiveChannelSyncing &&
                IsJoined(channel)) {
                GetChannel(channel).Topic = newtopic;
#if LOG4NET
                Logger.ChannelSyncing.Debug("stored topic for channel: "+channel);
#endif
            }

            if (OnTopicChange != null) {
                OnTopicChange(this, new TopicChangeEventArgs(ircdata, channel, who, newtopic));
            }
        }

        private void _Event_NICK(IrcMessageData ircdata)
        {
            string oldnickname = ircdata.Nick;
            string newnickname = ircdata.Message;

            if (IsMe(ircdata.Nick)) {
                _Nickname = newnickname;
            }

            if (ActiveChannelSyncing) {
                IrcUser ircuser = GetIrcUser(oldnickname);
                
                // if we don't have any info about him, don't update him!
                // (only queries or ourself in no channels)
                if (ircuser != null) {
                    string[] joinedchannels = ircuser.JoinedChannels;

                    // update his nickname
                    ircuser.Nick = newnickname;
                    // remove the old entry 
                    // remove first to avoid duplication, Foo -> foo
                    _IrcUsers.Remove(oldnickname);
                    // add him as new entry and new nickname as key
                    _IrcUsers.Add(newnickname, ircuser);
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
                        channel.UnsafeUsers.Remove(oldnickname);
                        channel.UnsafeUsers.Add(newnickname, channeluser);
                        if (channeluser.IsOp) {
                            channel.UnsafeOps.Remove(oldnickname);
                            channel.UnsafeOps.Add(newnickname, channeluser);
                        }
                        if (SupportNonRfc && ((NonRfcChannelUser)channeluser).IsHalfop) {
                            NonRfcChannel nchannel = (NonRfcChannel)channel;
                            nchannel.UnsafeHalfops.Remove(oldnickname);
                            nchannel.UnsafeHalfops.Add(newnickname, channeluser);
                        }
                        if (channeluser.IsVoice) {
                            channel.UnsafeVoices.Remove(oldnickname);
                            channel.UnsafeVoices.Add(newnickname, channeluser);
                        }
                    }
                }
            }
            
            if (OnNickChange != null) {
                OnNickChange(this, new NickChangeEventArgs(ircdata, oldnickname, newnickname));
            }
        }

        private void _Event_INVITE(IrcMessageData ircdata)
        {
            string channel = ircdata.Channel;
            string inviter = ircdata.Nick;

            if (OnInvite != null) {
                OnInvite(this, new InviteEventArgs(ircdata, channel, inviter));
            }
        }

        private void _Event_MODE(IrcMessageData ircdata)
        {
            if (IsMe(ircdata.RawMessageArray[2])) {
                // my user mode changed
                _Usermode = ircdata.RawMessageArray[3].Substring(1);
            } else {
                // channel mode changed
                string mode = ircdata.RawMessageArray[3];
                string parameter = String.Join(" ", ircdata.RawMessageArray, 4, ircdata.RawMessageArray.Length-4);
                _InterpretChannelMode(ircdata, mode, parameter);
            }
            
            if ((ircdata.Type == ReceiveType.UserModeChange) &&
                (OnUserModeChange != null)) {
                OnUserModeChange(this, new IrcEventArgs(ircdata));
            }

            if ((ircdata.Type == ReceiveType.ChannelModeChange) &&
                (OnChannelModeChange != null)) {
                OnChannelModeChange(this, new IrcEventArgs(ircdata));
            }

            if (OnModeChange != null) {
                OnModeChange(this, new IrcEventArgs(ircdata));
            }
        }

        private void _Event_RPL_CHANNELMODEIS(IrcMessageData ircdata)
        {
            if (ActiveChannelSyncing &&
                IsJoined(ircdata.Channel)) {
                string mode = ircdata.RawMessageArray[4];
                string parameter = String.Join(" ", ircdata.RawMessageArray, 5, ircdata.RawMessageArray.Length-5);
                _InterpretChannelMode(ircdata, mode, parameter);
            }
        }
        
        private void _Event_RPL_WELCOME(IrcMessageData ircdata)
        {
            // updating our nickname, that we got (maybe cutted...)
            _Nickname = ircdata.RawMessageArray[2];

            if (OnRegistered != null) {
                OnRegistered(this, EventArgs.Empty);
            }
        }

        private void _Event_RPL_TOPIC(IrcMessageData ircdata)
        {
            string topic   = ircdata.Message;
            string channel = ircdata.Channel;

            if (ActiveChannelSyncing &&
                IsJoined(channel)) {
                GetChannel(channel).Topic = topic;
#if LOG4NET
                Logger.ChannelSyncing.Debug("stored topic for channel: "+channel);
#endif
            }

            if (OnTopic != null) {
                OnTopic(this, new TopicEventArgs(ircdata, channel, topic));
            }
        }

        private void _Event_RPL_NOTOPIC(IrcMessageData ircdata)
        {
            string channel = ircdata.Channel;

            if (ActiveChannelSyncing &&
                IsJoined(channel)) {
                GetChannel(channel).Topic = "";
#if LOG4NET
                Logger.ChannelSyncing.Debug("stored empty topic for channel: "+channel);
#endif
            }

            if (OnTopic != null) {
                OnTopic(this, new TopicEventArgs(ircdata, channel, ""));
            }
        }

        private void _Event_RPL_NAMREPLY(IrcMessageData ircdata)
        {
            string   channelname  = ircdata.Channel;
            string[] userlist     = ircdata.MessageArray;
            if (ActiveChannelSyncing &&
                IsJoined(channelname)) {
                string nickname;
                bool   op;
                bool   halfop;
                bool   voice;
                foreach (string user in userlist) {
                    if (user.Length <= 0) {
                        continue;
                    }

                    op = false;
                    halfop = false;
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
                            halfop = true;
                            nickname = user.Substring(1);
                        break;
                        case '~':
                            nickname = user.Substring(1);
                        break;
                        default:
                            nickname = user;
                        break;
                    }

                    IrcUser     ircuser     = GetIrcUser(nickname);
                    ChannelUser channeluser = GetChannelUser(channelname, nickname);

                    if (ircuser == null) {
#if LOG4NET
                        Logger.ChannelSyncing.Debug("creating IrcUser: "+nickname+" because he doesn't exist yet");
#endif
                        ircuser = new IrcUser(nickname, this);
                        _IrcUsers.Add(nickname, ircuser);
                    }

                    if (channeluser == null) {
#if LOG4NET
                        Logger.ChannelSyncing.Debug("creating ChannelUser: "+nickname+" for Channel: "+channelname+" because he doesn't exist yet");
#endif
                        channeluser = new ChannelUser(channelname, ircuser);
                        Channel channel = GetChannel(channelname);
                        
                        channel.UnsafeUsers.Add(nickname, channeluser);
                        if (op) {
                            channel.UnsafeOps.Add(nickname, channeluser);
#if LOG4NET
                            Logger.ChannelSyncing.Debug("added op: "+nickname+" to: "+channelname);
#endif
                        }
                        if (SupportNonRfc && halfop)  {
                            ((NonRfcChannel)channel).UnsafeHalfops.Add(nickname, channeluser);
#if LOG4NET
                            Logger.ChannelSyncing.Debug("added halfop: "+nickname+" to: "+channelname);
#endif
                        }
                        if (voice) {
                            channel.UnsafeVoices.Add(nickname, channeluser);
#if LOG4NET
                            Logger.ChannelSyncing.Debug("added voice: "+nickname+" to: "+channelname);
#endif
                        }
                    }

                    channeluser.IsOp    = op;
                    channeluser.IsVoice = voice;
                    if (SupportNonRfc) {
                        ((NonRfcChannelUser)channeluser).IsHalfop = halfop;
                    }
                }
            }
            
            if (OnNames != null) {
                OnNames(this, new NamesEventArgs(ircdata, channelname, userlist));
            }
        }
        
        private void _Event_RPL_ENDOFNAMES(IrcMessageData ircdata)
        {
            string channelname = ircdata.RawMessageArray[3];
            if (ActiveChannelSyncing &&
                IsJoined(channelname)) {
#if LOG4NET
                Logger.ChannelSyncing.Debug("passive synced: "+channelname);
#endif
                if (OnChannelPassiveSynced != null) {
                    OnChannelPassiveSynced(this, new IrcEventArgs(ircdata));
                }
            }
        }
        
        private void _Event_RPL_WHOREPLY(IrcMessageData ircdata)
        {
            string channel  = ircdata.Channel;
            string ident    = ircdata.RawMessageArray[4];
            string host     = ircdata.RawMessageArray[5];
            string server   = ircdata.RawMessageArray[6];
            string nick     = ircdata.RawMessageArray[7];
            string usermode = ircdata.RawMessageArray[8];
            string realname = ircdata.Message.Substring(2);
            int    hopcount = 0;
            string temp     = ircdata.RawMessageArray[9].Substring(1);
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

            if (ActiveChannelSyncing &&
                IsJoined(channel)) {
                // checking the irc and channel user I only do for sanity!
                // according to RFC they must be known to us already via RPL_NAMREPLY
                // psyBNC is not very correct with this... maybe other bouncers too
                IrcUser ircuser  = GetIrcUser(nick);
                ChannelUser channeluser = GetChannelUser(channel, nick);
#if LOG4NET
                if (ircuser == null) {
                    Logger.ChannelSyncing.Error("GetIrcUser(nick) returned null in _Event_WHOREPLY! Ignoring...");
                }
#endif

#if LOG4NET
                if (channeluser == null) {
                    Logger.ChannelSyncing.Error("GetChannelUser(nick) returned null in _Event_WHOREPLY! Ignoring...");
                }
#endif

                if (ircuser != null) {                                
#if LOG4NET
                    Logger.ChannelSyncing.Debug("updating userinfo (from whoreply) for user: "+nick+" channel: "+channel);
#endif

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
                            if (channeluser != null) {
                                channeluser.IsOp    = op;
                                channeluser.IsVoice = voice;
                            }
                        break;
                    }
                }
            }
            
            if (OnWho != null) {
                OnWho(this, new WhoEventArgs(ircdata, channel, nick, ident, host, realname, away, op, voice, ircop, server, hopcount));
            }
        }
        
        private void _Event_RPL_MOTD(IrcMessageData ircdata)
        {
            if (!_MotdReceived) {
                _Motd.Add(ircdata.Message);
            }
            
            if (OnMotd != null) {
                OnMotd(this, new MotdEventArgs(ircdata, ircdata.Message));
            }
        }
        
        private void _Event_RPL_ENDOFMOTD(IrcMessageData ircdata)
        {
            _MotdReceived = true;
        }
        
        // TODO, need to sync with the banlist!
        private void _Event_RPL_BANLIST(IrcMessageData ircdata)
        {
        }
        
        private void _Event_RPL_ENDOFBANLIST(IrcMessageData ircdata)
        {
            string channelname = ircdata.Channel;
            if (ActiveChannelSyncing &&
                IsJoined(channelname)) {
                Channel channel = GetChannel(channelname);
                channel.ActiveSyncStop = DateTime.Now;
#if LOG4NET
                Logger.ChannelSyncing.Debug("active synced: "+channelname+
                    " (in "+channel.ActiveSyncTime.TotalSeconds+" sec)");
#endif
                if (OnChannelActiveSynced != null) {
                    OnChannelActiveSynced(this, new IrcEventArgs(ircdata));
                }
            }
        }
        
        private void _Event_ERR(IrcMessageData ircdata)
        {
            if (OnErrorMessage != null) {
                OnErrorMessage(this, new IrcEventArgs(ircdata));
            }
        }
        
        private void _Event_ERR_NICKNAMEINUSE(IrcMessageData ircdata)
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

            RfcNick(nickname, Priority.Critical);
        }

        // </internal messagehandler>
    }
}
