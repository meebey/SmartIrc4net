/**
 * $Id: test.cs,v 1.2 2003/11/27 23:32:21 meebey Exp $
 * $Revision: 1.2 $
 * $Author: meebey $
 * $Date: 2003/11/27 23:32:21 $
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
using Meebey.SmartIrc4net;

public class main
{
    public static void Main(string[] args)
    {
        IrcClient irc = new IrcClient();
        irc.AutoReconnect = true;
        if(irc.Connect("localhost", 6667) == true) {
            irc.Login("SmartIRC", "Mirco Bauer");
            irc.Join("#test");
            for(int i = 0; i < 32; i++) {
                irc.Message(SendType.Message, "#test", "test message "+i.ToString());
                irc.Message(SendType.Action, "#test", " thinks this is cool "+i.ToString());
                irc.Message(SendType.Notice, "#test", "you all suck "+i.ToString());
            }
            irc.Listen();
            irc.Disconnect();
        } else {
            System.Console.WriteLine("couldn't connect!");
        }
    }
}
