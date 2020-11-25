using System;

namespace Beamable.Samples.TBF
{
   /// <summary>
   /// Store commonly used static values
   /// </summary>
   public static class TBFConstants
   {
      //  Fields ---------------------------------------
      public const int UnsetValue = -1;

      /// <summary>
      /// The index for the LOCAL player in <see cref="System.Collections.Generic.List{T}"/>s.
      /// </summary>
      public const int PlayerIndexLocal = 0;

      /// <summary>
      /// The index for the REMOTE player in <see cref="System.Collections.Generic.List{T}"/>s.
      /// </summary>
      public const int PlayerIndexRemote = 1;

      public const string StatusText_Beamable_Loading = "Beamable Loading...";
      public const string StatusText_Beamable_Loaded = "Beamable Loaded...";
      //
      public const string StatusText_Multiplayer_Initializing = "Beamable Initializing...";
      public const string StatusText_Multiplayer_Initialized = "Beamable Initialized...";
      //
      public const string StatusText_Multiplayer_OnConnect = "Joining Player {0}/{1}...";
      //
      public const string StatusText_Before_Move = "Prepare to move in {0} seconds...";
      public const string StatusText_During_Move = "Choose your move within {0} seconds...";

      public enum GameState
      {
         PreGame,
         Game,
         PostGame
      }
   }
}