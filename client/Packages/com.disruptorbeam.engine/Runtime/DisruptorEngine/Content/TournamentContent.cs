using System;
using System.Collections.Generic;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Content.Validation;
using Beamable.Modules.Tournaments;
using UnityEngine;

namespace Beamable.Content
{
   [System.Serializable]
   [Agnostic]
   public class TournamentRef : ContentRef<TournamentContent>
   {
   }

   [System.Serializable]
   [Agnostic]
   public class TournamentLink : ContentLink<TournamentContent>
   {
   }

   [System.Serializable]
   [Agnostic]
   public class TournamentRankReward
   {
      public string name;
      [Tooltip("The index of the tier you want, in the tiers array")]
      public int tier; // should line up with tiers

      [MustBePositive(true)]
      public OptionalInt stageMin;
      [MustBePositive(true)]
      public OptionalInt stageMax;
      [MustBePositive]
      public OptionalInt minRank;
      [MustBePositive]
      public OptionalInt maxRank;
      public List<CurrencyAmount> currencyRewards;
   }


   [System.Serializable]
   [Agnostic]
   public class TournamentStageChange
   {
      [MustBePositive]
      public int minRank;
      [MustBePositive]
      public int maxRank;

      [Range(-1, 2)] public int delta;

      public Color color;

      public bool AcceptsRank(long rank)
      {
         return rank >= minRank && rank <= maxRank;
      }
   }

   [System.Serializable]
   [Agnostic]
   public class TournamentTier
   {
      public string name;
      public Color color;
   }

   [ContentType("tournaments")]
   [Agnostic(new[]{typeof(TournamentColorConstants)})]
   public class TournamentContent : ContentObject
   {
      public new string name = "sample";


      /*
       * every day relative from August 11th, 2021 2:05PM
       * every 3 hours relative from August 11th, 1999 2:05PM
       * every day at 2:05PM
       * every 3 days relative to August 11th, ????
       * every week relative to August 19th 2020, 1:00 PM (UTC)
       */

      //       [Tooltip("Cron-like string. https://crontab.guru/")]
      //       public string schedule = "30 14 * * *";
      //
      [Tooltip("ISO UTC Anchor time. From what time are the cycles relative to?")]
      [MustBeDateString]
      public string anchorTimeUTC = "2020-01-01T12:00:00Z";

      [Tooltip("ISO duration string. How long does each tournament cycle last?")]
      public readonly string cycleDuration = "P1D"; // XXX: hardcoded to 1 day.

      [Tooltip("The number of players allowed to be in each stage")]
      [MustBePositive]
      public int playerLimit;

      [Tooltip("The names of the stages, from worst to best")]
      public List<TournamentTier> tiers = new List<TournamentTier>
      {
         new TournamentTier {color = TournamentColorConstants.BRONZE, name = "Bronze"},
         new TournamentTier {color = TournamentColorConstants.SILVER, name = "Silver"},
         new TournamentTier {color = TournamentColorConstants.GOLD, name = "Gold"}
      };

      [MustBePositive]
      public int stagesPerTier;


      public Color DefaultEntryColor = TournamentColorConstants.RED; // TODO: Add Purple color
      public Color ChampionColor = TournamentColorConstants.GOLD;
      [Tooltip("")]
      public List<TournamentStageChange> stageChanges = new List<TournamentStageChange>
      {
         //1-3 goes up 2stages, 4-10 goes up 1 stage, and 11-40 stay same. 41-50 goes down 1 stage.
         new TournamentStageChange {minRank = 1, maxRank = 1, delta = 2, color = TournamentColorConstants.GOLD},
         new TournamentStageChange {minRank = 2, maxRank = 2, delta = 2, color = TournamentColorConstants.SILVER},
         new TournamentStageChange {minRank = 3, maxRank = 3, delta = 2, color = TournamentColorConstants.BRONZE},
         new TournamentStageChange {minRank = 4, maxRank = 10, delta = 1},
         new TournamentStageChange {minRank = 11, maxRank = 20, delta = 0, color = TournamentColorConstants.GREY},
         new TournamentStageChange {minRank = 41, maxRank = 50, delta = -1, color = TournamentColorConstants.RED}
      };

      public List<TournamentRankReward> rankRewards = new List<TournamentRankReward>
      {
         new TournamentRankReward
         {
            name = "Winner",
            tier = 0,
            minRank = new OptionalInt { HasValue = true, Value = 1},
            maxRank = new OptionalInt { HasValue = true, Value = 1},
            stageMin = new OptionalInt { HasValue = true, Value = 1},
            stageMax = new OptionalInt { HasValue = true, Value = 1},
            currencyRewards = new List<CurrencyAmount>()
         }
      };

      public TournamentTier GetTier(int tierIndex)
      {
         if (tierIndex < 0 || tierIndex >= tiers.Count)
            return new TournamentTier
            {
               color = new Color(.2f, .2f, .2f),
               name = "Void"
            };

         return tiers[tierIndex];
      }

      public DateTime GetUTCOfCycle(int cycle)
      {
         // TODO: HACKED TO ONLY WORK FOR DAYS ATM.
         var date = DateTime.Parse(anchorTimeUTC);
         return date.AddDays(cycle);
      }

      public int GetCurrentCycleNumber()
      {
         var anchor = DateTime.Parse(anchorTimeUTC);
         var now = DateTime.UtcNow;
         var daysDiff = now.Subtract(anchor);
         return (int) daysDiff.TotalDays;
      }

      public DateTime GetUTCOfCyclesPrior(int cyclesPrior)
      {
         // TODO: Hacked To only work for days atm.
         var currentCycle = GetCurrentCycleNumber();
         return GetUTCOfCycle(currentCycle - cyclesPrior);
      }
   }
}