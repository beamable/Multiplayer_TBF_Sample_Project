
using System.Collections.Generic;
using Beamable.Common;

namespace Beamable.Common.Api.Tournaments
{
   public interface ITournamentApi
   {
      Promise<TournamentInfo> GetTournamentInfo(string tournamentContentId);
      Promise<TournamentInfoResponse> GetAllTournaments();
      Promise<TournamentChampionsResponse> GetChampions(string tournamentId, int cycleLimit=30);

      Promise<TournamentStandingsResponse> GetGlobalStandings(string tournamentId, int cycle = 0, int from = -1,
         int max = -1, int focus = -1);

      Promise<TournamentStandingsResponse> GetStandings(string tournamentId, int cycle=0, int from=-1,
         int max=-1, int focus=-1);

      Promise<TournamentRewardsResponse> GetUnclaimedRewards(string tournamentId);
      Promise<TournamentRewardsResponse> ClaimAllRewards(string tournamentId);
      Promise<TournamentPlayerStatus> JoinTournament(string tournamentId, double startScore=0);
      Promise<Unit> SetScore(string tournamentId, long dbid, double score);
      Promise<bool> HasJoinedTournament(string tournamentId);
      Promise<TournamentPlayerStatusResponse> GetPlayerStatus();

      Promise<string> GetPlayerAlias(long playerId, string statName = "alias");
      Promise<string> GetPlayerAvatar(long playerId, string statName = "avatar");
   }


   [System.Serializable]
   public class TournamentEntry
   {
      public long playerId;
      public long rank;
      public int stageChange;
      public double score;
      public List<TournamentRewardCurrency> currencyRewards;
   }

   [System.Serializable]
   public class TournamentChampionEntry
   {
      public long playerId;
      public double score;
      public int cyclesPrior;
   }

   [System.Serializable]
   public class TournamentStandingsResponse
   {
      public TournamentEntry me;
      public List<TournamentEntry> entries;
   }

   [System.Serializable]
   public class TournamentChampionsResponse
   {
      public List<TournamentChampionEntry> entries;
   }

   [System.Serializable]
   public class TournamentRewardCurrency
   {
      public string symbol;
      public int amount;
   }

   [System.Serializable]
   public class TournamentRewardsResponse
   {
      public List<TournamentRewardCurrency> rewardCurrencies;
   }

   [System.Serializable]
   public class TournamentJoinRequest
   {
      public string tournamentId;
   }

   [System.Serializable]
   public class TournamentScoreRequest
   {
      public string tournamentId;
      public long playerId;
      public double score;
      // TODO: add optionalBool incrememnt?
      // TODO: add optionalMap<string, any> stats?

   }

   [System.Serializable]
   public class TournamentInfo
   {
      public string tournamentId;
      public string contentId;
      public long secondsRemaining;
   }

   [System.Serializable]
   public class TournamentInfoResponse
   {
      public List<TournamentInfo> tournaments;
   }

   [System.Serializable]
   public class TournamentPlayerStatus
   {
      public string contentId;
      public string tournamentId;
      public long playerId;
      public int tier;
      public int stage;
   }

   [System.Serializable]
   public class TournamentScoreResponse
   {
      public string result;
   }

   [System.Serializable]
   public class TournamentPlayerStatusResponse
   {
      public List<TournamentPlayerStatus> statuses;
   }
}