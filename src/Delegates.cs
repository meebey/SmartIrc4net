/**
 * $Id: Delegates.cs,v 1.2 2003/12/14 12:40:47 meebey Exp $
 * $Revision: 1.2 $
 * $Author: meebey $
 * $Date: 2003/12/14 12:40:47 $
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

namespace Meebey.SmartIrc4net.Delegates
{
    public delegate void SimpleEventHandler();
    
    // for IrcClient
    public delegate void PingEventHandler(string data);
    public delegate void KickEventHandler(string channel, string victim, string who, string reason, Data ircdata);
    public delegate void JoinEventHandler(string channel, string nickname, Data ircdata);
    public delegate void MessageEventHandler(Data ircdata);

    // for Connection
    public delegate void ReadLineEventHandler(string data);
    public delegate void WriteLineEventHandler(string data);
}
