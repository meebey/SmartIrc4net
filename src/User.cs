/**
 * $Id: User.cs,v 1.2 2003/11/27 23:29:30 meebey Exp $
 * $Revision: 1.2 $
 * $Author: meebey $
 * $Date: 2003/11/27 23:29:30 $
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

namespace Meebey.SmartIrc4net
{
    public abstract class User
    {
        public string    Nick;
        public string    Ident;
        public string    Host;
        public string    Realname;
        public bool      Ircop;
        public bool      Away;
        public string    Server;
        public int       Hopcount;
    }
}

