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
    public class IrcCommands: IrcConnection
    {
        public void SendMessage(SendType type, string destination, string message, Priority priority)
        {
            switch(type) {
                case SendType.Message:
                    Privmsg(destination, message, priority);
                break;
                case SendType.Action:
                    Privmsg(destination, "\001ACTION "+message+"\001", priority);
                break;
                case SendType.Notice:
                    Notice(destination, message, priority);
                break;
                case SendType.CtcpRequest:
                    Privmsg(destination, "\001"+message+"\001", priority);
                break;
                case SendType.CtcpReply:
                    Notice(destination, "\001"+message+"\001", priority);
                break;
            }
        }

        public void SendMessage(SendType type, string destination, string message)
        {
            SendMessage(type, destination, message, Priority.Medium);
        }

        public void Pong(string data)
        {
            WriteLine(Rfc2812.Pong(data), Priority.Critical);
        }

        public void Pass(string password, Priority priority)
        {
            WriteLine(Rfc2812.Pass(password), priority);
        }

        public void Pass(string password)
        {
            WriteLine(Rfc2812.Pass(password));
        }

        public void User(string username, int usermode, string realname, Priority priority)
        {
            WriteLine(Rfc2812.User(username, usermode, realname), priority);
        }

        public void User(string username, int usermode, string realname)
        {
            WriteLine(Rfc2812.User(username, usermode, realname));
        }

        public void Oper(string name, string password, Priority priority)
        {
            WriteLine(Rfc2812.Oper(name, password), priority);
        }

        public void Oper(string name, string password)
        {
            WriteLine(Rfc2812.Oper(name, password));
        }

        public void Privmsg(string destination, string message, Priority priority)
        {
            WriteLine(Rfc2812.Privmsg(destination, message), priority);
        }

        public void Privmsg(string destination, string message)
        {
            WriteLine(Rfc2812.Privmsg(destination, message));
        }

        public void Notice(string destination, string message, Priority priority)
        {
            WriteLine(Rfc2812.Notice(destination, message), priority);
        }

        public void Notice(string destination, string message)
        {
            WriteLine(Rfc2812.Notice(destination, message));
        }

        public void Join(string channel, Priority priority)
        {
            WriteLine(Rfc2812.Join(channel), priority);
        }

        public void Join(string channel)
        {
            WriteLine(Rfc2812.Join(channel));
        }

        public void Join(string[] channels, Priority priority)
        {
            WriteLine(Rfc2812.Join(channels), priority);
        }
        
        public void Join(string[] channels)
        {
            WriteLine(Rfc2812.Join(channels));
        }
        
        public void Join(string channel, string key, Priority priority)
        {
            WriteLine(Rfc2812.Join(channel, key), priority);
        }

        public void Join(string channel, string key)
        {
            WriteLine(Rfc2812.Join(channel, key));
        }

        public void Join(string[] channels, string[] keys, Priority priority)
        {
            WriteLine(Rfc2812.Join(channels, keys), priority);
        }

        public void Join(string[] channels, string[] keys)
        {
            WriteLine(Rfc2812.Join(channels, keys));
        }

        public void Part(string channel, Priority priority)
        {
            WriteLine(Rfc2812.Part(channel), priority);
        }

        public void Part(string channel)
        {
            WriteLine(Rfc2812.Part(channel));
        }

        public void Part(string[] channels, Priority priority)
        {
            WriteLine(Rfc2812.Part(channels), priority);
        }

        public void Part(string[] channels)
        {
            WriteLine(Rfc2812.Part(channels));
        }

        public void Part(string channel, string partmessage, Priority priority)
        {
            WriteLine(Rfc2812.Part(channel, partmessage), priority);
        }

        public void Part(string channel, string partmessage)
        {
            WriteLine(Rfc2812.Part(channel, partmessage));
        }

        public void Part(string[] channels, string partmessage, Priority priority)
        {
            WriteLine(Rfc2812.Part(channels, partmessage), priority);
        }

        public void Part(string[] channels, string partmessage)
        {
            WriteLine(Rfc2812.Part(channels, partmessage));
        }

        public void Kick(string channel, string nickname, Priority priority)
        {
            WriteLine(Rfc2812.Kick(channel, nickname), priority);
        }

        public void Kick(string channel, string nickname)
        {
            WriteLine(Rfc2812.Kick(channel, nickname));
        }

        public void Kick(string[] channels, string nickname, Priority priority)
        {
            WriteLine(Rfc2812.Kick(channels, nickname), priority);
        }

        public void Kick(string[] channels, string nickname)
        {
            WriteLine(Rfc2812.Kick(channels, nickname));
        }
        
        public void Kick(string channel, string[] nicknames, Priority priority)
        {
            WriteLine(Rfc2812.Kick(channel, nicknames), priority);
        }

        public void Kick(string channel, string[] nicknames)
        {
            WriteLine(Rfc2812.Kick(channel, nicknames));
        }
        
        public void Kick(string[] channels, string[] nicknames, Priority priority)
        {
            WriteLine(Rfc2812.Kick(channels, nicknames), priority);
        }

        public void Kick(string[] channels, string[] nicknames)
        {
            WriteLine(Rfc2812.Kick(channels, nicknames));
        }
        
        public void Kick(string channel, string nickname, string comment, Priority priority)
        {
            WriteLine(Rfc2812.Kick(channel, nickname, comment), priority);
        }

        public void Kick(string channel, string nickname, string comment)
        {
            WriteLine(Rfc2812.Kick(channel, nickname, comment));
        }
        
        public void Kick(string[] channels, string nickname, string comment, Priority priority)
        {
            WriteLine(Rfc2812.Kick(channels, nickname, comment), priority);
        }

        public void Kick(string[] channels, string nickname, string comment)
        {
            WriteLine(Rfc2812.Kick(channels, nickname, comment));
        }

        public void Kick(string channel, string[] nicknames, string comment, Priority priority)
        {
            WriteLine(Rfc2812.Kick(channel, nicknames, comment), priority);
        }

        public void Kick(string channel, string[] nicknames, string comment)
        {
            WriteLine(Rfc2812.Kick(channel, nicknames, comment));
        }

        public void Kick(string[] channels, string[] nicknames, string comment, Priority priority)
        {
            WriteLine(Rfc2812.Kick(channels, nicknames, comment), priority);
        }

        public void Kick(string[] channels, string[] nicknames, string comment)
        {
            WriteLine(Rfc2812.Kick(channels, nicknames, comment));
        }

        public void Motd(Priority priority)
        {
            WriteLine(Rfc2812.Motd(), priority);
        }

        public void Motd()
        {
            WriteLine(Rfc2812.Motd());
        }

        public void Motd(string target, Priority priority)
        {
            WriteLine(Rfc2812.Motd(target), priority);
        }

        public void Motd(string target)
        {
            WriteLine(Rfc2812.Motd(target));
        }

        public void Luser(Priority priority)
        {
            WriteLine(Rfc2812.Luser(), priority);
        }

        public void Luser()
        {
            WriteLine(Rfc2812.Luser());
        }

        public void Luser(string mask, Priority priority)
        {
            WriteLine(Rfc2812.Luser(mask), priority);
        }

        public void Luser(string mask)
        {
            WriteLine(Rfc2812.Luser(mask));
        }

        public void Luser(string mask, string target, Priority priority)
        {
            WriteLine(Rfc2812.Luser(mask, target), priority);
        }

        public void Luser(string mask, string target)
        {
            WriteLine(Rfc2812.Luser(mask, target));
        }

        public void Version(Priority priority)
        {
            WriteLine(Rfc2812.Version(), priority);
        }

        public void Version()
        {
            WriteLine(Rfc2812.Version());
        }

        public void Version(string target, Priority priority)
        {
            WriteLine(Rfc2812.Version(target), priority);
        }

        public void Version(string target)
        {
            WriteLine(Rfc2812.Version(target));
        }

        public void Stats(Priority priority)
        {
            WriteLine(Rfc2812.Stats(), priority);
        }

        public void Stats()
        {
            WriteLine(Rfc2812.Stats());
        }

        public void Stats(string query, Priority priority)
        {
            WriteLine(Rfc2812.Stats(query), priority);
        }

        public void Stats(string query)
        {
            WriteLine(Rfc2812.Stats(query));
        }
        
        public void Stats(string query, string target, Priority priority)
        {
            WriteLine(Rfc2812.Stats(query, target), priority);
        }

        public void Stats(string query, string target)
        {
            WriteLine(Rfc2812.Stats(query, target));
        }
        
        public void List(string channel, Priority priority)
        {
            WriteLine(Rfc2812.List(channel), priority);
        }

        public void List(string channel)
        {
            WriteLine(Rfc2812.List(channel));
        }

        public void List(string[] channels, Priority priority)
        {
            WriteLine(Rfc2812.List(channels), priority);
        }

        public void List(string[] channels)
        {
            WriteLine(Rfc2812.List(channels));
        }

        public void List(string channel, string target, Priority priority)
        {
            WriteLine(Rfc2812.List(channel, target), priority);
        }

        public void List(string channel, string target)
        {
            WriteLine(Rfc2812.List(channel, target));
        }

        public void List(string[] channels, string target, Priority priority)
        {
            WriteLine(Rfc2812.List(channels, target), priority);
        }

        public void List(string[] channels, string target)
        {
            WriteLine(Rfc2812.List(channels, target));
        }

        public void Names(string channel, Priority priority)
        {
            WriteLine(Rfc2812.Names(channel), priority);
        }

        public void Names(string channel)
        {
            WriteLine(Rfc2812.Names(channel));
        }

        public void Names(string[] channels, Priority priority)
        {
            WriteLine(Rfc2812.Names(channels), priority);
        }

        public void Names(string[] channels)
        {
            WriteLine(Rfc2812.Names(channels));
        }

        public void Names(string channel, string target, Priority priority)
        {
            WriteLine(Rfc2812.Names(channel, target), priority);
        }

        public void Names(string channel, string target)
        {
            WriteLine(Rfc2812.Names(channel, target));
        }

        public void Names(string[] channels, string target, Priority priority)
        {
            WriteLine(Rfc2812.Names(channels, target), priority);
        }

        public void Names(string[] channels, string target)
        {
            WriteLine(Rfc2812.Names(channels, target));
        }

        public void Topic(string channel, Priority priority)
        {
            WriteLine(Rfc2812.Topic(channel), priority);
        }

        public void Topic(string channel)
        {
            WriteLine(Rfc2812.Topic(channel));
        }

        public void Topic(string channel, string newtopic, Priority priority)
        {
            WriteLine(Rfc2812.Topic(channel, newtopic), priority);
        }

        public void Topic(string channel, string newtopic)
        {
            WriteLine(Rfc2812.Topic(channel, newtopic));
        }

        public void Mode(string target, Priority priority)
        {
            WriteLine(Rfc2812.Mode(target), priority);
        }

        public void Mode(string target)
        {
            WriteLine(Rfc2812.Mode(target));
        }

        public void Mode(string target, string newmode, Priority priority)
        {
            WriteLine(Rfc2812.Mode(target, newmode), priority);
        }

        public void Mode(string target, string newmode)
        {
            WriteLine(Rfc2812.Mode(target, newmode));
        }

        public void Service(string nickname, string distribution, string info, Priority priority)
        {
            WriteLine(Rfc2812.Service(nickname, distribution, info), priority);
        }

        public void Service(string nickname, string distribution, string info)
        {
            WriteLine(Rfc2812.Service(nickname, distribution, info));
        }

        public void Op(string channel, string nickname, Priority priority)
        {
            WriteLine(Rfc2812.Mode(channel, "+o "+nickname), priority);
        }

        public void Op(string channel, string nickname)
        {
            WriteLine(Rfc2812.Mode(channel, "+o "+nickname));
        }

        public void Deop(string channel, string nickname, Priority priority)
        {
            WriteLine(Rfc2812.Mode(channel, "-o "+nickname), priority);
        }

        public void Deop(string channel, string nickname)
        {
            WriteLine(Rfc2812.Mode(channel, "-o "+nickname));
        }

        public void Voice(string channel, string nickname, Priority priority)
        {
            WriteLine(Rfc2812.Mode(channel, "+v "+nickname), priority);
        }

        public void Voice(string channel, string nickname)
        {
            WriteLine(Rfc2812.Mode(channel, "+v "+nickname));
        }

        public void Devoice(string channel, string nickname, Priority priority)
        {
            WriteLine(Rfc2812.Mode(channel, "-v "+nickname), priority);
        }

        public void Devoice(string channel, string nickname)
        {
            WriteLine(Rfc2812.Mode(channel, "-v "+nickname));
        }

        public void Ban(string channel, Priority priority)
        {
            WriteLine(Rfc2812.Mode(channel, "+b"), priority);
        }

        public void Ban(string channel)
        {
            WriteLine(Rfc2812.Mode(channel, "+b"));
        }

        public void Ban(string channel, string hostmask, Priority priority)
        {
            WriteLine(Rfc2812.Mode(channel, "+b "+hostmask), priority);
        }

        public void Ban(string channel, string hostmask)
        {
            WriteLine(Rfc2812.Mode(channel, "+b "+hostmask));
        }

        public void Unban(string channel, string hostmask, Priority priority)
        {
            WriteLine(Rfc2812.Mode(channel, "-b "+hostmask), priority);
        }

        public void Unban(string channel, string hostmask)
        {
            WriteLine(Rfc2812.Mode(channel, "-b "+hostmask));
        }

        public void Invite(string nickname, string channel, Priority priority)
        {
            WriteLine(Rfc2812.Invite(nickname, channel), priority);
        }

        public void Invite(string nickname, string channel)
        {
            WriteLine(Rfc2812.Invite(nickname, channel));
        }

        public void Nick(string newnickname, Priority priority)
        {
            WriteLine(Rfc2812.Nick(newnickname), priority);
        }

        public void Nick(string newnickname)
        {
            WriteLine(Rfc2812.Nick(newnickname));
        }

        public void Who(string target, Priority priority)
        {
            WriteLine(Rfc2812.Who(target), priority);
        }

        public void Who(string target)
        {
            WriteLine(Rfc2812.Who(target));
        }

        public void Whois(string target, Priority priority)
        {
            WriteLine(Rfc2812.Whois(target), priority);
        }

        public void Whois(string target)
        {
            WriteLine(Rfc2812.Whois(target));
        }

        public void Whowas(string target, Priority priority)
        {
            WriteLine(Rfc2812.Whowas(target), priority);
        }

        public void Whowas(string target)
        {
            WriteLine(Rfc2812.Whowas(target));
        }

        public void Quit(Priority priority)
        {
            WriteLine(Rfc2812.Quit(), priority);
        }

        public void Quit()
        {
            WriteLine(Rfc2812.Quit());
        }

        public void Quit(string quitmessage, Priority priority)
        {
            WriteLine(Rfc2812.Quit(quitmessage), priority);
        }

        public void Quit(string quitmessage)
        {
            WriteLine(Rfc2812.Quit(quitmessage));
        }

        public void Squit(string server, string comment, Priority priority)
        {
            WriteLine(Rfc2812.Squit(server, comment), priority);
        }

        public void Squit(string server, string comment)
        {
            WriteLine(Rfc2812.Squit(server, comment));
        }
    }
}
