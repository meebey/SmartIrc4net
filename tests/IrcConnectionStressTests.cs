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
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

namespace Meebey.SmartIrc4net
{
    internal class IrcTestChassis : IrcClient
    {
        /// <summary>
        /// Login credentials. These are for a Twitch account especially created for testing SmartIrc4Net.
        /// </summary>
        public const string AccountName = "SmartIrc4NetTestAccount";
        public const string AccountPassword = "oauth:59mfpivnzqtgpjojzkulygz1v6mbq2";

        /// <summary>
        /// Event counters
        /// </summary>
        public static Dictionary<string, int> Counts { get; set; } = new Dictionary<string, int>();

        /// <summary>
        /// Unique ID of latest instance
        /// </summary>
        private static int NextId = 1;

        /// <summary>
        /// Unique ID of this instance
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Epoch (start time of current test)
        /// </summary>
        private static DateTime Epoch;

        /// <summary>
        /// Global counts across all test IRC instances
        /// </summary>
        public static void Reset()
        {
            Counts.Clear();
            Counts.Add("Rx", 0);
            Counts.Add("Tx", 0);
            Counts.Add("Connect", 0);
            Counts.Add("Connecting", 0);
            Counts.Add("Disconnect", 0);
            Counts.Add("Disconnecting", 0);
            Counts.Add("Error", 0);
            Counts.Add("Register", 0);
            Counts.Add("Join", 0);

            Epoch = DateTime.Now;
        }

        /// <summary>
        /// Get an IRC client instance configured for tests
        /// </summary>
        /// <returns></returns>
        public IrcTestChassis()
        {
            Id = NextId++;

            AutoReconnect = false;
            AutoRetry = false;
            SendDelay = 300;

            PingInterval = 60;
            PingTimeout = 10;
            IdleWorkerInterval = Math.Min(PingInterval, PingTimeout) / 5;

            OnReadLine += (s, e) => {
                Debug.WriteLine("[" + Id.ToString().PadRight(3) + "] [" + Math.Round((DateTime.Now - Epoch).TotalSeconds, 3).ToString().PadRight(5) + "] RX: " + e.Line);
                Counts["Rx"]++;
            };

            OnWriteLine += (s, e) => {
                Debug.WriteLine("[" + Id.ToString().PadRight(3) + "] [" + Math.Round((DateTime.Now - Epoch).TotalSeconds, 3).ToString().PadRight(5) + "] TX: " + e.Line);
                Counts["Tx"]++;
            };

            OnConnecting += (s, e) => {
                Debug.WriteLine("[" + Id.ToString().PadRight(3) + "] [" + Math.Round((DateTime.Now - Epoch).TotalSeconds, 3).ToString().PadRight(5) + "] Connecting");
                Counts["Connecting"]++;
            };

            OnConnected += (s, e) => {
                Debug.WriteLine("[" + Id.ToString().PadRight(3) + "] [" + Math.Round((DateTime.Now - Epoch).TotalSeconds, 3).ToString().PadRight(5) + "] Connected");
                Counts["Connect"]++;
            };

            OnRegistered += (s, e) => {
                Debug.WriteLine("[" + Id.ToString().PadRight(3) + "] [" + Math.Round((DateTime.Now - Epoch).TotalSeconds, 3).ToString().PadRight(5) + "] Registered");
                Counts["Register"]++;
            };

            OnConnectionError += (s, e) => {
                Debug.WriteLine("[" + Id.ToString().PadRight(3) + "] [" + Math.Round((DateTime.Now - Epoch).TotalSeconds, 3).ToString().PadRight(5) + "] Connection error");
                Debug.WriteLine(System.Environment.StackTrace);
                Counts["Error"]++;
            };

            OnDisconnecting += (s, e) => {
                Debug.WriteLine("[" + Id.ToString().PadRight(3) + "] [" + Math.Round((DateTime.Now - Epoch).TotalSeconds, 3).ToString().PadRight(5) + "] Disconnecting");
                Counts["Disconnecting"]++;
            };

            OnDisconnected += (s, e) => {
                Debug.WriteLine("[" + Id.ToString().PadRight(3) + "] [" + Math.Round((DateTime.Now - Epoch).TotalSeconds, 3).ToString().PadRight(5) + "] Disconnected");
                Counts["Disconnect"]++;
            };

            OnJoin += (s, e) => {
                Debug.WriteLine("[" + Id.ToString().PadRight(3) + "] [" + Math.Round((DateTime.Now - Epoch).TotalSeconds, 3).ToString().PadRight(5) + "] Joined");
                Counts["Join"]++;
            };
        }
    }

    /// <summary>
    /// IrcConnection stress tests
    /// </summary>
    [TestFixture]
    internal class IrcConnectionStressTests
    {
        /// <summary>
        /// Twitch TCP IRC Chat servers
        /// Last update 01.08.2015
        /// </summary>
        private readonly string[] f_TCPChatHosts = new string[] {
            "192.16.64.146:443",
            "192.16.70.169:443",
            "192.16.64.51:443",
            "192.16.64.45:443",
            "192.16.64.37:443",
            "199.9.248.236:443",
            "192.16.64.144:443",
            "192.16.64.152:443",
            "192.16.64.11:443",
            "192.16.64.155:443",
            "199.9.251.168:443"
        };

        /// <summary>
        /// Twitch WebSocket IRC Chat servers, eg. https://api.twitch.tv/api/channels/djkaty/chat_properties
        /// Last update 31.07.2015
        /// </summary>
        private readonly string[] f_WSChatHosts = new string[] {
            "ws://192.16.64.145",
            "ws://192.16.64.37",
            "ws://192.16.70.169",
            "ws://192.16.64.155",
            "ws://199.9.248.236",
            "ws://192.16.64.146",
            "ws://199.9.251.168",
            "ws://192.16.64.11",
            "ws://192.16.64.51",
            "ws://192.16.64.152",
            "ws://192.16.64.45",
            "ws://192.16.64.144"
        };

        /// <summary>
        /// Our channel
        /// </summary>
        private const string f_Channel = "#smartirc4nettestaccount";

        /// <ummsary>
        /// These tests include rate-limit testing and must be executed serially
        /// </ummsary>
        private AutoResetEvent f_Ready = new AutoResetEvent(true);

        [SetUp]
        public void Setup()
        {
            f_Ready.WaitOne();

            IrcTestChassis.Reset();
        }

        [TearDown]
        public void Teardown()
        {
            f_Ready.Set();
        }

        /// <summary>
        /// This is where the actual test work is done.
        /// The idea is to connect as many times as possible as fast as possible, join channels and
        /// check that SmartIrc4net can handle it and gracefully deal with rate limit-related errors and disconnections.
        ///
        /// Twitch has no per-IP connection limits so we use this IRC network for testing to make things simple.
        /// Twitch limits IP addresses to 50 JOINs and auths (combined total) in a 15 second period
        /// The test values check all possible combinations.
        /// </summary>
        /// <remarks>
        /// The test should PASS when:
        /// - the specified parameters do not exceed the documented rate limits and the server does not rate limit us
        /// - the specified parameters do exceed the documented rate limits and the server rate limits us
        /// 
        /// The test FAILS in ALL other circumstances, so it will therefore fail with the default values
        /// if the rate limits or scope rules change in Twitch's policy-making.
        /// 
        /// The following assumptions are made:
        /// - connection rate limits are per server IP, not across the whole network
        /// - join rate limits are per server IP, not per connection (ie. join limits are shared across connections to the same server)
        /// - clean disconnects do not make the server 'forget' the connect/joins in rate limiting terms
        /// - if joinChannel is set, the number of connections used will be halved and each connection will join 1 channel
        /// 
        /// If non-rate-limiting connection or socket errors occur, the connections will be retried and the expected number of
        /// connects, disconnects, errors and successful logins will be adjusted to match.
        /// 
        /// A 30-second cooldown is executed at the end of each test so that the next test in the sequence does not
        /// exceed the rate limits.
        /// </remarks>
        /// <param name="authJoinTimes">Number of times to auth/join</param>
        /// <param name="secondsLimit">Max number of seconds to allow the test run</param>
        /// <param name="sameServer">True to make all connections to the same server IP, false to round-robin</param>
        /// <param name="joinChannel">True to also join a channel and add it to the authjoin count, false to just auth</param>
        [Test]
        public void ConnectAndJoinStressTest([Values(50, 60)] int authJoinTimes,
            [Values(true, false)] bool sameServer, [Values(true, false)] bool joinChannel, [Values(true, false)] bool useWebSockets)
        {
            // Assumptions

            const int TwitchAuthJoinLimit = 50;
            const int TwitchAuthJoinTimeLimit = 15;

            // Arrange

            // The total number of auths and joins we are expecting to get by the end of the test
            int wantedAuths, wantedJoins;

            // Split expected auths and joins in half if joining a channel
            if (joinChannel) {
                Assert.That(authJoinTimes % 2 == 0, "AuthJoinTimes must be an even number if joining channels");

                wantedAuths = authJoinTimes / 2;
                wantedJoins = authJoinTimes / 2;
            } else {
                wantedAuths = authJoinTimes;
                wantedJoins = 0;
            }

            // Each connection has its own IrcClient and runs in its own thread
            List<IrcTestChassis> clients = new List<IrcTestChassis>(wantedAuths);
            List<Task> connectionTasks = new List<Task>(wantedAuths);

            // Next server in the round-robin list if using multiple servers
            int nextServer = 0;

            // The number of successful and rate-limited auths and joins so far
            int authCount = 0;
            int joinCount = 0;
            int rateLimitedAuth = 0;
            int rateLimitedJoin = 0;

            // Errors expected from re-trying the connection and socket issues which are not related to the test itself
            int expectedReconnects = 0;
            int expectedErrors = 0;

            // Create a bunch of IRC clients
            for (int i = 0; i < wantedAuths; i++) {
                IrcTestChassis client = new IrcTestChassis();

                // Set all clients to zero send delay
                client.SendDelay = 0;

                // Add to client pool
                clients.Add(client);
            }

            // Act
            DateTime startTime = DateTime.Now;

            // Start all the clients on separate threads
            foreach (var client in clients) {
                connectionTasks.Add(Task.Run(() => {

                    bool retry;

                    do {
                        retry = false;

                        // Use a round-robin server or the same server repeatedly
                        while (!client.IsConnected) {
                            // Occasionally we won't be able to connect.
                            // Just re-connect - this isn't related to the test and won't affect the results.
                            // Connection rate limits are based on auth, not actual TCP connections.
                            // IsConnectionError isn't raised during Connect().
                            try {
                                if (!sameServer) {
                                    if (useWebSockets) {
                                        client.Connect(new IrcWebSocketTransport(f_WSChatHosts[nextServer]));
                                        nextServer = (nextServer + 1) % f_WSChatHosts.Length;
                                    } else {
                                        client.Connect(new IrcTcpTransport(f_TCPChatHosts[nextServer]));
                                        nextServer = (nextServer + 1) % f_TCPChatHosts.Length;
                                    }
                                } else {
                                    if (useWebSockets) {
                                        client.Connect(new IrcWebSocketTransport(f_WSChatHosts[0]));
                                    } else {
                                        client.Connect(new IrcTcpTransport(f_TCPChatHosts[0]));
                                    }
                                }
                            } catch (CouldNotConnectException) {
                                Debug.WriteLine("[" + client.Id.ToString().PadRight(3) + "] Could not connect - will retry");
                            }
                        };

                        // Keep an eye out for rate limit warning
                        ManualResetEvent gotLoginResponse = new ManualResetEvent(false);
                        bool rateLimited = false;

                        // After logging in, we'll either get a NOTICE for rate limiting, or an 001 welcome message for successful auth
                        // Make event handlers to catch both of these
                        IrcEventHandler queryEventHandler = (s, e) => {
                            if (e.Data.Message == "Login unsuccessful") {
                                rateLimitedAuth++;
                                rateLimited = true;

                                Debug.WriteLine("[" + client.Id.ToString().PadRight(3) + "] Rate-limited auth");

                                gotLoginResponse.Set();
                            }
                        };

                        EventHandler registeredEventHandler = (s, e) => {
                            gotLoginResponse.Set();
                        };

                        // Attach event handlers to client
                        client.OnQueryNotice += queryEventHandler;
                        client.OnRegistered += registeredEventHandler;

                        // (always uses Priority.Critical)
                        client.Login(IrcTestChassis.AccountName, "", 0, IrcTestChassis.AccountName, IrcTestChassis.AccountPassword);

                        // IsConnected will now be false if there was a socket error while sending the login (not rate-limited auth)
                        if (!client.IsRegistered && !client.IsConnected) {
                            Debug.WriteLine("[" + client.Id.ToString().PadRight(3) + "] Socket error while sending register - will retry");
                            expectedReconnects++;
                            expectedErrors++;
                            retry = true;
                        }

                        // Listen (blocking) until we are auth'd or no longer connected or too much time elapses
                        TimeSpan authResponseTimeout = TimeSpan.FromSeconds(3);
                        DateTime epoch = DateTime.Now;

                        while (client.IsConnected && DateTime.Now - epoch < authResponseTimeout && !gotLoginResponse.WaitOne(0)) {
                            client.Listen(false);
                            gotLoginResponse.WaitOne(100);
                        }

                        // Disconnect cleanly if we get rate-limited
                        // Seems like TCP closes the connection immediately while WebSockets doesn't
                        if (rateLimited) {
                            if (client.IsConnected)
                                client.Disconnect();
                        }

                        // If IsConnected is set, it was a response timeout, otherwise it was a socket error
                        else if (!client.IsRegistered) {
                            Debug.WriteLine("[" + client.Id.ToString().PadRight(3) + "] Timeout/socket error waiting for auth response - will retry");
                            expectedReconnects++;

                            if (!client.IsConnected) {
                                expectedErrors++;
                            }

                            retry = true;

                            if (client.IsConnected) {
                                client.Disconnect();
                            }
                        }

                        // Detach event handlers so they don't fire multiple times if retrying
                        client.OnQueryNotice -= queryEventHandler;
                        client.OnRegistered -= registeredEventHandler;

                    } while (!client.IsRegistered && retry);

                    // Successful auth?
                    if (client.IsRegistered) {
                        lock (this) {
                            ++authCount;
                            Debug.WriteLine("[" + client.Id.ToString().PadRight(3) + "] Auth count: " + authCount);
                        }
                    }

                    // If we're authed and we want to join a channel...
                    if (joinChannel && client.IsRegistered) {
                        ManualResetEvent joined = new ManualResetEvent(false);

                        // Watch for join message
                        client.OnJoin += (s, e) => {
                            if (e.Data.Nick.ToLower() == IrcTestChassis.AccountName.ToLower() && e.Data.Channel == f_Channel) {
                                joined.Set();
                            }
                        };

                        // Join
                        client.RfcJoin(f_Channel, Priority.Critical); // Critical == blocks til sent, we don't wait for the server response

                        // Wait (blocking) until we have joined the channel or been disconnected, or waited too long
                        TimeSpan joinResponseTimeout = TimeSpan.FromSeconds(3);
                        DateTime epoch = DateTime.Now;

                        // Listen (blocking) until we are auth'd or no longer connected or too much time elapses
                        while (client.IsConnected && DateTime.Now - epoch < joinResponseTimeout && !joined.WaitOne(0)) {
                            client.Listen(false);
                            joined.WaitOne(100);
                        }

                        if (joined.WaitOne(0)) {
                            // NOTE: Not thread-safe
                            joinCount++;
                            Debug.WriteLine("[" + client.Id.ToString().PadRight(3) + "] Join count: " + joinCount);
                        } else {
                            rateLimitedJoin++;

                            Debug.WriteLine("[" + client.Id.ToString().PadRight(3) + "] Rate-limited join");
                        }
                    }

                    // Might have already initiated a disconnect in OnQueryNotice if rate limited
                    if (client.IsConnected)
                        client.Disconnect(); // Disconnecting does not affect the rate limit calculations on the server
                }));
            }

            // All the tasks are now running
            Debug.WriteLine("All tasks started in " + Math.Round((DateTime.Now - startTime).TotalMilliseconds) + "ms");

            // Wait until all the connections and joins are made (and all tasks end) or until the time runs out
            int msRemaining = TwitchAuthJoinTimeLimit * 1000 - (int) (DateTime.Now - startTime).TotalMilliseconds;
            bool allComplete = Task.WaitAll(connectionTasks.ToArray(), msRemaining);

            // Show run information
            double timeTaken = (DateTime.Now - startTime).TotalSeconds;

            Debug.WriteLine("******************************* SUMMARY *******************************");

            Debug.WriteLine("Wanted (Auth/join): " + wantedAuths + "/" + wantedJoins + " - Single server? " + (sameServer ? "Yes" : "No"));
            Debug.WriteLine("Expected (Re/Error): " + expectedReconnects + "/" + expectedErrors);

            Debug.WriteLine("Authed to " + authCount + " servers "
                + (joinChannel ? "and joined " + joinCount + " channels " : "")
                + "in " + Math.Round(timeTaken, 2) + " seconds");

            Debug.WriteLine("Rate-limited (Auth/Join): " + rateLimitedAuth + " / " + rateLimitedJoin);

            foreach (var c in IrcTestChassis.Counts)
                Debug.WriteLine(c.Key.PadLeft(20) + ": " + c.Value);

            // Cooldown before making further connections
            Debug.WriteLine("Waiting for 30 second cooldown...");
            Thread.Sleep(30000);

            // Assert

            // Make sure we did it quickly enough (+0.01 for floating point error)
            Assert.Less(timeTaken, TwitchAuthJoinTimeLimit + 0.01, "Time taken");

            // Make sure all the connections completed
            Assert.AreEqual(true, allComplete, "Task completion flag");

            // Make sure counts match up
            Assert.AreEqual(wantedAuths + expectedReconnects, IrcTestChassis.Counts["Connect"], "Connect count");
            Assert.AreEqual(wantedAuths + expectedReconnects, IrcTestChassis.Counts["Disconnect"], "Disconnect count");

            // Connecting on different servers shouldn't trigger any rate limits
            Assert.AreEqual(wantedAuths - rateLimitedAuth, IrcTestChassis.Counts["Register"], "Register count");

            // Connecting on the same server without joins should allow us to auth up to the rate limit cap
            if (sameServer && !joinChannel) {
                Assert.AreEqual(Math.Min(TwitchAuthJoinLimit, wantedAuths), IrcTestChassis.Counts["Register"], "Register count");
            }

            // There shouldn't be any connection errors except from rate-limited joins
            // NOTE! With TCP we don't know we are disconnected from a rate-limited join, but with WebSockets we do :(
            if (useWebSockets) {
                Assert.AreEqual(rateLimitedJoin + expectedErrors, IrcTestChassis.Counts["Error"], "Error count");
            } else {
                Assert.AreEqual(expectedErrors, IrcTestChassis.Counts["Error"], "Error count");
            }

            // Connecting to the same server over the rate limit cap times should lead to (attempts - limit) rate-limited auths
            if (sameServer && !joinChannel)
                Assert.AreEqual(Math.Max(0, wantedAuths - TwitchAuthJoinLimit), rateLimitedAuth, "Auth rate limit count");

            // Check auth rate limit trigger went off only when expected
            if (sameServer)
                if (wantedAuths > TwitchAuthJoinLimit && !joinChannel)
                    Assert.That(rateLimitedAuth > 0, "Rate limit trigger did not occur when expected");
                else if (!joinChannel)
                    Assert.That(rateLimitedAuth == 0, "Rate limit trigger occurred when not expected");

            // Check total number of auths and joins when using channel joining did not exceed limit
            if (sameServer) {
                Assert.AreEqual(Math.Min(wantedAuths + wantedJoins, TwitchAuthJoinLimit),
                    IrcTestChassis.Counts["Register"] + IrcTestChassis.Counts["Join"],
                    "Auth + join total");
            } else {
                Assert.AreEqual(wantedAuths + wantedJoins, IrcTestChassis.Counts["Register"] + IrcTestChassis.Counts["Join"], "Auth + join total");
            }

            // The same number of joins should have been *sent* as auths accepted.
            if (joinChannel)
                Assert.AreEqual(IrcTestChassis.Counts["Join"] + rateLimitedJoin - expectedErrors, IrcTestChassis.Counts["Register"], "Register count with join");

            // The number of auths should have been the number of connections minus the number of rate-limited auths
            Assert.AreEqual(IrcTestChassis.Counts["Register"] + rateLimitedAuth, IrcTestChassis.Counts["Connect"] - expectedReconnects, "Auth vs connect count");
        }
    }
}
