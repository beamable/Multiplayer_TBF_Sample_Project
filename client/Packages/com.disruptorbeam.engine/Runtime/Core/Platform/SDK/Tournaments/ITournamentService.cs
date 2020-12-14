//
//using Beamable.Common;
//using Beamable.Common.Api.Tournaments;
//
//namespace Beamable.Platform.SDK.Tournaments
//{
//   public interface ITournamentService
//   {
//      Promise<TournamentInfo> GetTournamentInfo(string tournamentContentId);
//      Promise<TournamentInfoResponse> GetAllTournaments();
//      Promise<TournamentChampionsResponse> GetChampions(string tournamentId, int cycleLimit=30);
//
//      Promise<TournamentStandingsResponse> GetGlobalStandings(string tournamentId, int cycle = 0, int from = -1,
//         int max = -1, int focus = -1);
//
//      Promise<TournamentStandingsResponse> GetStandings(string tournamentId, int cycle=0, int from=-1,
//         int max=-1, int focus=-1);
//
//      Promise<TournamentRewardsResponse> GetUnclaimedRewards(string tournamentId);
//      Promise<TournamentRewardsResponse> ClaimAllRewards(string tournamentId);
//      Promise<TournamentPlayerStatus> JoinTournament(string tournamentId, double startScore=0);
//      Promise<Unit> SetScore(string tournamentId, long dbid, double score);
//      Promise<bool> HasJoinedTournament(string tournamentId);
//      Promise<TournamentPlayerStatusResponse> GetPlayerStatus();
//
//      Promise<string> GetPlayerAlias(long playerId, string statName = "alias");
//      Promise<string> GetPlayerAvatar(long playerId, string statName = "avatar");
//   }
//}