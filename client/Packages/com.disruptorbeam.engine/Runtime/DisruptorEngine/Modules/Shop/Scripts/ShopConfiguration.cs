using System.Collections.Generic;
using Beamable.Content;
using UnityEngine;

namespace Beamable.Modules.Shop
{
   [CreateAssetMenu(
      fileName = "Shop Configuration",
      menuName = ContentConstants.MENU_ITEM_PATH_ASSETS_BEAMABLE_CONFIGURATIONS + "/" +
      "Shop Configuration",
      order = ContentConstants.MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_1)]
   public class ShopConfiguration : ModuleConfigurationObject
   {
      public static ShopConfiguration Instance => Get<ShopConfiguration>();

      public List<StoreRef> Stores = new List<StoreRef>();
      public ListingRenderer ListingRenderer;
      public ObtainRenderer ObtainRenderer;
   }
}