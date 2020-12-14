using System;
using System.Collections;
using System.Collections.Generic;
using Beamable.ConsoleCommands;
using Beamable.Coroutines;
using Beamable.Api.Analytics;
using Beamable.Api.Entitlements;
using Beamable.Api.Groups;
using Beamable.Api.Mail;
using Beamable.Common.Api.Groups;
using Beamable.Common.Api.Mail;
using Beamable.Service;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Scripting;

namespace Beamable.Api
{
    [DBeamConsoleCommandProvider]
    public class PlatformConsoleCommands
    {
        private DBeamConsole DBeamConsole => ServiceManager.Resolve<DBeamConsole>();
        private CoroutineService CoroutineService => ServiceManager.Resolve<CoroutineService>();

        [Preserve]
        public PlatformConsoleCommands()
        {
        }

        [DBeamConsoleCommand("IDFA", "print advertising identifier", "IDFA")]
        private string PrintAdvertisingIdentifier(string[] args)
        {
            Application.RequestAdvertisingIdentifierAsync((id, trackingEnabled, error) =>
                DBeamConsole.Log($"AdId = {id}\nTrackingEnabled={trackingEnabled}\nError = {error}"));

            return String.Empty;
        }

        [DBeamConsoleCommand("RESET", "Clear the access token and start with a fresh account", "RESET")]
        protected string ResetAccount(params string[] args)
        {
            var platform = ServiceManager.Resolve<PlatformService>();
            platform.ClearToken();
            platform.ClearDeviceUsers();
            DBeamConsole.Log(ForceRestart());
            return "Attempting access token reset...";
        }

        [DBeamConsoleCommand(new [] { "FORCE-RESTART", "FR"}, "Restart the game as if it had just been launched", "FORCE-RESTART")]
        public static string ForceRestart(params string[] args)
        {
            ServiceManager.OnTeardown();
            return "Game Restarted.";
        }

        /// <summary>
        /// Send a local notification test at some delay.
        /// </summary>
        [DBeamConsoleCommand(new [] {"LOCALNOTE", "LN"}, "Send a local notification. Default delay is 10 seconds.", "LOCALNOTE [<delay> [<title> [<body>]]]")]
        private string LocalNotificationCommand(params string[] args)
        {
            var title = "Test Notification Message Title";
            var message =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";
            var delay = 10;
            if (args.Length >= 1)
            {
                int.TryParse(args[0], out delay);
            }
            if (args.Length >= 2)
            {
                title = args[1];
            }
            if (args.Length >= 3)
            {
                message = args[2];
            }
            var customData = new Dictionary<string, string> {{"evt", "test"}, {"foo", "123"}};
            var service = ServiceManager.Resolve<PlatformService>();

            string channel = "test";

            service.Notification.CreateNotificationChannel(channel, "Test", "Test notifications of regular importance.");
            service.Notification.ScheduleLocalNotification(channel, "DBCONSOLE", 0, title, message,
                TimeSpan.FromSeconds(delay), false, customData);
            return string.Format("Scheduled notification for {0} seconds in the future.", delay);
        }

        [DBeamConsoleCommand("GCCOLLECT", "Do a GC Collect and Unload Unused Assets", "GCCOLLECT")]
        private static string GCCollect(params string[] args)
        {
            Profiler.BeginSample("Memory collect test");
            Profiler.BeginSample("GC.Collect");
            System.GC.Collect();
            Profiler.EndSample();
            Profiler.BeginSample("Resources.UnloadUnusedAssets");
            Resources.UnloadUnusedAssets();
            Profiler.EndSample();
            Profiler.EndSample();
            return "";
        }

        [DBeamConsoleCommand(new [] {"TIMESCALE", "TS"}, "Sets the current timescale", "TIMESCALE <value> | variable")]
        private string Timescale(params string[] args)
        {
            if (args.Length < 1)
            {
                return DBeamConsole.Help("TIMESCALE");
            }

            float timescale = 1;
            CoroutineService.StopCoroutine("VariableTimescale");
            if (args[0] == "variable")
            {
                CoroutineService.StartCoroutine("VariableTimescale");
                return "variable timescale";
            }
            else if (float.TryParse(args[0], out timescale))
            {
                Time.timeScale = timescale;
                return "setting timescale to " + timescale;
            }

            return "unknown timescale";
        }

        private IEnumerator VariableTimescale()
        {
            while (true)
            {
                Time.timeScale = (float) Mathf.Sqrt(UnityEngine.Random.Range(0f, 20.0f));
                yield return null;
            }
        }


        [DBeamConsoleCommand("SUBSCRIBER_DETAILS", "Query subscriber details", "SUBSCRIBER_DETAILS")]
        public string SubscriberDetails(string[] args)
        {
            ServiceManager.Resolve<PlatformService>().PubnubNotificationService.GetSubscriberDetails().Then(rsp => {
                DBeamConsole.Log(
                    rsp.authenticationKey + " " +
                    rsp.customChannelPrefix + " " +
                    rsp.gameGlobalNotificationChannel + " " +
                    rsp.gameNotificationChannel + " " +
                    rsp.playerChannel + " " +
                    rsp.playerForRealmChannel + " " +
                    rsp.subscribeKey
                );
            }).Error(err => {
                DBeamConsole.Log("Failed: " + err.ToString());
            });
            return "";
        }

        [DBeamConsoleCommand("DBID", "Show current player DBID", "DBID")]
        private string ShowDBID(params string[] args)
        {
            return ServiceManager.Resolve<PlatformService>().User.id.ToString();
        }

        [DBeamConsoleCommand("ENTITLEMENTS", "Show current player entitlements", "ENTITLEMENTS <symbol> <state>")]
        private string ShowEntitlements(params string[] args)
        {
            if (args.Length != 2)
            {
                return "ENTITLEMENTS <symbol> <state>";
            }

            ServiceManager.Resolve<PlatformService>().Entitlements.Get(args[0], args[1]).Then((rsp) => {
                var entitlements = rsp.entitlements;
                string result = "";
                for (int i = 0; i < entitlements.Count; i++)
                {
                    Entitlement next = entitlements[i];
                    result += next.symbol + " " + next.specialized + " " + next.state + "\n";
                }
                DBeamConsole.Log(result);
            }).Error((err) => {
                DBeamConsole.Log("Failed: " + err.ToString());
            });
            return "";
        }

        [DBeamConsoleCommand("HEARTBEAT", "Get heartbeat of a user", "HEARTBEAT <dbid>")]
        string GetHeartbeat(params string[] args)
        {
            if (args.Length != 1)
            {
                return "Requires dbid";
            }
            var dbid = long.Parse(args[0]);
            ServiceManager.Resolve<PlatformService>().Session.GetHeartbeat(dbid)
                .Then(rsp => { DBeamConsole.Log(rsp.ToString()); })
                .Error(err => { DBeamConsole.Log(String.Format("Error:", err)); });

            return "Querying...";
        }

        /**
         * Login to a previously registered account with the given username and password.
         */
        [DBeamConsoleCommand("LOGIN_ACCOUNT", "Log in to the DBID designated by the given username and password", "LOGIN_ACCOUNT <email> <password>")]
        string LoginAccount(params string[] args)
        {
            if (args.Length < 2)
            {
                return "Requires both an email and a password.";
            }
            var email = args[0];
            var password = args[1];
            ServiceManager.Resolve<PlatformService>().Auth.Login(email, password).Then(rsp =>
            {
                ServiceManager.Resolve<PlatformService>().SaveToken(rsp);
                ServiceManager.Resolve<PlatformService>().ReloadUser();
                DBeamConsole.Log(String.Format("Successfully logged in as {0}.", email));
            }).Error(err =>
            {
                if (err is PlatformRequesterException code && code.Error.error == "UnableToMergeError")
                {
                    ServiceManager.Resolve<PlatformService>().Auth.Login(email, password, mergeGamerTagToAccount: false)
                        .Then(rsp =>
                        {
                            ServiceManager.Resolve<PlatformService>().SaveToken(rsp);
                            ServiceManager.Resolve<PlatformService>().ReloadUser();
                            DBeamConsole.Log(String.Format("Successfully SWITCHED to {0}. Resetting", email));
                            DBeamConsole.Execute("RESET");
                        }).Error(err2 =>
                        {
                            DBeamConsole.Log(String.Format("There was an error trying to log in as user: {0} - {1}", email, err2));
                        });
                }
                else
                {
                    DBeamConsole.Log(String.Format("There was an error trying to log in as user: {0} - {1}", email, err));
                }
            });
            return "Logging in as user: " + email;
        }

        /**
         * Get the counts of the mailbox
         */
        [DBeamConsoleCommand("MAIL_GET", "Get mailbox messages", "MAIL_GET <category>")]
        string GetMail(params string[] args)
        {
            ServiceManager.Resolve<PlatformService>().Mail.GetMail(args[0]).Then(rsp =>
            {
                for (int i=0; i<rsp.result.Count; i++) {
                    var next = rsp.result[i];
                    DBeamConsole.Log("[" + next.id + "]");
                    DBeamConsole.Log("FROM: " + next.senderGamerTag);
                    DBeamConsole.Log(next.subject);
                    DBeamConsole.Log("(" + next.attachments.Count + " attachments)");
                    DBeamConsole.Log(next.body);
                    DBeamConsole.Log("");
                }
                DBeamConsole.Log("DONE");
            }).Error(err =>
            {
                DBeamConsole.Log(String.Format("Error:", err));
            });
            return "Querying...";
        }

        /**
         * Update a mail in the mailbox
         */
        [DBeamConsoleCommand("MAIL_UPDATE", "Update a mail", "MAIL_UODATE <id> <state> <acceptAttachments>")]
        string UpdateMail(params string[] args)
        {
            if (args.Length < 2)
            {
                return "Requires mailId and state";
            }

            string mailId = args[0];
            string stateStr = args[1];
            MailState state = (MailState)Enum.Parse(typeof(MailState), stateStr);
            bool acceptAttachments = args.Length >= 3;

            MailUpdateRequest updates = new MailUpdateRequest();
            updates.Add(long.Parse(mailId), state, acceptAttachments, "");
            ServiceManager.Resolve<PlatformService>().Mail.Update(updates).Then(rsp =>
            {
                DBeamConsole.Log(JsonUtility.ToJson(rsp));
            }).Error(err =>
            {
                DBeamConsole.Log(String.Format("Error:", err));
            });
            return "Updating...";
        }

        /**
         * Registers the current DBID to the given username and password.
         */
        [DBeamConsoleCommand("REGISTER_ACCOUNT", "Registers this DBID with the given username and password", "REGISTER_ACCOUNT <email> <password>")]
        string RegisterAccount(params string[] args)
        {
            if (args.Length < 2)
            {
                return "Requires both an email and a password.";
            }
            var email = args[0];
            var password = args[1];
            ServiceManager.Resolve<PlatformService>().Auth.RegisterDBCredentials(email, password)
                .Then(rsp => { DBeamConsole.Log(String.Format("Successfully registered user {0}", email)); })
                .Error(err => { DBeamConsole.Log(err.ToString()); });

            return "Registering user: " + email;
        }

        [DBeamConsoleCommand("TOKEN", "Show current access token", "TOKEN")]
        private static string ShowToken(params string[] args)
        {
            return ServiceManager.Resolve<PlatformService>().AccessToken.Token;
        }

        [DBeamConsoleCommand("EXPIRE_TOKEN", "Expires the current access token to trigger the refresh flow", "EXPIRE_TOKEN")]
        public string ExpireAccessToken(params string[] args)
        {
            var platform = ServiceManager.Resolve<PlatformService>();
            platform.AccessToken.ExpireAccessToken();
            ServiceManager.OnTeardown();
            return "Access Token is now expired. Restarting.";
        }

        [DBeamConsoleCommand("CORRUPT_TOKEN", "Corrupts the current access token to trigger the refresh flow", "CORRUPT_TOKEN")]
        public string CorruptAccessToken(params string[] args)
        {
            var platform = ServiceManager.Resolve<PlatformService>();
            platform.AccessToken.CorruptAccessToken();
            return "Access Token has been corrupted.";
        }

        [DBeamConsoleCommand("TEST-ANALYTICS", "Run 1000 events to test batching/load", "TEST-ANALYTICS")]
        public string TestAnalytics(params string[] args)
        {
            var evt = new SampleCustomEvent("lorem ipsum dolar set amet", "T-T-T-Test the base!");

            ServiceManager.Resolve<PlatformService>().Analytics.TrackEvent(evt);
            for (var i = 0; i < 1000; ++i)
            {
                ServiceManager.Resolve<PlatformService>().Analytics.TrackEvent(evt);
            }
            return "Analytics Sent";
        }

        [DBeamConsoleCommand("IAP_BUY", "Invokes the real money transaction flow to purchase the given item_symbol.", "IAP_BUY <listing> <sku>")]
        string IAPBuy(params string[] args)
        {
            if (args.Length != 2)
            {
                return "Requires: <listing> <sku>";
            }

            ServiceManager.Resolve<PlatformService>().PaymentDelegate.StartPurchase(args[0], args[1])
                .Then((txn) => { DBeamConsole.Log("Purchase Complete: " + txn.Txid); })
                .Error((err) => { DBeamConsole.Log("Purchase Failed: " + err.ToString()); });

            return "Purchasing item: " + args[0];
        }

        [DBeamConsoleCommand("IAP_PENDING", "Displays pending transactions", "IAP_PENDING")]
        string IAPPending(params string[] args)
        {
            return PlayerPrefs.GetString("pending_purchases");
        }

        [DBeamConsoleCommand("IAP_UNFULFILLED", "Display unfulfilled purchases", "IAP_UNFULFILLED")]
        string IAPUnfulfilled(params string[] args)
        {
            return PlayerPrefs.GetString("unfulfilled_transactions");
        }

        /**
         * Get the group info of a user
         */
        [DBeamConsoleCommand("GROUP_USER", "Query a user for group info", "GROUP_USER <dbid>")]
        string GetGroupUser(params string[] args)
        {
            long gamerTag;
            if (args.Length < 1) {
                gamerTag = ServiceManager.Resolve<PlatformService>().User.id;
            } else {
                gamerTag = long.Parse(args[0]);
            }
            ServiceManager.Resolve<PlatformService>().Groups.GetUser(gamerTag)
                .Then(rsp => { DBeamConsole.Log(JsonUtility.ToJson(rsp)); })
                .Error(err => { DBeamConsole.Log(String.Format("Error:", err)); });
            return "Querying...";
        }

        [DBeamConsoleCommand("GROUP_LEAVE", "Leave the current group", "GROUP_LEAVE")]
        string GroupLeave(params string[] args)
        {
            long gamerTag = ServiceManager.Resolve<PlatformService>().User.id;
            ServiceManager.Resolve<PlatformService>().Groups.GetUser(gamerTag)
                .FlatMap<GroupMembershipResponse>(userRsp => {
                    long group = 0;
                    if (userRsp.member.guild.Count > 0) {
                        group = userRsp.member.guild[0].id;
                    }
                    return ServiceManager.Resolve<PlatformService>().Groups.LeaveGroup(group);
                })
                .Error(err => { DBeamConsole.Log(String.Format("Error:", err)); });
            return "Querying...";
        }

        /**
         * Change your registered alias
         */
        [DBeamConsoleCommand("SET_ALIAS", "Set your alias", "SET_ALIAS <name>")]
        string SetAlias(params string[] args)
        {
            if (args.Length == 0)
            {
                return "Expected <alias>";
            }

            ServiceManager.Resolve<PlatformService>().Tag.UpdateAlias(args[0])
                .Then(rsp => { DBeamConsole.Log(JsonUtility.ToJson(rsp)); })
                .Error(err => { DBeamConsole.Log(String.Format("Error:", err)); });
            return "Querying...";
        }


        /**
         * View stats for a user
         */
        [DBeamConsoleCommand("GET_STATS", "Get stats for some user", "GET_STATS <domain> <access> <type> <id>")]
        string GetStats(params string[] args)
        {
            if (args.Length != 4)
            {
                return "Requires: <DOMAIN> <ACCESS> <TYPE> <ID>";
            }

            var platform = ServiceManager.Resolve<PlatformService>();
            platform.Stats.GetStats(args[0], args[1], args[2], long.Parse(args[3]))
                .Then(rsp =>
                {
                    foreach (var next in rsp)
                    {
                        DBeamConsole.Log(String.Format("{0} = {1}", next.Key, next.Value));
                    }
                    DBeamConsole.Log("Done");
                })
                .Error(err => { DBeamConsole.Log(String.Format("Error:", err)); });
            return "Querying...";
        }

        /**
         * Set stat for a user
         */
        [DBeamConsoleCommand("SET_STAT", "Sets client stat for self", "SET_STAT <access> <key> <value>")]
        string SetStat(params string[] args)
        {
            if (args.Length != 3)
            {
                return "Requires: <ACCESS> <KEY> <VALUE>";
            }

            var platform = ServiceManager.Resolve<PlatformService>();
            Dictionary<string, string> stats = new Dictionary<string, string>();
            stats.Add(args[1], args[2]);
            platform.Stats.SetStats(args[0], stats)
                .Then(rsp => DBeamConsole.Log("Done"))
                .Error(err => DBeamConsole.Log(String.Format("Error:", err)) );
            return "Querying...";
        }

        [DBeamConsoleCommand("SET_TIME", "Sets the override time. If no time is specified, then there will be no override", "SET_TIME <time>")]
        string SetTime(params string[] args)
        {
            var platform = ServiceManager.Resolve<PlatformService>();
            if (args.Length == 0)
            {
                platform.TimeOverride = null;
                return "Clearing Override Time";
            }

            try
            {
                platform.TimeOverride = args[0];
                return String.Format("Setting Override: {0}", platform.TimeOverride);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return "Invalid Time";
            }
        }

        [DBeamConsoleCommand("QA_TEST_PUBLISH", "Test a QA publish", "QA_TEST_PUBLISH")]
        string QATestPublish(params string[] args)
        {
            ServiceManager.Resolve<PlatformService>().QA.Publish("Hello World", new List<string>{ "foo", "bar" })
                .Then(rsp => { DBeamConsole.Log(JsonUtility.ToJson(rsp)); })
                .Error(err => { DBeamConsole.Log(String.Format("Error:", err)); });
            return "Querying...";
        }
    }
}