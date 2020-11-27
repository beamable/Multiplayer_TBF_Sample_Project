
namespace Beamable.Samples.TBF.Multiplayer
{
   public class TBFMatchmaking
   {
      //TEMP: works for single player only
      public static string GetRandomRoomId () 
      {
         return "TBFRoomId" + string.Format("{00:00}", UnityEngine.Random.Range(0, 1000));
      }

      //TODO: do full matchmaking
   }
}