using Beamable.Content;
using UnityEngine;

namespace Beamable.Examples.Features.Multiplayer
{
   public class MatchmakingExample : MonoBehaviour
   {
      /// <summary>
      /// This defines the matchmaking criteria including "NumberOfPlayers"
      /// </summary>
      [SerializeField]
      private SimGameTypeRef _simGameTypeRef;

      protected void Start()
      {
         SetupBeamable();
      }

      protected async void SetupBeamable()
      {
         SimGameType simGameType = await _simGameTypeRef.Resolve();

         await Beamable.API.Instance.Then(async de =>
         {
            MyMatchmaking myMatchmaking = new MyMatchmaking(de.Matchmaking, simGameType, de.User.id);
            myMatchmaking.OnProgress += MyMatchmaking_OnProgress;
            myMatchmaking.OnComplete += MyMatchmaking_OnComplete;
            await myMatchmaking.Start();
         });
      }

      private void MyMatchmaking_OnProgress(MyMatchmakingResult myMatchmakingResult)
      {
         Debug.Log($"MyMatchmaking_OnProgress() " +
            $"Players={myMatchmakingResult.Players.Count}/{myMatchmakingResult.TargetPlayerCount} " +
            $"RoomId={myMatchmakingResult.RoomId}");
      }

      private void MyMatchmaking_OnComplete(MyMatchmakingResult myMatchmakingResult)
      {
         if (!myMatchmakingResult.IsError)
         {
            Debug.Log($"MyMatchmaking_OnComplete() " +
               $"Players={myMatchmakingResult.Players.Count}/{myMatchmakingResult.TargetPlayerCount} " +
               $"RoomId={myMatchmakingResult.RoomId}");
         }
         else
         {
            Debug.Log($"MyMatchmaking_OnComplete() " +
               $"Error={myMatchmakingResult.ErrorMessage}.");
         }
      }
   }
}