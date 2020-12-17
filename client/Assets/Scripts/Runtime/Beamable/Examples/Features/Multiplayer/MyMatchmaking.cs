using Beamable.Api;
using Beamable.Api.Matchmaking;
using Beamable.Common;
using Beamable.Content;
using Beamable.Samples.TBF;
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
   [Serializable]
   public class SimGameTypeRef : ContentRef<SimGameType> { }

   /// <summary>
   /// Contains the in-progress matchmaking data. When the process is complete, 
   /// this contains the players list and the RoomId
   /// </summary>
   [Serializable]
   public class MyMatchmakingResult
   {
      //  Properties  -------------------------------------
      public bool IsComplete { get { return !string.IsNullOrEmpty(RoomId); } }
      public long LocalPlayerDbid { get { return _localPlayerDbid; } }
      public int TargetPlayerCount { get { return _targetPlayerCount; } }

      //  Fields  -----------------------------------------
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


      //  Other Methods  ----------------------------------
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
      //  Events  -----------------------------------------
      public event Action<MyMatchmakingResult> OnProgress;
      public event Action<MyMatchmakingResult> OnComplete;

      //  Properties  -------------------------------------
      public MyMatchmakingResult MyMatchmakingResult { get { return _myMatchmakingResult; } }

      //  Fields  -----------------------------------------
      public const string DefaultRoomId = "DefaultRoom";
      public const int Delay = 1000;

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

      //  Other Methods  ----------------------------------

      /// <summary>
      /// Start the matchmaking process
      /// </summary>
      /// <returns></returns>
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
               DebugLog($"MyMatchmaking.Start() TargetPlayerCount={_simGameType.numberOfPlayers}");

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

            // Did the server send a RoomId with enough players?
            if (_myMatchmakingResult.Players.Count == _myMatchmakingResult.TargetPlayerCount &&
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

      /// <summary>
      /// Stop the matchmaking process
      /// </summary>
      /// <returns></returns>
      public void Stop()
      {
         //Next tick this will properly dispatch
         //an OnComplete with Error
         _myMatchmakingResult.IsInProgress = false;
      }

      private void DebugLog(string message)
      {
         if (TBFConstants.IsDebugLogging)
         {
            Debug.Log(message);
         }
      }
   }
}