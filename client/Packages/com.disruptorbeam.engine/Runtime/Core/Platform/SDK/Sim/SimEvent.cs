using System;

namespace Beamable.Api.Sim {
   [Serializable]
   public class SimEvent {
      public long Frame;
      public string Type;
      public string Origin;
      public string Body;

      public SimEvent (string origin, string type, string body) {
         this.Origin = origin;
         this.Type = type;
         this.Body = body;
      }

      public override string ToString () {
         return Frame + ": [" + Origin + " " + Type + " " + Body + "]";
      }
   }
}