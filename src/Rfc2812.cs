/**
 * $Id$
 * $URL$
 * $Rev$
 * $Author$
 * $Date$
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
    /// <summary>
    ///
    /// </summary>
    public class Rfc2812
    {
        public static string Pong(string data)
        {
            return "PONG :"+data;
        }

        public static string Pass(string password)
        {
            return "PASS "+password;
        }
        
        public static string Nick(string nickname)
        {
            return "NICK "+nickname;
        }
        
        public static string User(string username, int usermode, string realname)
        {
            return "USER "+username+" "+usermode.ToString()+" * :"+realname;
        }

        public static string Privmsg(string destination, string message)
        {
            return "PRIVMSG "+destination+" :"+message;
        }

        public static string Notice(string destination, string message)
        {
            return "NOTICE "+destination+" :"+message;
        }

        public static string Join(string channel)
        {
            return "JOIN "+channel;
        }

        public static string Part(string channel)
        {
            return "PART "+channel;
        }

        public static string Part(string channel, string reason)
        {
            return "PART "+channel+" :"+reason;
        }

        public static string Kick(string channel, string nickname)
        {
            return "KICK "+channel+" "+nickname;
        }

        public static string Kick(string channel, string nickname, string reason)
        {
            return "KICK "+channel+" "+nickname+" :"+reason;
        }

        public static string List()
        {
            return "LIST";
        }

        public static string List(string channel)
        {
            return "LIST "+channel;
        }

        public static string Names()
        {
            return "NAMES";
        }

        public static string Names(string channel)
        {
            return "NAMES "+channel;
        }

        public static string Topic(string channel)
        {
            return "TOPIC "+channel;
        }

        public static string Topic(string channel, string newtopic)
        {
            return "TOPIC "+channel+" :"+newtopic;
        }

        public static string Mode(string target)
        {
            return "MODE "+target;
        }

        public static string Mode(string target, string newmode)
        {
            return "MODE "+target+" "+newmode;
        }

        public static string Invite(string nickname, string channel)
        {
            return "INVITE "+nickname+" "+channel;
        }

        public static string Who(string target)
        {
            return "WHO "+target;
        }

        public static string Whois(string target)
        {
            return "WHOIS "+target;
        }

        public static string Whowas(string target)
        {
            return "WHOWAS "+target;
        }

        public static string Quit()
        {
            return "QUIT";
        }

        public static string Quit(string reason)
        {
            return "QUIT :"+reason;
        }
    }
}
