using System;
using System.Collections.Generic;
using Beamable.Common;

namespace Beamable.Platform.SDK.Leaderboard
{
   public class LeaderboardService
   {
      private PlatformService _platform;
      private PlatformRequester _requester;
      private static long TTL_MS = 60 * 1000;
      private Dictionary<string, UserDataCache<RankEntry>> caches = new Dictionary<string, UserDataCache<RankEntry>>();

      public LeaderboardService (PlatformService platform, PlatformRequester requester)
      {
         _platform = platform;
         _requester = requester;
      }

      public UserDataCache<RankEntry> GetCache(string boardId)
      {
         UserDataCache<RankEntry> cache;
         if (!caches.TryGetValue(boardId, out cache))
         {
            cache = new UserDataCache<RankEntry>(
               $"Leaderboard.{boardId}",
               TTL_MS,
               (gamerTags => resolve(boardId, gamerTags))
            );
            caches.Add(boardId, cache);
         }

         return cache;
      }

      private Promise<Dictionary<long, RankEntry>> resolve(string boardId, List<long> gamerTags)
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

         return _requester.Request<LeaderBoardV2ViewResponse>(
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

      public Promise<RankEntry> GetUser(string boardId, long gamerTag)
      {
         return GetCache(boardId).Get(gamerTag);
      }

      public Promise<LeaderBoardView> GetBoard(string boardId, int from, int max, long? focus = null, long? outlier = null)
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

         return _requester.Request<LeaderBoardV2ViewResponse>(
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

      private Promise<EmptyResponse> Update(string boardId, double score, bool increment = false)
      {
         return _requester.Request<EmptyResponse>(
            Method.PUT,
            $"/object/leaderboards/{boardId}/entry?id={_platform.User.id}&score={score}&increment={increment}"
         ).Then(_ => GetCache(boardId).Remove(_platform.User.id));
      }
   }

   [Serializable]
   public class LeaderBoardV2ViewResponse
   {
      public LeaderBoardView lb;
   }

   [Serializable]
   public class LeaderBoardView
   {
      public long boardsize;
      public RankEntry rankgt;
      public List<RankEntry> rankings;

      public Dictionary<long, RankEntry> ToDictionary()
      {
         Dictionary<long, RankEntry> result = new Dictionary<long, RankEntry>();
         for (int i = 0; i < rankings.Count; i++)
         {
            var next = rankings[i];
            result.Add(next.gt, next);
         }

         return result;
      }
   }

   [Serializable]
   public class RankEntry
   {
      public long gt;
      public long rank;
      public double score;
      public RankEntryStat[] stats;

      // DEPRECATED: Do not use
      public RankEntryColumns columns;

      public string GetStat(string name)
      {
         int length = stats.Length;
         for(int i = 0; i < length; ++i)
         {
            ref var stat = ref stats[i];
            if (stat.name == name)
            {
               return stat.value;
            }
         }

         return null;
      }

      public double GetDoubleStat(string name, double fallback = 0)
      {
         var stringValue = GetStat(name);

         if (stringValue != null && double.TryParse(stringValue, out var result))
         {
            return result;
         }
         else
         {
            return fallback;
         }
      }
   }

   [Serializable]
   public class RankEntryColumns
   {
      public long score;
   }

   [Serializable]
   public struct RankEntryStat
   {
      public string name;
      public string value;
   }
}