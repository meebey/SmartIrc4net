/**
 * $Id$
 * $Revision$
 * $Author$
 * $Date$
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 * This is a simple test client for the library.
 *
 * Copyright (c) 2003-2004 Mirco Bauer <meebey@meebey.net> <http://www.meebey.net>
 * Copyright (c) 2015 Katy Coe <djkaty@start.no> <http://www.djkaty.com>
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
using System.Threading;
using System.Collections;
using System.Collections.Generic;

using Meebey.SmartIrc4net;

// This is an VERY basic example how your IRC application could be written
// its mainly for showing how to use the API, this program just connects sends
// a few message to a channel and waits for commands on the console
// (raw RFC commands though! it's later explained).
// There are also a few commands the IRC bot/client allows via private message.
public class Test
{
    // make an instance of the high-level API
    public static IrcClient irc = new IrcClient();

    // this method we will use to analyse queries (also known as private messages)
    public static void OnQueryMessage(object sender, IrcEventArgs e)
    {
        switch (e.Data.MessageArray[0]) {
            // debug stuff
            case "dump_channel":
                string requested_channel = e.Data.MessageArray[1];
                // getting the channel (via channel sync feature)
                Channel channel = irc.GetChannel(requested_channel);
                
                // here we send messages
                irc.SendMessage(SendType.Message, e.Data.Nick, "<channel '"+requested_channel+"'>");
                
                irc.SendMessage(SendType.Message, e.Data.Nick, "Name: '"+channel.Name+"'");
                irc.SendMessage(SendType.Message, e.Data.Nick, "Topic: '"+channel.Topic+"'");
                irc.SendMessage(SendType.Message, e.Data.Nick, "Mode: '"+channel.Mode+"'");
                irc.SendMessage(SendType.Message, e.Data.Nick, "Key: '"+channel.Key+"'");
                irc.SendMessage(SendType.Message, e.Data.Nick, "UserLimit: '"+channel.UserLimit+"'");
                
                // here we go through all users of the channel and show their
                // hashtable key and nickname 
                string nickname_list = "";
                nickname_list += "Users: ";
                foreach (var de in channel.Users) {
                    string      key         = de.Key;
                    ChannelUser channeluser = de.Value;
                    nickname_list += "(";
                    if (channeluser.IsOp) {
                        nickname_list += "@";
                    }
                    if (channeluser.IsVoice) {
                        nickname_list += "+";
                    }
                    nickname_list += ")"+key+" => "+channeluser.Nick+", ";
                }
                irc.SendMessage(SendType.Message, e.Data.Nick, nickname_list);

                irc.SendMessage(SendType.Message, e.Data.Nick, "</channel>");
            break;
            case "gc":
                GC.Collect();
            break;
            // typical commands
            case "join":
                irc.RfcJoin(e.Data.MessageArray[1]);
            break;
            case "part":
                irc.RfcPart(e.Data.MessageArray[1]);
            break;
            case "die":
                Exit();
            break;
        }
    }

    // this method handles when we receive "ERROR" from the IRC server
    public static void OnError(object sender, ErrorEventArgs e)
    {
        System.Console.WriteLine("Error: "+e.ErrorMessage);
        Exit();
    }
    
    // this method will get all IRC messages
    public static void OnRawMessage(object sender, IrcEventArgs e)
    {
        System.Console.WriteLine("Received: "+e.Data.RawMessage);
    }

    public static void OnCapLsReply(object sender, CapEventArgs e) {
        Console.WriteLine("Available caps:");

        foreach (var c in e.CapList) {
            Console.WriteLine(c);
        }

        ((IrcClient) sender).CapReq(e.CapList);
    }

    public static void OnCapListReply(object sender, CapEventArgs e) {
        Console.WriteLine("Activated caps:");

        foreach (var c in e.CapList) {
            Console.WriteLine(c);
        }
    }

    public static void OnCapReqReply(object sender, CapEventArgs e) {
        if (e.Data.Type == ReceiveType.CapAck)
            Console.WriteLine("Allowed caps:");

        if (e.Data.Type == ReceiveType.CapNak)
            Console.WriteLine("Denied caps:");

        foreach (var c in e.CapList) {
            Console.WriteLine(c);
        }
    
        ((IrcClient) sender).CapList();
    }

    public static void Main(string[] args)
    {
        Thread.CurrentThread.Name = "Main";

        // Enable IRCv3 protocol features
        irc.UseIrcV3 = true;
        
        // wait time between messages, we can set this lower on own irc servers
        irc.SendDelay = 200;
        
        // we use channel sync, means we can use irc.GetChannel() and so on
        irc.ActiveChannelSyncing = true;
        
        // here we connect the events of the API to our written methods
        // most have own event handler types, because they ship different data
        irc.OnQueryMessage += new IrcEventHandler(OnQueryMessage);
        irc.OnError += new ErrorEventHandler(OnError);
        irc.OnRawMessage += new IrcEventHandler(OnRawMessage);

        irc.OnReconnected += (s, e) => {
            // Don't forget to log in again after reconnecting!
            irc.Login("SmartIRC", "SmartIrc4net Test Bot");
            irc.RfcJoin("#smartirc-test");
        };

        // Listen for caps messages
        irc.OnCapLsReply += OnCapLsReply;
        irc.OnCapListReply += OnCapListReply;
        irc.OnCapAckReply += OnCapReqReply;
        irc.OnCapNakReply += OnCapReqReply;
        
        // ===============================================
        // Standard IRC over TCP
        // ===============================================

        // New single-address syntax:
        //IrcTcpTransport transport = new IrcTcpTransport("irc.freenode.org", 6667);

        // Alternative single-address syntax:
        //IrcTcpTransport transport = new IrcTcpTransport();
        //transport.Address = "irc.freenode.org";
        //transport.Port = 6667;

        // Alternative syntax for classic TCP backwards compatibility:
        string[] serverlist = new string[] { "irc.freenode.org" };
        int port = 6667; // use 6697 for SSL on Freenode

        // Uncomment to set non-default encoding
        //transport.Encoding = System.Text.Encoding.UTF8;

        // Uncomment to use anonymous SOCKS or HTTP proxy
        //transport.ProxyType = ProxyType.Socks5;
        //transport.ProxyHost = "207.190.116.231";
        //transport.ProxyPort = 30188;

        // Uncomment to use SSL
        //transport.UseSsl = true;

        // ===============================================
        // IRC over WebSockets
        // ===============================================

        // Example Twitch WebSockets server
        //IrcWebSocketTransport transport = new IrcWebSocketTransport("ws://192.16.64.144");

        // Uncomment to enable compression
        //transport.Compression = WebSocketSharp.CompressionMethod.Deflate;

        // Uncomment to enable HTTP auth
        //transport.SetCredentials("username", "password", false);

        // Uncomment to enable HTTP proxy
        //transport.SetProxy("http://162.208.49.45:7808", "", "");

        // Uncomment to send HTTP cookies with connection request
        //transport.Cookies = new WebSocketSharp.Net.Cookie[] {
        //  new WebSocketSharp.Net.Cookie("name", "value", "path", "domain"),
        //  ...
        //};

        // For SSL, use: transport.SslConfiguration...

        // ===============================================
        // From this point, all usage is the same regardless of transport protocol
        // ===============================================

        // Uncomment to use AutoRetry
        //irc.AutoRetry = true;
        //irc.AutoRetryDelay = 5;
        //irc.AutoRetryLimit = 4;

        // Channel to join

        string channel = "#smartirc-test";

        try {
            // Connect to IRC server using any specified transport
            // If the transport doesn't support multiple addresses, you must use this (eg. WebSockets)
            //irc.Connect(transport);

            // Alternative syntax:
            //irc.Connect(transport, serverlist, port);

            // Multi-server WebSocket syntax example:
            // irc.AutoRetry = true;
            // irc.Connect(new IrcWebSocketTransport(), new string[] { "ws://1.2.3.4", "ws://192.16.64.144" });

            // Alternative syntax for classic TCP backwards compatibility:
            irc.Connect(serverlist, port);
        }
        catch (ConnectionException e) {
            // something went wrong, the reason will be shown
            System.Console.WriteLine("couldn't connect! Reason: "+e.Message);
            Exit();
        }
        
        try {
            // here we logon and register our nickname and so on 
            irc.Login("SmartIRC", "SmartIrc4net Test Bot");

            // join the channel
            irc.RfcJoin(channel);
            
            for (int i = 0; i < 3; i++) {
                // here we send just 3 different types of messages, 3 times for
                // testing the delay and flood protection (messagebuffer work)
                irc.SendMessage(SendType.Message, channel, "test message ("+i.ToString()+")");
                irc.SendMessage(SendType.Action, channel, "thinks this is cool ("+i.ToString()+")");
                irc.SendMessage(SendType.Notice, channel, "SmartIrc4net rocks ("+i.ToString()+")");
            }
            
            // spawn a new thread to read the stdin of the console, this we use
            // for reading IRC commands from the keyboard while the IRC connection
            // stays in its own thread
            new Thread(new ThreadStart(ReadCommands)).Start();
            
            // here we tell the IRC API to go into a receive mode, all events
            // will be triggered by _this_ thread (main thread in this case)
            // Listen() blocks by default, you can also use ListenOnce() if you
            // need that does one IRC operation and then returns, so you need then 
            // an own loop 
            irc.Listen();
            
            // You should not call Disconnect() when Listen() returns as the connection
            // will already be closed and a ConnectionException will be thrown.

        } catch (Exception e) {
            // this should not happen by just in case we handle it nicely
            System.Console.WriteLine("Error occurred! Message: "+e.Message);
            System.Console.WriteLine("Exception: "+e.StackTrace);
            Exit();
        }
    }
    
    public static void ReadCommands()
    {
        // here we read the commands from the stdin and send it to the IRC API
        // WARNING, it uses WriteLine() means you need to enter RFC commands
        // like "JOIN #test" and then "PRIVMSG #test :hello to you"
        while (true) {
            string cmd = System.Console.ReadLine();
            if (cmd.StartsWith("/list")) {
                int pos = cmd.IndexOf(" ");
                string channel = null;
                if (pos != -1) {
                    channel = cmd.Substring(pos + 1);
                }
                
                IList<ChannelInfo> channelInfos = irc.GetChannelList(channel);
                Console.WriteLine("channel count: {0}", channelInfos.Count);
                foreach (ChannelInfo channelInfo in channelInfos) {
                    Console.WriteLine("channel: {0} user count: {1} topic: {2}",
                                      channelInfo.Channel,
                                      channelInfo.UserCount,
                                      channelInfo.Topic);
                }
            } else if (cmd.StartsWith("/reconnect")) {
                irc.Reconnect();

            } else if (cmd.StartsWith("/quit")) {

                // This disconnects the connection then releases Listen() from blocking
                irc.Disconnect();
                break;
            } else {
                irc.WriteLine(cmd);
            }
        }
    }
    
    public static void Exit()
    {
        // we are done, lets exit...
        System.Console.WriteLine("Exiting...");
        System.Environment.Exit(0);
    }
}
