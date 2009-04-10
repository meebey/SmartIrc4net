/*
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 *
 * Copyright (c) 2008-2009 Thomas Bruderer <apophis@apophis.ch> <http://www.apophis.ch>
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

namespace Meebey.SmartIrc4net
{
    /// <summary>
    /// Special IRC Charakters
    /// </summary>
    public class IrcConstants {
        public const char CtcpChar = '\x1';
        public const char IrcBold = '\x2';
        public const char IrcColor = '\x3';
        public const char IrcReverse = '\x16';
        public const char IrcNormal = '\xf';
        public const char IrcUnderline = '\x1f';
        public const char CtcpQuoteChar = '\x20';
        
    }
        
    public enum DccSpeed
    {
        /// <summary>
        /// slow, ack every packet
        /// </summary>
        Rfc,
        /// <summary>
        /// hack, ignore acks, just send at max speed
        /// </summary>
        RfcSendAhead,
        /// <summary>
        /// fast, Turbo extension, no acks (Virc)
        /// </summary>
        Turbo
    }
    
    /// <summary>
    /// Mirc Compatible Colors
    /// </summary>
    public enum IrcColors {
        White         = 0,
        Black         = 1,
        Blue          = 2,
        Green         = 3,
        LightRed      = 4,
        Brown         = 5,
        Purple        = 6,
        Orange        = 7,
        Yellow        = 8,
        LightGreen    = 9,
        Cyan          = 10,
        LightCyan     = 11,
        LightBlue     = 12,
        Pink          = 13,
        Grey          = 14,
        LightGrey     = 15,
        Transparent   = 99
    }        
}
