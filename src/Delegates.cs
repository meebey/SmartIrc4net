/**
 * $Id: Delegates.cs,v 1.7 2004/07/31 22:56:22 meebey Exp $
 * $Revision: 1.7 $
 * $Author: meebey $
 * $Date: 2004/07/31 22:56:22 $
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

namespace Meebey.SmartIrc4net.Delegates
{
    public delegate void SimpleEventHandler();
    
    // for IrcClient
    public delegate void MessageEventHandler(Data ircdata);
    public delegate void ActionEventHandler(string action, Data ircdata);
    public delegate void ErrorEventHandler(string message, Data ircdata);
    public delegate void PingEventHandler(string data);
    public delegate void KickEventHandler(string channel, string victim, string who, string reason, Data ircdata);
    public delegate void JoinEventHandler(string channel, string who, Data ircdata);
    public delegate void NamReplyEventHandler(string channel, string[] userlist, Data ircdata);
    public delegate void PartEventHandler(string channel, string who, string partmessage, Data ircdata);
    public delegate void InviteEventHandler(string inviter, string channel, Data ircdata);
    public delegate void OpEventHandler(string channel, string who, string whom, Data ircdata);
    public delegate void DeopEventHandler(string channel, string who, string whom, Data ircdata);
    public delegate void VoiceEventHandler(string channel, string who, string whom, Data ircdata);
    public delegate void DevoiceEventHandler(string channel, string who, string whom, Data ircdata);
    public delegate void BanEventHandler(string channel, string who, string userhostmask, Data ircdata);
    public delegate void UnbanEventHandler(string channel, string who, string userhostmask, Data ircdata);
    public delegate void TopicEventHandler(string channel, string topic, Data ircdata);
    public delegate void TopicChangeEventHandler(string channel, string who, string newtopic, Data ircdata);
    public delegate void NickChangeEventHandler(string oldnickname, string newnickname, Data ircdata);
    public delegate void QuitEventHandler(string who, string quitmessage, Data ircdata);
    public delegate void WhoEventHandler(string channel, string nick, string ident, string host, string realname, bool away, bool op, bool voice, bool ircop, string server, int hopcount, Data ircdata);

    // for IrcConnection
    public delegate void ReadLineEventHandler(string rawline);
    public delegate void WriteLineEventHandler(string rawline);
}
