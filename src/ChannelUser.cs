/**
 * $Id$
 * $URL$
 * $Rev$
 * $Author$
 * $Date$
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

namespace Meebey.SmartIrc4net
{
    /// <summary>
    ///
    /// </summary>
    public class ChannelUser
    {
        private string    _Channel;
        private IrcUser   _IrcUser  = null;
        private bool      _IsOp     = false;
        private bool      _IsVoice  = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"> </param>
        /// <param name="ircuser"> </param>
        internal ChannelUser(string channel, IrcUser ircuser)
        {
            _Channel = channel;
            _IrcUser = ircuser;
        }

        ~ChannelUser()
        {
#if LOG4NET
            Logger.ChannelSyncing.Debug("ChannelUser ("+Channel+":"+IrcUser.Nick+") destroyed");
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string Channel
        {
            get {
                return _Channel;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public bool IsIrcOp
        {
            get {
                return _IrcUser.IsIrcOp;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public bool IsOp
        {
            get {
                return _IsOp;
            }
            set {
                _IsOp = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public bool IsVoice
        {
            get {
                return _IsVoice;
            }
            set {
                _IsVoice = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public bool IsAway
        {
            get {
                return _IrcUser.IsAway;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public IrcUser IrcUser
        {
            get {
                return _IrcUser;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string Nick
        {
            get {
                return _IrcUser.Nick;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string Ident
        {
            get {
                return _IrcUser.Ident;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string Host
        {
            get {
                return _IrcUser.Host;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string Realname
        {
            get {
                return _IrcUser.Realname;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string Server
        {
            get {
                return _IrcUser.Server;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public int HopCount
        {
            get {
                return _IrcUser.HopCount;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string[] JoinedChannels
        {
            get {
                return _IrcUser.JoinedChannels;
            }
        }
    }
}
