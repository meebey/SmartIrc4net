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
    public delegate void SimpleEventHandler();
    
    // delegates for IrcClient
    public delegate void IrcEventHandler(object sender, IrcEventArgs args);
    public delegate void ActionEventHandler(object sender, ActionEventArgs args);
    public delegate void ErrorEventHandler(object sender, ErrorEventArgs args);
    public delegate void PingEventHandler(object sender, PingEventArgs args);
    public delegate void KickEventHandler(object sender, KickEventArgs args);
    public delegate void JoinEventHandler(object sender, JoinEventArgs args);
    public delegate void NamesEventHandler(object sender, NamesEventArgs args);
    public delegate void PartEventHandler(object sender, PartEventArgs args);
    public delegate void InviteEventHandler(object sender, InviteEventArgs args);
    public delegate void OpEventHandler(object sender, OpEventArgs args);
    public delegate void DeopEventHandler(object sender, DeopEventArgs args);
    public delegate void VoiceEventHandler(object sender, VoiceEventArgs args);
    public delegate void DevoiceEventHandler(object sender, DevoiceEventArgs args);
    public delegate void BanEventHandler(object sender, BanEventArgs args);
    public delegate void UnbanEventHandler(object sender, UnbanEventArgs args);
    public delegate void TopicEventHandler(object sender, TopicEventArgs args);
    public delegate void TopicChangeEventHandler(object sender, TopicChangeEventArgs args);
    public delegate void NickChangeEventHandler(object sender, NickChangeEventArgs args);
    public delegate void QuitEventHandler(object sender, QuitEventArgs args);
    public delegate void WhoEventHandler(object sender, WhoEventArgs args);
    
    // delegates for IrcConnection
    public delegate void ReadLineEventHandler(object sender, ReadLineEventArgs args);
    public delegate void WriteLineEventHandler(object sender, WriteLineEventArgs args);
}
