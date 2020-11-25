using Beamable.Editor.Content.Models;
using Beamable.Editor.Content;
using UnityEngine;
using Beamable.Editor.UI.Buss.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common;
using Beamable.Platform.SDK;
using Beamable.Editor.UI.Components;
using UnityEditor;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Content.Components
{
   public class DownloadContentVisualElement : ContentManagerComponent
   {
      public Promise<DownloadSummary> Model { get; set; }

      public event Action OnCancelled;
      public event Action OnClosed;
      public event Action<DownloadSummary, HandleContentProgress, HandleDownloadFinished> OnDownloadStarted;

      private Button _cancelBtn;
      private Button _downloadBtn;
      private LoadingBarVisualElement _loadingBar;

      public DownloadContentVisualElement() : base(nameof(DownloadContentVisualElement))
      {
      }

      public override void Refresh()
      {
         base.Refresh();

         // we need to show a loading indicator until the download diff is available.

         var mainElement = Root.Q<VisualElement>("mainVisualElement");

         var loadingElement = Root.Q<LoadingIndicatorVisualElement>();
         loadingElement.SetPromise(Model, mainElement);

         Model.Then(summary =>
         {
            var messageLabel = Root.Q<Label>("message");
            messageLabel.text = ContentManagerContants.DownloadMessageText;
            messageLabel.AddTextWrapStyle();

            // var summaryLabel = Root.Q<Label>("summary");
            // summaryLabel.text = String.Format("Summary:  {0}  override,  {1}  add in", summary.Overwrites.Count(), summary.Additions.Count());
            var overrideCount = Root.Q<CountVisualElement>("overrideCount");
            overrideCount.SetValue(summary.Overwrites.Count());
            var addInCount = Root.Q<CountVisualElement>("addInCount");
            addInCount.SetValue(summary.Additions.Count());

            _cancelBtn = Root.Q<Button>("cancelBtn");
            _cancelBtn.clickable.clicked += CancelButton_OnClicked;

            _downloadBtn = Root.Q<Button>("downloadBtn");
            _downloadBtn.clickable.clicked += DownloadButton_OnClicked;

            _loadingBar = Root.Q<LoadingBarVisualElement>();

            var noDownloadLabel = Root.Q<Label>("noDownloadLbl");
            noDownloadLabel.text = ContentManagerContants.DownloadNoDataText;
            noDownloadLabel.AddTextWrapStyle();
            if (summary.TotalDownloadEntries > 0)
            {
               noDownloadLabel.parent.Remove(noDownloadLabel);
            }

            // TODO show preview of download content.
            var modifiedFold = Root.Q<Foldout>("overwriteFoldout");
            modifiedFold.text = "Overwrites";
            var modifiedSource = new List<ContentDownloadEntryDescriptor>();
            var modifiedList = new ListView
            {
               itemHeight = 24,
               itemsSource = modifiedSource,
               makeItem = MakeElement,
               bindItem = CreateBinder(modifiedSource)
            };
            modifiedFold.contentContainer.Add(modifiedList);

            var additionFold = Root.Q<Foldout>("addFoldout");
            additionFold.text = "Additions";
            var addSource = new List<ContentDownloadEntryDescriptor>();
            var addList = new ListView
            {
               itemHeight = 24,
               itemsSource = addSource,
               makeItem = MakeElement,
               bindItem = CreateBinder(addSource)
            };
            additionFold.contentContainer.Add(addList);

            if (summary.AnyOverwrites)
            {
               modifiedSource.AddRange(summary.Overwrites);
               modifiedFold.Q<ListView>().style.height = modifiedList.itemHeight * summary.Overwrites.Count();
               modifiedList.Refresh();
            }
            else
            {
               modifiedFold.parent.Remove(modifiedFold);
            }

            if (summary.AnyAdditions)
            {
               addSource.AddRange(summary.Additions);
               additionFold.Q<ListView>().style.height = addList.itemHeight * summary.Additions.Count();
               addList.Refresh();

            }
            else
            {
               additionFold.parent.Remove(additionFold);
            }
         });
      }

      private ContentPopupLinkVisualElement MakeElement()
      {
         return new ContentPopupLinkVisualElement();
      }

      private Action<VisualElement, int> CreateBinder(List<ContentDownloadEntryDescriptor> source)
      {
         return (elem, index) =>
         {
            var link = elem as ContentPopupLinkVisualElement;
            link.Model = source[index];
            link.Refresh();
         };
      }

      private void CancelButton_OnClicked()
      {
         // TODO Be smarter about how we cancel the download.
         OnCancelled?.Invoke();
      }

      private void DownloadButton_OnClicked()
      {
         OnDownloadStarted?.Invoke(Model.GetResult(), progress =>
         {
            _loadingBar.Value = progress;
         }, finalPromise =>
            {
               finalPromise.Then(_ =>
                  {
                     _loadingBar.Value = 1;
                     EditorApplication.delayCall += () =>
                     {
                        EditorUtility.DisplayDialog("Download Complete", "All content has been downloaded successfully.", "OK");
                        OnClosed?.Invoke();
                     };

                  }).Error(_ =>
                  {
                     _loadingBar.Value = 1;
                     // TODO make this error reporting better.
                     EditorApplication.delayCall += () =>
                     {
                        EditorUtility.DisplayDialog("Download Failed", "See console for errors.", "OK");
                        OnClosed?.Invoke();
                     };
                  });
            });
      }
   }
}