using Beamable.Editor.Content.Models;
using Beamable.Editor.Content;
using Beamable.Editor.UI.Buss.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common;
using Beamable.Platform.SDK;
using Beamable.Editor.UI.Components;
using UnityEditor;
using Debug = UnityEngine.Debug;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Content.Components
{
    public class PublishContentVisualElement : ContentManagerComponent
    {
        private LoadingBarVisualElement _loadingBar;
        private Label _messageLabel;
        private Button _detailButton;
        public event Action OnCancelled;
        public event Action<ContentPublishSet, HandleContentProgress, HandleDownloadFinished> OnPublishRequested;
        public ContentDataModel DataModel { get; set; }
        public Promise<ContentPublishSet> PublishSet { get; set; }

        public PublishContentVisualElement() : base(nameof(PublishContentVisualElement))
        {
        }

        public override void Refresh()
        {
            base.Refresh();

            _loadingBar = Root.Q<LoadingBarVisualElement>();
            var loadingIndicator = Root.Q<LoadingIndicatorVisualElement>();
            var mainContent = Root.Q<VisualElement>("mainVisualElement");

            _messageLabel = Root.Q<Label>("message");
            _messageLabel.text = ContentManagerContants.PublishMessageLoading;
            // _detailButton = Root.Q<Button>("detailBtn");
            // _detailButton.clickable.clicked += DetailButton_OnClicked;
            loadingIndicator.SetPromise(PublishSet, mainContent);

            var overrideCountElem = Root.Q<CountVisualElement>("overrideCount");
            var addCountElem = Root.Q<CountVisualElement>("addInCount");
            var deleteCountElem = Root.Q<CountVisualElement>("deleted");

            var addFoldoutElem = Root.Q<Foldout>("addFoldout");
            addFoldoutElem.text = "Additions";
            addFoldoutElem.value = false;
            var addSource = new List<ContentDownloadEntryDescriptor>();
            var addList = new ListView
            {
                itemHeight = 24,
                itemsSource = addSource,
                makeItem = MakeElement,
                bindItem = CreateBinder(addSource)
            };
            addFoldoutElem.contentContainer.Add(addList);

            var modifyFoldoutElem = Root.Q<Foldout>("modifyFoldout");
            modifyFoldoutElem.text = "Modifications";
            modifyFoldoutElem.value = false;
            var modifySource = new List<ContentDownloadEntryDescriptor>();
            var modifyList = new ListView
            {
                itemHeight = 24,
                itemsSource = modifySource,
                makeItem = MakeElement,
                bindItem = CreateBinder(modifySource)
            };
            modifyFoldoutElem.contentContainer.Add(modifyList);


            var deleteFoldoutElem = Root.Q<Foldout>("deleteFoldout");
            deleteFoldoutElem.text = "Deletions";
            deleteFoldoutElem.value = false;
            var deleteSource = new List<ContentDownloadEntryDescriptor>();
            var deleteList = new ListView
            {
                itemHeight = 24,
                itemsSource = deleteSource,
                makeItem = MakeElement,
                bindItem = CreateBinder(deleteSource)
            };
            deleteFoldoutElem.contentContainer.Add(deleteList);

            var publishBtn = Root.Q<Button>("publishBtn");
            var cancelBtn = Root.Q<Button>("cancelBtn");
            cancelBtn.clickable.clicked += CancelButton_OnClicked;


            PublishSet.Then(publishSet =>
            {
                _messageLabel.text = ContentManagerContants.PublishMessagePreview;

                overrideCountElem.SetValue(publishSet.ToModify.Count);
                addCountElem.SetValue(publishSet.ToAdd.Count);
                deleteCountElem.SetValue(publishSet.ToDelete.Count);

                publishBtn.clickable.clicked += PublishButton_OnClicked;

                foreach (var toAdd in publishSet.ToAdd)
                {
                    if (DataModel.GetDescriptorForId(toAdd.Id, out var desc))
                    {
                        var data = new ContentDownloadEntryDescriptor
                        {
                            AssetPath = desc.AssetPath,
                            ContentId = toAdd.Id,
                            Operation = "upload",
                            Tags = toAdd.Tags,
                            Uri = ""
                        };
                        addSource.Add(data);
                    }
                }

                addFoldoutElem.Q<ListView>().style.height = addList.itemHeight * addSource.Count;
                addList.Refresh();

                foreach (var toModify in publishSet.ToModify)
                {
                    if (DataModel.GetDescriptorForId(toModify.Id, out var desc))
                    {
                        var data = new ContentDownloadEntryDescriptor
                        {
                            AssetPath = desc.AssetPath,
                            ContentId = toModify.Id,
                            Operation = "modify",
                            Tags = toModify.Tags,
                            Uri = ""
                        };
                        modifySource.Add(data);
                    }
                }

                modifyFoldoutElem.Q<ListView>().style.height = modifyList.itemHeight * modifySource.Count;
                modifyList.Refresh();

                foreach (var toDelete in publishSet.ToDelete)
                {
                    if (DataModel.GetDescriptorForId(toDelete, out var desc))
                    {
                        var data = new ContentDownloadEntryDescriptor
                        {
                            AssetPath = desc.AssetPath,
                            ContentId = toDelete,
                            Tags = desc.ServerTags?.ToArray(),
                            Operation = "delete",
                            Uri = ""
                        };
                        deleteSource.Add(data);
                    }
                }

                deleteFoldoutElem.Q<ListView>().style.height = deleteList.itemHeight * deleteSource.Count;
                deleteList.Refresh();

            });
        }

        private void DetailButton_OnClicked()
        {
            DataModel.ToggleStatusFilter(ContentModificationStatus.LOCAL_ONLY, true);
            DataModel.ToggleStatusFilter(ContentModificationStatus.MODIFIED, true);
            DataModel.ToggleStatusFilter(ContentModificationStatus.SERVER_ONLY, true);
        }

        private void CancelButton_OnClicked()
        {
            OnCancelled?.Invoke();
        }

        private void PublishButton_OnClicked()
        {
            var publishSet = PublishSet.GetResult();
            _messageLabel.text = ContentManagerContants.PublishMessagePreview;

            OnPublishRequested?.Invoke(publishSet, progress =>
            {
                _loadingBar.Value = progress;
            }, promise =>
            {
        promise.Then(_ =>
        {
            Debug.Log("Publish compelted");
        });
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

    }
}

// namespace Beamable.Editor.Content.Components.PublishContentVisualElement
// {
//     public interface PublishContentVisualElement
//     {
//
//     }
// }