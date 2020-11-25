using System.Collections.Generic;
using Beamable.Editor.Content.Components;
using Beamable.Editor.Content.Models;
using Beamable.Editor;
using UnityEditor;
using Beamable.Editor.NoUser;
using Beamable.Editor.UI.Buss.Components;
using Beamable.Platform.SDK;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Content
{
   public class ContentManagerWindow : EditorWindow
   {
#if BEAMABLE_DEVELOPER
      [MenuItem(
      ContentConstants.MENU_ITEM_PATH_WINDOW_BEAMABLE_UTILITIES_BEAMABLE_DEVELOPER + "/" +
      ContentConstants.OPEN + " " +
      ContentConstants.CONTENT_MANAGER_V2,
      priority = ContentConstants.MENU_ITEM_PATH_WINDOW_PRIORITY_2
      )]
#endif
      public static void Init()
      {
         // Ensure at most one Beamable ContentManagerWindow exists
         // If exists, rebuild it from scratch (easy refresh mechanism)
         if (ContentManagerWindow.IsInstantiated)
         {
            ContentManagerWindow.Instance.Close();
            DestroyImmediate(ContentManagerWindow.Instance);
         }

         // Create Beamable ContentManagerWindow and dock it next to Unity Hierarchy Window
         var hierarchy = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.PreviewWindow");
         var contentManagerWindow = GetWindow<ContentManagerWindow>(ContentConstants.CONTENT_MANAGER, true, typeof(SceneView));

         contentManagerWindow.Show(true);
      }

      public static ContentManagerWindow Instance { get; private set; }
      public static bool IsInstantiated { get { return Instance != null; } }


      private ContentManager _contentManager;
      private VisualElement _windowRoot;
      private VisualElement _explorerContainer, _statusBarContainer;

      private ActionBarVisualElement _actionBarVisualElement;
      private ExplorerVisualElement _explorerElement;
      private StatusBarVisualElement _statusBarElement;

      private void OnEnable()
      {
         // Refresh if/when the user logs-in or logs-out while this window is open
         EditorAPI.Instance.Then(de => { de.OnUserChange += _ => Refresh(); });

         // Force refresh to build the initial window
         Refresh();
      }

      private async void Refresh()
      {
         var de = await EditorAPI.Instance;
         var isLoggedIn = de.User != null;
         if (isLoggedIn)
         {
            SetForContent();

         }
         else
         {
            SetForLogin();
         }
      }

      void SetForLogin()
      {
         var root = this.GetRootVisualContainer();
         root.Clear();
         var noUserVisualElement = new NoUserVisualElement();
         root.Add(noUserVisualElement);
      }

      void SetForContent()
      {
         Instance?._contentManager?.Destroy();

         Instance = this;
         _contentManager?.Destroy();
         _contentManager = new ContentManager();
         _contentManager.Initialize();

         var root = this.GetRootVisualContainer();
         root.Clear();
         var uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{ContentManagerContants.BASE_PATH}/ContentManagerWindow.uxml");
         _windowRoot = uiAsset.CloneTree();
         _windowRoot.AddStyleSheet($"{ContentManagerContants.BASE_PATH}/ContentManagerWindow.uss");
         _windowRoot.name = nameof(_windowRoot);

         root.Add(_windowRoot);

         _actionBarVisualElement = root.Q<ActionBarVisualElement>("actionBarVisualElement");
         _actionBarVisualElement.Model = _contentManager.Model;
         _actionBarVisualElement.Refresh();

         // Handlers for Buttons (Left To Right in UX)
         _actionBarVisualElement.OnAddItemButtonClicked += () =>
         {
            _contentManager.AddItem();
         };
         //
         _actionBarVisualElement.OnValidateButtonClicked += () =>
         {
            var validatePopup = new ValidateContentVisualElement();
            validatePopup.DataModel = _contentManager.Model;
            var wnd = BeamablePopupWindow.ShowUtility(ContentManagerContants.ValidateContent, validatePopup, this);
            wnd.minSize = ContentManagerContants.WindowSizeMinimum;

            validatePopup.OnCancelled += () => wnd.Close();
            validatePopup.OnClosed += () => wnd.Close();

            _contentManager.ValidateContent(validatePopup.SetProgress, validatePopup.HandleValidationErrors)
               .Then(_ => validatePopup.HandleFinished());
         };

         _actionBarVisualElement.OnPublishButtonClicked += () =>
         {
            // validate and create publish set.
            var validatePopup = new ValidateContentVisualElement();
            validatePopup.DataModel = _contentManager.Model;

            var wnd = BeamablePopupWindow.ShowUtility(ContentManagerContants.ValidateContent, validatePopup, this);

            validatePopup.OnCancelled += () => wnd.Close();
            validatePopup.OnClosed += () => wnd.Close();

            _contentManager.ValidateContent(validatePopup.SetProgress, validatePopup.HandleValidationErrors)
               .Then(errors =>
               {
                  validatePopup.HandleFinished();

                  if (errors.Count != 0) return;

                  var publishPopup = new PublishContentVisualElement();
                  publishPopup.DataModel = _contentManager.Model;
                  publishPopup.PublishSet = _contentManager.CreatePublishSet();
                  wnd.SwapContent(publishPopup);
                  wnd.titleContent = new GUIContent("Publish Content");

                  publishPopup.OnCancelled += () => wnd.Close();
                  publishPopup.OnPublishRequested += (set, prog, finished) =>
                  {
                     _contentManager.PublishContent(set, prog, finished).Then(__ => {
                        wnd.Close();
                        _contentManager.RefreshWindow(true);
                     });
                  };

               });

         };

         _actionBarVisualElement.OnDownloadButtonClicked += () =>
         {
            var downloadPopup = new DownloadContentVisualElement();

            downloadPopup.Model = _contentManager.PrepareDownloadSummary();
            var wnd = BeamablePopupWindow.ShowUtility(ContentManagerContants.DownloadContent, downloadPopup, this);
            wnd.minSize = ContentManagerContants.WindowSizeMinimum;

            downloadPopup.OnClosed += () => wnd.Close();
            downloadPopup.OnCancelled += () => wnd.Close();
            downloadPopup.OnDownloadStarted += (summary, prog, finished) =>
            {
               _contentManager.DownloadContent(summary, prog, finished).Then(_ => Refresh());
            };
         };

         _actionBarVisualElement.OnRefreshButtonClicked += () =>
         {
            _contentManager.RefreshWindow(true);
         };

         _actionBarVisualElement.OnDocsButtonClicked += () =>
         {
            _contentManager.ShowDocs();
         };

         _explorerContainer = root.Q<VisualElement>("explorer-container");
         _statusBarContainer = root.Q<VisualElement>("status-bar-container");

         _explorerElement = new ExplorerVisualElement();
         _explorerContainer.Add(_explorerElement);
         _explorerElement.OnAddItemButtonClicked += ExplorerElement_OnAddItemButtonClicked;
         _explorerElement.OnAddItemRequested += ExplorerElement_OnAddItem;
         _explorerElement.OnItemDownloadRequested += ExplorerElement_OnDownloadItem;

         _explorerElement.Model = _contentManager.Model;
         _explorerElement.Refresh();

         _statusBarElement = new StatusBarVisualElement();
         _statusBarElement.Model = _contentManager.Model;
         _statusBarContainer.Add(_statusBarElement);
         _statusBarElement.Refresh();



      }

      private void ExplorerElement_OnAddItemButtonClicked()
      {
         var newContent = _contentManager.AddItem();
         EditorApplication.delayCall += () =>
         {
            if (_contentManager.Model.GetDescriptorForId(newContent.Id, out var item))
            {
               item.ForceRename();
            }
         };
      }

      private void ExplorerElement_OnAddItem(ContentTypeDescriptor type)
      {
         var newContent = _contentManager.AddItem(type);
         EditorApplication.delayCall += () =>
         {
            if (_contentManager.Model.GetDescriptorForId(newContent.Id, out var item))
            {
               item.ForceRename();
            }
         };
      }

      private void ExplorerElement_OnDownloadItem(List<ContentItemDescriptor> items)
      {
         var downloadPopup = new DownloadContentVisualElement();

         downloadPopup.Model = _contentManager.PrepareDownloadSummary(items.ToArray());
         var wnd = BeamablePopupWindow.ShowUtility(ContentManagerContants.DownloadContent, downloadPopup, this);
         wnd.minSize = ContentManagerContants.WindowSizeMinimum;

         downloadPopup.OnClosed += () => wnd.Close();
         downloadPopup.OnCancelled += () => wnd.Close();
         downloadPopup.OnDownloadStarted += (summary, prog, finished) =>
         {
            _contentManager.DownloadContent(summary, prog, finished).Then(_ => Refresh());
         };
      }
   }
}