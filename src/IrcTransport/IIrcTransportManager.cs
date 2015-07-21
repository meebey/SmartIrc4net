/*
 * $Id$
 * $URL$
 * $Rev$
 * $Author$
 * $Date$
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 *
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

namespace Meebey.SmartIrc4net
{
    /// <summary>
    /// This interface can be implemented by end users to interface with IRC servers using
    /// new transport layers, protocols or with in-place interception and modification of the packet flow
    /// </summary>
    public interface IIrcTransportManager
    {
        /// <summary>
        /// Connect using pre-specified settings
        /// </summary>
        /// <remarks>
        /// Implementations MUST NOT set IsConnectionError for any kind of connection problem.
        /// Throw an exception instead.
        /// </remarks>
        void Connect();

        /// <summary>
        /// Disconnect
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Write a line of data to the connection
        /// </summary>
        /// <param name="data">String to write, newline will be added</param>
        /// <returns></returns>
        bool WriteLine(string data);

        /// <summary>
        /// Returns true if connected
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Returns true if there is a connection error
        /// </summary>
        bool IsConnectionError { get; }

        /// <summary>
        /// Gets or sets (where allowed) the hostname of the target connection
        /// </summary>
        string Address { get; set; }

        /// <summary>
        /// Gets or sets (where allowed) the port of the target connection
        /// </summary>
        int Port { get; set; }

        /// <summary>
        /// Gets or sets (where allowed) the text encoding for messages on the connection
        /// </summary>
        System.Text.Encoding Encoding { get; set; }

        /// <summary>
        /// Event which fires when a line of text is received from the connection
        /// </summary>
        event ReadLineEventHandler OnMessageReceived;

        /// <summary>
        /// Event which fires when a connection error occurs
        /// </summary>
        event Action OnConnectionError;
    }
}
