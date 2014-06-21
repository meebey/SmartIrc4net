//  SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
//
//  Copyright (c) 2014 Mirco Bauer <meebey@meebey.net>
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
using System.Collections.Generic;
using NUnit.Framework;

namespace Meebey.SmartIrc4net
{
    [TestFixture]
    public class ChannelModeChangeInfoTests
    {
        [Test]
        public void ParseWithParameter()
        {
            var modeMap = new ChannelModeMap();
            List<ChannelModeChangeInfo> changeInfos;
            ChannelModeChangeInfo changeInfo;

            changeInfos = ChannelModeChangeInfo.Parse(modeMap, "#test", "+o", "meebey");
            Assert.IsNotNull(changeInfos);
            Assert.AreEqual(1, changeInfos.Count);

            changeInfo = changeInfos[0];
            Assert.AreEqual(ChannelModeChangeAction.Set, changeInfo.Action);
            Assert.AreEqual(ChannelMode.Op, changeInfo.Mode);
            Assert.AreEqual('o', changeInfo.ModeChar);
            Assert.AreEqual("meebey", changeInfo.Parameter);
        }

        [Test]
        public void ParseWithoutParameter()
        {
            var modeMap = new ChannelModeMap();
            List<ChannelModeChangeInfo> changeInfos;
            ChannelModeChangeInfo changeInfo;

            changeInfos = ChannelModeChangeInfo.Parse(modeMap, "#test", "+nt", "");
            Assert.IsNotNull(changeInfos);
            Assert.AreEqual(2, changeInfos.Count);

            changeInfo = changeInfos[0];
            Assert.AreEqual(ChannelModeChangeAction.Set, changeInfo.Action);
            Assert.AreEqual(ChannelMode.Unknown, changeInfo.Mode);
            Assert.AreEqual('n', changeInfo.ModeChar);
            Assert.AreEqual(null, changeInfo.Parameter);

            changeInfo = changeInfos[1];
            Assert.AreEqual(ChannelModeChangeAction.Set, changeInfo.Action);
            Assert.AreEqual(ChannelMode.TopicLock, changeInfo.Mode);
            Assert.AreEqual('t', changeInfo.ModeChar);
            Assert.AreEqual(null, changeInfo.Parameter);
        }

        [Test]
        public void ParseComplex()
        {
            var modeMap = new ChannelModeMap();
            List<ChannelModeChangeInfo> changeInfos;
            ChannelModeChangeInfo changeInfo;

            changeInfos = ChannelModeChangeInfo.Parse(modeMap, "#test", "-l+o-k+v", "op_nick * voice_nick");
            Assert.IsNotNull(changeInfos);
            Assert.AreEqual(4, changeInfos.Count);

            changeInfo = changeInfos[0];
            Assert.AreEqual(ChannelModeChangeAction.Unset, changeInfo.Action);
            Assert.AreEqual(ChannelMode.UserLimit, changeInfo.Mode);
            Assert.AreEqual('l', changeInfo.ModeChar);
            Assert.AreEqual(null, changeInfo.Parameter);

            changeInfo = changeInfos[1];
            Assert.AreEqual(ChannelModeChangeAction.Set, changeInfo.Action);
            Assert.AreEqual(ChannelMode.Op, changeInfo.Mode);
            Assert.AreEqual('o', changeInfo.ModeChar);
            Assert.AreEqual("op_nick", changeInfo.Parameter);

            changeInfo = changeInfos[2];
            Assert.AreEqual(ChannelModeChangeAction.Unset, changeInfo.Action);
            Assert.AreEqual(ChannelMode.Key, changeInfo.Mode);
            Assert.AreEqual('k', changeInfo.ModeChar);
            Assert.AreEqual("*", changeInfo.Parameter);

            changeInfo = changeInfos[3];
            Assert.AreEqual(ChannelModeChangeAction.Set, changeInfo.Action);
            Assert.AreEqual(ChannelMode.Voice, changeInfo.Mode);
            Assert.AreEqual('v', changeInfo.ModeChar);
            Assert.AreEqual("voice_nick", changeInfo.Parameter);
        }

        [Test]
        public void ParseUnknown()
        {
            var modeMap = new ChannelModeMap();
            List<ChannelModeChangeInfo> changeInfos;
            ChannelModeChangeInfo changeInfo;

            changeInfos = ChannelModeChangeInfo.Parse(modeMap, "#test", "+X-Y", "foo bar");
            Assert.IsNotNull(changeInfos);
            Assert.AreEqual(2, changeInfos.Count);

            changeInfo = changeInfos[0];
            Assert.AreEqual(ChannelModeChangeAction.Set, changeInfo.Action);
            Assert.AreEqual(ChannelMode.Unknown, changeInfo.Mode);
            Assert.AreEqual('X', changeInfo.ModeChar);
            Assert.AreEqual("foo", changeInfo.Parameter);

            changeInfo = changeInfos[1];
            Assert.AreEqual(ChannelModeChangeAction.Unset, changeInfo.Action);
            Assert.AreEqual(ChannelMode.Unknown, changeInfo.Mode);
            Assert.AreEqual('Y', changeInfo.ModeChar);
            Assert.AreEqual("bar", changeInfo.Parameter);
        }
    }
}

