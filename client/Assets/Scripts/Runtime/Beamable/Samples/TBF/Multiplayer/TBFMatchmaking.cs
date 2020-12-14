
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
      //TODO: Needed?: works for single player only
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