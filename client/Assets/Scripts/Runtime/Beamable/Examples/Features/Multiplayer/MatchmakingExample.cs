using Beamable.Content;
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
         SimGameType simGameType = await _simGameTypeRef.Resolve();

         await Beamable.API.Instance.Then(async de =>
         {
            MyMatchmaking myMatchmaking = new MyMatchmaking(de.Matchmaking, simGameType, de.User.id);
            myMatchmaking.OnProgress += MyMatchmaking_OnProgress;
            myMatchmaking.OnComplete += MyMatchmaking_OnComplete;
            await myMatchmaking.Start();
         });
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