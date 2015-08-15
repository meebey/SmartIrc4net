//  SmartIrc4net - the IRC library for .NET/C# <http://smartirc4net.sf.net>
//
//  Copyright (c) 2015 Katy Coe <djkaty@start.no> <http://www.djkaty.com>
//
//  Full LGPL License: <http://www.gnu.org/licenses/lgpl.txt>
//
//  This library is free software; you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as
//  published by the Free Software Foundation; either version 2.1 of the
//  License, or (at your option) any later version.
//
//  This library is distributed in the hope that it will be useful, but
//  WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//  Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public
//  License along with this library; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Meebey.SmartIrc4net
{
    /// <summary>
    /// Test connection and disconnection scenarios (on Windows)
    /// </summary>
    [TestFixture]
    internal class IrcConnectionTests
    {
        private IrcClient irc;

        // IRC server to use for testing (TCP)
        private const string ircHost = "130.239.18.119"; // One of the irc.freenode.net servers
        private const int ircPort = 6665;

        // IRC server to use for testing (WebSockets)
        private string wsHost = "ws://192.16.64.144";

        // Ping timeout settings
        private int pingInterval;
        private int pingTimeout;

        // Number of re-connects we have done so far in tests using AutoReconnect
        private int reconnects;

        // Number of re-connects to do (+1) before stopping a test using AutoReconnect
        private const int maxReconnects = 2;

        /// <summary>
        /// Time to wait before attempting a disconnection in milliseconds
        /// </summary>
        private const int killDelay = 5000;

        /// <summary>
        /// Time to wait before attempting a disconnection in milliseconds
        /// </summary>
        private const int pongDelay = 10000;

        /// <summary>
        /// Time to wait before attempting a re-connection in milliseconds (to avoid throttling)
        /// (gets adjusted as necessary automatically)
        /// </summary>
        private int reconnectDelay = 1000;

        /// <summary>
        /// Last server we conncted to
        /// (used to handle throttling)
        /// </summary>
        private string lastServer = "";

        /// <summary>
        /// Number of times we have connected to the current server in a row during the entire test fixture
        /// (used to handle throttling)
        /// </summary>
        private int totalConnects = 0;

        // Connection state
        private struct ConnectionInfo
        {
            public int dwState;
            public int dwLocalAddr;
            public int dwLocalPort;
            public int dwRemoteAddr;
            public int dwRemotePort;
        }

        // Win32 API: Get all the TCP connections active on the machine (same as doing netstat -an)
        [DllImport("iphlpapi.dll")]
        private static extern int GetTcpTable(IntPtr pTcpTable, ref int pdwSize, bool bOrder);

        // Win32 API: Replace an entry in the TCP connection table
        [DllImport("iphlpapi.dll")]
        private static extern int SetTcpEntry(IntPtr pConnInfo);

        // To prevent certain NUnit Test Runners from running these tests in parallel,
        // as they will open multiple connections to the same server
        // and/or cause flooding/throttling errors
        private AutoResetEvent ready = new AutoResetEvent(true);

        [SetUp]
        public void Setup()
        {
            ready.WaitOne();

            irc = new IrcClient();
            irc.AutoReconnect = false;

            pingInterval = 60;
            pingTimeout = 10;

            reconnects = 0;
        }

        [TearDown]
        public void Teardown()
        {
            ready.Set();
        }

        /// <summary>
        /// This is some pretty low-level sorcery to forcibly interrupt a TCP connection on Windows
        /// The effect is as if something on the user's end caused the user to get immediately disconnected from the server
        /// </summary>
        /// <param name="remoteIp"></param>
        private void destroyTcpConnection(string remoteIp)
        {
            // The TCP table
            List<ConnectionInfo> list = new List<ConnectionInfo>();

            IntPtr buffer = IntPtr.Zero;
            bool allocated = false;

            try {
                // First find out how many bytes we need to store the table
                int iBytes = 0;
                GetTcpTable(IntPtr.Zero, ref iBytes, false);

                // Allocate memory
                buffer = Marshal.AllocCoTaskMem(iBytes);
                allocated = true;

                // Get the TCP table
                GetTcpTable(buffer, ref iBytes, false);

                // Re-engineer it into ConnectionInfo objects
                int structCount = Marshal.ReadInt32(buffer); // first 4 bytes of buffer are number of structs returned
                IntPtr buffSubPointer = buffer;
                buffSubPointer = (IntPtr) ((int) buffer + 4); // skip over those 4 bytes

                ConnectionInfo foo = new ConnectionInfo();
                int sizeOfCI = Marshal.SizeOf(foo); // calculate struct size

                list.Capacity = structCount; // set TCP table size to match

                // Loop through each TCP table row and copy it into a ConnectionInfo struct
                for (int i = 0; i < structCount; i++) {
                    list.Add((ConnectionInfo) Marshal.PtrToStructure(buffSubPointer, typeof(ConnectionInfo)));
                    buffSubPointer = (IntPtr) ((int) buffSubPointer + sizeOfCI);
                }

            } catch (Exception) {
                // Make NUnit fail if we couldn't fetch the table
                Assert.Fail("Could not get TCP table");
            } finally {
                // Free memory allocated for TCP table
                if (allocated)
                    Marshal.FreeCoTaskMem(buffer);
            }

            IPAddress ip = IPAddress.Parse(remoteIp);
            int ipInt = BitConverter.ToInt32(ip.GetAddressBytes(), 0); // NOTE: relies on little-endian CPU architecture
            ConnectionInfo c;
            c.dwLocalAddr = c.dwLocalPort = c.dwRemoteAddr = c.dwRemotePort = c.dwState = 0;

            int result = -1;
            IntPtr pConn = (IntPtr) 0;

            try {
                // Now use LINQ to find the TCP connection to our IRC server
                // Throws InvalidOperationException if no matching entry exists
                c = (from conn in list where conn.dwRemoteAddr == ipInt select conn).First();

                // Change the state of the TCP entry to indicate the connection should be destroyed
                c.dwState = 12; // Delete_TCB

                // Copy the TCP entry into unmanaged space and get a pointer to it
                pConn = Marshal.AllocCoTaskMem(Marshal.SizeOf(c));
                Marshal.StructureToPtr(c, pConn, false);

                // Commit the TCP entry using Win32
                result = SetTcpEntry(pConn);

            } catch (InvalidOperationException ex) {

                // The connection was probably already closed (which it shouldn't be if the tests are working)
                Assert.Fail(ex.Message);

            } finally {

                // Free the unmanaged memory
                Marshal.FreeCoTaskMem(pConn);
            }

            // If we get back error 317, it just means the connection has been closed already,
            // probably from throttling due to reconnecting too fast in the case of using Repeat() on the tests.
            // Increase the timeout or decrease the number of repeats on the tests to avoid this.
            // https://msdn.microsoft.com/en-us/library/windows/desktop/aa366378(v=vs.85).aspx

            // NOTE: This has to go outside the try block otherwise it will crash NUnit as an unhandled user exception
            Assert.That(result, Is.EqualTo(0) | Is.EqualTo(317), "Error setting TCP table entry"); // No error setting TCP table entry

            if (result == 317) {
                Debug.WriteLine("Failed to set TCP entry due to an already closed connection - continuing");
            }

            // The connection will now be destroyed
        }

        /// <summary>
        /// Inject auto-reconnection capability into tests
        /// </summary>
        private void enableAutoReconnect(bool reconnect)
        {
            if (!reconnect)
                return;

            irc.AutoReconnect = true;
            irc.AutoRetryDelay = 2;
            irc.OnReconnected += (s, e) => {
                irc.Login("SmartIRC", "SmartIrc4net Test Bot");
                irc.RfcJoin("#smartirc-test");

                if (++reconnects >= maxReconnects)
                    irc.AutoReconnect = false;
            };
        }

        /// <summary>
        /// Connects to an IRC server with a well-known configuration, runs a connection scenario in another thread
        /// and checks that everything went according to plan
        /// </summary>
        /// <param name="connectionTask">The connection scenario to run in another thread</param>
        private void runConnectionScenario(IIrcTransportManager transport, Action connectionMethod, bool wantError, bool reconnect,
                                            bool connectionExpected = true)
        {
            int rx = 0;
            int gotErrorEvent = 0;
            int gotDisconnectingEvent = 0;
            int gotDisconnectedEvent = 0;

            // Configure connection
            irc.IdleWorkerInterval = Math.Min(pingInterval, pingTimeout) / 5;
            irc.PingInterval = pingInterval;
            irc.PingTimeout = pingTimeout;

            // Prove we are receiving data
            irc.OnRawMessage += (s, e) => {
                Debug.WriteLine("RX: " + e.Data.RawMessage);

                // Fail the test if we got throttled - change the timeouts or repeat quantities to avoid this
                Assert.False(e.Data.RawMessage.Contains("throttled"), "Connection was throttled by the remote service - change your timeouts or repeat values to avoid this and re-run the test");

                rx++;
            };

            // Prove event flow succeeded
            irc.OnConnecting += (s, e) => {
                Debug.WriteLine("OnConnecting fired");
            };

            irc.OnConnected += (s, e) => {
                Debug.WriteLine("OnConnected fired");
            };

            irc.OnRegistered += (s, e) => {
                Debug.WriteLine("OnRegistered fired");
            };

            irc.OnConnectionError += (s, e) => {
                Debug.WriteLine("OnConnectionError fired");
                gotErrorEvent++;
            };

            irc.OnDisconnecting += (s, e) => {
                Debug.WriteLine("OnDisconnecting fired");
                gotDisconnectingEvent++;
            };

            irc.OnDisconnected += (s, e) => {
                Debug.WriteLine("OnDisconnected fired");
                gotDisconnectedEvent++;
            };

            irc.OnReconnected += (s, e) => {
                Debug.WriteLine("OnReconnected fired");
            };

            irc.OnWriteLine += (s, e) => {
                Debug.WriteLine("TX: " + e.Line);
            };

            // Run the scenario in another thread once the connection is established
            irc.OnConnected += (s, e) => {
                Task.Run(connectionMethod);
            };

            // Enable auto-reconnect if applicable to test
            enableAutoReconnect(reconnect);

            // Note how many connection events we're expecting
            int eventExpectation = irc.AutoReconnect ? maxReconnects + 1 : 1;

            // Connect
            Debug.WriteLine("Connecting to IRC server " + transport.Address + ":" + transport.Port);

            // Wait 60 seconds after each 5 connection attempts to the same server avoid throttling
            if (irc.Address != lastServer) {
                totalConnects = 1;
                lastServer = irc.Address;
            } else {
                totalConnects++;
            }

            if ((totalConnects >= 8 && !lastServer.Contains("127.0.0.1") && !lastServer.Contains("localhost")) || irc.AutoReconnect) {
                reconnectDelay = 30000;
                totalConnects = 1;
            } else {
                reconnectDelay = 1000;
            }

            // Try to avert throttling exceptions
            Debug.WriteLine("Waiting for " + reconnectDelay + "ms cooldown period");
            Thread.Sleep(reconnectDelay);

            irc.Connect(transport);
            irc.Login("SmartIRC", "SmartIrc4net Test Bot");
            irc.RfcJoin("#smartirc-test");

            // Start processing incoming data
            Debug.WriteLine("Starting blocking Listen()");
            irc.Listen();
            Debug.WriteLine("Listen() returns");
            Debug.WriteLine(rx + " lines received from IRC server");

            // If we get here, we have detected disconnection fairly quickly (within the timeouts)
            // and disconnected and closed all our threads cleanly

            // NOTE: Doesn't guarantee the events were received in the right order

            // Check situation is as it should be
            Assert.AreEqual(false, irc.IsConnected, "Incorrect value for IsConnected");
            Assert.AreEqual(false, irc.IsDisconnecting, "Incorrect value for IsDisconnecting");
            Assert.AreEqual(false, irc.IsConnectionError, "Incorrect value for IsConnectionError"); // (error is set to false after disconnect completes)

            // Check we got all the events the right amount of times
            Assert.AreEqual(wantError ? eventExpectation : 0, gotErrorEvent, "Wrong number of IsConnectionError events");
            Assert.AreEqual(eventExpectation, gotDisconnectingEvent, "Wrong number of IsDisconnecting events");
            Assert.AreEqual(eventExpectation, gotDisconnectedEvent, "Wrong number of IsDisconnected events");

            // Check we received something
            if (connectionExpected) {
                Assert.Greater(rx, 0, "Didn't receive any data from server");
            } else {
                Assert.AreEqual(rx, 0, "Did not expect to receive any data from server");
            }
        }

        private void cleanDisconnectScenario(IIrcTransportManager transport, bool reconnect)
        {
            runConnectionScenario(transport, async () => {
                Debug.WriteLine("Waiting for " + killDelay + "ms on secondary thread before closing connection");

                await Task.Delay(killDelay);

                Debug.WriteLine("Closing connection");

                if (reconnect && reconnects < maxReconnects)
                    irc.Reconnect();
                else
                    irc.Disconnect();
            }, false, reconnect);
        }

        private void dirtyDisconnectScenario(IIrcTransportManager transport, bool reconnect)
        {
            runConnectionScenario(transport, async () => {
                Debug.WriteLine("Waiting for " + killDelay + "ms on secondary thread before destroying connection");

                await Task.Delay(killDelay);

                Debug.WriteLine("Destroying connection");

                // Does nothing to 1.2.3.4 but turns ws://1.2.3.4 into 1.2.3.4
                string ipNumbers = "/" + transport.Address;
                ipNumbers = ipNumbers.Substring(ipNumbers.LastIndexOf("/") + 1);

                destroyTcpConnection(ipNumbers);
            }, true, reconnect);
        }

        /// <summary>
        /// Simulate a scenario where the client becomes forcibly disconnected from the server (at the client end)
        /// </summary>
        [Test, Timeout(120000), Repeat(3)]
        public void ClientTcpCleanDisconnect()
        {
            IrcTcpTransport tcp = new IrcTcpTransport(ircHost, ircPort);

            cleanDisconnectScenario(tcp, false);
        }

        /// <summary>
        /// Simulate a scenario where the client becomes forcibly disconnected from the server (at the client end) and reconnects
        /// </summary>
        [Test, Timeout(120000)]
        public void ClientTcpCleanDisconnectWithReconnect()
        {
            IrcTcpTransport tcp = new IrcTcpTransport(ircHost, ircPort);

            cleanDisconnectScenario(tcp, true);
        }

        /// <summary>
        /// Simulate a scenario where the client becomes forcibly disconnected from the server (at the client end)
        /// </summary>
        [Test, Timeout(120000), Repeat(3)]
        public void ClientTcpDirtyDisconnect()
        {
            IrcTcpTransport tcp = new IrcTcpTransport(ircHost, ircPort);

            dirtyDisconnectScenario(tcp, false);
        }

        /// <summary>
        /// Simulate a scenario where the client becomes forcibly disconnected from the server (at the client end) and reconnects
        /// </summary>
        [Test, Timeout(120000)]
        public void ClientTcpDirtyDisconnectWithReconnect()
        {
            IrcTcpTransport tcp = new IrcTcpTransport(ircHost, ircPort);

            dirtyDisconnectScenario(tcp, true);
        }

        /// <summary>
        /// Simulate a scenario where the client becomes forcibly disconnected from the server (at the client end)
        /// </summary>
        [Test, Timeout(120000), Repeat(3)]
        public void ClientWsCleanDisconnect()
        {
            IrcWebSocketTransport ws = new IrcWebSocketTransport(wsHost);

            cleanDisconnectScenario(ws, false);
        }

        /// <summary>
        /// Simulate a scenario where the client becomes forcibly disconnected from the server (at the client end) and reconnects
        /// </summary>
        [Test, Timeout(120000)]
        public void ClientWsCleanDisconnectWithReconnect()
        {
            IrcWebSocketTransport ws = new IrcWebSocketTransport(wsHost);

            cleanDisconnectScenario(ws, true);
        }

        /// <summary>
        /// Simulate a scenario where the client becomes forcibly disconnected from the server (at the client end)
        /// </summary>
        [Test, Timeout(120000), Repeat(3)]
        public void ClientWsDirtyDisconnect()
        {
            IrcWebSocketTransport ws = new IrcWebSocketTransport(wsHost);

            dirtyDisconnectScenario(ws, false);
        }

        /// <summary>
        /// Simulate a scenario where the client becomes forcibly disconnected from the server (at the client end) and reconnects
        /// </summary>
        [Test, Timeout(120000)]
        public void ClientWsDirtyDisconnectWithReconnect()
        {
            IrcWebSocketTransport ws = new IrcWebSocketTransport(wsHost);

            dirtyDisconnectScenario(ws, true);
        }

        /// <summary>
        /// Simulate a scenario where the server cannot be reached (can't connect)
        /// </summary>
        [Test]
        public void ConnectionFailedTest([Values(true, false)] bool retry)
        {
            // Don't start the fake IRC server, just let it fail
            IrcTcpTransport tcp = new IrcTcpTransport(FakeIrcServerTask.Address, FakeIrcServerTask.Port);

            // Reconnection parameters if applicable
            if (retry) {
                irc.AutoRetry = true;
                irc.AutoRetryDelay = 3;
                irc.AutoRetryLimit = maxReconnects + 1;
            }

            // Don't do anything in a separate thread, just check for IsConnectionError
            Assert.Throws<CouldNotConnectException>(() => {
                runConnectionScenario(tcp, () => { }, true, false, false);
            });
        }

        /// <summary>
        /// Simulate a scenario where the client has a half-open TCP connection
        /// (ie. the server knows it has been disconnected but the client doesn't, therefore is waiting for an IRC ping-pong)
        /// This also covers the scenario where the user's internet is dropped via router or network cable unplugged.
        /// We need to make sure this doesn't take too long.
        /// </summary>
        [Test, Timeout(120000)]
        public void ClientDisconnectViaPingTimeout([Values(false, true)] bool reconnect)
        {
            // Start fake IRC server
            FakeIrcServerTask fakeIrc = new FakeIrcServerTask();
            fakeIrc.Start();

            // Configure the scenario to kill pong responses after a while
            try {
                IrcTcpTransport tcp = new IrcTcpTransport(FakeIrcServerTask.Address, FakeIrcServerTask.Port);
                tcp.Encoding = Encoding.UTF8;

                pingInterval = 10;
                pingTimeout = 5;

                runConnectionScenario(tcp, async () => {
                    fakeIrc.SendPong = true;

                    Debug.WriteLine("Waiting for " + pongDelay + "ms on secondary thread before disabling pongs");

                    await Task.Delay(pongDelay);

                    Debug.WriteLine("Disabling pongs");

                    fakeIrc.SendPong = false;
                }, true, reconnect);
            } catch (Exception e) {
                Debug.WriteLine(e.Message);
            } finally {
                fakeIrc.Stop();
            }
        }

        /// <summary>
        /// Simulate a scenario where the server deliberately disconnects the client
        /// </summary>
        [Test]
        public void ServerDisconnect([Values(true, false)] bool reconnect)
        {
            // Start fake IRC server
            FakeIrcServerTask fakeIrc = new FakeIrcServerTask();
            fakeIrc.Start();

            // For this test we're going to get the fake IRC server to kill our connection on purpose
            try {
                IrcTcpTransport tcp = new IrcTcpTransport(FakeIrcServerTask.Address, FakeIrcServerTask.Port);
                tcp.Encoding = Encoding.UTF8;

                pingInterval = 20;
                pingTimeout = 5;

                runConnectionScenario(tcp, async () => {
                    Debug.WriteLine("Waiting for " + killDelay + "ms on secondary thread before initiating server disconnect");

                    await Task.Delay(killDelay);

                    Debug.WriteLine("Killing connection from server side");

                    fakeIrc.Reset();
                }, true, reconnect);
            } catch (Exception e) {
                Debug.WriteLine(e.Message);
            } finally {
                fakeIrc.Stop();
            }
        }

        // TODO: Test auto-relogin scenarios
    }

    /// <summary>
    /// A fake IRC server for testing purposes
    /// </summary>
    internal class FakeIrcServerTask
    {
        // The address and port of the server (address should be localhost)
        public const string Address = "127.0.0.1";
        public const int Port = 6661;

        // Set this to stop pongs being sent
        public bool SendPong { get; set; }

        // Our thread
        private Task ircServer;

        // Stream reader
        private StreamReader reader;

        /// <summary>
        /// Cancellation token for ourself
        /// </summary>
        private CancellationTokenSource _CancelTokenSource = new CancellationTokenSource();
        private CancellationToken _CancelToken;

        /// <summary>
        /// Start IRC server as Task
        /// </summary>
        public void Start()
        {
            _CancelToken = _CancelTokenSource.Token;
            ircServer = Task.Factory.StartNew(task, _CancelToken);
        }

        /// <summary>
        /// Stop IRC server
        /// </summary>
        public void Stop()
        {
            _CancelTokenSource.Cancel();

            try {
                ircServer.Wait();
            } catch (AggregateException ex) {

                // We should have a TaskCanceledException here if the CancellationToken was processed correctly
                foreach (var exInner in ex.InnerExceptions) {
                    Debug.WriteLine(exInner.Message);
                }
            }
        }

        /// <summary>
        /// Kill off the current connection and make the server wait for a new connection
        /// </summary>
        public void Reset()
        {
            reader.Close();
        }

        private void task()
        {
            // Configure and start server
            IPAddress host = IPAddress.Parse(Address);
            TcpListener server = new TcpListener(host, Port);

            SendPong = true;

            server.Start();

            Debug.WriteLine("IRC Fake listening");

            while (true) {

                // Wait for a little while to see if something connects or we are quitting
                while (!_CancelToken.IsCancellationRequested && !server.Pending()) {
                    Thread.Sleep(1000);
                }

                if (_CancelToken.IsCancellationRequested)
                    break;

                // Accept connection
                using (TcpClient client = server.AcceptTcpClient()) {

                    // Get strings
                    NetworkStream stream = client.GetStream();
                    reader = new StreamReader(stream, Encoding.UTF8);
                    StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
                    writer.AutoFlush = true;

                    string line, nick = "";
                    Random rand = new Random((int) DateTime.Now.ToBinary());

                    try {
                        while ((line = reader.ReadLine()) != null) {

                            Debug.WriteLine("FakeRX: " + line);

                            if (line.Trim().Length > 0) {
                                switch (line.Substring(0, line.IndexOf(" "))) {

                                    // Store nickname
                                    case "NICK":

                                    nick = line.Substring(line.IndexOf(" ") + 1);
                                    break;

                                    // Login
                                    case "USER":

                                    // Arbitrary delay for 'realism'
                                    Thread.Sleep(2000);

                                    // Send just enough stuff to pacify SmartIrc4net into continuing
                                    writer.WriteLine(":" + Address + " 001 :SmartIRC Welcome message");
                                    writer.WriteLine(":" + Address + " 375 :Start MOTD");
                                    writer.WriteLine(":" + Address + " 372 :MOTD");
                                    writer.WriteLine(":" + Address + " 375 :End of /MOTD command.");
                                    break;

                                    // Join channel
                                    case "JOIN":

                                    string channel = line.Substring(line.IndexOf(" ") + 1);

                                    writer.WriteLine(":" + nick + "!" + Address + " JOIN " + channel);
                                    writer.WriteLine(":" + Address + " MODE " + channel + " +ns");
                                    writer.WriteLine(":" + Address + " 353 " + nick + " @ " + channel + " :@" + nick);
                                    writer.WriteLine(":" + Address + " 356 " + nick + " " + channel + " :End of /NAMES list.");
                                    break;

                                    // Ping
                                    case "PING":

                                    if (SendPong) {
                                        // Wait an arbitrary amount of time before sending PONG
                                        Thread.Sleep(rand.Next(500, 4000));

                                        writer.WriteLine("PONG " + Address);
                                    }
                                    break;
                                }
                            }
                        }
                    } catch (IOException ex) {
                        // We got disconnected from the client for some reason
                        Debug.WriteLine("IRC Fake disconnected: " + ex.Message);
                    }

                    stream.Close();
                }
            }
            server.Stop();
        }
    }
}

