/**
 * $Id: ChannelUser.cs,v 1.3 2003/12/14 12:38:37 meebey Exp $
 * $Revision: 1.3 $
 * $Author: meebey $
 * $Date: 2003/12/14 12:38:37 $
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

using System.Collections.Specialized;

namespace Meebey.SmartIrc4net
{
    public class ChannelUser
    {
        private IrcUser   _IrcUser = null;
        private bool      _Op      = false;
        private bool      _Voice   = false;

        public bool Op
        {
            get {
                return _Op;
            }
        }

        public bool Voice
        {
            get {
                return _Voice;
            }
        }

        public string Nick
        {
            get {
                return _IrcUser.Nick;
            }
        }

        public string Ident
        {
            get {
                return _IrcUser.Ident;
            }
        }

        public string Host
        {
            get {
                return _IrcUser.Host;
            }
        }

        public string Realname
        {
            get {
                return _IrcUser.Realname;
            }
        }

        public bool IrcOp
        {
            get {
                return _IrcUser.IrcOp;
            }
        }

        public bool Away
        {
            get {
                return _IrcUser.Away;
            }
        }

        public string Server
        {
            get {
                return _IrcUser.Server;
            }
        }

        public int HopCount
        {
            get {
                return _IrcUser.HopCount;
            }
        }

        public StringCollection JoinedChannels
        {
            get {
                return _IrcUser.JoinedChannels;
            }
        }
    }
}
