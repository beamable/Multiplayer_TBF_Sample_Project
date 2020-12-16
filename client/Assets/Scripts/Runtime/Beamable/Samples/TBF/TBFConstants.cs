
namespace Beamable.Samples.TBF
{
   /// <summary>
   /// Store commonly used static values
   /// </summary>
   public static class TBFConstants
   {
      //  Fields ---------------------------------------

      /// <summary>
      /// Determines if using Unity debug log statements.
      /// </summary>
      public static bool IsDebugLogging = true;

      /// <summary>
      /// Used as a 'null' value.
      /// </summary>
      public const int UnsetValue = -1;

      /// <summary>
      /// The index for the LOCAL player in <see cref="System.Collections.Generic.List{T}"/>s.
      /// </summary>
      public const int PlayerIndexLocal = 0;

      /// <summary>
      /// The index for the REMOTE player in <see cref="System.Collections.Generic.List{T}"/>s.
      /// </summary>
      public const int PlayerIndexRemote = 1;

      public const string StatusText_GameState_Loading = "Beamable Loading ...";
      public const string StatusText_GameState_Loaded = "Beamable Loaded ...";
      //
      public const string StatusText_GameState_Initializing = "Beamable Initializing ...";
      public const string StatusText_GameState_Initialized = "Beamable Initialized ...";
      //
      public const string StatusText_GameState_Connecting = "Connecting. Players <b>{0}</b>/<b>{1}</b> ...";
      public const string StatusText_Multiplayer_OnDisconnect = "Disconnecting. Players <b>{0}</b>/<b>{1}</b> ...";
      public const string StatusText_GameState_PlayerMoving = "Round <b>{0}</b>. Waiting for moves...";
      public const string StatusText_GameState_PlayerMoved = "Round <b>{0}</b>. Player <b>{1}</b> moves <b>{2}</b> ...";
      public const string StatusText_GameState_PlayersAllMoved = "Round <b>{0}</b>. All moves complete ...";
      public const string StatusText_GameState_Evaluated = "Round <b>{0}</b> over. Round winner <b>{1}</b> ...";
      public const string StatusText_GameState_Ending = "Round <b>{0}</b> over. Game over. Game winner <b>{1}</b> ...";
      //
      public static string Avatar_Idle = "Idle"; //start here
      public static string Avatar_Attack_01 = "Attack_01";
      public static string Avatar_Attack_02 = "Attack_02";
      public static string Avatar_Attack_03 = "Attack_03";
      public static string Avatar_Death = "Death"; //end here

      //Lobby
      public static string StatusText_Joining = "Player {0}/{1} joined. Waiting...";
      public static string StatusText_Joined = "Player {0}/{1} joined. Ready.";
   }
}