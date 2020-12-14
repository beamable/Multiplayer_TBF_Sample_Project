using System;
using System.Collections;
using System.Collections.Generic;
using Beamable.Api;
using Beamable.Common;
using Beamable.UI.Scripts;
using Beamable.Platform.SDK;
using Beamable.Api.Inventory;
using Beamable.Common.Api.Inventory;
using UnityEngine;

namespace Beamable.Modules.Inventory.Scripts
{
   [HelpURL(ContentConstants.URL_FEATURE_INVENTORY_FLOW)]
   public class InventoryMenuBehaviour : MonoBehaviour
    {
        public MenuManagementBehaviour MenuManager;

        private Promise<PlatformSubscription<InventoryView>> _inventorySubscription;

        private Promise<Unit> _inventoryViewPromise = new Promise<Unit>();
        private InventoryView _inventoryView;

        public void HandleToggle(bool shouldShow)
        {
            if (!shouldShow && MenuManager.IsOpen)
            {
                MenuManager.CloseAll();
            }
            else if (shouldShow && !MenuManager.IsOpen)
            {
                MenuManager.Show<InventoryMainMenu>();
            }
        }

        void Start()
        {
            // auto load the inventory...
            //_inventorySubscription = DisruptorEngine.Instance.Map(de => de.InventoryService.Subscribe(HandleInventoryEvent));
        }

        private void OnDestroy()
        {
            //_inventorySubscription.Then(s => s.Unsubscribe());
        }

        void HandleInventoryEvent(InventoryView inventory)
        {
            _inventoryView = inventory;
            _inventoryViewPromise.CompleteSuccess(PromiseBase.Unit);
        }
    }
}