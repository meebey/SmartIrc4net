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
using System.Threading;
using Meebey.SmartIrc4net;

public class Test
{
    public static IrcClient irc = new IrcClient();

    public static void OnQueryMessage(object sender, IrcEventArgs e)
    {
        switch (e.Data.MessageArray[0]) {
            case "dump_channel":
                string requested_channel = e.Data.MessageArray[1];
                Channel channel = irc.GetChannel(requested_channel);
                irc.SendMessage(SendType.Message, e.Data.Nick, "<channel '"+requested_channel+"'>");

                irc.SendMessage(SendType.Message, e.Data.Nick, "Name: '"+channel.Name+"'");
                irc.SendMessage(SendType.Message, e.Data.Nick, "Topic: '"+channel.Topic+"'");
                irc.SendMessage(SendType.Message, e.Data.Nick, "Mode: '"+channel.Mode+"'");
                irc.SendMessage(SendType.Message, e.Data.Nick, "Key: '"+channel.Key+"'");
                irc.SendMessage(SendType.Message, e.Data.Nick, "UserLimit: '"+channel.UserLimit+"'");

                string nickname_list = "";
                nickname_list += "Users: ";
                IDictionaryEnumerator it = channel.Users.GetEnumerator();
                while(it.MoveNext()) {
                    string      key         = (string)it.Key;
                    ChannelUser channeluser = (ChannelUser)it.Value;
                    nickname_list += key+" => "+channeluser.Nick+", ";
                }
                irc.SendMessage(SendType.Message, e.Data.Nick, nickname_list);

                irc.SendMessage(SendType.Message, e.Data.Nick, "</channel>");
            break;
            case "join":
                irc.RfcJoin(e.Data.MessageArray[1]);
            break;
            case "part":
                irc.RfcPart(e.Data.MessageArray[1]);
            break;
            case "gc":
                GC.Collect();
            break;
        }
    }

    public static void OnError(object sender, ErrorEventArgs e)
    {
        System.Console.WriteLine("Error: "+e.ErrorMessage);
    }
    
    public static void OnRawMessage(object sender, IrcEventArgs e)
    {
        System.Console.WriteLine("Received: "+e.Data.RawMessage);
    }
    
    public static void Main(string[] args)
    {
        System.Threading.Thread.CurrentThread.Name = "Main";
        irc.SendDelay = 200;
        irc.AutoRetry = true;
        irc.ActiveChannelSyncing = true;
        irc.OnQueryMessage += new IrcEventHandler(OnQueryMessage);
        irc.OnError += new ErrorEventHandler(OnError);
        irc.OnRawMessage += new IrcEventHandler(OnRawMessage);

        string[] serverlist;
        serverlist = new string[] {"irc.fu-berlin.de"};
        int port = 6667;
        string channel = "#smartirc";
        try {
            irc.Connect(serverlist, port);
        } catch (ConnectionException e) {
            System.Console.WriteLine("couldn't connect! Reason: "+e.Message);
        }
        
        try {
            irc.Login("SmartIRC", "SmartIrc4net Test Bot");
            irc.RfcJoin(channel);
            for (int i = 0; i < 3; i++) {
                irc.SendMessage(SendType.Message, channel, "test message "+i.ToString());
                irc.SendMessage(SendType.Action, channel, "thinks this is cool "+i.ToString());
                irc.SendMessage(SendType.Notice, channel, "SmartIrc4net rocks "+i.ToString());
            }
            
            new Thread(new ThreadStart(ReadCommand)).Start();
            irc.Listen();
            irc.Disconnect();
        } catch (ConnectionException) {
        } catch (Exception e) {
            System.Console.WriteLine("Error occurred! Reason: "+e.Message);
        }
    }
    
    public static void ReadCommand()
    {
        while (true) {
            irc.WriteLine(System.Console.ReadLine());
        }
    }
}
