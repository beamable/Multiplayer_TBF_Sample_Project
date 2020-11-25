﻿using UnityEditor;
using System;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif


namespace Beamable.Editor.Content
{
   /// <summary>
   /// Hold common values used by <see cref="ContentManager"/>
   /// for easy editability.
   /// </summary>
   public static class ContentManagerContants
   {
      // Paths
      public const string BASE_PATH = "Packages/com.disruptorbeam.engine/Editor/Content/UI/V2";
      public const string COMP_PATH = BASE_PATH + "/Components";

      // Configuration
      public static SelectionType ContentListSelectionType = SelectionType.Multiple;
      public static SelectionType ContentTypeViewSelectionType = SelectionType.Single;
      public static string UssExt => EditorGUIUtility.isProSkin ? "uss" : "light.uss";
      public static Vector2 WindowSizeMinimum = new Vector2(300, 300);

      // Display Text
      public const string BreadcrumbsAllContentText = "All Content";
      public const string BreadcrumbsCountText = " {0}/{1}";
      public const string ContentTypeViewHeaderText = "Content group/content";
      //
      public const string DownloadSummaryLabelText = "summary";
      public const string DownloadMessageText = "Clicking download will overwrite local content changes with the content on published to Beamable.";
      public const string DownloadNoDataText = "There is no content to download. All your local content is up to date.";
      //
      public const string ValidateStartMessage = "Starting validation...";
      public const string ValidateProgressMessage = "Validating content...";
      public const string ValidationCompleteMessage = "Validation complete";

      public const string ValidateButtonStartText = "Validating";
      public const string ValidateButtonDoneWithErrorsText = "View";
      public const string ValidateButtonDoneWithoutErrorsText = "Okay";
      //
      public const string PublishMessageLoading = "Preparing...";
      public const string PublishMessagePreview = "Clicking publish will upload the following changes to Beamable.";
      public const string PublishMessageInProgress = "Publishing content...";

      //
      public static string ContentListHeaderText = "Object Name                       Path                                     Tag";
      public static string ContentListCreateItem = "Create";
      public static string ContentListDeleteItem = "Delete Item";
      public static string ContentListDeleteItems = "Delete Items";
      public static string ContentListRenameItem = "Rename Item";
      public static string ContentListDownloadItem = "Download Item";
      public static string ContentListRevertItem = "Revert Item";
      public static string ContentListDownloadItems = "Download Items";
      //
      public static string DownloadContent = "Download Content";
      public static string ValidateContent = "Validate Content";
      public static string BreadcrumbTokenArrow = ">";
      //
      public static Vector2 CreateNewPopupWindowSize = new Vector2(200, 30);
      public static string CreateNewPopupWindowTitle = "Create New";
      public static string CreateNewPopupAddButtonDisabledText = "New Item";
      public static string CreateNewPopupAddButtonEnabledText = "New Item: {0}";

      /// <summary>
      /// Creates a name with NO SPACES. Spaces are not allowed by the backend - srivello
      /// </summary>
      /// <param name="type"></param>
      /// <returns></returns>
      public static string GetNameForNewContentFileByType(Type type)
      {
         return string.Format("New_{0}", type.Name);
      }
   }
}