/**
 * $Id: User.cs,v 1.1 2003/11/16 16:58:44 meebey Exp $
 * $Revision: 1.1 $
 * $Author: meebey $
 * $Date: 2003/11/16 16:58:44 $
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

using System;

namespace SmartIRC
{
    public abstract class User
    {
        public string    nick;
        public string    ident;
        public string    host;
        public string    realname;
        public bool      ircop;
        public bool      away;
        public string    server;
        public int       hopcount;
    }
}

