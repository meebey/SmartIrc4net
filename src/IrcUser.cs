/**
 * $Id: IrcUser.cs,v 1.3 2003/12/14 12:44:11 meebey Exp $
 * $Revision: 1.3 $
 * $Author: meebey $
 * $Date: 2003/12/14 12:44:11 $
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
    public class IrcUser
    {
        private string  _Nick     = null;
        private string  _Ident    = null;
        private string  _Host     = null;
        private string  _Realname = null;
        private bool    _IrcOp    = false;
        private bool    _Away     = false;
        private string  _Server   = null;
        private int     _HopCount = -1;

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

        public string Realname
        {
            get {
                return _Realname;
            }
        }

        public bool IrcOp
        {
            get {
                return _IrcOp;
            }
        }

        public bool Away
        {
            get {
                return _Away;
            }
        }

        public string Server
        {
            get {
                return _Server;
            }
        }

        public int HopCount
        {
            get {
                return _HopCount;
            }
        }

        public StringCollection JoinedChannels
        {
            get {
                return new StringCollection();
            }
        }
    }
}
