/**
 * $Id: test.cs,v 1.4 2004/05/20 14:20:38 meebey Exp $
 * $Revision: 1.4 $
 * $Author: meebey $
 * $Date: 2004/05/20 14:20:38 $
 *
 * SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
 * This is a benchmark test client for the library.
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
 
using System;
using Meebey.SmartIrc4net;

public class Benchmark
{
	public static void Main(string[] args)
	{
	   DateTime start, end;

	   start = DateTime.Now;
	   IrcClientList();
	   end = DateTime.Now;
	   Console.WriteLine("IrcClientList() took "+end.Subtract(start).TotalSeconds+" sec");
	   
	   start = DateTime.Now;
	   IrcConnectionList();
	   end = DateTime.Now;
	   Console.WriteLine("IrcConnectionList() took "+end.Subtract(start).TotalSeconds+" sec");
	}
	
	public static void IrcClientList()
	{
	   IrcClient irc = new IrcClient();
	   irc.OnRawMessage += new IrcEventHandler(IrcClientListCallback);
	   irc.Connect("irc.freenet.de", 6667);
	   irc.Login("SmartIRC", "Benchmark Bot");
	   irc.RfcList("#C#");
	   irc.Listen();
	}
	
	public static void IrcClientListCallback(object sender, IrcEventArgs e)
	{
	   if (e.Data.ReplyCode == ReplyCode.List) {
	       Console.WriteLine("On the IRC channel #php are "+e.Data.RawMessageArray[4]+" users");
	       e.Data.Irc.Disconnect();
	   }
	}
	
	public static void IrcConnectionList()
	{
	   IrcConnection irc = new IrcConnection();
	   irc.OnReadLine += new ReadLineEventHandler(IrcConnectionListCallback);
	   irc.Connect("irc.freenet.de", 6667);
	   irc.WriteLine(Rfc2812.Nick("SmartIRC"), Priority.Critical);
	   irc.WriteLine(Rfc2812.User("SmartIRC", 0, "Benchmark Bot"), Priority.Critical);
	   irc.WriteLine(Rfc2812.List("#C#"));
	   irc.Listen();
	}
	
	public static void IrcConnectionListCallback(object sender, ReadLineEventArgs e)
	{
	   string[] linear = e.Line.Split(new char[] {' '});
	   if (linear.Length >= 5 && linear[1] == "322") {
	       Console.WriteLine("On the IRC channel #php are "+linear[4]+" users");
	       ((IrcConnection)sender).Disconnect();
	   }
	}
}
