/**
 * $Id: Channel.cs,v 1.4 2003/12/28 14:03:50 meebey Exp $
 * $Revision: 1.4 $
 * $Author: meebey $
 * $Date: 2003/12/28 14:03:50 $
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

using System.Collections;
using System.Collections.Specialized;

namespace Meebey.SmartIrc4net
{
    public class Channel
    {
        private string           _Name;
        private string           _Key       = "";
        private Hashtable        _Users     = new Hashtable();
        private Hashtable        _Ops       = new Hashtable();
        private Hashtable        _Voices    = new Hashtable();
        private StringCollection _Bans      = new StringCollection();
        private string           _Topic     = "";
        private int              _UserLimit = 0;
        private string           _Mode      = "";
        private int              _SynctimeStart;
        private int              _SynctimeStop;
        private int              _Synctime;

        public Channel(string name)
        {
            _Name = name;
        }

        ~Channel()
        {
#if LOG4NET
            Logger.ChannelSyncing.Debug("Channel ("+Name+") destroyed");
#endif
        }

        public string Name
        {
            get {
                return _Name;
            }
        }

        public string Key
        {
            get {
                return _Key;
            }
            set {
                _Key = value;
            }
        }

        public Hashtable Users
        {
            get {
                return _Users;
            }
            set {
                _Users = value;
            }
        }

        public Hashtable Ops
        {
            get {
                return _Ops;
            }
            set {
                _Ops = value;
            }
        }

        public Hashtable Voices
        {
            get {
                return _Voices;
            }
            set {
                _Voices = value;
            }
        }

        public StringCollection Bans
        {
            get {
                return _Bans;
            }
            set {
                _Bans = value;
            }
        }

        public string Topic
        {
            get {
                return _Topic;
            }
            set {
                _Topic = value;
            }
        }

        public int UserLimit
        {
            get {
                return _UserLimit;
            }
            set {
                _UserLimit = value;
            }
        }

        public string Mode
        {
            get {
                return _Mode;
            }
            set {
                _Mode = value;
            }
        }

        public int SynctimeStart
        {
            get {
                return _SynctimeStart;
            }
            set {
                _SynctimeStart = value;
            }
        }

        public int SynctimeStop
        {
            get {
                return _SynctimeStop;
            }
            set {
                _SynctimeStop = value;
            }
        }

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
