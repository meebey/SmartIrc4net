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

using System.Collections.Specialized;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    /// This class manages the user information
    /// </summary>
    public class IrcUser
    {
        private IrcClient _IrcClient;
        private string    _Nick     = null;
        private string    _Ident    = null;
        private string    _Host     = null;
        private string    _Realname = null;
        private bool      _IsIrcOp  = false;
        private bool      _IsAway   = false;
        private string    _Server   = null;
        private int       _HopCount = -1;

        internal IrcUser(string nickname, IrcClient ircclient)
        {
            _IrcClient = ircclient;
            _Nick      = nickname;
        }

#if LOG4NET
        ~IrcUser()
        {
            Logger.ChannelSyncing.Debug("IrcUser ("+Nick+") destroyed");
        }
#endif

        /// <summary>
        /// Gets or sets the nickname of the user.
        /// </summary>
        public string Nick
        {
            get {
                return _Nick;
            }
            set {
                _Nick = value;
            }
        }

        /// <summary>
        /// Gets or sets an Ident daemon which is still used by some IRC networks for authentication. 
        /// </summary>
        public string Ident
        {
            get {
                return _Ident;
            }
            set {
                _Ident = value;
            }
        }

        /// <summary>
        /// Gets or sets the hostname of the local machine. 
        /// </summary>
        public string Host
        {
            get {
                return _Host;
            }
            set {
                _Host = value;
            }
        }

        /// <summary>
        /// Gets or sets the supposed real name.
        /// </summary>
        /// <remarks>
        /// System username is set by default 
        /// </remarks>
        public string Realname
        {
            get {
                return _Realname;
            }
            set {
                _Realname = value;
            }
        }

        /// <summary>
        /// Gets or sets user server operator status
        /// </summary>
        public bool IsIrcOp
        {
            get {
                return _IsIrcOp;
            }
            set {
                _IsIrcOp = value;
            }
        }

        /// <summary>
        /// Gets or sets user away status
        /// </summary>
        public bool IsAway
        {
            get {
                return _IsAway;
            }
            set {
                _IsAway = value;
            }
        }

        /// <summary>
        /// Server the user is connected too
        /// </summary>
        public string Server
        {
            get {
                return _Server;
            }
            set {
                _Server = value;
            }
        }

        /// <summary>
        /// Note: Meebey? Count of half ops
        /// </summary>
        public int HopCount
        {
            get {
                return _HopCount;
            }
            set {
                _HopCount = value;
            }
        }

        /// <summary>
        /// Gets the list of channels a user has joined
        /// </summary>
        public string[] JoinedChannels
        {
            get {
                Channel          channel;
                string[]         result;
                string[]         channels       = _IrcClient.GetChannels();
                StringCollection joinedchannels = new StringCollection();
                foreach (string channelname in channels) {
                    channel = _IrcClient.GetChannel(channelname);
                    if (channel.UnsafeUsers.ContainsKey(_Nick)) {
                        joinedchannels.Add(channelname);
                    }
                }

                result = new string[joinedchannels.Count];
                joinedchannels.CopyTo(result, 0);
                return result;
                //return joinedchannels;
            }
        }
    }
}
