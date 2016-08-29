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
    public class ServerPropertiesTests
    {
        [Test]
        public void ParseFromRawMessage()
        {
            var rawline = ":irc.example.com 005 meebey3 CALLERID CASEMAPPING=rfc1459 DEAF=D KICKLEN=160 MODES=4 NICKLEN=30 PREFIX=(ov)@+ STATUSMSG=@+ TOPICLEN=390 NETWORK=EFnet MAXLIST=beI:25 MAXTARGETS=4 CHANTYPES=#& :are supported by this server";
            var props = new ServerProperties();
            props.ParseFromRawMessage(rawline.Split(' '));
            Assert.AreEqual(13, props.RawProperties.Count);
            Assert.IsTrue(props.RawProperties.ContainsKey("CALLERID"));
            Assert.AreEqual(null, props.RawProperties["CALLERID"]);
            Assert.IsTrue(props.RawProperties.ContainsKey("CASEMAPPING"));
            Assert.AreEqual("rfc1459", props.RawProperties["CASEMAPPING"]);
            Assert.IsTrue(props.RawProperties.ContainsKey("DEAF"));
            Assert.AreEqual("D", props.RawProperties["DEAF"]);
            Assert.IsTrue(props.RawProperties.ContainsKey("KICKLEN"));
            Assert.AreEqual("160", props.RawProperties["KICKLEN"]);
            Assert.IsTrue(props.RawProperties.ContainsKey("MODES"));
            Assert.AreEqual("4", props.RawProperties["MODES"]);
            Assert.IsTrue(props.RawProperties.ContainsKey("NICKLEN"));
            Assert.AreEqual("30", props.RawProperties["NICKLEN"]);
            Assert.IsTrue(props.RawProperties.ContainsKey("PREFIX"));
            Assert.AreEqual("(ov)@+", props.RawProperties["PREFIX"]);
            Assert.IsTrue(props.RawProperties.ContainsKey("STATUSMSG"));
            Assert.AreEqual("@+", props.RawProperties["STATUSMSG"]);
            Assert.IsTrue(props.RawProperties.ContainsKey("TOPICLEN"));
            Assert.AreEqual("390", props.RawProperties["TOPICLEN"]);
            Assert.IsTrue(props.RawProperties.ContainsKey("NETWORK"));
            Assert.AreEqual("EFnet", props.RawProperties["NETWORK"]);
            Assert.IsTrue(props.RawProperties.ContainsKey("MAXLIST"));
            Assert.AreEqual("beI:25", props.RawProperties["MAXLIST"]);
            Assert.IsTrue(props.RawProperties.ContainsKey("MAXTARGETS"));
            Assert.AreEqual("4", props.RawProperties["MAXTARGETS"]);
            Assert.IsTrue(props.RawProperties.ContainsKey("CHANTYPES"));
            Assert.AreEqual("#&", props.RawProperties["CHANTYPES"]);

            rawline = ":irc.example.com 005 meebey3 CHANLIMIT=#&:25 CHANNELLEN=50 CHANMODES=eIb,k,l,imnpstMRS KNOCK ELIST=CMNTU SAFELIST AWAYLEN=160 EXCEPTS=e INVEX=I :are supported by this server";
            props.ParseFromRawMessage(rawline.Split(' '));
            Assert.AreEqual(13+9, props.RawProperties.Count);
            Assert.AreEqual("#&:25", props.RawProperties["CHANLIMIT"]);
            Assert.AreEqual("50", props.RawProperties["CHANNELLEN"]);
            Assert.AreEqual("eIb,k,l,imnpstMRS", props.RawProperties["CHANMODES"]);
            Assert.AreEqual(null, props.RawProperties["KNOCK"]);
            Assert.AreEqual("CMNTU", props.RawProperties["ELIST"]);
            Assert.AreEqual(null, props.RawProperties["SAFELIST"]);
            Assert.AreEqual("160", props.RawProperties["AWAYLEN"]);
            Assert.AreEqual("e", props.RawProperties["EXCEPTS"]);
            Assert.AreEqual("I", props.RawProperties["INVEX"]);
        }
    }
}