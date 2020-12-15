using Beamable.Api.Matchmaking;
using Beamable.Content;
using Beamable.Examples.Features.Multiplayer;

namespace Beamable.Samples.TBF.Multiplayer
{
   /// <summary>
   /// For the SAMPLE PROJECT scene(s). For this project the needs are so similar to the 
   /// EXAMPLE SCENE, that the EXAMPLE <see cref="MyMatchmaking"/> is extended. 
   /// 
   /// NOTE: For your production uses, simply copy <see cref="MyMatchmaking"/> as inspiration
   /// into a new class.
   /// </summary>
   public class TBFMatchmaking : MyMatchmaking
   {
      /// <summary>
      /// During development, if the game scene is loaded directly (and thus no matchmaking)
      /// this method is used to give a RoomId. Why random? So that each connection is fresh
      /// and has no history. Otherwise a new connection (within 10-15 seconds of the last connection)
      /// may remember the 'old' session and contain 'old' events.
      /// </summary>
      /// <returns></returns>
      public static string GetRandomRoomId()
      {
         return "TBFRoomId" + string.Format("{00:00}", UnityEngine.Random.Range(0, 1000));
      }

      public TBFMatchmaking(MatchmakingService matchmakingService, SimGameType simGameType, long LocalPlayerDbid) : 
         base(matchmakingService, simGameType, LocalPlayerDbid)
      {
      }
   }
}