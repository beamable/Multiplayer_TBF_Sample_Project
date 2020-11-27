﻿using System;

namespace Beamable.Samples.TBF.Multiplayer.Events
{
   /// <summary>
   /// The base event for all multiplayer events in 
   /// this sample game project.
   /// </summary>
   [Serializable]
   public class TBFEvent
   {
      //  Properties -----------------------------------
      public long PlayerDbid { get { return _playerDbid; } }

      //  Fields ---------------------------------------
      private long _playerDbid;
   }
}