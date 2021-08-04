﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Common.Content;
using Beamable.Experimental.Api.Matchmaking;
using UnityEngine;
using UnityEngine.Events;

namespace Beamable.Samples.TBF.Multiplayer
{
   public class MyMatchmakingEvent : UnityEvent<MyMatchmakingResult>{}

   /// <summary>
   /// Contains the in-progress matchmaking data.
   /// When the process is complete, this contains
   /// the players list and the MatchId
   /// </summary>
   [Serializable]
   public class MyMatchmakingResult
   {
      //  Properties  -------------------------------------
      public List<long> Players
      {
         get
         {
            if (_matchmakingHandle == null)
            {
               return new List<long>();
            }
            return _matchmakingHandle.Status.Players.Select(i => long.Parse(i)).ToList();
         }
      }
      
      public int PlayerCountMin
      {
         get
         {
            int playerCountMin = 0;
            foreach (TeamContent teamContent in _simGameType.teams)
            {
               if (teamContent.minPlayers.HasValue)
               {
                  playerCountMin += teamContent.minPlayers.Value;
               }
            }
            return playerCountMin;
         }
      }
      
      public int PlayerCountMax
      {
         get
         {
            int playerCountMax = 0;
            foreach (TeamContent teamContent in _simGameType.teams)
            {
               playerCountMax += teamContent.maxPlayers;
            }
            return playerCountMax;
         }
      }
      
      public string MatchId
      {
         get
         {
            return _matchmakingHandle?.Match?.matchId;
         }
      }
      
      public long LocalPlayer { get { return _localPlayer; } }
      public SimGameType SimGameType { get { return _simGameType; } }
      public MatchmakingHandle MatchmakingHandle { get { return _matchmakingHandle; } set { _matchmakingHandle = value;} }
      public bool IsInProgress { get { return _isInProgress; } set { _isInProgress = value;} }

      //  Fields  -----------------------------------------
      public string ErrorMessage = "";
      private long _localPlayer;
      private bool _isInProgress = false;
      private MatchmakingHandle _matchmakingHandle = null;
      private SimGameType _simGameType;

      //  Constructor  ------------------------------------
      public MyMatchmakingResult(SimGameType simGameType, long localPlayer)
      {
         _simGameType = simGameType;
         _localPlayer = localPlayer;
      }

      //  Other Methods  ----------------------------------
      public override string ToString()
      {
         return $"[MyMatchmakingResult (" +
            $"MatchId = {MatchId}, " +
            $"Teams = {_matchmakingHandle.Match.teams}, " +
            $"players.Count = {Players.Count})]";
      }
   }

   /// <summary>
   /// This example is for reference only. Use as
   /// inspiration for usage in production.
   /// </summary>
   public class MyMatchmaking
   {
      //  Events  -----------------------------------------
      public MyMatchmakingEvent OnProgress = new MyMatchmakingEvent();
      public MyMatchmakingEvent OnComplete = new MyMatchmakingEvent();
      public MyMatchmakingEvent OnError = new MyMatchmakingEvent();
      
      
      //  Properties  -------------------------------------
      public MyMatchmakingResult MyMatchmakingResult { get { return _myMatchmakingResult; } }

      
      //  Fields  -----------------------------------------
      private MyMatchmakingResult _myMatchmakingResult = null;
      private Experimental.Api.Matchmaking.MatchmakingService _matchmakingService = null;
      

      //  Constructor  ------------------------------------
      public MyMatchmaking(Experimental.Api.Matchmaking.MatchmakingService matchmakingService,
         SimGameType simGameType, long localPlayerDbid)
      {
         Debug.Log("a");
         _matchmakingService = matchmakingService;
         _myMatchmakingResult = new MyMatchmakingResult(simGameType, localPlayerDbid);
      }

      
      //  Other Methods  ----------------------------------
      public async Task StartMatchmaking()
      {
         if (_myMatchmakingResult.IsInProgress)
         {
            Debug.LogError($"MyMatchmaking.StartMatchmaking() failed. " +
                           $"IsInProgress must not be {_myMatchmakingResult.IsInProgress}.\n\n");
            return;
         }
         
         Debug.Log("b");
         
         _myMatchmakingResult.IsInProgress = true;
         
         _myMatchmakingResult.MatchmakingHandle =  await _matchmakingService.StartMatchmaking(
            _myMatchmakingResult.SimGameType.Id,
            maxWait: TimeSpan.FromSeconds(10),
            updateHandler: handle =>
            {
               Debug.Log("1");
               OnProgress.Invoke(_myMatchmakingResult);
            },
            readyHandler: handle =>
            {
               Debug.Log("2");
               Debug.Assert(handle.State == MatchmakingState.Ready);
               _myMatchmakingResult.IsInProgress = false;
               OnComplete.Invoke(_myMatchmakingResult);
            },
            timeoutHandler: handle =>
            {
               Debug.Log("3");
               _myMatchmakingResult.IsInProgress = false;
               _myMatchmakingResult.ErrorMessage = "Timeout";
               OnError?.Invoke(_myMatchmakingResult);
            });
      }
      
      
      public async void Stop()
      {
         await _matchmakingService.CancelMatchmaking(_myMatchmakingResult.MatchmakingHandle.Tickets[0].ticketId);
      }
   }
}
