/**
 * $Id$
 * $URL$
 * $Rev$
 * $Author$
 * $Date$
 *
 * Copyright (c) 2003-2004 Mirco Bauer <meebey@meebey.net> <http://www.meebey.net>
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

using System.Collections;
using System.Collections.Specialized;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    /// 
    /// </summary>
    public class Channel
    {
        private string           _Name;
        private string           _Key       = "";
        private Hashtable        _Users     = Hashtable.Synchronized(new Hashtable());
        private Hashtable        _Ops       = Hashtable.Synchronized(new Hashtable());
        private Hashtable        _Voices    = Hashtable.Synchronized(new Hashtable());
        private StringCollection _Bans      = new StringCollection();
        private string           _Topic     = "";
        private int              _UserLimit = 0;
        private string           _Mode      = "";
        private int              _SynctimeStart;
        private int              _SynctimeStop;
        private int              _Synctime;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"> </param>
        internal Channel(string name)
        {
            _Name = name;
        }

#if LOG4NET
        ~Channel()
        {
            Logger.ChannelSyncing.Debug("Channel ("+Name+") destroyed");
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string Name
        {
            get {
                return _Name;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string Key
        {
            get {
                return _Key;
            }
            set {
                _Key = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public Hashtable Users
        {
            get {
                return (Hashtable)_Users.Clone();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        internal Hashtable UnsafeUsers
        {
            get {
                return _Users;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public Hashtable Ops
        {
            get {
                return (Hashtable)_Ops.Clone();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        internal Hashtable UnsafeOps
        {
            get {
                return _Ops;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public Hashtable Voices
        {
            get {
                return (Hashtable)_Voices.Clone();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        internal Hashtable UnsafeVoices
        {
            get {
                return _Voices;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public StringCollection Bans
        {
            get {
                return _Bans;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string Topic
        {
            get {
                return _Topic;
            }
            set {
                _Topic = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public int UserLimit
        {
            get {
                return _UserLimit;
            }
            set {
                _UserLimit = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public string Mode
        {
            get {
                return _Mode;
            }
            set {
                _Mode = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public int SynctimeStart
        {
            get {
                return _SynctimeStart;
            }
            set {
                _SynctimeStart = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public int SynctimeStop
        {
            get {
                return _SynctimeStop;
            }
            set {
                _SynctimeStop = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <value> </value>
        public int Synctime
        {
            get {
                return _Synctime;
            }
            set {
                _Synctime = value;
            }
        }
    }
}
