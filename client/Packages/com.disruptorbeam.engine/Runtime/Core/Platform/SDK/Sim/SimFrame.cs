using System.Collections.Generic;
using System;
using UnityEngine;

namespace Beamable.Platform.SDK.Sim {
   public class SimFrame {
      [Serializable]
      public class FramePacket {
         public long Frame {get; set;}
         public List<SimEvent> Events {get; private set;}

         public FramePacket (long frame) {
               this.Frame = frame;
               this.Events = new List<SimEvent>();
         }

         public void AddEvent (SimEvent evt) {
               evt.Frame = Frame;
               Events.Add(evt);
         }
      }

      public long Frame { get; set; }
      public List<SimEvent> Events { get; private set; }

      public SimFrame (long frame, List<SimEvent> events) {
         this.Frame = frame;
         this.Events = events;
      }

      public void Sort () {
         Events.Sort((x,y) => {
               var left = x.Frame + x.Origin + x.Type + x.Body;
               var right = y.Frame + y.Origin + y.Type + y.Body;
               return left.CompareTo(right);
         });
      }

      public bool Apply (FramePacket packet) {
         Events.AddRange(packet.Events);

         if (packet.Frame > Frame) {
               Frame = packet.Frame;
         }

         // Always re-stamp all events
         foreach (SimEvent evt in Events) {
               evt.Frame = Frame;
         }

         return true;
      }
   }
}