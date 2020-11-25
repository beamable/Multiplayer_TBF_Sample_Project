using System;
using System.Collections.Generic;

namespace Beamable.Common.Api.Leaderboards
{
   public class LeaderboardApi : ILeaderboardApi
   {
      private readonly UserDataCache<RankEntry>.FactoryFunction _factoryFunction;
      public IBeamableRequester Requester { get; }
      public IUserContext UserContext { get; }

      private static long TTL_MS = 60 * 1000;
      private Dictionary<string, UserDataCache<RankEntry>> caches = new Dictionary<string, UserDataCache<RankEntry>>();

      public LeaderboardApi(IBeamableRequester requester, IUserContext userContext, UserDataCache<RankEntry>.FactoryFunction factoryFunction)
      {
         _factoryFunction = factoryFunction;
         Requester = requester;
         UserContext = userContext;
      }

      public UserDataCache<RankEntry> GetCache(string boardId)
      {
         UserDataCache<RankEntry> cache;
         if (!caches.TryGetValue(boardId, out cache))
         {
            cache = _factoryFunction(
               $"Leaderboard.{boardId}",
               TTL_MS,
               (gamerTags => Resolve(boardId, gamerTags))
            );
            caches.Add(boardId, cache);
         }

         return cache;
      }


      public Promise<RankEntry> GetUser(string boardId, long gamerTag)
      {
         return GetCache(boardId).Get(gamerTag);
      }

      public Promise<LeaderBoardView> GetBoard(string boardId, int @from, int max, long? focus = null, long? outlier = null)
      {
         if(string.IsNullOrEmpty(boardId))
         {
            return Promise<LeaderBoardView>.Failed(new Exception("Leaderboard ID cannot be uninitialized."));
         }
         string query = $"from={from}&max={max}";
         if (focus.HasValue)
         {
            query += $"&focus={focus.Value}";
         }
         if (outlier.HasValue)
         {
            query += $"&outlier={outlier.Value}";
         }

         return Requester.Request<LeaderBoardV2ViewResponse>(
            Method.GET,
            $"/object/leaderboards/{boardId}/view?{query}"
         ).Map(rsp => rsp.lb);
      }

      public Promise<EmptyResponse> SetScore(string boardId, double score)
      {
         return Update(boardId, score);
      }

      public Promise<EmptyResponse> IncrementScore(string boardId, double score)
      {
         return Update(boardId, score, true);
      }

      protected Promise<EmptyResponse> Update(string boardId, double score, bool increment = false)
      {
         return Requester.Request<EmptyResponse>(
            Method.PUT,
            $"/object/leaderboards/{boardId}/entry?id={UserContext.UserId}&score={score}&increment={increment}"
         ).Then(_ => GetCache(boardId).Remove(UserContext.UserId));
      }
//      {
//         return Requester.Request<EmptyResponse>(
//            Method.PUT,
//            $"/object/leaderboards/{boardId}/entry?id={_platform.User.id}&score={score}&increment={increment}"
//         ).Then(_ => GetCache(boardId).Remove(_platform.User.id));
//      }

      private Promise<Dictionary<long, RankEntry>> Resolve(string boardId, List<long> gamerTags)
      {
         string queryString = "";
         for (int i = 0; i < gamerTags.Count; i++)
         {
            if (i > 0)
            {
               queryString += ",";
            }

            queryString += gamerTags[i].ToString();
         }

         return Requester.Request<LeaderBoardV2ViewResponse>(
            Method.GET,
            $"/object/leaderboards/{boardId}/ranks?ids={queryString}"
         ).Map(rsp =>
         {
            Dictionary<long, RankEntry> result = new Dictionary<long, RankEntry>();
            var rankings = rsp.lb.ToDictionary();
            for (int i = 0; i < gamerTags.Count; i++)
            {
               RankEntry entry;
               if (!rankings.TryGetValue(gamerTags[i], out entry))
               {
                  entry = new RankEntry();
                  entry.gt = gamerTags[i];
                  entry.columns = new RankEntryColumns();
               }

               result.Add(gamerTags[i], entry);
            }
            return result;
         });
      }

   }
}