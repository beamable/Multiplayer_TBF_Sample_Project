using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common;
using Beamable.Platform.SDK.Stats;
using Beamable.Platform.SDK;
using JetBrains.Annotations;

namespace Beamable.Platform.SDK.Tournaments
{
   public class TournamentService : ITournamentService
   {
      const string SERVICE_PATH = "/basic/tournaments";

      private readonly StatsService _stats;
      private PlatformRequester _requester;
      public TournamentService(StatsService stats, PlatformRequester requester)
      {
         _stats = stats;
         _requester = requester;
      }

      public Promise<TournamentInfo> GetTournamentInfo(string tournamentContentId)
      {
         return GetAllTournaments().Map(resp =>
            resp.tournaments.FirstOrDefault(tournament => string.Equals(tournament.contentId, tournamentContentId)));
      }

      public Promise<TournamentInfoResponse> GetAllTournaments()
      {
         var path = $"{SERVICE_PATH}";
         return _requester.Request<TournamentInfoResponse>(Method.GET, path);
      }



      private string ConstructStandingsURLArgs(string tournamentId, int cycle=0, int from=-1, int max=-1, int focus=-1)
      {
         var cycleArg = $"&cycle={cycle}";
         var fromArg = from < 0 ? "" : $"&from={from}";
         var maxArg = max < 0 ? "": $"&max={max}";
         var focusArg = focus < 0 ? "": $"&focus={focus}";
         return $"?tournamentId={tournamentId}{cycleArg}{fromArg}{maxArg}{focusArg}";
      }

      public Promise<TournamentChampionsResponse> GetChampions(string tournamentId, int cycleLimit=30)
      {
         var path = $"{SERVICE_PATH}/champions?tournamentId={tournamentId}&cycles={cycleLimit}";
         return _requester.Request<TournamentChampionsResponse>(Method.GET, path);
      }

      public Promise<TournamentStandingsResponse> GetGlobalStandings(string tournamentId, int cycle = 0, int from = -1,
         int max = -1, int focus = -1)
      {
         var path = $"{SERVICE_PATH}/global{ConstructStandingsURLArgs(tournamentId, cycle, from, max, focus)}";
         return WithEmptyResultsOn404(_requester.Request<TournamentStandingsResponse>(Method.GET, path));
      }

      public Promise<TournamentStandingsResponse> GetStandings(string tournamentId, int cycle=0, int from=-1,
         int max=-1, int focus=-1)
      {
         var path = $"{SERVICE_PATH}/standings{ConstructStandingsURLArgs(tournamentId, cycle, from, max, focus)}";
         return WithEmptyResultsOn404(_requester.Request<TournamentStandingsResponse>(Method.GET, path));
      }

      private Promise<TournamentStandingsResponse> WithEmptyResultsOn404(Promise<TournamentStandingsResponse> promise)
      {
         return promise.Recover(ex =>
         {
            switch (ex)
            {
               case PlatformRequesterException err when err.Status == 404:
                  return new TournamentStandingsResponse
                  {
                     entries = new List<TournamentEntry>(),
                     me = null
                  };
               default:
                  throw ex;
            }
         });
      }

      public Promise<TournamentRewardsResponse> GetUnclaimedRewards(string tournamentId)
      {
         var path = $"{SERVICE_PATH}/rewards?tournamentId={tournamentId}";
         return _requester.Request<TournamentRewardsResponse>(Method.GET, path);
      }

      public Promise<TournamentRewardsResponse> ClaimAllRewards(string tournamentId)
      {
         var path = $"{SERVICE_PATH}/rewards?tournamentId={tournamentId}";
         return _requester.Request<TournamentRewardsResponse>(Method.POST, path);
      }

      public Promise<TournamentPlayerStatus> JoinTournament(string tournamentId, double startScore=0)
      {
         return GetPlayerStatus().FlatMap(allStatus =>
         {
            var existing = allStatus.statuses.FirstOrDefault(status => status.tournamentId.Equals(tournamentId));
            if (existing != null)
            {
               // we have already joined the tournament. Don't do anything.
               return Promise<TournamentPlayerStatus>.Successful(existing);
            }
            else
            {
               // we actually need to join, and set a start score.
               var path = $"{SERVICE_PATH}";
               var body = new TournamentJoinRequest { tournamentId = tournamentId };
               return _requester.Request<TournamentPlayerStatus>(Method.POST, path, body).FlatMap(status =>
                  SetScore(tournamentId, status.playerId, startScore).Map(_ => status)
               );
            }
         });
      }

      public Promise<Unit> SetScore(string tournamentId, long dbid, double score)
      {
         var path = $"{SERVICE_PATH}/score";
         var body = new TournamentScoreRequest
         {
            tournamentId = tournamentId,
            score = score,
            playerId = dbid
         };

         return _requester.Request<TournamentScoreResponse>(Method.POST, path, body).Map(_ => PromiseBase.Unit);
      }

      public Promise<bool> HasJoinedTournament(string tournamentId)
      {
         /*
          * REWORK this to run a call againt /me, and see if it contains it.
          */
//         return GetPlayerStatus(tournamentId)
//            .Map(_ => true)
//            .Recover(ex =>
//            {
//
//               // TODO. if 404's, then say, "false", otherwise, map to true.
//               return false;
//            });
         throw new NotImplementedException();
      }

      public Promise<TournamentPlayerStatusResponse> GetPlayerStatus()
      {
         var path = $"{SERVICE_PATH}/me";
         return _requester.Request<TournamentPlayerStatusResponse>(Method.GET, path);
      }

      public Promise<string> GetPlayerAlias(long playerId, string statName = "alias")
      {
         return _stats.GetStats("client", "public", "player", playerId).Map(stats =>
         {
            var defaultAlias = "unknown";
            stats.TryGetValue(statName, out defaultAlias);
            return defaultAlias;
         });
      }

      public Promise<string> GetPlayerAvatar(long playerId, string statName = "avatar")
      {
         return _stats.GetStats("client", "public", "player", playerId).Map(stats =>
         {
            var defaultAlias = "0";
            stats.TryGetValue("avatar", out defaultAlias);
            return defaultAlias;
         });
      }
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