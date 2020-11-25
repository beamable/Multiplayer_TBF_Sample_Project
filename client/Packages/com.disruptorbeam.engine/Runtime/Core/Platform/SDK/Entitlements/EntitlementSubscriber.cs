using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Beamable.Platform.SDK.Entitlements
{
   public class EntitlementSubscriber
   {
      private PlatformService _platform;
      public EntitlementSubscriber (PlatformService platform)
      {
         _platform = platform;
      }

      private Dictionary<string, Dictionary<string, List<Action<List<Entitlement>>>>> subscriptions = new Dictionary<string, Dictionary<string, List<Action<List<Entitlement>>>>>();
      private Dictionary<string, Dictionary<string, List<Entitlement>>> entitlements = new Dictionary<string, Dictionary<string, List<Entitlement>>>();

      public void SubscribeClaimed (string entitlement, Action<List<Entitlement>> callback)
      {
         Subscribe(entitlement, "claimed", callback);
      }

      public void SubscribeGranted (string entitlement, Action<List<Entitlement>> callback)
      {
         Subscribe(entitlement, "granted", callback);
      }

      private void Subscribe (string entitlement, string state, Action<List<Entitlement>> callback)
      {
         // Only invoke subscriptions after platform is initialized
         bool needRefresh = false;
         if (!subscriptions.ContainsKey(entitlement))
         {
            subscriptions.Add(entitlement, new Dictionary<string, List<Action<List<Entitlement>>>>());
            entitlements.Add(entitlement, new Dictionary<string, List<Entitlement>>());
         }
         var stateSubscriptions = subscriptions[entitlement];
         var stateEntitlements = entitlements[entitlement];
         if (!stateSubscriptions.ContainsKey(state))
         {
            stateSubscriptions.Add(state, new List<Action<List<Entitlement>>>());
            stateEntitlements.Add(state, new List<Entitlement>());
            // First subscription for this entitlement, so a refresh is needed
            needRefresh = true;
         }
         stateSubscriptions[state].Add(callback);

         Callback(entitlement, state, callback);
         if (needRefresh)
         {
            Refresh(entitlement, state);
         }
      }

      public void Refresh ()
      {
         foreach (string entitlement in subscriptions.Keys)
         {
            foreach (string state in subscriptions[entitlement].Keys)
            {
               Refresh(entitlement, state);
            }
         }
      }

      private void Refresh (string entitlement, string state)
      {
         var stateSubscriptions = subscriptions[entitlement][state];
         _platform.Entitlements.Get(entitlement, state).Then((rsp) => {
            entitlements[entitlement][state] = rsp.entitlements;
            Callback(entitlement, state);
         }).Error((err) => {
            Debug.LogError(err.ToString());
         });
      }

      private void Callback (string entitlement, string state)
      {
         var callbacks = subscriptions[entitlement][state];
         for (int i=0; i<callbacks.Count; i++)
         {
            Callback(entitlement, state, callbacks[i]);
         }
      }

      private void Callback (string entitlement, string state, Action<List<Entitlement>> callback)
      {
         callback(entitlements[entitlement][state]);
      }
   }
}