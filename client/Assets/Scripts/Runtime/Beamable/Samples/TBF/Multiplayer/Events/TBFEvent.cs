using System;

namespace Beamable.Samples.TBF.Multiplayer.Events
{
   /// <summary>
   /// The base event for all multiplayer events in 
   /// this sample game project.
   /// </summary>
   [Serializable]
   public class TBFEvent
   {
      /// <summary>
      /// True, Determines if local must recieve from all players
      /// before handling the event.
      /// </summary>
      public virtual bool IsConsensusRequired { get { return false;} }

   }
}