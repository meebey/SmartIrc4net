/**
 * $Id: IrcCommands.cs,v 1.1 2003/11/16 16:58:42 meebey Exp $
 * $Revision: 1.1 $
 * $Author: meebey $
 * $Date: 2003/11/16 16:58:42 $
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
using System.Collections;

namespace SmartIRC
{
    public class IrcCommands
    {
        private Connection _connection;

        public IrcCommands(Connection con)
        {
            _connection = con;
        }

        public void Pong(string data)
        {
            _connection.WriteLine(Rfc2812.Pong(data), Priority.Critical);
        }

        public void Join(string channel, Priority priority)
        {
            _connection.WriteLine(Rfc2812.Join(channel), priority);
        }

        public void Join(string channel)
        {
            _connection.WriteLine(Rfc2812.Join(channel));
        }

        public void Pass(string password, Priority priority)
        {
            _connection.WriteLine(Rfc2812.Pass(password), priority);
        }
        
        public void Pass(string password)
        {
            _connection.WriteLine(Rfc2812.Pass(password));
        }
        
        public void User(string username, int usermode, string realname, Priority priority)
        {
            _connection.WriteLine(Rfc2812.User(username, usermode, realname), priority);
        }
        
        public void User(string username, int usermode, string realname)
        {
            _connection.WriteLine(Rfc2812.User(username, usermode, realname));
        }
        
        public void Nick(string nickname, Priority priority)
        {
            _connection.WriteLine(Rfc2812.Nick(nickname), priority);
        }
        
        public void Nick(string nickname)
        {
            _connection.WriteLine(Rfc2812.Nick(nickname));
        }
    }
}
