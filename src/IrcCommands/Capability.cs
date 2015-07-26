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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    /// Class representing a single IRCv3 capability
    /// </summary>
    public class Capability
    {
        private IdnMapping mapping = new IdnMapping();
        private string _Vendor;

        /// <summary>
        /// Vendor
        /// </summary>
        public string Vendor {
            get {
                // If value is already Punycode, converts it to a normal string, otherwise does nothing
                return mapping.GetUnicode(_Vendor);
            }
            private set {
                // If value is already Punycode, does nothing, otherwise converts to Punycode
                _Vendor = mapping.GetAscii(value);
            }
        }

        /// <summary>
        /// Vendor as Punycode
        /// </summary>
        public string VendorAsPunycode {
            get {
                return mapping.GetAscii(_Vendor);
            }
        }

        // NOTE: These two properties have private set only to shut up the Travis CI build tester on github

        /// <summary>
        /// Capability name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Optional capability parameter
        /// </summary>
        public string Option { get; private set; }

        /// <summary>
        /// Get the capability in human-readable format
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (VendorAsPunycode != "" ? VendorAsPunycode + "/" : "") + Name + (Option != "" ? "=" + Option : "");
        }

        /// <summary>
        /// Create a new capability
        /// </summary>
        /// <param name="name"></param>
        /// <param name="vendor"></param>
        /// <param name="option"></param>
        public Capability(string name, string vendor = "", string option = "")
        {
            Vendor = vendor;
            Name = name;
            Option = option;
        }

        /// <summary>
        /// Convert a human-readable capability into a new Capability object
        /// </summary>
        /// <param name="capstr"></param>
        /// <returns></returns>
        public static Capability FromString(string capstr)
        {
            Match m = Regex.Match(capstr.Trim(), @"^(?<vendor>.*?(?=/))?/?(?<name>[^=]+)=?(?<option>.*)?$");

            if (m.Success)
                return new Capability(m.Groups["name"].Value, m.Groups["vendor"].Value, m.Groups["option"].Value);
            else
                throw new ArgumentException("Invalid capability string supplied: " + capstr);
        }

        /// <summary>
        /// Get an array of Capability objects from a space-delimited string of capabilities
        /// </summary>
        /// <param name="caplist"></param>
        /// <returns></returns>
        public static IEnumerable<Capability> FromStrings(string caplist)
        {
            return from cap in caplist.Trim().Split(new char[] { ' ' }) select FromString(cap);
        }
    }
}
