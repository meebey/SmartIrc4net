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

namespace Meebey.SmartIrc4net
{
    /// <summary>
    /// This class contains an IRC message in a parsed form
    /// </summary>
    /// <threadsafety static="true" instance="true" />
    public class IrcMessageData
    {
        private IrcClient   _Irc;
        private string      _Nick;
        private string      _Ident;
        private string      _Host;
        private string      _Channel;
        private string[]    _MessageArray;
        private string      _RawMessage;
        private string[]    _RawMessageArray;
        private ReceiveType _Type;
        private ReplyCode   _ReplyCode;
        
        private string   _Prefix;
        private string   _Command;
        private string[] _Args;
        private string   _Rest;
        
        /// <summary>
        /// Gets the IrcClient object the message originated from
        /// </summary>
        public IrcClient Irc {
            get {
                return _Irc;
            }
        }
        
        /// <summary>
        /// Gets the combined nickname, identity and hostname of the user that sent the message
        /// </summary>
        /// <example>
        /// nick!ident@host
        /// </example>
        public string From {
            get {
                return _Prefix;
            }
        }
        
        /// <summary>
        /// Gets the nickname of the user that sent the message
        /// </summary>
        public string Nick {
            get {
                return _Nick;
            }
        }

        /// <summary>
        /// Gets the identity (username) of the user that sent the message
        /// </summary>
        public string Ident {
            get {
                return _Ident;
            }
        }

        /// <summary>
        /// Gets the hostname of the user that sent the message
        /// </summary>
        public string Host {
            get {
                return _Host;
            }
        }

        /// <summary>
        /// Gets the channel the message originated from
        /// </summary>
        public string Channel {
            get {
                return _Channel;
            }
        }
        
        /// <summary>
        /// Gets the message
        /// </summary>
        public string Message {
            get {
                return _Rest;
            }
        }
        
        /// <summary>
        /// Gets the message as an array of strings (splitted by space)
        /// </summary>
        public string[] MessageArray {
            get {
                return _MessageArray;
            }
        }
        
        /// <summary>
        /// Gets the raw message sent by the server
        /// </summary>
        public string RawMessage {
            get {
                return _RawMessage;
            }
        }
        
        /// <summary>
        /// Gets the raw message sent by the server as array of strings (splitted by space)
        /// </summary>
        public string[] RawMessageArray {
            get {
                return _RawMessageArray;
            }
        }

        /// <summary>
        /// Gets the message type
        /// </summary>
        public ReceiveType Type {
            get {
                return _Type;
            }
        }

        /// <summary>
        /// Gets the message reply code
        /// </summary>
        public ReplyCode ReplyCode {
            get {
                return _ReplyCode;
            }
        }

        /// <summary>
        /// Gets the message prefix
        /// </summary>
        public string Prefix {
            get {
                return _Prefix;
            }
        }

        /// <summary>
        /// Gets the message command word
        /// </summary>
        public string Command {
            get {
                return _Command;
            }
        }

        /// <summary>
        /// Gets the message arguments
        /// </summary>
        public string[] Args {
            get {
                return _Args;
            }
        }

        /// <summary>
        /// Gets the message trailing argument
        /// </summary>
        public string Rest {
            get {
                return _Rest;
            }
        }

        /// <summary>
        /// Constructor to create an instace of IrcMessageData
        /// </summary>
        /// <param name="ircclient">IrcClient the message originated from</param>
        /// <param name="from">combined nickname, identity and host of the user that sent the message (nick!ident@host)</param>
        /// <param name="nick">nickname of the user that sent the message</param>
        /// <param name="ident">identity (username) of the userthat sent the message</param>
        /// <param name="host">hostname of the user that sent the message</param>
        /// <param name="channel">channel the message originated from</param>
        /// <param name="message">message</param>
        /// <param name="rawmessage">raw message sent by the server</param>
        /// <param name="type">message type</param>
        /// <param name="replycode">message reply code</param>
        public IrcMessageData(IrcClient ircclient, string from, string nick, string ident, string host, string channel, string message, string rawmessage, ReceiveType type, ReplyCode replycode)
        {
            _Irc = ircclient;
            _RawMessage = rawmessage;
            _RawMessageArray = rawmessage.Split(new char[] {' '});
            _Type = type;
            _ReplyCode = replycode;
            _Prefix = from;
            _Nick = nick;
            _Ident = ident;
            _Host = host;
            _Channel = channel;
            if (message != null) {
                // message is optional
                _Rest = message;
                _MessageArray = message.Split(new char[] {' '});
            }
        }

        /// <summary>
        /// Constructor to create an instace of IrcMessageData
        /// </summary>
        /// <param name="ircclient">IrcClient the message originated from</param>
        /// <param name="rawMessage">message as it appears on wire, stripped of newline</param>
        public IrcMessageData (IrcClient ircClient, string rawMessage)
        {
            if (rawMessage == null)
                throw new System.ArgumentException ("Cannot parse null message");
            if (rawMessage == "")
                throw new System.ArgumentException ("Cannot parse empty message");

            _Irc = ircClient;
            _RawMessage = rawMessage;
            _RawMessageArray = rawMessage.Split(' ');
            _Prefix = "";
            _Rest = "";
            
            int start = 0;
            int len = 0;
            if (_RawMessageArray[0][0] == ':')
            {
                _Prefix = _RawMessageArray[0].Substring(1);
                start = 1;
                len += _Prefix.Length + 1;
            }
            
            _Command = _RawMessageArray[start];
            len += _Command.Length + 1;
            
            int rest = _RawMessageArray.Length;
            
            if (start + 1 < rest)
            {
                for (int i = start + 1; i < _RawMessageArray.Length; i++)
                {
                    if (_RawMessageArray[i][0] == ':')
                    {
                        rest = i;
                        break;
                    }
                    else
                        len += _RawMessageArray[i].Length + 1;
                }
                
                _Args = new string [rest - start - 1];
                System.Array.Copy (_RawMessageArray, start+1, _Args, 0, rest - start - 1);
                if (rest < _RawMessageArray.Length)
                {
                    _Rest = _RawMessage.Substring (_RawMessage.IndexOf (':', len) + 1);
                    _MessageArray = _Rest.Split (' ');
                }
            }
            else
                _Args = new string [0];
            
            _ReplyCode = ReplyCode.Null;
            _Type = ReceiveType.Unknown;
            
            _ParseLegacyInfo ();
        }
        
        private static readonly System.Text.RegularExpressions.Regex _PrefixRegex = new System.Text.RegularExpressions.Regex ("([^!@]+)(![^@]+)?(@.+)?");

        // refactored old field parsing code below, ignore for own sanity
        private void _ParseLegacyInfo ()
        {
            var match = _PrefixRegex.Match (_Prefix);
            
            if (match.Success)
            {
                if (match.Groups[2].Success || match.Groups[3].Success || (_Prefix.IndexOf ('.') < 0))
                    _Nick = match.Groups[1].ToString();
            }
            
            if (match.Groups[2].Success)
                _Ident = match.Groups[2].ToString().Substring(1);
            if (match.Groups[3].Success)
                _Host = match.Groups[3].ToString().Substring(1);
            
            int replyCode;
            if (int.TryParse (_Command, out replyCode))
                _ReplyCode = (ReplyCode) replyCode;
            else
                _ReplyCode = ReplyCode.Null;
                
            if (_ReplyCode != ReplyCode.Null)
            {
                // categorize replies
                
                switch (_ReplyCode)
                {
                    case ReplyCode.Welcome:
                    case ReplyCode.YourHost:
                    case ReplyCode.Created:
                    case ReplyCode.MyInfo:
                    case ReplyCode.Bounce:
                    case ReplyCode.SaslSuccess:
                    case ReplyCode.SaslFailure1:
                    case ReplyCode.SaslFailure2:
                    case ReplyCode.SaslAbort:
                        _Type = ReceiveType.Login;
                        break;
                        
                    case ReplyCode.LuserClient:
                    case ReplyCode.LuserOp:
                    case ReplyCode.LuserUnknown:
                    case ReplyCode.LuserMe:
                    case ReplyCode.LuserChannels:
                        _Type = ReceiveType.Info;
                        break;
                        
                    case ReplyCode.MotdStart:
                    case ReplyCode.Motd:
                    case ReplyCode.EndOfMotd:
                        _Type = ReceiveType.Motd;
                        break;
                        
                    case ReplyCode.NamesReply:
                    case ReplyCode.EndOfNames:
                        _Type = ReceiveType.Name;
                        break;
                        
                    case ReplyCode.WhoReply:
                    case ReplyCode.EndOfWho:
                        _Type = ReceiveType.Who;
                        break;
                        
                    case ReplyCode.ListStart:
                    case ReplyCode.List:
                    case ReplyCode.ListEnd:
                        _Type = ReceiveType.List;
                        break;
                        
                    case ReplyCode.BanList:
                    case ReplyCode.EndOfBanList:
                        _Type = ReceiveType.BanList;
                        break;
                        
                    case ReplyCode.Topic:
                    case ReplyCode.NoTopic:
                        _Type = ReceiveType.Topic;
                        break;
                        
                    case ReplyCode.WhoIsUser:
                    case ReplyCode.WhoIsServer:
                    case ReplyCode.WhoIsOperator:
                    case ReplyCode.WhoIsIdle:
                    case ReplyCode.WhoIsChannels:
                    case ReplyCode.EndOfWhoIs:
                        _Type = ReceiveType.WhoIs;
                        break;
                        
                    case ReplyCode.WhoWasUser:
                    case ReplyCode.EndOfWhoWas:
                        _Type = ReceiveType.WhoWas;
                        break;
                        
                    case ReplyCode.UserModeIs:
                        _Type = ReceiveType.UserMode;
                        break;
                        
                    case ReplyCode.ChannelModeIs:
                        _Type = ReceiveType.ChannelMode;
                        break;
                        
                    default:
                        if ((replyCode >= 400) &&
                            (replyCode <= 599)) {
                            _Type = ReceiveType.ErrorMessage;
                        } else {
                            _Type = ReceiveType.Unknown;
                        }
                        break;
                }
            }
            else
            {
                // categorize commands
                
                switch (_Command)
                {
                    case "PING":
                        _Type = ReceiveType.Unknown;
                        break;
                    
                    case "ERROR":
                        _Type = ReceiveType.Error;
                        break;
                    
                    case "PRIVMSG":
                        if (_Args.Length > 0 && _Rest.StartsWith("\x1" + "ACTION") && _Rest.EndsWith("\x1"))
                        {
                            switch (_Args[0][0])
                            {
                                case '#':
                                case '!':
                                case '&':
                                case '+':
                                    _Type = ReceiveType.ChannelAction;
                                    break;
                                    
                                default:
                                    _Type = ReceiveType.QueryAction;
                                    break;
                            }
                        }
                        else if (_Rest.StartsWith("\x1") && _Rest.EndsWith("\x1"))
                        {
                            _Type = ReceiveType.CtcpRequest;
                        }
                        else if (_Args.Length > 0)
                        {
                            switch (_Args[0][0])
                            {
                                case '#':
                                case '!':
                                case '&':
                                case '+':
                                    _Type = ReceiveType.ChannelMessage;
                                    break;
                                    
                                default:
                                    _Type = ReceiveType.QueryMessage;
                                    break;
                            }
                        }
                        break;
                    
                    case "NOTICE":
                        if (_Rest.StartsWith("\x1") && _Rest.EndsWith("\x1"))
                        {
                            _Type = ReceiveType.CtcpReply;
                        }
                        else if (_Args.Length > 0)
                        {
                            switch (_Args[0][0])
                            {
                                case '#':
                                case '!':
                                case '&':
                                case '+':
                                    _Type = ReceiveType.ChannelNotice;
                                    break;
                                    
                                default:
                                    _Type = ReceiveType.QueryNotice;
                                    break;
                            }
                        }
                        break;
                    
                    case "INVITE":
                        _Type = ReceiveType.Invite;
                        break;
                    
                    case "JOIN":
                        _Type = ReceiveType.Join;
                        break;
                    
                    case "PART":
                        _Type = ReceiveType.Part;
                        break;
                    
                    case "TOPIC":
                        _Type = ReceiveType.TopicChange;
                        break;

                    case "NICK":
                        _Type = ReceiveType.NickChange;
                        break;
                    
                    case "KICK":
                        _Type = ReceiveType.Kick;
                        break;
                    
                    case "MODE":
                        switch (_Args[0][0])
                        {
                            case '#':
                            case '!':
                            case '&':
                            case '+':
                                _Type = ReceiveType.ChannelModeChange;
                                break;
                                
                            default:
                                _Type = ReceiveType.UserModeChange;
                                break;
                        }
                        break;
                    
                    case "QUIT":
                        _Type = ReceiveType.Quit;
                        break;
                    
                    case "CAP":
                    case "AUTHENTICATE":
                        _Type = ReceiveType.Other;
                        break;
                }
            }
            
            switch (_Type) {
                case ReceiveType.Join:
                case ReceiveType.Kick:
                case ReceiveType.Part:
                case ReceiveType.TopicChange:
                case ReceiveType.ChannelModeChange:
                case ReceiveType.ChannelMessage:
                case ReceiveType.ChannelAction:
                case ReceiveType.ChannelNotice:
                    _Channel = _RawMessageArray[2];
                    break;
                    
                case ReceiveType.Who:
                case ReceiveType.Topic:
                case ReceiveType.Invite:
                case ReceiveType.BanList:
                case ReceiveType.ChannelMode:
                    _Channel = _RawMessageArray[3];
                    break;
                    
                case ReceiveType.Name:
                    _Channel = _RawMessageArray[4];
                    break;
            }
            
            switch (_ReplyCode) {
                case ReplyCode.List:
                case ReplyCode.ListEnd:
                case ReplyCode.ErrorNoChannelModes:
                    _Channel = _Args[1];
                    break;
            }
            
            if (_Channel != null && _Channel.StartsWith(":"))
                _Channel = Channel.Substring (1);
        }
        
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder ("[");
            
            sb.Append ("<");
            sb.Append (_Prefix ?? "null");
            sb.Append ("> ");
            
            sb.Append ("<");
            sb.Append (_Command ?? "null");
            sb.Append ("> ");
            
            sb.Append ("<");
            string sep = "";
            foreach (var a in (_Args ?? new string [0]))
            {
                sb.Append (sep); sep = ", ";
                sb.Append (a);
            }
            sb.Append ("> ");
            
            sb.Append ("<");
            sb.Append (_Rest ?? "null");
            sb.Append ("> ");

            sb.Append ("(Type=");
            sb.Append (_Type.ToString());
            sb.Append (") ");
                                    
            sb.Append ("(Nick=");
            sb.Append (_Nick ?? "null");
            sb.Append (") ");
            
            sb.Append ("(Ident=");
            sb.Append (_Ident ?? "null");
            sb.Append (") ");
            
            sb.Append ("(Host=");
            sb.Append (_Host ?? "null");
            sb.Append (") ");
            
            sb.Append ("(Channel=");
            sb.Append (_Channel ?? "null");
            sb.Append (") ");
            
            return sb.ToString();
        }
    }
}
