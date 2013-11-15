/*
 * $Id$
 * $URL$
 * $Rev$
 * $Author$
 * $Date$
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 *
 * Copyright (c) 2003-2005 Mirco Bauer <meebey@meebey.net> <http://www.meebey.net>
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

#if LOG4NET
using System.IO;
using System.Collections;
using log4net;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    ///
    /// </summary>
    /// <threadsafety static="true" instance="true" />
    internal static class Logger
    {
        static public ILog Main { get; private set; }
        static public ILog Connection { get; private set; }
        static public ILog Socket { get; private set; }
        static public ILog Queue { get; private set; }
        static public ILog IrcMessages { get; private set; }
        static public ILog MessageTypes { get; private set; }
        static public ILog MessageParser { get; private set; }
        static public ILog ActionHandler { get; private set; }
        static public ILog TimeHandler { get; private set; }
        static public ILog MessageHandler { get; private set; }
        static public ILog ChannelSyncing { get; private set; }
        static public ILog UserSyncing { get; private set; }
        static public ILog Modules { get; private set; }
        static public ILog Dcc { get; private set; }

        static Logger()
        {
            Main = log4net.LogManager.GetLogger("MAIN");
            Socket = log4net.LogManager.GetLogger("SOCKET");
            Queue = log4net.LogManager.GetLogger("QUEUE");
            Connection = log4net.LogManager.GetLogger("CONNECTION");
            IrcMessages = log4net.LogManager.GetLogger("IRCMESSAGE");
            MessageParser = log4net.LogManager.GetLogger("MESSAGEPARSER");
            MessageTypes = log4net.LogManager.GetLogger("MESSAGETYPES");
            ActionHandler = log4net.LogManager.GetLogger("ACTIONHANDLER");
            TimeHandler = log4net.LogManager.GetLogger("TIMEHANDLER");
            MessageHandler = log4net.LogManager.GetLogger("MESSAGEHANDLER");
            ChannelSyncing = log4net.LogManager.GetLogger("CHANNELSYNCING");
            UserSyncing = log4net.LogManager.GetLogger("USERSYNCING");
            Modules = log4net.LogManager.GetLogger("MODULES");
            Dcc = log4net.LogManager.GetLogger("DCC");
        }
    }
}
#endif
