using Beamable.Common.Api;
using Beamable.Common.Api.Leaderboards;

namespace Beamable.Api.Leaderboard
{
   public class LeaderboardService : LeaderboardApi
   {
      public LeaderboardService(PlatformService platform, IBeamableRequester requester,
         UserDataCache<RankEntry>.FactoryFunction cacheFactory)
         : base(requester, platform, cacheFactory)
      {
      }

      /*
       * Client specific API calls could be here. API calls that the server _shouldn't_ have.
       */
   }
}