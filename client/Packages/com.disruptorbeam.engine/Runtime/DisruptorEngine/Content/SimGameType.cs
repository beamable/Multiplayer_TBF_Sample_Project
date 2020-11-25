using System.Collections.Generic;
using Beamable.Common.Content;

namespace Beamable.Content
{
   [ContentType("game_types")]
   [System.Serializable]
   public class SimGameType : ContentObject
   {
      public int numberOfPlayers;
      public List<LeaderboardUpdate> leaderboardUpdates;
      public List<RewardsPerRank> rewards;

      // TODO: Need to specify rewards here.
   }

   [System.Serializable]
   public class RewardsPerRank
   {
      public int startRank;
      public int endRank;
      public List<Reward> rewards;
   }

   [System.Serializable]
   public class Reward
   {
      public RewardType type;
      // TODO: This should be a CurrencyRef but the serialization isn't supported on the backend.
      public string name;
      public long amount;
   }

   [System.Serializable]
   public enum RewardType
   {
      Currency
   }

   [System.Serializable]
   public class LeaderboardUpdate
   {
      // TODO: This should be a LeaderboardRef but the serialization isn't supported on the backend.
      public string leaderboard;
      public ScoringAlgorithm scoringAlgorithm;
   }

   [System.Serializable]
   public class ScoringAlgorithm
   {
      public AlgorithmType algorithm;
      public List<ScoringAlgoOption> options;
   }

   [System.Serializable]
   public class ScoringAlgoOption
   {
      public string key;
      public string value;
   }

   [System.Serializable]
   public enum AlgorithmType
   {
      MultiplayerElo
   }
}