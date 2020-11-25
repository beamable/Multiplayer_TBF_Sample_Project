using Beamable.Common;
using Beamable.Platform.SDK;
using Beamable.Platform.SDK.Matchmaking;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beamable.Examples.Features.Multiplayer
{
   [Serializable]
   public class MyMatchmakingResult
   {
      public bool IsComplete { get { return !string.IsNullOrEmpty(RoomId); } }
      public string RoomId;
      public int TicksRemaining;
      public int TargetPlayerCount;
      public List<long> players = new List<long>();
      public bool IsInProgress = false;
      public bool IsError = false;
      public string Error = "";
   }

   public class MyMatchmaking
   {
      public const string DefaultRoomId = "DefaultRoom";

      public const int Delay = 1000;

      public event Action<MyMatchmakingResult> OnProgress;
      public event Action<MyMatchmakingResult> OnComplete;
      public MyMatchmakingResult MyMatchmakingResult { get { return _myMatchmakingResult; } }

      private MyMatchmakingResult _myMatchmakingResult;
      private MatchmakingService _matchmakingService;

      public MyMatchmaking(MatchmakingService matchmakingService, int targetPlayerCount)
      {
         _matchmakingService = matchmakingService;

         _myMatchmakingResult = new MyMatchmakingResult();
         _myMatchmakingResult.TargetPlayerCount = targetPlayerCount;
      }

      public async Task<MyMatchmakingResult> Start()
      {
         _myMatchmakingResult.IsInProgress = true;
         _myMatchmakingResult.RoomId = "";
         _myMatchmakingResult.TicksRemaining = 0;
         //
         MatchmakingResponse matchmakingResponse = null;

         Debug.Log("0");

         while (_myMatchmakingResult.IsInProgress == true &&
            !_myMatchmakingResult.IsComplete)
         {
            try
            {
               Debug.Log("aaa");
               matchmakingResponse = await _matchmakingService.Match();
               Debug.Log("bbbb");
            }
            catch (PlatformRequesterException e)
            {
               Debug.Log("e");
               // Invoke Error
               _myMatchmakingResult.IsInProgress = false;
               _myMatchmakingResult.IsError = true;
               _myMatchmakingResult.Error = e.Message;
               OnComplete?.Invoke(_myMatchmakingResult);
               return _myMatchmakingResult;
            }

            // Invoke Progress #1
            if (matchmakingResponse != null)
            {

            }
            _myMatchmakingResult.players = matchmakingResponse?.players;
            _myMatchmakingResult.TicksRemaining = matchmakingResponse.ticksRemaining;
            OnProgress?.Invoke(_myMatchmakingResult);

            Debug.Log("1");
            // Wait
            if (matchmakingResponse.ticksRemaining - _myMatchmakingResult.TicksRemaining > 1)
            {
               await Task.Delay(matchmakingResponse.players.Count * Delay);
            }
            Debug.Log("2");
            await Task.Delay(Delay);
            Debug.Log("3");
            _myMatchmakingResult.TicksRemaining = matchmakingResponse.ticksRemaining;

            // When target is reached, mark as complete
            if (matchmakingResponse.players.Count >= _myMatchmakingResult.TargetPlayerCount)
            {
               _myMatchmakingResult.RoomId = matchmakingResponse.game;
               _myMatchmakingResult.IsInProgress = false;
            }
         }
         Debug.Log("4");
         // Invoke Progress #2
         OnProgress?.Invoke(_myMatchmakingResult); 

         // Invoke Complete
         _myMatchmakingResult.IsInProgress = false;
         OnComplete?.Invoke(_myMatchmakingResult);
         return _myMatchmakingResult;
      }

      public void Stop()
      {
         _myMatchmakingResult.IsInProgress = false;
         OnComplete?.Invoke(_myMatchmakingResult);
      }
   }
}