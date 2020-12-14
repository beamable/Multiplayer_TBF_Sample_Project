﻿using Beamable.Api;
using Beamable.Api.Matchmaking;
using Beamable.Common;
using Beamable.Content;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// NOTE ON USAGE: Both the EXAMPLE and the SAMPLE use the classes below. If the EXAMPLE
/// and SAMPLE are moved to separate  unity projects in the future, then ...
/// 
/// 1. leave the file below as is for sole use in the EXAMPLE, and 
/// 2. copy/move/rename/renamespace the file below for sole use in the SAMPLE.
/// 
/// </summary>
namespace Beamable.Examples.Features.Multiplayer
{
   //ASKJUSTIN: I had to create this. Suggestion: Add to the SDK?
   [Serializable]
   public class SimGameTypeRef : ContentRef<SimGameType> { }

   /// <summary>
   /// Contains the in-progress matchmaking data. When the process is complete, 
   /// this contains the players list and the RoomId
   /// </summary>
   [Serializable]
   public class MyMatchmakingResult
   {
      public bool IsComplete { get { return !string.IsNullOrEmpty(RoomId); } }
      public long LocalPlayerDbid { get { return _localPlayerDbid; } }
      public int TargetPlayerCount { get { return _targetPlayerCount; } }
      //
      public string RoomId;
      public int TicksRemaining;
      public List<long> Players = new List<long>();
      public bool IsInProgress = false;
      public bool IsError = false;
      public string ErrorMessage = "";
      //
      private long _localPlayerDbid;
      private int _targetPlayerCount;

      public MyMatchmakingResult(long localPlayerDbid, int targetPlayerCount)
      {
         _localPlayerDbid = localPlayerDbid;
         _targetPlayerCount = targetPlayerCount;
      }

      public override string ToString()
      {
         return $"[MyMatchmakingResult (" +
            $"RoomId={RoomId}, " +
            $"TargetPlayerCount={TargetPlayerCount}, " +
            $"players.Count={Players.Count})]";
      }
   }

   /// <summary>
   /// For the EXAMPLE scene(s). This is a custom implementation of the Matchmaking. This serves as a working template
   /// for real-world use. Feel free to copy this source as inspiration for production games.
   /// </summary>
   public class MyMatchmaking
   {
      public const string DefaultRoomId = "DefaultRoom";

      public const int Delay = 1000;

      public event Action<MyMatchmakingResult> OnProgress;
      public event Action<MyMatchmakingResult> OnComplete;
      public MyMatchmakingResult MyMatchmakingResult { get { return _myMatchmakingResult; } }

      private MyMatchmakingResult _myMatchmakingResult;
      private MatchmakingService _matchmakingService;
      private SimGameType _simGameType;

      public MyMatchmaking(MatchmakingService matchmakingService,  
         SimGameType simGameType, long LocalPlayerDbid)
      {
         _matchmakingService = matchmakingService;
         _simGameType = simGameType;

         _myMatchmakingResult = new MyMatchmakingResult(LocalPlayerDbid, _simGameType.numberOfPlayers);
      }

      public async Task<MyMatchmakingResult> Start()
      {
         _myMatchmakingResult.IsInProgress = true;
         _myMatchmakingResult.RoomId = "";
         _myMatchmakingResult.TicksRemaining = 0;
         //
         MatchmakingResponse matchmakingResponse = null;

         while (_myMatchmakingResult.IsInProgress)
         {
            try
            {
               Debug.Log($"MyMatchmaking.Start() TargetPlayerCount={_simGameType.numberOfPlayers}");

               //ASKJUSTIN: parameter API says "SimGameType". I would assume that is the "ContentName", but instead its "Id". Thoughts?
               matchmakingResponse = await _matchmakingService.Match(_simGameType.Id);
            }
            catch (PlatformRequesterException e)
            {
               // Invoke Error
               _myMatchmakingResult.IsInProgress = false;
               _myMatchmakingResult.IsError = true;
               _myMatchmakingResult.ErrorMessage = e.Message;
               OnComplete?.Invoke(_myMatchmakingResult);
               return _myMatchmakingResult;
            }

            // Invoke Progress #1
            _myMatchmakingResult.Players = matchmakingResponse?.players;
            _myMatchmakingResult.TicksRemaining = matchmakingResponse.ticksRemaining;
            OnProgress?.Invoke(_myMatchmakingResult);

            // Wait
            if (matchmakingResponse.ticksRemaining - _myMatchmakingResult.TicksRemaining > 1)
            {
               await Task.Delay(matchmakingResponse.players.Count * Delay);
            }
            await Task.Delay(Delay);
            _myMatchmakingResult.TicksRemaining = matchmakingResponse.ticksRemaining;

            //ASKJUSTIN: I check A and B here both. Sound good? Edge cases? Error cases?

            //A
            if (_myMatchmakingResult.Players.Count == _myMatchmakingResult.TargetPlayerCount &&

               //B
               !string.IsNullOrEmpty(matchmakingResponse.game))
            {
               _myMatchmakingResult.RoomId = matchmakingResponse.game;
               _myMatchmakingResult.IsInProgress = false;
            }
         }

         // Invoke Progress #2
         OnProgress?.Invoke(_myMatchmakingResult); 

         // Invoke Complete
         OnComplete?.Invoke(_myMatchmakingResult);
         return _myMatchmakingResult;
      }

      public void Stop()
      {
         //Next tick this will properly dispatch
         //an OnComplete with Error
         _myMatchmakingResult.IsInProgress = false;
      }
   }
}