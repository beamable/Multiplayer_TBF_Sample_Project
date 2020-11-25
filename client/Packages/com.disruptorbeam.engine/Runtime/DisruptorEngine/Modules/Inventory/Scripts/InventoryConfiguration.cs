using System.Collections.Generic;
using Beamable.Content;
using UnityEngine;

namespace Beamable.Modules.Inventory.Scripts
{

   [System.Serializable]
   public struct InventoryGroup
   {
      public ItemRef ItemRef;
      public string DisplayName;
   }

   [CreateAssetMenu(
      fileName = "Inventory Configuration",
      menuName = ContentConstants.MENU_ITEM_PATH_ASSETS_BEAMABLE_CONFIGURATIONS + "/" +
      "Inventory Configuration",
      order = ContentConstants.MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_1)]
   public class InventoryConfiguration : ModuleConfigurationObject
   {
      public static InventoryConfiguration Instance => Get<InventoryConfiguration>();

      public List<InventoryGroup> Groups;

      public InventoryObjectUI DefaultObjectPrefab;
   }
}