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
        
        public void Links()
        {
            WriteLine(Rfc2812.Links());
        }

        public void Links(string server_mask, Priority priority)
        {
            WriteLine(Rfc2812.Links(server_mask), priority);
        }

        public void Links(string server_mask)
        {
            WriteLine(Rfc2812.Links(server_mask));
        }
        
        public void Links(string remote_server, string server_mask, Priority priority)
        {
            WriteLine(Rfc2812.Links(remote_server, server_mask), priority);
        }

        public void Links(string remote_server, string server_mask)
        {
            WriteLine(Rfc2812.Links(remote_server, server_mask));
        }
        
        public void Time(Priority priority)
        {
            WriteLine(Rfc2812.Time(), priority);
        }

        public void Time()
        {
            WriteLine(Rfc2812.Time());
        }
        
        public void Time(string target, Priority priority)
        {
            WriteLine(Rfc2812.Time(target), priority);
        }

        public void Time(string target)
        {
            WriteLine(Rfc2812.Time(target));
        }
        
        public void Connect(string target_server, string port, Priority priority)
        {
            WriteLine(Rfc2812.Connect(target_server, port), priority);
        }

        public void Connect(string target_server, string port)
        {
            WriteLine(Rfc2812.Connect(target_server, port));
        }
        
        public void Connect(string target_server, string port, string remote_server, Priority priority)
        {
            WriteLine(Rfc2812.Connect(target_server, port, remote_server), priority);
        }

        public void Connect(string target_server, string port, string remote_server)
        {
            WriteLine(Rfc2812.Connect(target_server, port, remote_server));
        }
        
        public void Trace(Priority priority)
        {
            WriteLine(Rfc2812.Trace(), priority);
        }

        public void Trace()
        {
            WriteLine(Rfc2812.Trace());
        }
        
        public void Trace(string target, Priority priority)
        {
            WriteLine(Rfc2812.Trace(target), priority);
        }

        public void Trace(string target)
        {
            WriteLine(Rfc2812.Trace(target));
        }
        
        public void Admin(Priority priority)
        {
            WriteLine(Rfc2812.Admin(), priority);
        }

        public void Admin()
        {
            WriteLine(Rfc2812.Admin());
        }
        
        public void Admin(string target, Priority priority)
        {
            WriteLine(Rfc2812.Admin(target), priority);
        }

        public void Admin(string target)
        {
            WriteLine(Rfc2812.Admin(target));
        }
        
        public void Info(Priority priority)
        {
            WriteLine(Rfc2812.Info(), priority);
        }

        public void Info()
        {
            WriteLine(Rfc2812.Info());
        }
        
        public void Info(string target, Priority priority)
        {
            WriteLine(Rfc2812.Info(target), priority);
        }

        public void Info(string target)
        {
            WriteLine(Rfc2812.Info(target));
        }
        
        public void Servlist(Priority priority)
        {
            WriteLine(Rfc2812.Servlist(), priority);
        }

        public void Servlist()
        {
            WriteLine(Rfc2812.Servlist());
        }
        
        public void Servlist(string mask, Priority priority)
        {
            WriteLine(Rfc2812.Servlist(mask), priority);
        }

        public void Servlist(string mask)
        {
            WriteLine(Rfc2812.Servlist(mask));
        }
        
        public void Servlist(string mask, string type, Priority priority)
        {
            WriteLine(Rfc2812.Servlist(mask, type), priority);
        }

        public void Servlist(string mask, string type)
        {
            WriteLine(Rfc2812.Servlist(mask, type));
        }
        
        public void Squery(string servicename, string text, Priority priority)
        {
            WriteLine(Rfc2812.Squery(servicename, text), priority);
        }

        public void Squery(string servicename, string text)
        {
            WriteLine(Rfc2812.Squery(servicename, text));
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

        public void Who(Priority priority)
        {
            WriteLine(Rfc2812.Who(), priority);
        }

        public void Who()
        {
            WriteLine(Rfc2812.Who());
        }

        public void Who(string mask, Priority priority)
        {
            WriteLine(Rfc2812.Who(mask), priority);
        }

        public void Who(string mask)
        {
            WriteLine(Rfc2812.Who(mask));
        }

        public void Who(string mask, bool ircop, Priority priority)
        {
            WriteLine(Rfc2812.Who(mask, ircop), priority);
        }

        public void Who(string mask, bool ircop)
        {
            WriteLine(Rfc2812.Who(mask, ircop));
        }

        public void Whois(string mask, Priority priority)
        {
            WriteLine(Rfc2812.Whois(mask), priority);
        }

        public void Whois(string mask)
        {
            WriteLine(Rfc2812.Whois(mask));
        }

        public void Whois(string[] masks, Priority priority)
        {
            WriteLine(Rfc2812.Whois(masks), priority);
        }

        public void Whois(string[] masks)
        {
            WriteLine(Rfc2812.Whois(masks));
        }

        public void Whois(string target, string mask, Priority priority)
        {
            WriteLine(Rfc2812.Whois(target, mask), priority);
        }

        public void Whois(string target, string mask)
        {
            WriteLine(Rfc2812.Whois(target, mask));
        }

        public void Whois(string target, string[] masks, Priority priority)
        {
            WriteLine(Rfc2812.Whois(target ,masks), priority);
        }

        public void Whois(string target, string[] masks)
        {
            WriteLine(Rfc2812.Whois(target, masks));
        }

        public void Whowas(string nickname, Priority priority)
        {
            WriteLine(Rfc2812.Whowas(nickname), priority);
        }

        public void Whowas(string nickname)
        {
            WriteLine(Rfc2812.Whowas(nickname));
        }

        public void Whowas(string[] nicknames, Priority priority)
        {
            WriteLine(Rfc2812.Whowas(nicknames), priority);
        }

        public void Whowas(string[] nicknames)
        {
            WriteLine(Rfc2812.Whowas(nicknames));
        }

        public void Whowas(string nickname, string count, Priority priority)
        {
            WriteLine(Rfc2812.Whowas(nickname, count), priority);
        }

        public void Whowas(string nickname, string count)
        {
            WriteLine(Rfc2812.Whowas(nickname, count));
        }

        public void Whowas(string[] nicknames, string count, Priority priority)
        {
            WriteLine(Rfc2812.Whowas(nicknames, count), priority);
        }

        public void Whowas(string[] nicknames, string count)
        {
            WriteLine(Rfc2812.Whowas(nicknames, count));
        }

        public void Whowas(string nickname, string count, string target, Priority priority)
        {
            WriteLine(Rfc2812.Whowas(nickname, count, target), priority);
        }

        public void Whowas(string nickname, string count, string target)
        {
            WriteLine(Rfc2812.Whowas(nickname, count, target));
        }

        public void Whowas(string[] nicknames, string count, string target, Priority priority)
        {
            WriteLine(Rfc2812.Whowas(nicknames, count, target), priority);
        }

        public void Whowas(string[] nicknames, string count, string target)
        {
            WriteLine(Rfc2812.Whowas(nicknames, count, target));
        }

        public void Kill(string nickname, string comment, Priority priority)
        {
            WriteLine(Rfc2812.Kill(nickname, comment), priority);
        }

        public void Kill(string nickname, string comment)
        {
            WriteLine(Rfc2812.Kill(nickname, comment));
        }
        
        public void Ping(string server, Priority priority)
        {
            WriteLine(Rfc2812.Ping(server), priority);
        }

        public void Ping(string server)
        {
            WriteLine(Rfc2812.Ping(server));
        }
        
        public void Ping(string server, string server2, Priority priority)
        {
            WriteLine(Rfc2812.Ping(server, server2), priority);
        }

        public void Ping(string server, string server2)
        {
            WriteLine(Rfc2812.Ping(server, server2));
        }
        
        public void Pong(string server, Priority priority)
        {
            WriteLine(Rfc2812.Pong(server), priority);
        }

        public void Pong(string server)
        {
            WriteLine(Rfc2812.Pong(server));
        }
        
        public void Pong(string server, string server2, Priority priority)
        {
            WriteLine(Rfc2812.Pong(server, server2), priority);
        }

        public void Pong(string server, string server2)
        {
            WriteLine(Rfc2812.Pong(server, server2));
        }
        
        public void Away(Priority priority)
        {
            WriteLine(Rfc2812.Away(), priority);
        }

        public void Away()
        {
            WriteLine(Rfc2812.Away());
        }
        
        public void Away(string text, Priority priority)
        {
            WriteLine(Rfc2812.Away(text), priority);
        }

        public void Away(string text)
        {
            WriteLine(Rfc2812.Away(text));
        }
        
        public void Rehash()
        {
            WriteLine(Rfc2812.Rehash());
        }
        
        public void Die()
        {
            WriteLine(Rfc2812.Die());
        }
        
        public void Restart()
        {
            WriteLine(Rfc2812.Restart());
        }
        
        public void Summon(string user, Priority priority)
        {
            WriteLine(Rfc2812.Summon(user), priority);
        }

        public void Summon(string user)
        {
            WriteLine(Rfc2812.Summon(user));
        }

        public void Summon(string user, string target, Priority priority)
        {
            WriteLine(Rfc2812.Summon(user, target), priority);
        }

        public void Summon(string user, string target)
        {
            WriteLine(Rfc2812.Summon(user, target));
        }

        public void Summon(string user, string target, string channel, Priority priority)
        {
            WriteLine(Rfc2812.Summon(user, target, channel), priority);
        }

        public void Summon(string user, string target, string channel)
        {
            WriteLine(Rfc2812.Summon(user, target, channel));
        }

        public void Users(Priority priority)
        {
            WriteLine(Rfc2812.Users(), priority);
        }

        public void Users()
        {
            WriteLine(Rfc2812.Users());
        }

        public void Users(string target, Priority priority)
        {
            WriteLine(Rfc2812.Users(target), priority);
        }

        public void Users(string target)
        {
            WriteLine(Rfc2812.Users(target));
        }

        public void Wallops(string text, Priority priority)
        {
            WriteLine(Rfc2812.Wallops(text), priority);
        }

        public void Wallops(string text)
        {
            WriteLine(Rfc2812.Wallops(text));
        }

        public void Userhost(string nickname, Priority priority)
        {
            WriteLine(Rfc2812.Userhost(nickname), priority);
        }

        public void Userhost(string nickname)
        {
            WriteLine(Rfc2812.Userhost(nickname));
        }

        public void Userhost(string[] nicknames, Priority priority)
        {
            WriteLine(Rfc2812.Userhost(nicknames), priority);
        }

        public void Userhost(string[] nicknames)
        {
            WriteLine(Rfc2812.Userhost(nicknames));
        }

        public void Ison(string nickname, Priority priority)
        {
            WriteLine(Rfc2812.Ison(nickname), priority);
        }

        public void Ison(string nickname)
        {
            WriteLine(Rfc2812.Ison(nickname));
        }

        public void Ison(string[] nicknames, Priority priority)
        {
            WriteLine(Rfc2812.Ison(nicknames), priority);
        }

        public void Ison(string[] nicknames)
        {
            WriteLine(Rfc2812.Ison(nicknames));
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
