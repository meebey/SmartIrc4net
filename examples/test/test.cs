/**
 * $Id: test.cs,v 1.4 2004/05/20 14:20:38 meebey Exp $
 * $Revision: 1.4 $
 * $Author: meebey $
 * $Date: 2004/05/20 14:20:38 $
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
using Meebey.SmartIrc4net;
using Meebey.SmartIrc4net.Delegates;

public class Test
{
    public static IrcClient irc = new IrcClient();

    public static void OnQueryMessage(Data ircdata)
    {
        switch (ircdata.MessageEx[0]) {
            case "dump_channel":
                string requested_channel = ircdata.MessageEx[1];
                Channel channel = irc.GetChannel(requested_channel);
                irc.Message(SendType.Message, ircdata.Nick, "<channel '"+requested_channel+"'>");

                irc.Message(SendType.Message, ircdata.Nick, "Name: '"+channel.Name+"'");
                irc.Message(SendType.Message, ircdata.Nick, "Topic: '"+channel.Topic+"'");
                irc.Message(SendType.Message, ircdata.Nick, "Mode: '"+channel.Mode+"'");
                irc.Message(SendType.Message, ircdata.Nick, "Key: '"+channel.Key+"'");
                irc.Message(SendType.Message, ircdata.Nick, "UserLimit: '"+channel.UserLimit+"'");

                string nickname_list = "";
                nickname_list += "Users: ";
                IDictionaryEnumerator it = channel.Users.GetEnumerator();
                while(it.MoveNext()) {
                    string      key         = (string)it.Key;
                    ChannelUser channeluser = (ChannelUser)it.Value;
                    nickname_list += key+" => "+channeluser.Nick+", ";
                }
                irc.Message(SendType.Message, ircdata.Nick, nickname_list);

                irc.Message(SendType.Message, ircdata.Nick, "</channel>");
            break;
            case "join":
                irc.Join(ircdata.MessageEx[1]);
            break;
            case "part":
                irc.Part(ircdata.MessageEx[1]);
            break;
            case "gc":
                GC.Collect();
            break;
        }
    }

    public static void Main(string[] args)
    {
        irc.SendDelay = 200;
        irc.AutoRetry = true;
        irc.ChannelSyncing = true;
        irc.OnQueryMessage += new MessageEventHandler(OnQueryMessage);

        string[] serverlist;
        serverlist = new string[] {"irc.fu-berlin.de"};

        int    port   = 6667;
        if(irc.Connect(serverlist, port) == true) {
            irc.Login("SmartIRC", "Mirco Bauer");
            irc.Join("#smartirc");
            for(int i = 0; i < 3; i++) {
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
