/*
 * $Id: IrcUser.cs 198 2005-06-08 16:50:11Z meebey $
 * $URL: svn+ssh://svn.qnetp.net/svn/smartirc/SmartIrc4net/trunk/src/IrcClient/IrcUser.cs $
 * $Rev: 198 $
 * $Author: meebey $
 * $Date: 2005-06-08 18:50:11 +0200 (Wed, 08 Jun 2005) $
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 *
 * Copyright (c) 2008 Mirco Bauer <meebey@meebey.net> <http://www.meebey.net>
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
    public class WhoInfo
    {
        private string   f_Channel;
        private string   f_Ident;
        private string   f_Host;
        private string   f_Server;
        private string   f_Nick;
        private int      f_HopCount;
        private string   f_Realname;
        private bool     f_IsAway;
        private bool     f_IsOp;
        private bool     f_IsVoice;
        private bool     f_IsIrcOp;
        
        public string Channel {
            get {
                return f_Channel;
            }
        }

        public string Ident {
            get {
                return f_Ident;
            }
        }
        
        public string Host {
            get {
                return f_Host;
            }
        }
        
        public string Server {
            get {
                return f_Server;
            }
        }
        
        public string Nick {
            get {
                return f_Nick;
            }
        }
        
        public int HopCount {
            get {
                return f_HopCount;
            }
        }
        
        public string Realname {
            get {
                return f_Realname;
            }
        }

        public bool IsAway {
            get {
                return f_IsAway;
            }
        }

        public bool IsOp {
            get {
                return f_IsOp;
            }
        }

        public bool IsVoice {
            get {
                return f_IsVoice;
            }
        }

        public bool IsIrcOp {
            get {
                return f_IsIrcOp;
            }
        }
        
        private WhoInfo()
        {
        }
        
        public static WhoInfo Parse(IrcMessageData data)
        {
            WhoInfo whoInfo = new WhoInfo();
            // :fu-berlin.de 352 meebey_ * ~meebey e176002059.adsl.alicedsl.de fu-berlin.de meebey_ H :0 Mirco Bauer..
            whoInfo.f_Channel  = data.RawMessageArray[3];
            whoInfo.f_Ident    = data.RawMessageArray[4];
            whoInfo.f_Host     = data.RawMessageArray[5];
            whoInfo.f_Server   = data.RawMessageArray[6];
            whoInfo.f_Nick     = data.RawMessageArray[7];
            // skip hop count
            whoInfo.f_Realname = String.Join(" ", data.MessageArray, 1, data.MessageArray.Length - 1);
            
            int    hopcount = 0;
            string hopcountStr = data.MessageArray[0];
            try {
                hopcount = int.Parse(hopcountStr);
            } catch (FormatException ex) {
#if LOG4NET
                Logger.MessageParser.Warn("Parse(): couldn't parse (as int): '" + hopcountStr + "'", ex);
#endif
            }

            string usermode = data.RawMessageArray[8];
            bool op = false;
            bool voice = false;
            bool ircop = false;
            bool away = false;
            int usermodelength = usermode.Length;
            for (int i = 0; i < usermodelength; i++) {
                switch (usermode[i]) {
                    case 'H':
                        away = false;
                    break;
                    case 'G':
                        away = true;
                    break;
                    case '@':
                        op = true;
                    break;
                    case '+':
                        voice = true;
                    break;
                    case '*':
                        ircop = true;
                    break;
                }
            }
            whoInfo.f_IsAway = away;
            whoInfo.f_IsOp = op;
            whoInfo.f_IsVoice = voice;
            whoInfo.f_IsIrcOp = ircop;
            
            return whoInfo;
        }
    }
}
