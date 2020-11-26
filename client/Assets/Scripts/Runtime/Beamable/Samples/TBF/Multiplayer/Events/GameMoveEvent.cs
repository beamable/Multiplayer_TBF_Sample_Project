using System;

namespace Beamable.Samples.TBF.Multiplayer.Events
{
   [Serializable]
   public class GameMoveEvent : TBFEvent
   {
      //  Properties -----------------------------------
      public GameMoveType GameMoveType {  get { return _gameMoveType; } }

      //  Fields ---------------------------------------
      private GameMoveType _gameMoveType;

      //  Constructor   --------------------------------
      public GameMoveEvent(GameMoveType gameMoveType)
      {
         _gameMoveType = gameMoveType;
      }

      //  Other Methods   ------------------------------
   }
}