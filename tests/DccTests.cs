/*
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
using System.Net;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    /// Visual Studio Unit Testing Framework tests for DccConnection
    /// </summary>
    [TestClass]
    public class DccTests
    {
        /// <summary>
        /// Test DccConnection.HostToDccInt() as updated for .NET 4.0 compliance
        /// </summary>
        [TestMethod]
        public void DccConnection_HostToDccInt()
        {
            // arbitrary IP addresses with bytes <0x80 and >=0x80 to test signed-ness
            _Test_HostToDccInt("1.2.3.4");
            _Test_HostToDccInt("128.1.2.254");
        }

        private void _Test_HostToDccInt(string ipstr)
        {
            IPAddress ip = IPAddress.Parse(ipstr);

            PrivateObject o = new PrivateObject(typeof(DccConnection));

            // Result of calling HostToDccInt() in .NET 4.5 code
            long dccIntBytes = Convert.ToInt64(o.Invoke("HostToDccInt", ip));

#pragma warning disable CS0618 // Type or member is obsolete
                              // Compare result with old-style using obsolete IPAddress.Address property from old HostToDccInt() code
            long temp = (ip.Address & 0xff) << 24;
            temp |= (ip.Address & 0xff00) << 8;
            temp |= (ip.Address >> 8) & 0xff00;
            temp |= (ip.Address >> 24) & 0xff;
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.AreEqual(dccIntBytes, temp);
        }
    }
}
