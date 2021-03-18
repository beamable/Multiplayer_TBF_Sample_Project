using UnityEngine;

namespace Beamable.Examples.Features.Multiplayer
{
   /// <summary>
   /// Demonstrates the creation of and joining to a
   /// Multiplayer game room with Beamable Multiplayer.
   /// </summary>
   public class MatchmakingExample : MonoBehaviour
   {
      //  Fields ------------------------------------------

      /// <summary>
      /// This defines the matchmaking criteria including "NumberOfPlayers"
      /// </summary>
      [SerializeField]
      private SimGameTypeRef _simGameTypeRef;

      //  Unity Methods -----------------------------------
      protected void Start()
      {
         SetupBeamable();
      }

      //  Other Methods -----------------------------------

      protected async void SetupBeamable()
      {
         var simGameType = await _simGameTypeRef.Resolve();
         var beamable = await Beamable.API.Instance;

         var myMatchmaking = new MyMatchmaking(beamable.Experimental.MatchmakingService, simGameType, beamable.User.id);
         myMatchmaking.OnProgress += MyMatchmaking_OnProgress;
         myMatchmaking.OnComplete += MyMatchmaking_OnComplete;
         await myMatchmaking.Start();
      }

      //  Event Handlers ----------------------------------

      private void MyMatchmaking_OnProgress(MyMatchmakingResult myMatchmakingResult)
      {
         Debug.Log($"MyMatchmaking_OnProgress() " +
            $"Players={myMatchmakingResult.Players.Count}/{myMatchmakingResult.TargetPlayerCount} " +
            $"RoomId={myMatchmakingResult.RoomId}");
      }

      private void MyMatchmaking_OnComplete(MyMatchmakingResult myMatchmakingResult)
      {
         Debug.Log($"MyMatchmaking_OnComplete() " +
                   $"Error={myMatchmakingResult.ErrorMessage}.");
      }
   }
}
