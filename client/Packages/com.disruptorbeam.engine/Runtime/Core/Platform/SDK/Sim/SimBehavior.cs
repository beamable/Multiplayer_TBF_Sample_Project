using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Beamable.Platform.SDK.Sim {
   public class SimBehavior : MonoBehaviour {
      public const int FRAME_RATE = 20;
      protected string SimId { get; private set; }
      protected SimClient SimClient { get; private set; }
      public List<SimClient.EventCallback<string>> EventCallbacks { get; private set; }

      public void SimInit (SimClient client, string simId) {
         this.SimId = simId;
         this.SimClient = client;
         this.EventCallbacks = new List<SimClient.EventCallback<string>>();
      }

      public void Destroy () {
         SimClient.RemoveSimBehavior(this);
      }

      // Called when the object enters the simulation for the first time
      public virtual void SimEnter () {}

      // Called when the object exits the simulation
      public virtual void SimExit () {
         GameObject.Destroy(this.gameObject);
      }

      // Subscribe for callback events from the simulation
      public void On<T> (SimClient.EventCallback<T> callback) {
         EventCallbacks.Add(SimClient.On(typeof(T).ToString(), SimId, callback));
      }

      public void SendEvent (object evt) {
         SimClient.SendEvent(evt);
      }

      public void OnTick (SimClient.EventCallback<long> callback) {
         EventCallbacks.Add(SimClient.OnTick(callback));
      }

      public virtual int StateHash () { return 0; }

      public int RandomInt () {
         return SimClient.RandomInt();
      }

      public T Spawn<T> (SimBehavior original, string id = "") {
         return SimClient.Spawn<T>(original, id);
      }
   }
}