using System.Linq;
using UnityEditor;
using UnityEngine;
using Beamable.Content;
using Beamable.Platform.SDK;
using Beamable.Common.Content;
using System;
using System.Collections.Generic;
using System.Reflection;
using Beamable.Common;
using Beamable.Content.Serialization;
using Beamable.Editor.NoUser;
using Debug = UnityEngine.Debug;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Content.UI.ProjectView
{
   public class ContentListWindow : EditorWindow
   {

      private const string Asset_UXML_ContentListWindow =
         "Packages/com.disruptorbeam.engine/Editor/Content/UI/ProjectView/contentListWindow.uxml";

      private const string Asset_USS_ContentListWindow =
         "Packages/com.disruptorbeam.engine/Editor/Content/UI/ProjectView/contentListWindow.uss";

      private VisualElement _windowRoot;
      private ContentListVisualElement _listElement;
      private VisualElement _progressBar;
      private VisualElement _progressBarContainer;
      private readonly Vector2 windowMax = new Vector2(600, 750);
      private readonly Vector2 windowMin = new Vector2(400, 400);
      private readonly Dictionary<string, Type> contentTypeToClass = new Dictionary<string, Type>();

      [MenuItem(
         ContentConstants.MENU_ITEM_PATH_WINDOW_BEAMABLE + "/" +
         ContentConstants.OPEN + " " +
         ContentConstants.CONTENT_MANAGER,
         priority = ContentConstants.MENU_ITEM_PATH_WINDOW_PRIORITY_2)]
      public static void Init()
      {
         var hierarchy = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.SceneHierarchyWindow");
         var wnd = GetWindow<ContentListWindow>(ContentConstants.CONTENT_MANAGER, true, hierarchy);
      }

      public void OnEnable()
      {
         this.maxSize = windowMax;
         this.minSize = windowMin;
         EditorAPI.Instance.Then(de => { de.OnUserChange += _ => Refresh(); });
         Refresh();
      }

      void UpdateProgressBar(float ratio)
      {
         var parentWidth = _progressBarContainer.layout.width;
         _progressBar.style.SetRight(parentWidth * (1 - ratio));
      }

      async void Refresh()
      {
         // check if logged in.
         var root = this.GetRootVisualContainer();
         root.Clear();
         root.RemoveStyleSheet(Asset_USS_ContentListWindow);
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
         var noUserComponent = new NoUserVisualElement();
         root.Add(noUserComponent);
      }

      void SetForContent()
      {
         var root = this.GetRootVisualContainer();
         var uiAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Asset_UXML_ContentListWindow);
         _windowRoot = uiAsset.CloneTree();
         _windowRoot.name = nameof(_windowRoot);
         root.AddStyleSheet(Asset_USS_ContentListWindow);
         root.Add(_windowRoot);

         _listElement = new ContentListVisualElement();

         // Nullcheck prevents exception in the following situation...
         // When CMv1.0 is openned without user login,
         // THen user logs in, then an error is thrown here - srivello
         if (_listElement != null)
         {
            root.Q<VisualElement>(name = "list-root").Add(_listElement);
            root.Q<Button>(name = "refresh").clickable.clicked += () => { _listElement.Refresh(); };
            root.Q<Button>(name = "publish").clickable.clicked += Publish;
            root.Q<Button>(name = "download").clickable.clicked += DownloadContent;

            root.Q<Label>(name = "contentListTitle").text = ContentConstants.CONTENT_MANAGER.ToUpper();

            _progressBar = root.Q<VisualElement>(name = "progress-bar");
            _progressBarContainer = root.Q<VisualElement>(name = "progress-bar-container");
         }
      }

      public async void Publish()
      {
         var de = await EditorAPI.Instance;
         var publishSet = await de.ContentPublisher.CreatePublishSet();
         if (publishSet.HasValidationErrors(de.ContentIO.GetValidationContext(), out var errors))
         {
            var errMessage = string.Join("\n", errors);
            EditorUtility.DisplayDialog("Invalid content.", errMessage, "Okay");
            return;
         }
         UpdateProgressBar(0);

         var message = "The following changes will be made...";

         message += "\n\nAdditions...\n";
         message += string.Join("\n", publishSet.ToAdd.Select(c => c.Id).ToArray());

         message += "\n\nModifications...\n";
         message += string.Join("\n", publishSet.ToModify.Select(c => c.Id).ToArray());

         message += "\n\nDeletions...\n";
         message += string.Join("\n", publishSet.ToDelete.ToArray());


         var confirmed = EditorUtility.DisplayDialog("Are you sure you want to publish?", message, "Publish", "Cancel");
         if (!confirmed) return;

         var publishJob = de.ContentPublisher.Publish(publishSet,
            p =>
            {
               Debug.Log("PROGRESS: " + p.CompletedOperations + " / " + p.TotalOperations + " = " + (p.Progress * 100) +
                         "%");
               UpdateProgressBar(p.Progress);
            });
         await publishJob.Then(_ =>
         {
            Debug.Log("PUBLISH IS COMPLETE");

            de.ContentIO.FetchManifest().Then(manifest =>
            {
               UpdateProgressBar(0);
               _listElement.Refresh();
            });
         }).Error(ex =>
         {
            UpdateProgressBar(0);
            EditorUtility.DisplayDialog("Unable to publish content.", ex.Message, "Okay");
         });
      }

      public async void DownloadContent()
      {

         var serializer = new ClientContentSerializer();
         var de = await EditorAPI.Instance;
         var localManifest = de.ContentIO.BuildLocalManifest();
         var manifest = await de.ContentIO.FetchManifest();

         var operations = new List<ContentDownloadEntry>();

         var downloadPromises = new List<Promise<Unit>>();
         foreach (var reference in manifest.References)
         {
            if (reference.Visibility != "public") continue;

            var assetPath = ""; // default.
            var exists = false;
            if (localManifest.Content.TryGetValue(reference.Id, out var localEntry))
            {
               assetPath = localEntry.AssetPath;
               exists = true;

               var checksum = de.ContentIO.Checksum(localEntry.Content);
               if (checksum == reference.Checksum && localEntry.Tags.SequenceEqual(reference.Tags))
               {
                  continue; // already up to date.
               }

            }

            operations.Add(new ContentDownloadEntry
            {
               AssetPath = assetPath,
               ContentId = reference.Id,
               Uri = reference.Uri,
               Operation = exists ? "MODIFY" : "ADD",
               Tags = reference.Tags
            });

         }

         var additions = operations
            .Count(entry => entry.Operation.Equals("ADD"));
         var additionsMessage = $"Additions\n {string.Join("\n", additions)}";

         var modifications = operations
            .Count(entry => entry.Operation.Equals("MODIFY"));
         var modificationsMessage = $"Modifications\n {string.Join("\n", modifications)}";

         var operationsMessage = $"\n{additionsMessage}\n\n{modificationsMessage}";

         var results = EditorUtility.DisplayDialog("Download and replace local content?",
            "Clicking 'Download' will overwrite local content changes with published content from Beamable Content." +
            operationsMessage,
            "Download",
            "Cancel");

         if (!results)
         {
            return;
         }

         downloadPromises = operations.Select(operation =>
         {
            Debug.Log($"Downloading content id=[{operation.ContentId}] to path=[{operation.AssetPath}]");
            return FetchContentFromCDN(operation.Uri).Map(response =>
            {
               var contentType = ContentRegistry.GetTypeFromId(operation.ContentId);

               var newAsset = serializer.DeserializeByType(response, contentType);
               newAsset.Tags = operation.Tags;
               de.ContentIO.Create(newAsset, operation.AssetPath);
               return PromiseBase.Unit;
            });
         }).ToList();

         await Promise.Sequence(downloadPromises).Then(_ =>
         {
            EditorUtility.DisplayDialog("Download Complete", "Content has finished downloading.", "OK");
         });
      }

      private Promise<string> FetchContentFromCDN(string uri)
      {
         return EditorAPI.Instance.FlatMap(de =>
            de.Requester.Request(Method.GET, uri, includeAuthHeader: false, parser: s => s)
         );
      }

      struct ContentDownloadEntry
      {
         public string ContentId;
         public string AssetPath;
         public string Uri;
         public string Operation;
         public string[] Tags;
      }
   }
}
