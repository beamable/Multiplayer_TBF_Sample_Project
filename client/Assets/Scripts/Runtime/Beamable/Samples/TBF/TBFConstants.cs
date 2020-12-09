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

      public const string StatusText_GameState_Loading = "Beamable Loading...";
      public const string StatusText_GameState_Loaded = "Beamable Loaded...";
      //
      public const string StatusText_GameState_Initializing = "Beamable Initializing...";
      public const string StatusText_GameState_Initialized = "Beamable Initialized...";
      //
      public const string StatusText_GameState_Connecting = "Connecting. Players {0}/{1}...";
      public const string StatusText_Multiplayer_OnDisconnect = "Disconnecting. Players {0}/{1}...";
      public const string StatusText_GameState_Moving = "Round '{0}'. Waiting for moves...";
      public const string StatusText_GameState_Moved = "Round '{0}'. Moves complete...";
      public const string StatusText_GameState_Evaluated = "Round '{0}' over. Round winner '{1}'...";
      public const string StatusText_GameState_Ending = "Round '{0}' over. Game over. Game winner '{1}'...";
      //
      public const string StatusText_Before_Move = "Prepare to move in {0} seconds...";
      public const string StatusText_During_Move = "Choose your move within {0} seconds...";

      //
      public static string Avatar_Idle = "Idle"; //start here
      public static string Avatar_Attack_01 = "Attack_01";
      public static string Avatar_Attack_02 = "Attack_02";
      public static string Avatar_Attack_03 = "Attack_03";
      public static string Avatar_Death = "Death"; //end here

      //
      public static int TaskDelayMin = 100;
      public static int MillisecondMultiplier = 1000;

   }
}