/**
 * $Id: Consts.cs,v 1.4 2003/11/27 23:22:21 meebey Exp $
 * $Revision: 1.4 $
 * $Author: meebey $
 * $Date: 2003/11/27 23:22:21 $
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

namespace Meebey.SmartIrc4net
{
    public enum Priority
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    public enum Category
    {
        Main,
        Connection,
        Socket,
        Queue,
        IrcMessages,
        MessageTypes,
        MessageParser,
        ActionHandler,
        TimeHandler,
        MessageHandler,
        ChannelSyncing,
        UserSyncing,
        Modules,
        DCC
    }

    public enum SendType
    {
        Message,
        Action,
        Notice,
        CtcpReply,
        CtcpRequest
    }

    public enum ReceiveType
    {
        Join,
        Kick,
        Part,
        Quit,
        Who,
        Name,
        Topic,
        BanList,
        TopicChange,
        ModeChange,
        ChannelMode,
        ChannelMessage,
        ChannelAction,
        ChannelNotice,
        QueryMessage,
        QueryAction,
        QueryNotice,
        CtcpReply,
        CtcpRequest,
    }
}

