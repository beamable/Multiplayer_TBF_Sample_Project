using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Beamable.Modules.Avatars
{
   [System.Serializable]
   public class AccountAvatar
   {
      public string Name;
      public Sprite Sprite;
   }

   [CreateAssetMenu(
      fileName = "Avatar Configuration",
      menuName = ContentConstants.MENU_ITEM_PATH_ASSETS_BEAMABLE_CONFIGURATIONS + "/" +
      "Avatar Configuration",
      order = ContentConstants.MENU_ITEM_PATH_ASSETS_BEAMABLE_ORDER_1)]
   public class AvatarConfiguration : ModuleConfigurationObject
   {
      public static AvatarConfiguration Instance => Get<AvatarConfiguration>();

      public AccountAvatar Default;
      public List<AccountAvatar> Avatars;
   }
}