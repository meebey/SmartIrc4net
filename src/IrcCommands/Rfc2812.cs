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

using System;
using System.Globalization;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    ///
    /// </summary>
    public sealed class Rfc2812
    {
        private Rfc2812()
        {
        }
        
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

        public static string Oper(string name, string password)
        {
            return "OPER "+name+" "+password;
        }
        
        public static string Privmsg(string destination, string message)
        {
            return "PRIVMSG "+destination+" :"+message;
        }

        public static string Notice(string destination, string message)
        {
            return "NOTICE "+destination+" :"+message;
        }

        public static string Join(string channel, string key)
        {
            return "JOIN "+channel+" "+key;
        }

        public static string Join(string[] channels, string[] keys)
        {
            string channellist = String.Join(",", channels);
            string keylist = String.Join(",", keys);
            return "JOIN "+channellist+" "+keylist;
        }
        
        public static string Join(string channel)
        {
            return "JOIN "+channel;
        }
        
        public static string Join(string[] channels)
        {
            string channellist = String.Join(",", channels);
            return "JOIN "+channellist;
        }
        
        public static string Part(string channel)
        {
            return "PART "+channel;
        }

        public static string Part(string[] channels)
        {
            string channellist = String.Join(",", channels);
            return "PART "+channellist;
        }
        
        public static string Part(string channel, string partmessage)
        {
            return "PART "+channel+" :"+partmessage;
        }

        public static string Part(string[] channels, string partmessage)
        {
            string channellist = String.Join(",", channels);
            return "PART "+channellist+" :"+partmessage;
        }

        public static string Kick(string channel, string nickname)
        {
            return "KICK "+channel+" "+nickname;
        }

        public static string Kick(string channel, string nickname, string comment)
        {
            return "KICK "+channel+" "+nickname+" :"+comment;
        }
        
        public static string Kick(string[] channels, string nickname)
        {
            string channellist = String.Join(",", channels);
            return "KICK "+channellist+" "+nickname;
        }

        public static string Kick(string[] channels, string nickname, string comment)
        {
            string channellist = String.Join(",", channels);
            return "KICK "+channellist+" "+nickname+" :"+comment;
        }

        public static string Kick(string channel, string[] nicknames)
        {
            string nicknamelist = String.Join(",", nicknames);
            return "KICK "+channel+" "+nicknamelist;
        }

        public static string Kick(string channel, string[] nicknames, string comment)
        {
            string nicknamelist = String.Join(",", nicknames);
            return "KICK "+channel+" "+nicknamelist+" :"+comment;
        }

        public static string Kick(string[] channels, string[] nicknames)
        {
            string channellist = String.Join(",", channels);
            string nicknamelist = String.Join(",", nicknames);
            return "KICK "+channellist+" "+nicknamelist;
        }

        public static string Kick(string[] channels, string[] nicknames, string comment)
        {
            string channellist = String.Join(",", channels);
            string nicknamelist = String.Join(",", nicknames);
            return "KICK "+channellist+" "+nicknamelist+" :"+comment;
        }
        
        public static string Motd()
        {
            return "MOTD";
        }

        public static string Motd(string target)
        {
            return "MOTD "+target;
        }

        public static string Luser()
        {
            return "LUSER";
        }

        public static string Luser(string mask)
        {
            return "LUSER "+mask;
        }

        public static string Luser(string mask, string target)
        {
            return "LUSER "+mask+" "+target;
        }
        
        public static string Version()
        {
            return "VERSION";
        }

        public static string Version(string target)
        {
            return "VERSION "+target;
        }

        public static string Stats()
        {
            return "STATS";
        }

        public static string Stats(string query)
        {
            return "STATS "+query;
        }

        public static string Stats(string query, string target)
        {
            return "STATS "+query+" "+target;
        }

        public static string List()
        {
            return "LIST";
        }

        public static string List(string channel)
        {
            return "LIST "+channel;
        }

        public static string List(string[] channels)
        {
            string channellist = String.Join(",", channels);
            return "LIST "+channellist;
        }
        
        public static string List(string channel, string target)
        {
            return "LIST "+channel+" "+target;
        }

        public static string List(string[] channels, string target)
        {
            string channellist = String.Join(",", channels);
            return "LIST "+channellist+" "+target;
        }
        
        public static string Names()
        {
            return "NAMES";
        }

        public static string Names(string channel)
        {
            return "NAMES "+channel;
        }

        public static string Names(string[] channels)
        {
            string channellist = String.Join(",", channels);
            return "NAMES "+channellist;
        }
        
        public static string Names(string channel, string target)
        {
            return "NAMES "+channel+" "+target;
        }
        
        public static string Names(string[] channels, string target)
        {
            string channellist = String.Join(",", channels);
            return "NAMES "+channellist+" "+target;
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

        public static string Service(string nickname, string distribution, string info)
        {
            return "SERVICE "+nickname+" * "+distribution+" * * :"+info;
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

        public static string Quit(string quitmessage)
        {
            return "QUIT :"+quitmessage;
        }
        
        public static string Squit(string server, string comment)
        {
            return "SQUIT "+server+" :"+comment;
        }
    }
}
