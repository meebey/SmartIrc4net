/**
 * $Id$
 * $URL$
 * $Rev$
 * $Author$
 * $Date$
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

namespace Meebey.SmartIrc4net
{
    public class SmartIrc4netException : ApplicationException
    {
        public SmartIrc4netException(string message) : base(message)
        {
        }
        
        public SmartIrc4netException(string message, Exception e) : base(message, e)
        {
        }
    }
    
    public class ConnectionException : SmartIrc4netException
    {
        public ConnectionException(string message) : base(message)
        {
        }
        
        public ConnectionException(string message, Exception e) : base(message, e)
        {
        }
    }
    
    public class CouldNotConnectException : ConnectionException
    {
        public CouldNotConnectException(string message) : base(message)
        {
        }
        
        public CouldNotConnectException(string message, Exception e) : base(message, e)
        {
        }
    }

    public class NotConnectedException : ConnectionException
    {
        public NotConnectedException(string message) : base(message)
        {
        }
        
        public NotConnectedException(string message, Exception e) : base(message, e)
        {
        }
    }

    public class AlreadyConnectedException : ConnectionException
    {
        public AlreadyConnectedException(string message) : base(message)
        {
        }
        
        public AlreadyConnectedException(string message, Exception e) : base(message, e)
        {
        }
    }
}
