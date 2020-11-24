using DisruptorBeam;
using UnityEngine;

namespace Beamable.Examples.Features.Multiplayer
{
   public class MatchmakingExample : MonoBehaviour
   {
      private const int TargetPlayerCount = 2;

      protected void Start()
      {
         SetupBeamable();
      }

      protected async void SetupBeamable()
      {
         await DisruptorEngine.Instance.Then(async de =>
         {
            MyMatchmaking myMatchmaking = new MyMatchmaking(de.Matchmaking, TargetPlayerCount);
            myMatchmaking.OnProgress += MyMatchmaking_OnProgress;
            myMatchmaking.OnComplete += MyMatchmaking_OnComplete;
            await myMatchmaking.Start();
         });
      }

      private void MyMatchmaking_OnProgress(MyMatchmakingResult myMatchmakingResult)
      {
         Debug.Log($"MyMatchmaking_OnProgress() " +
            $"Players={myMatchmakingResult.players.Count}/{TargetPlayerCount} " +
            $"RoomId={myMatchmakingResult.RoomId}");
      }

      private void MyMatchmaking_OnComplete(MyMatchmakingResult myMatchmakingResult)
      {
         Debug.Log("is : " + myMatchmakingResult);

         if (!myMatchmakingResult.IsError)
         {
            Debug.Log($"MyMatchmaking_OnComplete() " +
               $"Players={myMatchmakingResult.players.Count}/{TargetPlayerCount} " +
               $"RoomId={myMatchmakingResult.RoomId}");
         }
         else
         {
            Debug.Log($"MyMatchmaking_OnComplete() " +
               $"Error={myMatchmakingResult.Error}.");
         }

      }
   }
}