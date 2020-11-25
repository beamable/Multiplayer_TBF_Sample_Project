using System;
using System.Collections.Generic;

namespace Beamable.Common.Api.Stats
{
   public interface IStatsApi
   {
      UserDataCache<Dictionary<string, string>> GetCache(string prefix);
      Promise<EmptyResponse> SetStats(string access, Dictionary<string, string> stats);
      Promise<Dictionary<string, string>> GetStats(string domain, string access, string type, long id);
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