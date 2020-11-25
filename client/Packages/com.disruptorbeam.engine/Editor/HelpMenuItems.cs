using UnityEditor;
using UnityEngine;

namespace Beamable.Editor
{
   public static class HelpMenuItems
   {
      [MenuItem(
         ContentConstants.MENU_ITEM_PATH_WINDOW_BEAMABLE_HELP + "/" +
         ContentConstants.OPEN + " " +
         ContentConstants.BEAMABLE_MAIN_WEBSITE,
         priority = ContentConstants.MENU_ITEM_PATH_WINDOW_PRIORITY_3)]
      private static void OpenBeamableMainWebsite()
      {
         Application.OpenURL(ContentConstants.URL_BEAMABLE_MAIN_WEBSITE);
      }

      [MenuItem(
         ContentConstants.MENU_ITEM_PATH_WINDOW_BEAMABLE_HELP + "/" +
         ContentConstants.OPEN + " " +
         ContentConstants.BEAMABLE_DOCS_WEBSITE,
         priority = ContentConstants.MENU_ITEM_PATH_WINDOW_PRIORITY_3)]
      private static void OpenBeamableDocsWebsite()
      {
         Application.OpenURL(ContentConstants.URL_BEAMABLE_DOCS_WEBSITE);
      }
   }
}