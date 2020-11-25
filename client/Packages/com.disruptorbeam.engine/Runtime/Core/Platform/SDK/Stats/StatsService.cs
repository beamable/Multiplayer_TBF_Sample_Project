using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common;
using Beamable.Platform.SDK.Caches;

namespace Beamable.Platform.SDK.Stats
{
   public class StatsService
   {
      private static long TTL_MS = 15 * 60 * 1000;
      private Dictionary<string, UserDataCache<Dictionary<string, string>>> caches = new Dictionary<string, UserDataCache<Dictionary<string, string>>>();
      private PlatformService _platform;
      private PlatformRequester _requester;

      public StatsService (PlatformService platform, PlatformRequester requester)
      {
         _platform = platform;
         _requester = requester;
      }

      public UserDataCache<Dictionary<string, string>> GetCache(string prefix)
      {
         if (!caches.TryGetValue(prefix, out var cache))
         {
            cache = new UserDataCache<Dictionary<string, string>>(
               $"Stats.{prefix}",
               TTL_MS,
               (gamerTags => resolve(prefix, gamerTags))
            );
            caches.Add(prefix, cache);
         }

         return cache;
      }

      private Promise<Dictionary<long, Dictionary<string, string>>> resolve(string prefix, List<long> gamerTags)
      {
         string queryString = "";
         for (int i = 0; i < gamerTags.Count; i++)
         {
            if (i > 0)
            {
               queryString += ",";
            }
            queryString += $"{prefix}{gamerTags[i]}";
         }
         return _requester.Request<BatchReadStatsResponse>(
            Method.GET,
            $"/basic/stats/client/batch?format=stringlist&objectIds={queryString}",
            useCache: true
         ).RecoverWith(ex =>
            {
               return OfflineCache.RecoverDictionary<Dictionary<string, string>>(ex, "stats", _requester.Token, gamerTags).Map(
                  stats =>
                  {
                     var results = stats.Select(kvp =>
                     {
                        return new BatchReadEntry
                        {
                           id = kvp.Key,
                           stats = kvp.Value.Select(statKvp => new StatEntry
                           {
                              k = statKvp.Key,
                              v = statKvp.Value
                           }).ToList()
                        };
                     }).ToList();

                     var rsp = new BatchReadStatsResponse
                     {
                        results = results
                     };
                     return rsp;
                  });
               /*
                * Handle the NoNetworkConnectivity error, by using a custom cache layer.
                *
                * the "stats" key cache maintains stats for all users, not per request.
                */

            })
            .Map(rsp => rsp.ToDictionary())
            .Then(playerStats =>
            {
               /*
                * Successfully looked up stats. Commit them to the offline cache.
                *
                */
               OfflineCache.Merge("stats", _requester.Token, playerStats);
            });
      }

      public Promise<EmptyResponse> SetStats (string access, Dictionary<string, string> stats) {
         long gamerTag = _platform.User.id;
         string prefix = $"client.{access}.player.";
         return _requester.Request<EmptyResponse>(
            Method.POST,
            $"/object/stats/{prefix}{gamerTag}/client/stringlist",
            new StatUpdates(stats)
         ).Then(_ => GetCache(prefix).Remove(gamerTag));
      }

      public Promise<Dictionary<string, string>> GetStats (string domain, string access, string type, long id)
      {
         string prefix = $"{domain}.{access}.{type}.";
         return GetCache(prefix).Get(id);
      }

   }

   [Serializable]
   public class BatchReadStatsResponse
   {
      public List<BatchReadEntry> results;

      public Dictionary<long, Dictionary<string, string>> ToDictionary () {
         Dictionary<long, Dictionary<string, string>> result = new Dictionary<long, Dictionary<string, string>>();
         foreach (var entry in results)
         {
            result[entry.id] = entry.ToStatsDictionary();
         }
         return result;
      }
   }

   [Serializable]
   public class BatchReadEntry
   {
      public long id;
      public List<StatEntry> stats;

      public Dictionary<string, string> ToStatsDictionary () {
         Dictionary<string, string> result = new Dictionary<string, string>();
         foreach (var stat in stats)
         {
            result[stat.k] = stat.v;
         }
         return result;
      }
   }

   [Serializable]
   public class StatEntry
   {
      public string k;
      public string v;
   }

   [Serializable]
   public class StatUpdates
   {
      public List<StatEntry> set;

      public StatUpdates(Dictionary<string, string> stats)
      {
         set = new List<StatEntry>();
         foreach (var stat in stats)
         {
            var entry = new StatEntry {k = stat.Key, v = stat.Value};
            set.Add(entry);
         }
      }
   }
}
