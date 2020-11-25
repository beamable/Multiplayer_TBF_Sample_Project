using System;
using Beamable.Api;
using Beamable.Content;
using Beamable.Platform.SDK;
using Beamable.Platform.SDK.Inventory;
using UnityEngine;
using UnityEngine.Events;

namespace Beamable.Modules.Inventory.Scripts
{



   [System.Serializable]
   public class InventoryUpdateEvent : UnityEvent<InventoryUpdateArg>
   {

   }

   public class InventoryBehaviour : MonoBehaviour
   {
      public InventoryGroup Group;
      public InventoryUpdateEvent OnInventoryReceived;

      private PlatformSubscription<InventoryView> _subscription;
      private ItemContent _content;

      private void Start()
      {
         API.Instance.Then(de =>
         {
            if (this == null) return; // unity lifecycle check.

            Group.ItemRef.Resolve().Then(content =>
            {
               _content = content;
               _subscription = de.InventoryService.Subscribe(content.Id, HandleInventory);
            });
         });
      }

      private void OnDestroy()
      {
         _subscription?.Unsubscribe();
      }

      void HandleInventory(InventoryView inventory)
      {
         var arg = new InventoryUpdateArg
         {
            Group = Group,
            Inventory = inventory.items[_content.Id]
         };
         OnInventoryReceived?.Invoke(arg);
      }
   }
}