/*
 * $Id$
 * $URL$
 * $Rev$
 * $Author$
 * $Date$
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 *
 * Copyright (c) 2003-2005 Mirco Bauer <meebey@meebey.net> <http://www.meebey.net>
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

namespace Meebey.SmartIrc4net
{
    /// <summary>
    ///
    /// </summary>
    public class ActionEventArgs : CtcpEventArgs
    {
        private string _ActionMessage;
        
        public string ActionMessage {
            get {
                return _ActionMessage;
            }
        }
         
        internal ActionEventArgs(IrcMessageData data, string actionmsg) : base(data, "ACTION", actionmsg)
        {
            _ActionMessage = actionmsg;
        }
    }
    
    /// <summary>
    ///
    /// </summary>
    public class CtcpEventArgs : IrcEventArgs
    {
        private string _CtcpCommand;
        private string _CtcpParameter;
        
        public string CtcpCommand {
            get {
                return _CtcpCommand;
            }
        }
         
        public string CtcpParameter {
            get {
                return _CtcpParameter;
            }
        }
         
        internal CtcpEventArgs(IrcMessageData data, string ctcpcmd, string ctcpparam) : base(data)
        {
            _CtcpCommand = ctcpcmd;
            _CtcpParameter = ctcpparam;
        }
    }
    
    /// <summary>
    ///
    /// </summary>
    public class ErrorEventArgs : IrcEventArgs
    {
        private string _ErrorMessage;
        
        public string ErrorMessage {
            get {
                return _ErrorMessage;
            }
        }
         
        internal ErrorEventArgs(IrcMessageData data, string errormsg) : base(data)
        {
            _ErrorMessage = errormsg;
        }
    }
    
    /// <summary>
    ///
    /// </summary>
    public class MotdEventArgs : IrcEventArgs
    {
        private string _MotdMessage;
        
        public string MotdMessage {
            get {
                return _MotdMessage;
            }
        }
         
        internal MotdEventArgs(IrcMessageData data, string motdmsg) : base(data)
        {
            _MotdMessage = motdmsg;
        }
    }
    
    /// <summary>
    ///
    /// </summary>
    public class PingEventArgs : IrcEventArgs
    {
        private string _PingData;
        
        public string PingData {
            get {
                return _PingData;
            }
        }
         
        internal PingEventArgs(IrcMessageData data, string pingdata) : base(data)
        {
            _PingData = pingdata;
        }
    }
    
    /// <summary>
    ///
    /// </summary>
    public class PongEventArgs : IrcEventArgs
    {
        private TimeSpan _Lag;
        
        public TimeSpan Lag {
            get {
                return _Lag;
            }
        }

        internal PongEventArgs(IrcMessageData data, TimeSpan lag) : base(data)
        {
            _Lag = lag;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class KickEventArgs : IrcEventArgs
    {
        private string _Channel;
        private string _Who;
        private string _Whom;
        private string _KickReason;
        
        public string Channel {
            get {
                return _Channel;
            }
        }

        public string Who {
            get {
                return _Who;
            }
        }
         
        public string Whom {
            get {
                return _Whom;
            }
        }
         
        public string KickReason {
            get {
                return _KickReason;
            }
        }
         
        internal KickEventArgs(IrcMessageData data, string channel, string who, string whom, string kickreason) : base(data)
        {
            _Channel = channel;
            _Who = who;
            _Whom = whom;
            _KickReason = kickreason;
        }
    }
    
    /// <summary>
    ///
    /// </summary>
    public class JoinEventArgs : IrcEventArgs
    {
        private string _Channel;
        private string _Who;
        
        public string Channel {
            get {
                return _Channel;
            }
        }

        public string Who {
            get {
                return _Who;
            }
        }
         
        internal JoinEventArgs(IrcMessageData data, string channel, string who) : base(data)
        {
            _Channel = channel;
            _Who = who;
        }
    }
    
    /// <summary>
    ///
    /// </summary>
    public class NamesEventArgs : IrcEventArgs
    {
        private string   _Channel;
        private string[] _UserList;
        public string[] RawUserList { get; private set; }

        public string Channel {
            get {
                return _Channel;
            }
        }

        public string[] UserList {
            get {
                return _UserList;
            }
        }
         
        internal NamesEventArgs(IrcMessageData data, string channel, string[] userlist, string[] rawUserList) : base(data)
        {
            _Channel = channel;
            _UserList = userlist;
            RawUserList = rawUserList;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class ListEventArgs : IrcEventArgs
    {
        private ChannelInfo f_ListInfo;
        
        public ChannelInfo ListInfo {
            get {
                return f_ListInfo;
            }
        }

        internal ListEventArgs(IrcMessageData data, ChannelInfo listInfo) : base(data)
        {
            f_ListInfo = listInfo;
        }
    }
    
    /// <summary>
    ///
    /// </summary>
    public class InviteEventArgs : IrcEventArgs
    {
        private string   _Channel;
        private string   _Who;
        
        public string Channel {
            get {
                return _Channel;
            }
        }

        public string Who {
            get {
                return _Who;
            }
        }
         
        internal InviteEventArgs(IrcMessageData data, string channel, string who) : base(data)
        {
            _Channel = channel;
            _Who = who;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class PartEventArgs : IrcEventArgs
    {
        private string   _Channel;
        private string   _Who;
        private string   _PartMessage;
        
        public string Channel {
            get {
                return _Channel;
            }
        }

        public string Who {
            get {
                return _Who;
            }
        }
         
        public string PartMessage {
            get {
                return _PartMessage;
            }
        }
        
        internal PartEventArgs(IrcMessageData data, string channel, string who, string partmessage) : base(data)
        {
            _Channel = channel;
            _Who = who;
            _PartMessage = partmessage;
        }
    }
    
    /// <summary>
    ///
    /// </summary>
    public class WhoEventArgs : IrcEventArgs
    {
        private WhoInfo f_WhoInfo;

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public string Channel {
            get {
                return f_WhoInfo.Channel;
            }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public string Nick {
            get {
                return f_WhoInfo.Nick;
            }
        }
         
        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public string Ident {
            get {
                return f_WhoInfo.Ident;
            }
        }
        
        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public string Host {
            get {
                return f_WhoInfo.Host;
            }
        }
        
        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public string Realname {
            get {
                return f_WhoInfo.Realname;
            }
        }
        
        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public bool IsAway {
            get {
                return f_WhoInfo.IsAway;
            }
        }
        
        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public bool IsOp {
            get {
                return f_WhoInfo.IsOp;
            }
        }
        
        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public bool IsVoice {
            get {
                return f_WhoInfo.IsVoice;
            }
        }
        
        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public bool IsIrcOp {
            get {
                return f_WhoInfo.IsIrcOp;
            }
        }
        
        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public string Server {
            get {
                return f_WhoInfo.Server;
            }
        }

        [Obsolete("Use WhoEventArgs.WhoInfo instead.")]
        public int HopCount {
            get {
                return f_WhoInfo.HopCount;
            }
        }
        
        public WhoInfo WhoInfo {
            get {
                return f_WhoInfo;
            }
        }

        internal WhoEventArgs(IrcMessageData data, WhoInfo whoInfo) : base(data)
        {
            f_WhoInfo = whoInfo;
        }
    }
    
    /// <summary>
    ///
    /// </summary>
    public class QuitEventArgs : IrcEventArgs
    {
        private string   _Who;
        private string   _QuitMessage;
        
        public string Who {
            get {
                return _Who;
            }
        }

        public string QuitMessage {
            get {
                return _QuitMessage;
            }
        }
        
        internal QuitEventArgs(IrcMessageData data, string who, string quitmessage) : base(data)
        {
            _Who = who;
            _QuitMessage = quitmessage;
        }
    }


    /// <summary>
    ///
    /// </summary>
    public class AwayEventArgs : IrcEventArgs
    {
        private string   _Who;
        private string   _AwayMessage;
        
        public string Who {
            get {
                return _Who;
            }
        }

        public string AwayMessage{
            get {
                return _AwayMessage;
            }
        }
        
        internal AwayEventArgs(IrcMessageData data, string who, string awaymessage) : base(data)
        {
            _Who = who;
            _AwayMessage = awaymessage;
        }
    }
    /// <summary>
    ///
    /// </summary>
    public class NickChangeEventArgs : IrcEventArgs
    {
        private string   _OldNickname;
        private string   _NewNickname;
        
        public string OldNickname {
            get {
                return _OldNickname;
            }
        }

        public string NewNickname {
            get {
                return _NewNickname;
            }
        }
        
        internal NickChangeEventArgs(IrcMessageData data, string oldnick, string newnick) : base(data)
        {
            _OldNickname = oldnick;
            _NewNickname = newnick;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class TopicEventArgs : IrcEventArgs
    {
        private string   _Channel;
        private string   _Topic;
        
        public string Channel {
            get {
                return _Channel;
            }
        }

        public string Topic {
            get {
                return _Topic;
            }
        }
        
        internal TopicEventArgs(IrcMessageData data, string channel, string topic) : base(data)
        {
            _Channel = channel;
            _Topic = topic;
        }
    }
    
    /// <summary>
    ///
    /// </summary>
    public class TopicChangeEventArgs : IrcEventArgs
    {
        private string   _Channel;
        private string   _Who;
        private string   _NewTopic;
        
        public string Channel {
            get {
                return _Channel;
            }
        }

        public string Who {
            get {
                return _Who;
            }
        }

        public string NewTopic {
            get {
                return _NewTopic;
            }
        }
        
        internal TopicChangeEventArgs(IrcMessageData data, string channel, string who, string newtopic) : base(data)
        {
            _Channel = channel;
            _Who = who;
            _NewTopic = newtopic;
        }
    }
    
    /// <summary>
    ///
    /// </summary>
    public class BanEventArgs : IrcEventArgs
    {
        private string   _Channel;
        private string   _Who;
        private string   _Hostmask;
        
        public string Channel {
            get {
                return _Channel;
            }
        }

        public string Who {
            get {
                return _Who;
            }
        }

        public string Hostmask {
            get {
                return _Hostmask;
            }
        }
        
        internal BanEventArgs(IrcMessageData data, string channel, string who, string hostmask) : base(data)
        {
            _Channel = channel;
            _Who = who;
            _Hostmask = hostmask;
        }
    }
    
    /// <summary>
    ///
    /// </summary>
    public class UnbanEventArgs : IrcEventArgs
    {
        private string   _Channel;
        private string   _Who;
        private string   _Hostmask;
        
        public string Channel {
            get {
                return _Channel;
            }
        }

        public string Who {
            get {
                return _Who;
            }
        }

        public string Hostmask {
            get {
                return _Hostmask;
            }
        }
        
        internal UnbanEventArgs(IrcMessageData data, string channel, string who, string hostmask) : base(data)
        {
            _Channel = channel;
            _Who = who;
            _Hostmask = hostmask;
        }
    }

    /// <summary>
    /// Event arguments for any change in channel role.
    /// </summary>
    public class ChannelRoleChangeEventArgs : IrcEventArgs
    {
        public string Channel { get; private set; }
        public string Who { get; private set; }
        public string Whom { get; private set; }

        internal ChannelRoleChangeEventArgs(IrcMessageData data, string channel, string who, string whom) : base(data)
        {
            Channel = channel;
            Who = who;
            Whom = whom;
        }
    }

    /// <summary>
    /// User gained owner status (non-RFC, channel mode +q, prefix ~).
    /// </summary>
    public class OwnerEventArgs : ChannelRoleChangeEventArgs
    {
        internal OwnerEventArgs(IrcMessageData data, string channel, string who, string whom) : base(data, channel, who, whom)
        {
        }
    }

    /// <summary>
    /// User lost owner status (non-RFC, channel mode -q).
    /// </summary>
    public class DeownerEventArgs : ChannelRoleChangeEventArgs
    {
        internal DeownerEventArgs(IrcMessageData data, string channel, string who, string whom) : base(data, channel, who, whom)
        {
        }
    }

    /// <summary>
    /// User gained channel admin status (non-RFC, channel mode +a, prefix &amp;).
    /// </summary>
    public class ChannelAdminEventArgs : ChannelRoleChangeEventArgs
    {
        internal ChannelAdminEventArgs(IrcMessageData data, string channel, string who, string whom) : base(data, channel, who, whom)
        {
        }
    }

    /// <summary>
    /// User lost channel admin status (non-RFC, channel mode -a).
    /// </summary>
    public class DeChannelAdminEventArgs : ChannelRoleChangeEventArgs
    {
        internal DeChannelAdminEventArgs(IrcMessageData data, string channel, string who, string whom) : base(data, channel, who, whom)
        {
        }
    }

    /// <summary>
    /// User gained op status (channel mode +o, prefix @).
    /// </summary>
    public class OpEventArgs : ChannelRoleChangeEventArgs
    {
        internal OpEventArgs(IrcMessageData data, string channel, string who, string whom) : base(data, channel, who, whom)
        {
        }
    }

    /// <summary>
    /// User lost op status (channel mode -o).
    /// </summary>
    public class DeopEventArgs : ChannelRoleChangeEventArgs
    {
        internal DeopEventArgs(IrcMessageData data, string channel, string who, string whom) : base(data, channel, who, whom)
        {
        }
    }

    /// <summary>
    /// User gained halfop status (non-RFC, channel mode +h, prefix %).
    /// </summary>
    public class HalfopEventArgs : ChannelRoleChangeEventArgs
    {
        internal HalfopEventArgs(IrcMessageData data, string channel, string who, string whom) : base(data, channel, who, whom)
        {
        }
    }

    /// <summary>
    /// User lost halfop status (non-RFC, channel mode -h).
    /// </summary>
    public class DehalfopEventArgs : ChannelRoleChangeEventArgs
    {
        internal DehalfopEventArgs(IrcMessageData data, string channel, string who, string whom) : base(data, channel, who, whom)
        {
        }
    }

    /// <summary>
    /// User gained voice status (channel mode +v, prefix +).
    /// </summary>
    public class VoiceEventArgs : ChannelRoleChangeEventArgs
    {
        internal VoiceEventArgs(IrcMessageData data, string channel, string who, string whom) : base(data, channel, who, whom)
        {
        }
    }

    /// <summary>
    /// User lost voice status (channel mode -v).
    /// </summary>
    public class DevoiceEventArgs : ChannelRoleChangeEventArgs
    {
        internal DevoiceEventArgs(IrcMessageData data, string channel, string who, string whom) : base(data, channel, who, whom)
        {
        }
    }

    public class BounceEventArgs : IrcEventArgs
    {
        /// <summary>
        /// Hostname/address of the server to which the user is being redirected.
        /// May be null if not successfully parsed from the message.
        /// </summary>
        public string Server { get; private set; }

        /// <summary>
        /// Port of the server to which the user is being redirected.
        /// May be -1 if not successfully parsed from the message.
        /// </summary>
        public int Port { get; private set; }

        internal BounceEventArgs(IrcMessageData data, string server, int port) : base(data)
        {
            Server = server;
            Port = Port;
        }
    }
}
