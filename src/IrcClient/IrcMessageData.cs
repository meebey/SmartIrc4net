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

namespace Meebey.SmartIrc4net
{
    /// <summary>
    ///
    /// </summary>
    public class IrcMessageData
    {
        private IrcClient   _Irc;
        private string      _From;
        private string      _Nick;
        private string      _Ident;
        private string      _Host;
        private string      _Channel;
        private string      _Message;
        private string[]    _MessageArray;
        private string      _RawMessage;
        private string[]    _RawMessageArray;
        private ReceiveType _Type;
        private ReplyCode   _ReplyCode;
        
        public IrcClient Irc
        {
            get {
                return _Irc;
            }
        }
        
        public string From
        {
            get {
                return _From;
            }
        }
        
        public string Nick
        {
            get {
                return _Nick;
            }
        }

        public string Ident
        {
            get {
                return _Ident;
            }
        }

        public string Host
        {
            get {
                return _Host;
            }
        }

        public string Channel
        {
            get {
                return _Channel;
            }
        }
        
        public string Message
        {
            get {
                return _Message;
            }
        }
        
        public string[] MessageArray
        {
            get {
                return _MessageArray;
            }
        }
        
        public string RawMessage
        {
            get {
                return _RawMessage;
            }
        }
        
        public string[] RawMessageArray
        {
            get {
                return _RawMessageArray;
            }
        }

        public ReceiveType Type
        {
            get {
                return _Type;
            }
        }

        public ReplyCode ReplyCode
        {
            get {
                return _ReplyCode;
            }
        }

        public IrcMessageData(IrcClient ircclient, string from, string nick, string ident, string host, string channel, string message, string rawmessage, ReceiveType type, ReplyCode replycode)
        {
            _Irc = ircclient;
            _RawMessage = rawmessage;
            _RawMessageArray = rawmessage.Split(new char[] {' '});
            _Type = type;
            _ReplyCode = replycode;
            _From = from;
            _Nick = nick;
            _Ident = ident;
            _Host = host;
            _Channel = channel;
            if (message != null) {
                // message is optional
                _Message = message;
                _MessageArray = message.Split(new char[] {' '});
            }
        }
    }
}
