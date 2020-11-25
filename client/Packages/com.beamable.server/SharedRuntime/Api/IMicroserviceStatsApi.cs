using System.Collections.Generic;
using Beamable.Common;
using Beamable.Common.Api;

namespace Beamable.Server.Api
{
   public interface IMicroserviceStatsApi
   {
      /// <summary>
      /// Retrieve a stat value, by key
      /// </summary>
      /// <param name="userId"></param>
      /// <param name="key"></param>
      /// <returns></returns>
      Promise<string> GetProtectedPlayerStat(long userId, string key);

      /// <summary>
      /// Retrieve one or more stat values, each by key
      /// </summary>
      /// <param name="userId"></param>
      /// <param name="key"></param>
      /// <returns></returns>
      Promise<Dictionary<string, string>> GetProtectedPlayerStats(long userId, string[] stats);

      /// <summary>
      /// Set a stat value, by key
      /// </summary>
      /// <param name="userId"></param>
      /// <param name="key"></param>
      /// <param name="value"></param>
      /// <returns></returns>
      Promise<EmptyResponse> SetProtectedPlayerStat(long userId, string key, string value);

      /// <summary>
      /// Set one or more stat values, by key
      /// </summary>
      /// <param name="userId"></param>
      /// <param name="stats"></param>
      /// <returns></returns>
      Promise<EmptyResponse> SetProtectedPlayerStats(long userId, Dictionary<string, string> stats);

      Promise<EmptyResponse> SetStats(string domain, string access, string type, long userId,
         Dictionary<string, string> stats);

      Promise<Dictionary<string, string>> GetStats(string domain, string access, string type, long userId,
         string[] stats);
   }
}