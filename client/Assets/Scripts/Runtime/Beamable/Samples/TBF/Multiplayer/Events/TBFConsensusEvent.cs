using System;

namespace Beamable.Samples.TBF.Multiplayer.Events
{
   /// <summary>
   /// The base event for all multiplayer events in 
   /// this sample game project.
   /// </summary>
   [Serializable]
   public class TBFConsensusEvent : TBFEvent
   {
      public override bool IsConsensusRequired { get { return true;} }

   }
}