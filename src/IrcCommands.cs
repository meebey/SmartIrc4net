/**
 * $Id: IrcCommands.cs,v 1.2 2003/11/21 23:40:52 meebey Exp $
 * $Revision: 1.2 $
 * $Author: meebey $
 * $Date: 2003/11/21 23:40:52 $
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
        private Connection _Connection;

        public IrcCommands(Connection con)
        {
            _Connection = con;
        }

        public void Message(MessageType type, string destination, string message, Priority priority)
        {
            switch(type) {
                case MessageType.Channel:
                case MessageType.Query:
                    Privmsg(destination, message, priority);
                break;
                case MessageType.Action:
                    Privmsg(destination, (char)1+"ACTION "+message+(char)1, priority);
                break;
                case MessageType.Notice:
                    Notice(destination, message, priority);
                break;
                case MessageType.CtcpReply:
                    Notice(destination, (char)1+message+(char)1, priority);
                break;
                case MessageType.CtcpRequest:
                    Privmsg(destination, (char)1+message+(char)1, priority);
                break;
            }
        }

        public void Message(MessageType type, string destination, string message)
        {
            Message(type, destination, message, Priority.Medium);
        }

        public void Pong(string data)
        {
            _Connection.WriteLine(Rfc2812.Pong(data), Priority.Critical);
        }

        public void Pass(string password, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Pass(password), priority);
        }

        public void Pass(string password)
        {
            _Connection.WriteLine(Rfc2812.Pass(password));
        }

        public void User(string username, int usermode, string realname, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.User(username, usermode, realname), priority);
        }

        public void User(string username, int usermode, string realname)
        {
            _Connection.WriteLine(Rfc2812.User(username, usermode, realname));
        }

        public void Privmsg(string destination, string message, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Privmsg(destination, message), priority);
        }

        public void Privmsg(string destination, string message)
        {
            _Connection.WriteLine(Rfc2812.Privmsg(destination, message));
        }

        public void Notice(string destination, string message, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Notice(destination, message), priority);
        }

        public void Notice(string destination, string message)
        {
            _Connection.WriteLine(Rfc2812.Notice(destination, message));
        }

        public void Join(string channel, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Join(channel), priority);
        }

        public void Join(string channel)
        {
            _Connection.WriteLine(Rfc2812.Join(channel));
        }

        public void Part(string channel, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Part(channel), priority);
        }

        public void Part(string channel)
        {
            _Connection.WriteLine(Rfc2812.Part(channel));
        }

        public void Part(string channel, string reason, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Part(channel, reason), priority);
        }

        public void Part(string channel, string reason)
        {
            _Connection.WriteLine(Rfc2812.Part(channel, reason));
        }

        public void Kick(string channel, string nickname, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Kick(channel, nickname), priority);
        }

        public void Kick(string channel, string nickname)
        {
            _Connection.WriteLine(Rfc2812.Kick(channel, nickname));
        }

        public void Kick(string channel, string nickname, string reason, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Kick(channel, nickname, reason), priority);
        }

        public void Kick(string channel, string nickname, string reason)
        {
            _Connection.WriteLine(Rfc2812.Kick(channel, nickname, reason));
        }

        public void List(string channel, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.List(channel), priority);
        }

        public void List(string channel)
        {
            _Connection.WriteLine(Rfc2812.List(channel));
        }

        public void Names(string channel, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Names(channel), priority);
        }

        public void Names(string channel)
        {
            _Connection.WriteLine(Rfc2812.Names(channel));
        }

        public void Topic(string channel, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Topic(channel), priority);
        }

        public void Topic(string channel)
        {
            _Connection.WriteLine(Rfc2812.Topic(channel));
        }

        public void Topic(string channel, string newtopic, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Topic(channel, newtopic), priority);
        }

        public void Topic(string channel, string newtopic)
        {
            _Connection.WriteLine(Rfc2812.Topic(channel, newtopic));
        }

        public void Mode(string target, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Mode(target), priority);
        }

        public void Mode(string target)
        {
            _Connection.WriteLine(Rfc2812.Mode(target));
        }

        public void Mode(string target, string newmode, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Mode(target, newmode), priority);
        }

        public void Mode(string target, string newmode)
        {
            _Connection.WriteLine(Rfc2812.Mode(target, newmode));
        }

        public void Op(string channel, string nickname, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Mode(channel, "+o "+nickname), priority);
        }

        public void Op(string channel, string nickname)
        {
            _Connection.WriteLine(Rfc2812.Mode(channel, "+o "+nickname));
        }

        public void Deop(string channel, string nickname, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Mode(channel, "-o "+nickname), priority);
        }

        public void Deop(string channel, string nickname)
        {
            _Connection.WriteLine(Rfc2812.Mode(channel, "-o "+nickname));
        }

        public void Voice(string channel, string nickname, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Mode(channel, "+v "+nickname), priority);
        }

        public void Voice(string channel, string nickname)
        {
            _Connection.WriteLine(Rfc2812.Mode(channel, "+v "+nickname));
        }

        public void Devoice(string channel, string nickname, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Mode(channel, "-v "+nickname), priority);
        }

        public void Devoice(string channel, string nickname)
        {
            _Connection.WriteLine(Rfc2812.Mode(channel, "-v "+nickname));
        }

        public void Ban(string channel, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Mode(channel, "b"), priority);
        }

        public void Ban(string channel)
        {
            _Connection.WriteLine(Rfc2812.Mode(channel, "b"));
        }

        public void Ban(string channel, string hostmask, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Mode(channel, "+b "+hostmask), priority);
        }

        public void Ban(string channel, string hostmask)
        {
            _Connection.WriteLine(Rfc2812.Mode(channel, "+b "+hostmask));
        }

        public void Unban(string channel, string hostmask, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Mode(channel, "-b "+hostmask), priority);
        }

        public void Unban(string channel, string hostmask)
        {
            _Connection.WriteLine(Rfc2812.Mode(channel, "-b "+hostmask));
        }

        public void Invite(string nickname, string channel, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Invite(nickname, channel), priority);
        }

        public void Invite(string nickname, string channel)
        {
            _Connection.WriteLine(Rfc2812.Invite(nickname, channel));
        }

        public void Nick(string newnickname, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Nick(newnickname), priority);
        }

        public void Nick(string newnickname)
        {
            _Connection.WriteLine(Rfc2812.Nick(newnickname));
        }

        public void Who(string target, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Who(target), priority);
        }

        public void Who(string target)
        {
            _Connection.WriteLine(Rfc2812.Who(target));
        }

        public void Whois(string target, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Whois(target), priority);
        }

        public void Whois(string target)
        {
            _Connection.WriteLine(Rfc2812.Whois(target));
        }

        public void Whowas(string target, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Whowas(target), priority);
        }

        public void Whowas(string target)
        {
            _Connection.WriteLine(Rfc2812.Whowas(target));
        }

        public void Quit(Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Quit(), priority);
        }

        public void Quit()
        {
            _Connection.WriteLine(Rfc2812.Quit());
        }

        public void Quit(string reason, Priority priority)
        {
            _Connection.WriteLine(Rfc2812.Quit(reason), priority);
        }

        public void Quit(string reason)
        {
            _Connection.WriteLine(Rfc2812.Quit(reason));
        }
    }
}
