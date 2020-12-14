using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common;
using Beamable.Api.Inventory;
using Beamable.Api.Matchmaking;
using Beamable.Common.Api;
using Beamable.Common.Api.Inventory;
using Beamable.Serialization;
using Beamable.Serialization.SmallerJSON;
using UnityEngine;

namespace Beamable.Api.Sim
{
   public class GameRelayService
   {
      private PlatformService _platform;
      private PlatformRequester _requester;

      private const string GAME_RESULTS_EVENT_NAME = "gamerelay.game_results";

      public GameRelayService(PlatformService platform, PlatformRequester requester)
      {
         _platform = platform;
         _requester = requester;
      }

      public Promise<GameRelaySyncMsg> Sync(string roomId, GameRelaySyncMsg request)
      {
         return _requester.Request<GameRelaySyncMsg>(
            Method.POST,
            $"/object/gamerelay/{roomId}/sync",
            request
         );
      }

      /// <summary>
      /// Report the results of the game to the platform.
      /// </summary>
      /// <param name="roomId">The ID of the game session.</param>
      /// <param name="results">The array of `PlayerResult` to send to the platform for verification.</param>
      /// <returns>A promise of the confirmed game results</returns>
      public Promise<GameResults> ReportResults(string roomId, params PlayerResult[] results)
      {
         return _requester.Request<GameResults>(
            Method.POST,
            $"/object/gamerelay/{roomId}/results",
            new ResultsRequest(results)
         );
      }
   }

   [Serializable]
   public class ResultsRequest
   {
      public List<PlayerResult> results;

      public ResultsRequest(params PlayerResult[] results)
      {
         this.results = results.ToList();
      }
   }

   [Serializable]
   public class GameResults
   {
      public bool cheatingDetected;
      public List<DeltaScoresByLeaderBoardId> deltaScores;
      public List<CurrencyChange> currenciesGranted;
      public List<Item> itemsGranted;
   }

   [Serializable]
   public class DeltaScoresByLeaderBoardId
   {
      public string leaderBoardId;
      public double scoreDelta;
   }

   [Serializable]
   public class CurrencyChange
   {
      public string symbol;
      public long amount;
   }

   [Serializable]
   public class PlayerResult
   {
      public long playerId;
      public double score;
      public int rank;
   }

   [Serializable]
   public class GameRelaySyncMsg
   {
      public long t;
      public List<GameRelayEvent> events = new List<GameRelayEvent>();
   }

   [Serializable]
   public class GameRelayEvent
   {
      public long t;
      public string type;
      public long origin;
      public string body;

      public void FromSimEvent(SimEvent evt)
      {
         t = evt.Frame;
         type = evt.Type;
         origin = 0;
         body = evt.Body;
      }

      public SimEvent ToSimEvent()
      {
         string origin = this.origin.ToString();
         string type = this.type;
         if (origin == "-1")
         {
            origin = "$system";
            if (type == "_c")
            {
               type = "$connect";
            }
            else if (type == "_d")
            {
               type = "$disconnect";
            }
            else if (type == "_a")
            {
               type = "$init";
            }
         }

         SimEvent result = new SimEvent(origin, type, body);
         result.Frame = t;

         return result;
      }
   }
}