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



namespace Meebey.SmartIrc4net
{
    public class BanInfo
    {
        private string   f_Channel;
        private string   f_Mask;
        
        public string Channel {
            get {
                return f_Channel;
            }
        }

        public string Mask {
            get {
                return f_Mask;
            }
        }
        
        private BanInfo()
        {
        }
        
        public static BanInfo Parse(IrcMessageData data)
        {
            BanInfo info = new BanInfo();
            // :magnet.oftc.net 367 meebey #smuxi test!test@test meebey!~meebey@e176002059.adsl.alicedsl.de 1216309801..
            info.f_Channel = data.RawMessageArray[3];
            info.f_Mask= data.RawMessageArray[4];
            return info;
        }
    }
}
