/**
 * $Id$
 * $URL$
 * $Rev$
 * $Author$
 * $Date$
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 *
 * Copyright (c) 2003-2004 Mirco Bauer <meebey@meebey.net> <http://www.meebey.net>
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
        // API commands
        public void SendMessage(SendType type, string destination, string message, Priority priority)
        {
            switch(type) {
                case SendType.Message:
                    RfcPrivmsg(destination, message, priority);
                break;
                case SendType.Action:
                    RfcPrivmsg(destination, "\x1"+"ACTION "+message+"\x1", priority);
                break;
                case SendType.Notice:
                    RfcNotice(destination, message, priority);
                break;
                case SendType.CtcpRequest:
                    RfcPrivmsg(destination, "\x1"+message+"\x1", priority);
                break;
                case SendType.CtcpReply:
                    RfcNotice(destination, "\x1"+message+"\x1", priority);
                break;
            }
        }

        public void SendMessage(SendType type, string destination, string message)
        {
            SendMessage(type, destination, message, Priority.Medium);
        }

        public void SendReply(IrcMessageData data, string message, Priority priority)
        {
            switch (data.Type) {
                case ReceiveType.ChannelMessage:
                    SendMessage(SendType.Message, data.Channel, data.Message);
                break;
                case ReceiveType.QueryMessage:
                    SendMessage(SendType.Message, data.Nick, data.Message);
                break;
                case ReceiveType.QueryNotice:
                    SendMessage(SendType.Notice, data.Nick, data.Message);
                break;
            }
        }

        public void SendReply(IrcMessageData data, string message)
        {
            SendReply(data, message, Priority.Medium);
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
        
        // non-RFC commands
        public void Halfop(string channel, string nickname, Priority priority)
        {
            WriteLine(Rfc2812.Mode(channel, "+h "+nickname), priority);
        }

        public void Dehalfop(string channel, string nickname)
        {
            WriteLine(Rfc2812.Mode(channel, "-h "+nickname));
        }

        // RFC commands
        public void RfcPass(string password, Priority priority)
        {
            WriteLine(Rfc2812.Pass(password), priority);
        }

        public void RfcPass(string password)
        {
            WriteLine(Rfc2812.Pass(password));
        }

        public void RfcUser(string username, int usermode, string realname, Priority priority)
        {
            WriteLine(Rfc2812.User(username, usermode, realname), priority);
        }

        public void RfcUser(string username, int usermode, string realname)
        {
            WriteLine(Rfc2812.User(username, usermode, realname));
        }

        public void RfcOper(string name, string password, Priority priority)
        {
            WriteLine(Rfc2812.Oper(name, password), priority);
        }

        public void RfcOper(string name, string password)
        {
            WriteLine(Rfc2812.Oper(name, password));
        }

        public void RfcPrivmsg(string destination, string message, Priority priority)
        {
            WriteLine(Rfc2812.Privmsg(destination, message), priority);
        }

        public void RfcPrivmsg(string destination, string message)
        {
            WriteLine(Rfc2812.Privmsg(destination, message));
        }

        public void RfcNotice(string destination, string message, Priority priority)
        {
            WriteLine(Rfc2812.Notice(destination, message), priority);
        }

        public void RfcNotice(string destination, string message)
        {
            WriteLine(Rfc2812.Notice(destination, message));
        }

        public void RfcJoin(string channel, Priority priority)
        {
            WriteLine(Rfc2812.Join(channel), priority);
        }

        public void RfcJoin(string channel)
        {
            WriteLine(Rfc2812.Join(channel));
        }

        public void RfcJoin(string[] channels, Priority priority)
        {
            WriteLine(Rfc2812.Join(channels), priority);
        }
        
        public void RfcJoin(string[] channels)
        {
            WriteLine(Rfc2812.Join(channels));
        }
        
        public void RfcJoin(string channel, string key, Priority priority)
        {
            WriteLine(Rfc2812.Join(channel, key), priority);
        }

        public void RfcJoin(string channel, string key)
        {
            WriteLine(Rfc2812.Join(channel, key));
        }

        public void RfcJoin(string[] channels, string[] keys, Priority priority)
        {
            WriteLine(Rfc2812.Join(channels, keys), priority);
        }

        public void RfcJoin(string[] channels, string[] keys)
        {
            WriteLine(Rfc2812.Join(channels, keys));
        }

        public void RfcPart(string channel, Priority priority)
        {
            WriteLine(Rfc2812.Part(channel), priority);
        }

        public void RfcPart(string channel)
        {
            WriteLine(Rfc2812.Part(channel));
        }

        public void RfcPart(string[] channels, Priority priority)
        {
            WriteLine(Rfc2812.Part(channels), priority);
        }

        public void RfcPart(string[] channels)
        {
            WriteLine(Rfc2812.Part(channels));
        }

        public void RfcPart(string channel, string partmessage, Priority priority)
        {
            WriteLine(Rfc2812.Part(channel, partmessage), priority);
        }

        public void RfcPart(string channel, string partmessage)
        {
            WriteLine(Rfc2812.Part(channel, partmessage));
        }

        public void RfcPart(string[] channels, string partmessage, Priority priority)
        {
            WriteLine(Rfc2812.Part(channels, partmessage), priority);
        }

        public void RfcPart(string[] channels, string partmessage)
        {
            WriteLine(Rfc2812.Part(channels, partmessage));
        }

        public void RfcKick(string channel, string nickname, Priority priority)
        {
            WriteLine(Rfc2812.Kick(channel, nickname), priority);
        }

        public void RfcKick(string channel, string nickname)
        {
            WriteLine(Rfc2812.Kick(channel, nickname));
        }

        public void RfcKick(string[] channels, string nickname, Priority priority)
        {
            WriteLine(Rfc2812.Kick(channels, nickname), priority);
        }

        public void RfcKick(string[] channels, string nickname)
        {
            WriteLine(Rfc2812.Kick(channels, nickname));
        }
        
        public void RfcKick(string channel, string[] nicknames, Priority priority)
        {
            WriteLine(Rfc2812.Kick(channel, nicknames), priority);
        }

        public void RfcKick(string channel, string[] nicknames)
        {
            WriteLine(Rfc2812.Kick(channel, nicknames));
        }
        
        public void RfcKick(string[] channels, string[] nicknames, Priority priority)
        {
            WriteLine(Rfc2812.Kick(channels, nicknames), priority);
        }

        public void RfcKick(string[] channels, string[] nicknames)
        {
            WriteLine(Rfc2812.Kick(channels, nicknames));
        }
        
        public void RfcKick(string channel, string nickname, string comment, Priority priority)
        {
            WriteLine(Rfc2812.Kick(channel, nickname, comment), priority);
        }

        public void RfcKick(string channel, string nickname, string comment)
        {
            WriteLine(Rfc2812.Kick(channel, nickname, comment));
        }
        
        public void RfcKick(string[] channels, string nickname, string comment, Priority priority)
        {
            WriteLine(Rfc2812.Kick(channels, nickname, comment), priority);
        }

        public void RfcKick(string[] channels, string nickname, string comment)
        {
            WriteLine(Rfc2812.Kick(channels, nickname, comment));
        }

        public void RfcKick(string channel, string[] nicknames, string comment, Priority priority)
        {
            WriteLine(Rfc2812.Kick(channel, nicknames, comment), priority);
        }

        public void RfcKick(string channel, string[] nicknames, string comment)
        {
            WriteLine(Rfc2812.Kick(channel, nicknames, comment));
        }

        public void RfcKick(string[] channels, string[] nicknames, string comment, Priority priority)
        {
            WriteLine(Rfc2812.Kick(channels, nicknames, comment), priority);
        }

        public void RfcKick(string[] channels, string[] nicknames, string comment)
        {
            WriteLine(Rfc2812.Kick(channels, nicknames, comment));
        }

        public void RfcMotd(Priority priority)
        {
            WriteLine(Rfc2812.Motd(), priority);
        }

        public void RfcMotd()
        {
            WriteLine(Rfc2812.Motd());
        }

        public void RfcMotd(string target, Priority priority)
        {
            WriteLine(Rfc2812.Motd(target), priority);
        }

        public void RfcMotd(string target)
        {
            WriteLine(Rfc2812.Motd(target));
        }

        public void RfcLuser(Priority priority)
        {
            WriteLine(Rfc2812.Luser(), priority);
        }

        public void RfcLuser()
        {
            WriteLine(Rfc2812.Luser());
        }

        public void RfcLuser(string mask, Priority priority)
        {
            WriteLine(Rfc2812.Luser(mask), priority);
        }

        public void RfcLuser(string mask)
        {
            WriteLine(Rfc2812.Luser(mask));
        }

        public void RfcLuser(string mask, string target, Priority priority)
        {
            WriteLine(Rfc2812.Luser(mask, target), priority);
        }

        public void RfcLuser(string mask, string target)
        {
            WriteLine(Rfc2812.Luser(mask, target));
        }

        public void RfcVersion(Priority priority)
        {
            WriteLine(Rfc2812.Version(), priority);
        }

        public void RfcVersion()
        {
            WriteLine(Rfc2812.Version());
        }

        public void RfcVersion(string target, Priority priority)
        {
            WriteLine(Rfc2812.Version(target), priority);
        }

        public void RfcVersion(string target)
        {
            WriteLine(Rfc2812.Version(target));
        }

        public void RfcStats(Priority priority)
        {
            WriteLine(Rfc2812.Stats(), priority);
        }

        public void RfcStats()
        {
            WriteLine(Rfc2812.Stats());
        }

        public void RfcStats(string query, Priority priority)
        {
            WriteLine(Rfc2812.Stats(query), priority);
        }

        public void RfcStats(string query)
        {
            WriteLine(Rfc2812.Stats(query));
        }
        
        public void RfcStats(string query, string target, Priority priority)
        {
            WriteLine(Rfc2812.Stats(query, target), priority);
        }

        public void RfcStats(string query, string target)
        {
            WriteLine(Rfc2812.Stats(query, target));
        }
        
        public void RfcLinks()
        {
            WriteLine(Rfc2812.Links());
        }

        public void RfcLinks(string servermask, Priority priority)
        {
            WriteLine(Rfc2812.Links(servermask), priority);
        }

        public void RfcLinks(string servermask)
        {
            WriteLine(Rfc2812.Links(servermask));
        }
        
        public void RfcLinks(string remoteserver, string servermask, Priority priority)
        {
            WriteLine(Rfc2812.Links(remoteserver, servermask), priority);
        }

        public void RfcLinks(string remoteserver, string servermask)
        {
            WriteLine(Rfc2812.Links(remoteserver, servermask));
        }
        
        public void RfcTime(Priority priority)
        {
            WriteLine(Rfc2812.Time(), priority);
        }

        public void RfcTime()
        {
            WriteLine(Rfc2812.Time());
        }
        
        public void RfcTime(string target, Priority priority)
        {
            WriteLine(Rfc2812.Time(target), priority);
        }

        public void RfcTime(string target)
        {
            WriteLine(Rfc2812.Time(target));
        }
        
        public void RfcConnect(string targetserver, string port, Priority priority)
        {
            WriteLine(Rfc2812.Connect(targetserver, port), priority);
        }

        public void RfcConnect(string targetserver, string port)
        {
            WriteLine(Rfc2812.Connect(targetserver, port));
        }
        
        public void RfcConnect(string targetserver, string port, string remoteserver, Priority priority)
        {
            WriteLine(Rfc2812.Connect(targetserver, port, remoteserver), priority);
        }

        public void RfcConnect(string targetserver, string port, string remoteserver)
        {
            WriteLine(Rfc2812.Connect(targetserver, port, remoteserver));
        }
        
        public void RfcTrace(Priority priority)
        {
            WriteLine(Rfc2812.Trace(), priority);
        }

        public void RfcTrace()
        {
            WriteLine(Rfc2812.Trace());
        }
        
        public void RfcTrace(string target, Priority priority)
        {
            WriteLine(Rfc2812.Trace(target), priority);
        }

        public void RfcTrace(string target)
        {
            WriteLine(Rfc2812.Trace(target));
        }
        
        public void RfcAdmin(Priority priority)
        {
            WriteLine(Rfc2812.Admin(), priority);
        }

        public void RfcAdmin()
        {
            WriteLine(Rfc2812.Admin());
        }
        
        public void RfcAdmin(string target, Priority priority)
        {
            WriteLine(Rfc2812.Admin(target), priority);
        }

        public void RfcAdmin(string target)
        {
            WriteLine(Rfc2812.Admin(target));
        }
        
        public void RfcInfo(Priority priority)
        {
            WriteLine(Rfc2812.Info(), priority);
        }

        public void RfcInfo()
        {
            WriteLine(Rfc2812.Info());
        }
        
        public void RfcInfo(string target, Priority priority)
        {
            WriteLine(Rfc2812.Info(target), priority);
        }

        public void RfcInfo(string target)
        {
            WriteLine(Rfc2812.Info(target));
        }
        
        public void RfcServlist(Priority priority)
        {
            WriteLine(Rfc2812.Servlist(), priority);
        }

        public void RfcServlist()
        {
            WriteLine(Rfc2812.Servlist());
        }
        
        public void RfcServlist(string mask, Priority priority)
        {
            WriteLine(Rfc2812.Servlist(mask), priority);
        }

        public void RfcServlist(string mask)
        {
            WriteLine(Rfc2812.Servlist(mask));
        }
        
        public void RfcServlist(string mask, string type, Priority priority)
        {
            WriteLine(Rfc2812.Servlist(mask, type), priority);
        }

        public void RfcServlist(string mask, string type)
        {
            WriteLine(Rfc2812.Servlist(mask, type));
        }
        
        public void RfcSquery(string servicename, string text, Priority priority)
        {
            WriteLine(Rfc2812.Squery(servicename, text), priority);
        }

        public void RfcSquery(string servicename, string text)
        {
            WriteLine(Rfc2812.Squery(servicename, text));
        }
        
        public void RfcList(string channel, Priority priority)
        {
            WriteLine(Rfc2812.List(channel), priority);
        }

        public void RfcList(string channel)
        {
            WriteLine(Rfc2812.List(channel));
        }

        public void RfcList(string[] channels, Priority priority)
        {
            WriteLine(Rfc2812.List(channels), priority);
        }

        public void RfcList(string[] channels)
        {
            WriteLine(Rfc2812.List(channels));
        }

        public void RfcList(string channel, string target, Priority priority)
        {
            WriteLine(Rfc2812.List(channel, target), priority);
        }

        public void RfcList(string channel, string target)
        {
            WriteLine(Rfc2812.List(channel, target));
        }

        public void RfcList(string[] channels, string target, Priority priority)
        {
            WriteLine(Rfc2812.List(channels, target), priority);
        }

        public void RfcList(string[] channels, string target)
        {
            WriteLine(Rfc2812.List(channels, target));
        }

        public void RfcNames(string channel, Priority priority)
        {
            WriteLine(Rfc2812.Names(channel), priority);
        }

        public void RfcNames(string channel)
        {
            WriteLine(Rfc2812.Names(channel));
        }

        public void RfcNames(string[] channels, Priority priority)
        {
            WriteLine(Rfc2812.Names(channels), priority);
        }

        public void RfcNames(string[] channels)
        {
            WriteLine(Rfc2812.Names(channels));
        }

        public void RfcNames(string channel, string target, Priority priority)
        {
            WriteLine(Rfc2812.Names(channel, target), priority);
        }

        public void RfcNames(string channel, string target)
        {
            WriteLine(Rfc2812.Names(channel, target));
        }

        public void RfcNames(string[] channels, string target, Priority priority)
        {
            WriteLine(Rfc2812.Names(channels, target), priority);
        }

        public void RfcNames(string[] channels, string target)
        {
            WriteLine(Rfc2812.Names(channels, target));
        }

        public void RfcTopic(string channel, Priority priority)
        {
            WriteLine(Rfc2812.Topic(channel), priority);
        }

        public void RfcTopic(string channel)
        {
            WriteLine(Rfc2812.Topic(channel));
        }

        public void RfcTopic(string channel, string newtopic, Priority priority)
        {
            WriteLine(Rfc2812.Topic(channel, newtopic), priority);
        }

        public void RfcTopic(string channel, string newtopic)
        {
            WriteLine(Rfc2812.Topic(channel, newtopic));
        }

        public void RfcMode(string target, Priority priority)
        {
            WriteLine(Rfc2812.Mode(target), priority);
        }

        public void RfcMode(string target)
        {
            WriteLine(Rfc2812.Mode(target));
        }

        public void RfcMode(string target, string newmode, Priority priority)
        {
            WriteLine(Rfc2812.Mode(target, newmode), priority);
        }

        public void RfcMode(string target, string newmode)
        {
            WriteLine(Rfc2812.Mode(target, newmode));
        }

        public void RfcService(string nickname, string distribution, string info, Priority priority)
        {
            WriteLine(Rfc2812.Service(nickname, distribution, info), priority);
        }

        public void RfcService(string nickname, string distribution, string info)
        {
            WriteLine(Rfc2812.Service(nickname, distribution, info));
        }

        public void RfcInvite(string nickname, string channel, Priority priority)
        {
            WriteLine(Rfc2812.Invite(nickname, channel), priority);
        }

        public void RfcInvite(string nickname, string channel)
        {
            WriteLine(Rfc2812.Invite(nickname, channel));
        }

        public void RfcNick(string newnickname, Priority priority)
        {
            WriteLine(Rfc2812.Nick(newnickname), priority);
        }

        public void RfcNick(string newnickname)
        {
            WriteLine(Rfc2812.Nick(newnickname));
        }

        public void RfcWho(Priority priority)
        {
            WriteLine(Rfc2812.Who(), priority);
        }

        public void RfcWho()
        {
            WriteLine(Rfc2812.Who());
        }

        public void RfcWho(string mask, Priority priority)
        {
            WriteLine(Rfc2812.Who(mask), priority);
        }

        public void RfcWho(string mask)
        {
            WriteLine(Rfc2812.Who(mask));
        }

        public void RfcWho(string mask, bool ircop, Priority priority)
        {
            WriteLine(Rfc2812.Who(mask, ircop), priority);
        }

        public void RfcWho(string mask, bool ircop)
        {
            WriteLine(Rfc2812.Who(mask, ircop));
        }

        public void RfcWhois(string mask, Priority priority)
        {
            WriteLine(Rfc2812.Whois(mask), priority);
        }

        public void RfcWhois(string mask)
        {
            WriteLine(Rfc2812.Whois(mask));
        }

        public void RfcWhois(string[] masks, Priority priority)
        {
            WriteLine(Rfc2812.Whois(masks), priority);
        }

        public void RfcWhois(string[] masks)
        {
            WriteLine(Rfc2812.Whois(masks));
        }

        public void RfcWhois(string target, string mask, Priority priority)
        {
            WriteLine(Rfc2812.Whois(target, mask), priority);
        }

        public void RfcWhois(string target, string mask)
        {
            WriteLine(Rfc2812.Whois(target, mask));
        }

        public void RfcWhois(string target, string[] masks, Priority priority)
        {
            WriteLine(Rfc2812.Whois(target ,masks), priority);
        }

        public void RfcWhois(string target, string[] masks)
        {
            WriteLine(Rfc2812.Whois(target, masks));
        }

        public void RfcWhowas(string nickname, Priority priority)
        {
            WriteLine(Rfc2812.Whowas(nickname), priority);
        }

        public void RfcWhowas(string nickname)
        {
            WriteLine(Rfc2812.Whowas(nickname));
        }

        public void RfcWhowas(string[] nicknames, Priority priority)
        {
            WriteLine(Rfc2812.Whowas(nicknames), priority);
        }

        public void RfcWhowas(string[] nicknames)
        {
            WriteLine(Rfc2812.Whowas(nicknames));
        }

        public void RfcWhowas(string nickname, string count, Priority priority)
        {
            WriteLine(Rfc2812.Whowas(nickname, count), priority);
        }

        public void RfcWhowas(string nickname, string count)
        {
            WriteLine(Rfc2812.Whowas(nickname, count));
        }

        public void RfcWhowas(string[] nicknames, string count, Priority priority)
        {
            WriteLine(Rfc2812.Whowas(nicknames, count), priority);
        }

        public void RfcWhowas(string[] nicknames, string count)
        {
            WriteLine(Rfc2812.Whowas(nicknames, count));
        }

        public void RfcWhowas(string nickname, string count, string target, Priority priority)
        {
            WriteLine(Rfc2812.Whowas(nickname, count, target), priority);
        }

        public void RfcWhowas(string nickname, string count, string target)
        {
            WriteLine(Rfc2812.Whowas(nickname, count, target));
        }

        public void RfcWhowas(string[] nicknames, string count, string target, Priority priority)
        {
            WriteLine(Rfc2812.Whowas(nicknames, count, target), priority);
        }

        public void RfcWhowas(string[] nicknames, string count, string target)
        {
            WriteLine(Rfc2812.Whowas(nicknames, count, target));
        }

        public void RfcKill(string nickname, string comment, Priority priority)
        {
            WriteLine(Rfc2812.Kill(nickname, comment), priority);
        }

        public void RfcKill(string nickname, string comment)
        {
            WriteLine(Rfc2812.Kill(nickname, comment));
        }
        
        public void RfcPing(string server, Priority priority)
        {
            WriteLine(Rfc2812.Ping(server), priority);
        }

        public void RfcPing(string server)
        {
            WriteLine(Rfc2812.Ping(server));
        }
        
        public void RfcPing(string server, string server2, Priority priority)
        {
            WriteLine(Rfc2812.Ping(server, server2), priority);
        }

        public void RfcPing(string server, string server2)
        {
            WriteLine(Rfc2812.Ping(server, server2));
        }
        
        public void RfcPong(string server, Priority priority)
        {
            WriteLine(Rfc2812.Pong(server), priority);
        }

        public void RfcPong(string server)
        {
            WriteLine(Rfc2812.Pong(server));
        }
        
        public void RfcPong(string server, string server2, Priority priority)
        {
            WriteLine(Rfc2812.Pong(server, server2), priority);
        }

        public void RfcPong(string server, string server2)
        {
            WriteLine(Rfc2812.Pong(server, server2));
        }
        
        public void RfcAway(Priority priority)
        {
            WriteLine(Rfc2812.Away(), priority);
        }

        public void RfcAway()
        {
            WriteLine(Rfc2812.Away());
        }
        
        public void RfcAway(string text, Priority priority)
        {
            WriteLine(Rfc2812.Away(text), priority);
        }

        public void RfcAway(string text)
        {
            WriteLine(Rfc2812.Away(text));
        }
        
        public void RfcRehash()
        {
            WriteLine(Rfc2812.Rehash());
        }
        
        public void RfcDie()
        {
            WriteLine(Rfc2812.Die());
        }
        
        public void RfcRestart()
        {
            WriteLine(Rfc2812.Restart());
        }
        
        public void RfcSummon(string user, Priority priority)
        {
            WriteLine(Rfc2812.Summon(user), priority);
        }

        public void RfcSummon(string user)
        {
            WriteLine(Rfc2812.Summon(user));
        }

        public void RfcSummon(string user, string target, Priority priority)
        {
            WriteLine(Rfc2812.Summon(user, target), priority);
        }

        public void RfcSummon(string user, string target)
        {
            WriteLine(Rfc2812.Summon(user, target));
        }

        public void RfcSummon(string user, string target, string channel, Priority priority)
        {
            WriteLine(Rfc2812.Summon(user, target, channel), priority);
        }

        public void RfcSummon(string user, string target, string channel)
        {
            WriteLine(Rfc2812.Summon(user, target, channel));
        }

        public void RfcUsers(Priority priority)
        {
            WriteLine(Rfc2812.Users(), priority);
        }

        public void RfcUsers()
        {
            WriteLine(Rfc2812.Users());
        }

        public void RfcUsers(string target, Priority priority)
        {
            WriteLine(Rfc2812.Users(target), priority);
        }

        public void RfcUsers(string target)
        {
            WriteLine(Rfc2812.Users(target));
        }

        public void RfcWallops(string text, Priority priority)
        {
            WriteLine(Rfc2812.Wallops(text), priority);
        }

        public void RfcWallops(string text)
        {
            WriteLine(Rfc2812.Wallops(text));
        }

        public void RfcUserhost(string nickname, Priority priority)
        {
            WriteLine(Rfc2812.Userhost(nickname), priority);
        }

        public void RfcUserhost(string nickname)
        {
            WriteLine(Rfc2812.Userhost(nickname));
        }

        public void RfcUserhost(string[] nicknames, Priority priority)
        {
            WriteLine(Rfc2812.Userhost(nicknames), priority);
        }

        public void RfcUserhost(string[] nicknames)
        {
            WriteLine(Rfc2812.Userhost(nicknames));
        }

        public void RfcIson(string nickname, Priority priority)
        {
            WriteLine(Rfc2812.Ison(nickname), priority);
        }

        public void RfcIson(string nickname)
        {
            WriteLine(Rfc2812.Ison(nickname));
        }

        public void RfcIson(string[] nicknames, Priority priority)
        {
            WriteLine(Rfc2812.Ison(nicknames), priority);
        }

        public void RfcIson(string[] nicknames)
        {
            WriteLine(Rfc2812.Ison(nicknames));
        }

        public void RfcQuit(Priority priority)
        {
            WriteLine(Rfc2812.Quit(), priority);
        }

        public void RfcQuit()
        {
            WriteLine(Rfc2812.Quit());
        }

        public void RfcQuit(string quitmessage, Priority priority)
        {
            WriteLine(Rfc2812.Quit(quitmessage), priority);
        }

        public void RfcQuit(string quitmessage)
        {
            WriteLine(Rfc2812.Quit(quitmessage));
        }

        public void RfcSquit(string server, string comment, Priority priority)
        {
            WriteLine(Rfc2812.Squit(server, comment), priority);
        }

        public void RfcSquit(string server, string comment)
        {
            WriteLine(Rfc2812.Squit(server, comment));
        }
    }
}
