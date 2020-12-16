using Beamable.Samples.TBF.Exceptions;
using System;
using UnityEngine;

namespace Beamable.Samples.TBF.Multiplayer
{
   /// <summary>
   /// 
   /// </summary>
   [Serializable]
   public class RemotePlayerAI
   {
      //  Properties -----------------------------------
      public bool IsEnabled { get { return _isEnabled; } }

      //  Fields ---------------------------------------
      private bool _isEnabled;

      /// <summary>
      /// A custom Random is used for deterministic results for all
      /// players. This is a best practice for Beamable Multplayer.
      /// </summary>
      private System.Random _random;

      //  Other Methods --------------------------------
      public RemotePlayerAI (System.Random random, bool isEnabled)
      {
         _random = random;
         _isEnabled = isEnabled;
      }

      public GameMoveType GetNextGameMoveType()
      {
         GameMoveType gameMoveType = GameMoveType.Null;

         //Values of 1/2/3
         int index = _random.Next(1, 3);

         switch (index)
         {
            case 1:
               gameMoveType = GameMoveType.High;
               break;
            case 2:
               gameMoveType = GameMoveType.Medium;
               break;
            case 3:
               gameMoveType = GameMoveType.Low;
               break;
            default:
               SwitchDefaultException.Throw(index);
               break;
         }

         DebugLog($"GetNextGameMoveType() {gameMoveType}");
         return gameMoveType;
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
