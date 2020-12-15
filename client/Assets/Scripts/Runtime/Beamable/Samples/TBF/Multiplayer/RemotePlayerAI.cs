using System;

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

      //  Other Methods --------------------------------
      public RemotePlayerAI (bool isEnabled)
      {
         _isEnabled = isEnabled;
      }
   }
}
