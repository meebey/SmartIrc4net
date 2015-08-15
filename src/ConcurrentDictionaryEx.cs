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

using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    /// Convenience extension methods to System.Collections.Concurrent.ConcurrentDictionary
    /// </summary>
    internal static class ConcurrentDictionaryEx
    {
        /// <summary>
        /// Add a new item
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="self">Dictionary to use</param>
        /// <param name="key">New key</param>
        /// <param name="value">New value</param>
        /// <returns>True on success, false on failure</returns>
        public static bool Add<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> self, TKey key, TValue value)
        {
            return self.TryAdd(key, value);
        }

        /// <summary>
        /// Remove an item
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="self">Dictionary to use</param>
        /// <param name="key">Key of item to remove</param>
        /// <returns>True on success, false on failure</returns>
        public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> self, TKey key)
        {
            return ((IDictionary<TKey, TValue>) self).Remove(key);
        }
    }
}
