using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Api.Caches;
using Beamable.Common;
using Beamable.Common.Api;
using Beamable.Common.Api.Stats;

namespace Beamable.Api.Stats
{
   public class StatsService : AbsStatsApi
   {
      public StatsService (PlatformService platform, IBeamableRequester requester, UserDataCache<Dictionary<string, string>>.FactoryFunction factoryFunction)
      : base(requester, platform, factoryFunction)
      {
      }

      protected override Promise<Dictionary<long, Dictionary<string, string>>> Resolve(string prefix, List<long> gamerTags)
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
         return Requester.Request<BatchReadStatsResponse>(
            Method.GET,
            $"/basic/stats/client/batch?format=stringlist&objectIds={queryString}",
            useCache: true
         ).RecoverWith(ex =>
            {
               return OfflineCache.RecoverDictionary<Dictionary<string, string>>(ex, "stats", Requester.AccessToken, gamerTags).Map(
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
               OfflineCache.Merge("stats", Requester.AccessToken, playerStats);
            });
      }


   }

}