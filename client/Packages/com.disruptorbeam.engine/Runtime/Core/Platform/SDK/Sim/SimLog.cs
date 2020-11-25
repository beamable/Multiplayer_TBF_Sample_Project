using System.Collections.Generic;
using UnityEngine;
using System;

namespace Beamable.Platform.SDK.Sim {
   public class SimLog {
      public long Frame { get; private set; }
      private List<SimEvent> _events = new List<SimEvent>();
      private List<SimEvent> _eventsAtFrame = new List<SimEvent>();
      private List<SimEvent> _history = new List<SimEvent>();

      public void ApplyFrame (SimFrame frame) {
         //frame.Sort();
         this._history.AddRange(frame.Events);
         this._events.AddRange(frame.Events);

         if (frame.Frame <= Frame) {
               // Don't apply already seen frames
               return;
         }

         this.Frame = frame.Frame;
      }

      public List<SimEvent> GetEventsAtFrame (long frame) {
         _eventsAtFrame.Clear();
         for (int i=0; i<_events.Count; i++) {
               SimEvent evt = _events[i];
               if (evt.Frame == frame) {
                  _eventsAtFrame.Add(evt);
               }
         }
         return _eventsAtFrame;
      }

      public void Prune (long frame) {
         _events.RemoveAll((evt) => evt.Frame < frame);
      }

      public override string ToString () {
         return ToString(this.Frame);
      }

      public string ToString (long frame) {
         string result = "";
         result += "===========================\n";
         result += "FRAME = " + frame + "\n";
         foreach (SimEvent evt in _events) {
               if (evt.Frame <= frame) {
                  result += evt.ToString() + "\n";
               }
         }
         result += "===========================";
         return result;
      }

      public Snapshot ToSnapshot () {
         var result = new Snapshot();
         result.frame = Frame;
         result.events = _history.ToArray();
         return result;
      }

      public void FromSnapshot (Snapshot snapshot) {
         this._events = new List<SimEvent>(snapshot.events);
         this._history = new List<SimEvent>(snapshot.events);
         this.Frame = 0;
      }

      [Serializable]
      public class Snapshot {
         public SimEvent[] events;
         public long frame;
      }
   }
}