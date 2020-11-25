using System;
using System.Collections.Generic;

namespace Beamable.Common.Api.Leaderboards
{
   public interface ILeaderboardApi
   {
      UserDataCache<RankEntry> GetCache(string boardId);
      Promise<RankEntry> GetUser(string boardId, long gamerTag);
      Promise<LeaderBoardView> GetBoard(string boardId, int from, int max, long? focus = null, long? outlier = null);
      Promise<EmptyResponse> SetScore(string boardId, double score);
      Promise<EmptyResponse> IncrementScore(string boardId, double score);
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

}