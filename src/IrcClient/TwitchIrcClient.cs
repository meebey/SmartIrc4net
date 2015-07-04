/*
 * $Id$
 * $URL$
 * $Rev$
 * $Author$
 * $Date$
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 *
 * Copyright (c) 2003-2010, 2012-2014 Mirco Bauer <meebey@meebey.net>
 * Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch>
 * Copyright (c) 2015 Katy Coe <djkaty@start.no>
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

using System.Text.RegularExpressions;

namespace Meebey.SmartIrc4net
{
    public class TwitchIrcClient : IrcClient
    {
        private static Regex _UserStateRegex = new Regex("^:.*? USERSTATE .*$", RegexOptions.Compiled);
        private static Regex _RoomStateRegex = new Regex("^:.*? ROOMSTATE .*$", RegexOptions.Compiled);

        public event IrcEventHandler OnUserState;
        public event IrcEventHandler OnRoomState;

        /// <summary>
        /// Login using nick and password
        /// </summary>
        /// <remark>Login is used at the beginning of connection to specify the username, hostname and realname of a new user.</remark>
        /// <param name="nick">The users 'nick' name which may NOT contain spaces</param>
        /// <param name="password">The optional password can and MUST be set before any attempt to register
        public new void Login(string nick, string password)
        {
            // Set Twitch capabilities

            WriteLine("CAP REQ :twitch.tv/membership");
            WriteLine("CAP REQ :twitch.tv/commands");

            _Login(new string[] { nick }, "", 0, "", password);
        }

        protected override ReceiveType _GetMessageType(string rawline)
        {
            Match found;

            found = _UserStateRegex.Match(rawline);
            if (found.Success)
            {
                return ReceiveType.Custom;
            }

            found = _RoomStateRegex.Match(rawline);
            if (found.Success)
            {
                return ReceiveType.Custom;
            }

            return base._GetMessageType(rawline);
        }

        protected override string _GetCustomMessageType(string rawline)
        {
            Match found;

            found = _UserStateRegex.Match(rawline);
            if (found.Success)
            {
                return "USERSTATE";
            }

            found = _RoomStateRegex.Match(rawline);
            if (found.Success)
            {
                return "ROOMSTATE";
            }

            return base._GetCustomMessageType(rawline);
        }

        protected override void _HandleEvents(IrcMessageData ircdata)
        {
            base._HandleEvents(ircdata);

            string code = ircdata.RawMessageArray[1];

            switch (code)
            {
                case "USERSTATE":
                    _Event_USERSTATE(ircdata);
                    break;
                case "ROOMSTATE":
                    _Event_ROOMSTATE(ircdata);
                    break;
            }
        }

        /// <summary>
        /// Event handler for userstate messages
        /// </summary>
        /// <param name="ircdata">Message data containing userstate information</param>
        private void _Event_USERSTATE(IrcMessageData ircdata)
        {
            if (OnUserState != null)
            {
                OnUserState(this, new IrcEventArgs(ircdata));
            }
        }

        /// <summary>
        /// Event handler for roomstate messages
        /// </summary>
        /// <param name="ircdata">Message data containing roomstate information</param>
        private void _Event_ROOMSTATE(IrcMessageData ircdata)
        {
            if (OnRoomState != null)
            {
                OnRoomState(this, new IrcEventArgs(ircdata));
            }
        }

        /// <summary>
        /// Event handler for join messages
        /// </summary>
        /// <param name="ircdata">Message data containing join information</param>
        protected override void _Event_JOIN(IrcMessageData ircdata)
        {
            __Event_JOIN(ircdata, false);
        }

    }
}
