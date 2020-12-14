using Beamable.Api.Stats;
using Beamable.Common.Api;
using Beamable.Common.Api.Tournaments;

namespace Beamable.Api.Tournaments
{
   public class TournamentService : TournamentApi
   {
      public TournamentService(StatsService stats, IBeamableRequester requester, IUserContext ctx) : base(stats, requester, ctx)
      {
      }
   }

}