//  SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
//
//  Copyright (c) 2016 Mirco Bauer <meebey@meebey.net>
//
//  Full LGPL License: <http://www.gnu.org/licenses/lgpl.txt>
//
//  This library is free software; you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 2.1 of the
//  License, or (at your option) any later version.
//
//  This library is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//  Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
using System;
using NUnit.Framework;

namespace Meebey.SmartIrc4net
{
    [TestFixture]
    public class IrcClientTests
    {
        [Test]
        public void MessageParser()
        {
            var client = new IrcClient();

            var rawline = ":irc.example.com 001 meebey3 :Welcome to the EFnet Internet Relay Chat Network meebey3";
            var msg = client.MessageParser(rawline);
            Assert.AreSame(client, msg.Irc);
            Assert.AreEqual(rawline, msg.RawMessage);
            Assert.AreEqual("irc.example.com", msg.From);
            Assert.AreEqual(null, msg.Nick);
            Assert.AreEqual(null, msg.Ident);
            Assert.AreEqual(null, msg.Host);
            Assert.AreEqual(ReplyCode.Welcome, msg.ReplyCode);
            Assert.AreEqual(ReceiveType.Login, msg.Type);
            Assert.AreEqual("Welcome to the EFnet Internet Relay Chat Network meebey3", msg.Message);
            Assert.AreEqual(null, msg.Channel);
            Assert.IsNotNull(msg.Tags);
            Assert.AreEqual(0, msg.Tags.Count);

            rawline = ":irc.example.com 002 meebey3 :Your host is irc.example.com[127.0.0.1/6667], running version hybrid-7.2.2+oftc1.6.9";
            msg = client.MessageParser(rawline);
            Assert.AreSame(client, msg.Irc);
            Assert.AreEqual(rawline, msg.RawMessage);
            Assert.AreEqual("irc.example.com", msg.From);
            Assert.AreEqual(null, msg.Nick);
            Assert.AreEqual(null, msg.Ident);
            Assert.AreEqual(null, msg.Host);
            Assert.AreEqual(ReplyCode.YourHost, msg.ReplyCode);
            Assert.AreEqual(ReceiveType.Login, msg.Type);
            Assert.AreEqual("Your host is irc.example.com[127.0.0.1/6667], running version hybrid-7.2.2+oftc1.6.9", msg.Message);
            Assert.AreEqual(null, msg.Channel);
            Assert.IsNotNull(msg.Tags);
            Assert.AreEqual(0, msg.Tags.Count);

            rawline = ":irc.example.com 003 meebey3 :This server was created Aug  7 2011 at 12:43:41";
            msg = client.MessageParser(rawline);
            Assert.AreSame(client, msg.Irc);
            Assert.AreEqual(rawline, msg.RawMessage);
            Assert.AreEqual("irc.example.com", msg.From);
            Assert.AreEqual(null, msg.Nick);
            Assert.AreEqual(null, msg.Ident);
            Assert.AreEqual(null, msg.Host);
            Assert.AreEqual(ReplyCode.Created, msg.ReplyCode);
            Assert.AreEqual(ReceiveType.Login, msg.Type);
            Assert.AreEqual("This server was created Aug  7 2011 at 12:43:41", msg.Message);
            Assert.AreEqual(null, msg.Channel);
            Assert.IsNotNull(msg.Tags);
            Assert.AreEqual(0, msg.Tags.Count);

            rawline = ":irc.example.com 004 meebey3 irc.example.com hybrid-7.2.2+oftc1.6.9 CDGPRSabcdfgiklnorsuwxyz biklmnopstveI bkloveI";
            msg = client.MessageParser(rawline);
            Assert.AreSame(client, msg.Irc);
            Assert.AreEqual(rawline, msg.RawMessage);
            Assert.AreEqual("irc.example.com", msg.From);
            Assert.AreEqual(null, msg.Nick);
            Assert.AreEqual(null, msg.Ident);
            Assert.AreEqual(null, msg.Host);
            Assert.AreEqual(ReplyCode.MyInfo, msg.ReplyCode);
            Assert.AreEqual(ReceiveType.Login, msg.Type);
            Assert.AreEqual(null, msg.Message);
            Assert.AreEqual(null, msg.Channel);
            Assert.IsNotNull(msg.Tags);
            Assert.AreEqual(0, msg.Tags.Count);

            rawline = ":irc.example.com 005 meebey3 CALLERID CASEMAPPING=rfc1459 DEAF=D KICKLEN=160 MODES=4 NICKLEN=30 PREFIX=(ov)@+ STATUSMSG=@+ TOPICLEN=390 NETWORK=EFnet MAXLIST=beI:25 MAXTARGETS=4 CHANTYPES=#& :are supported by this server";
            msg = client.MessageParser(rawline);
            Assert.AreSame(client, msg.Irc);
            Assert.AreEqual(rawline, msg.RawMessage);
            Assert.AreEqual("irc.example.com", msg.From);
            Assert.AreEqual(null, msg.Nick);
            Assert.AreEqual(null, msg.Ident);
            Assert.AreEqual(null, msg.Host);
            Assert.AreEqual(ReplyCode.Bounce, msg.ReplyCode);
            Assert.AreEqual(ReceiveType.Login, msg.Type);
            Assert.AreEqual("are supported by this server", msg.Message);
            Assert.AreEqual(null, msg.Channel);
            Assert.IsNotNull(msg.Tags);
            Assert.AreEqual(0, msg.Tags.Count);

            rawline = ":irc.example.com 005 meebey3 CHANLIMIT=#&:25 CHANNELLEN=50 CHANMODES=eIb,k,l,imnpstMRS KNOCK ELIST=CMNTU SAFELIST AWAYLEN=160 EXCEPTS=e INVEX=I :are supported by this server";
            msg = client.MessageParser(rawline);
            Assert.AreSame(client, msg.Irc);
            Assert.AreEqual(rawline, msg.RawMessage);
            Assert.AreEqual("irc.example.com", msg.From);
            Assert.AreEqual(null, msg.Nick);
            Assert.AreEqual(null, msg.Ident);
            Assert.AreEqual(null, msg.Host);
            Assert.AreEqual(ReplyCode.Bounce, msg.ReplyCode);
            Assert.AreEqual(ReceiveType.Login, msg.Type);
            Assert.AreEqual("are supported by this server", msg.Message);
            Assert.AreEqual(null, msg.Channel);
            Assert.IsNotNull(msg.Tags);
            Assert.AreEqual(0, msg.Tags.Count);

            rawline = ":i_ron!~zbuddy@37.187.47.25 JOIN :#debian.de";
            msg = client.MessageParser(rawline);
            Assert.AreSame(client, msg.Irc);
            Assert.AreEqual(rawline, msg.RawMessage);
            Assert.AreEqual("i_ron!~zbuddy@37.187.47.25", msg.From);
            Assert.AreEqual("i_ron", msg.Nick);
            Assert.AreEqual("~zbuddy", msg.Ident);
            Assert.AreEqual("37.187.47.25", msg.Host);
            Assert.AreEqual(ReplyCode.Null, msg.ReplyCode);
            Assert.AreEqual(ReceiveType.Join, msg.Type);
            Assert.AreEqual("#debian.de", msg.Message);
            Assert.AreEqual("#debian.de", msg.Channel);
            Assert.IsNotNull(msg.Tags);
            Assert.AreEqual(0, msg.Tags.Count);
        }
    }
}

