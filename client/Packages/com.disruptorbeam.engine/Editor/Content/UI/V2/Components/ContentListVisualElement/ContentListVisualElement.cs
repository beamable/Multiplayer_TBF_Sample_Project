using System.Collections.Generic;
using Beamable.Editor.Content.Models;
using System.Linq;
using System;
using UnityEngine;
using UnityEditor;
using Beamable.Editor.UI.Buss.Components;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
using UnityEngine.Experimental.UIElements.StyleSheets;

#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Content.Components
{
   public class ContentListVisualElement : ContentManagerComponent
   {

      public event Action<IList<ContentItemDescriptor>> OnSelectionChanged;
      public event Action<ContentItemDescriptor> OnItemDelete;
      public event Action<ContentTypeDescriptor> OnItemAdd;
      public event Action<List<ContentItemDescriptor>> OnItemDownload;

      /// <summary>
      /// Provide explict height for every row so ListView can calculate
      /// how many items to actually display, as an optimization.
      /// </summary>
      private int ListViewItemHeight = 24;

      /// <summary>
      /// Indicates index when nothing is selected in the <see cref="ListView"/>
      /// </summary>
      private int NullIndex = -1;

      public ContentDataModel Model { get; set; }

      private VisualElement _mainVisualElement;
      private HeaderVisualElement _headerVisualElement;
      private ListView _listView;
      private List<ContentItemDescriptor> _contentItemDescriptorList;
      private List<HeaderSizeChange> _headerSizeChanges;

      public ContentListVisualElement() : base(nameof(ContentListVisualElement))
      {

      }

      #if UNITY_2018
      protected override void DoRepaint(IStylePainter painter)
      {
         base.DoRepaint(painter);
         _headerVisualElement.EmitFlexValues();
      }
      #endif


      public override void Refresh()
      {
         base.Refresh();



         _mainVisualElement = Root.Q<VisualElement>("mainVisualElement");
         _mainVisualElement.RegisterCallback<MouseDownEvent>(MainContent_OnMouseDownEvent,
            TrickleDown.NoTrickleDown);

         _headerVisualElement = Root.Q<HeaderVisualElement>("headerVisualElement");
         _headerVisualElement.Headers = new[] {"Content ID", "Folder", "Tags"};
         _headerVisualElement.Refresh();
         _headerVisualElement.OnValuesChanged += Header_OnValuesResized;
         _headerSizeChanges = _headerVisualElement.ComputeSizes(new List<float>{1, .5f, .2f});

         EditorApplication.delayCall += () => { _headerVisualElement.EmitFlexValues(); };



         //List
         _listView = CreateListView();
         _mainVisualElement.Add(_listView);

         Model.OnSelectedContentChanged += Model_OnSelectedContentChanged;
         Model.OnFilteredContentsChanged += Model_OnFilteredContentChanged;

         _listView.Refresh();
      }

      private void Header_OnValuesResized(List<HeaderSizeChange> headerFlexSizes)
      {
         _headerSizeChanges = headerFlexSizes;
         // update all content...
         foreach (var listElement in _listView.Children())
         {
            ApplyColumnSizes(listElement);
         }
      }

      private void ApplyColumnSizes(VisualElement listElement)
      {
         listElement.Q("nameTextField").style.flexGrow = (_headerSizeChanges[0].Flex);
         listElement.Q("pathLabel").style.flexGrow = (_headerSizeChanges[1].Flex);
         listElement.Q("tagListVisualElement").style.flexGrow =(_headerSizeChanges[2].Flex);

//         listElement.Q("nameTextField").style.minWidth = 40;
//         listElement.Q("pathLabel").style.minWidth = 40;
//         listElement.Q("tagListVisualElement").style.minWidth = 40;
         listElement.Q("nameTextField").style.minWidth = (_headerSizeChanges[0].MinWidth);
         listElement.Q("pathLabel").style.minWidth = (_headerSizeChanges[1].MinWidth);
         listElement.Q("tagListVisualElement").style.minWidth = (_headerSizeChanges[2].MinWidth);
      }

      private void Model_OnFilteredContentChanged()
      {
         _listView.Refresh();
      }

      private ListView CreateListView()
      {
         var view = new ListView()
         {
            makeItem = CreateListViewElement,
            bindItem = BindListViewElement,
            selectionType = ContentManagerContants.ContentListSelectionType,
            itemHeight = ListViewItemHeight,
            itemsSource = Model.FilteredContents
         };

         view.onItemChosen += ListView_OnItemChosen;
         view.onSelectionChanged += ListView_OnSelectionChanged;

         return view;
      }

      void BindListViewElement(VisualElement elem, int index)
      {
         ContentVisualElement contentVisualElement = (ContentVisualElement)elem;
         contentVisualElement.ContentItemDescriptor = Model.FilteredContents[index];//_contentItemDescriptorList[index];
         contentVisualElement.OnRightMouseButtonClicked -= ContentVisualElement_OnRightMouseButtonClicked;
         contentVisualElement.OnRightMouseButtonClicked += ContentVisualElement_OnRightMouseButtonClicked;
         contentVisualElement.Refresh();
         ApplyColumnSizes(contentVisualElement);
         contentVisualElement.MarkDirtyRepaint();
      }

      ContentVisualElement CreateListViewElement()
      {
         ContentVisualElement contentVisualElement = new ContentVisualElement();

         return contentVisualElement;
      }


      /// <summary>
      /// Capture when the background (Not an item) is clicked
      /// with ANY mouse button.
      /// </summary>
      private void MainContent_OnMouseDownEvent(MouseDownEvent evt)
      {
         VisualElement target = (VisualElement)evt.target;

         //HACK: Remove this hack. Instead, ideally observe the event in a way that
         //a click on an item does NOT call this handler - srivello
         if (target.name.Contains ("ContentViewport"))
         {
            SetSelectedIndexSafe(NullIndex);
         }
      }

      private void SelectItemInInspectorWindow(params ContentItemDescriptor[] contentItemDescriptor)
      {
         var selection = contentItemDescriptor
            .Where(item => !string.IsNullOrEmpty(item.AssetPath))
            .Select(item => new Tuple<UnityEngine.Object, ContentItemDescriptor>(AssetDatabase.LoadMainAssetAtPath(item.AssetPath), item))
            .ToList();

         foreach (var errorCase in selection.Where(obj => obj.Item1 == null))
         {
            Debug.LogError(new Exception("ListView_OnItemChosen() Error : " +
                                         "no unityObject for " + errorCase.Item2.Name));
         }

         var objects = selection.Select(x => x.Item1).ToArray();
         Selection.objects = objects;
      }

      private void PingItemInProjectWindow(ContentItemDescriptor contentItemDescriptor)
      {
         if (string.IsNullOrEmpty(contentItemDescriptor.AssetPath))
         {
            Debug.LogError(new Exception("ListView_OnItemChosen() Error : " +
               "no AssetPath for " + contentItemDescriptor.Name));
            return;
         }

         UnityEngine.Object unityObject =
            AssetDatabase.LoadMainAssetAtPath(contentItemDescriptor.AssetPath);

         if (unityObject == null)
         {
            Debug.LogError(new Exception ("ListView_OnItemChosen() Error :" +
               " no unityObject for " + contentItemDescriptor.Name));
            return;
         }

         EditorGUIUtility.PingObject(unityObject.GetInstanceID());
      }

      /// <summary>
      /// Set the selected <see cref="ListView"/> item like this.
      /// This prevents an infinite UI loop.
      /// </summary>
      /// <param name="index"></param>
      private void SetSelectedIndexSafe(int index)
      {
         if (_listView.selectedIndex != index)
         {
            _listView.selectedIndex = index;
         }
      }

      /// <summary>
      /// Lookup the <see cref="ContentVisualElement"/> by the <see cref="ContentItemDescriptor"/>
      /// </summary>
      /// <param name="contentItemDescriptor"></param>
      /// <returns></returns>
      private ContentVisualElement GetVisualItemByData(ContentItemDescriptor contentItemDescriptor)
      {
         List<VisualElement> visualElements = _listView.Children().ToList();

         return (ContentVisualElement)visualElements.Find((VisualElement visualElement) =>
         {
            ContentVisualElement nextContentVisualElement = (ContentVisualElement)visualElement;
            return nextContentVisualElement.ContentItemDescriptor == contentItemDescriptor;
         });
      }


      private void Model_OnSelectedContentChanged(IList<ContentItemDescriptor> contentItemDescriptors)
      {
         var x = contentItemDescriptors.FirstOrDefault<ContentItemDescriptor>();

         if (x == null)
         {
            SetSelectedIndexSafe(NullIndex);
         }
         else
         {
            SetSelectedIndexSafe(_listView.itemsSource.IndexOf(x));
         }
      }



      private void ContentVisualElement_OnRightMouseButtonClicked(ContentItemDescriptor contentItemDescriptor)
      {
          // Update selection to match the right clicked item
          if (!Model.SelectedContents.Contains(contentItemDescriptor))
          {
             _listView.selectedIndex = _listView.itemsSource.IndexOf(contentItemDescriptor);
          }

         ContextualMenuManipulator ContextualMenuManipulator =
            new ContextualMenuManipulator(ContentVisualElement_OnContextMenuOpen);

         ContextualMenuManipulator.target = Root;
      }

      private void AddCreateItemMenu(ContextualMenuPopulateEvent evt)
      {
         var selectedTypes = Model.SelectedContentTypes;
         var types = Model.GetContentTypes().ToList();

         if (selectedTypes.FirstOrDefault() is ContentTypeTreeViewItem selectedType)
         {
            types = types.Where(t => selectedType.TypeDescriptor.ContentType.IsAssignableFrom(t.ContentType)).ToList();

            evt.menu.BeamableAppendAction($"{ContentManagerContants.ContentListCreateItem} {selectedType.displayName}", (Action<Vector2>)((pos) =>
            {
               OnItemAdd?.Invoke(selectedType.TypeDescriptor);
            }));
         }

         foreach (var type in types)
         {
            evt.menu.BeamableAppendAction($"Create/{type.TypeName.Replace(".", "/")}", _ =>
            {
               OnItemAdd?.Invoke(type);
            });
         }
      }

      private void ShowContextMenuForSingle(ContextualMenuPopulateEvent evt, ContentItemDescriptor item)
      {
         if (item.LocalStatus == HostStatus.AVAILABLE) // cannot rename something that we don't have locally...
         {
            evt.menu.BeamableAppendAction(ContentManagerContants.ContentListDeleteItem, (Action<Vector2>)((pos) =>
            {
               ContentVisualElement_OnItemDelete((ContentItemDescriptor)item);
            }));
            evt.menu.BeamableAppendAction(ContentManagerContants.ContentListRenameItem, (Action<Vector2>)((pos) =>
            {
               ContentVisualElement_OnItemRenameGestureBegin((ContentItemDescriptor)item);
            }));

            if (item.Status == ContentModificationStatus.MODIFIED)
            {
               evt.menu.BeamableAppendAction(ContentManagerContants.ContentListRevertItem, (Action<Vector2>)((pos) =>
               {
                  ContentVisualElement_OnDownloadSingle(item);
               }));
            }
         }

         if (item.LocalStatus == HostStatus.NOT_AVAILABLE && item.ServerStatus == HostStatus.AVAILABLE)
         {
            evt.menu.BeamableAppendAction(ContentManagerContants.ContentListDownloadItem, (Action<Vector2>)((pos) =>
               {
                  ContentVisualElement_OnDownloadSingle(item);
               }));
         }
      }

      private void ShowContextMenuForMany(ContextualMenuPopulateEvent evt, List<ContentItemDescriptor> items)
      {

         var allLocal = items.All(i => i.LocalStatus == HostStatus.AVAILABLE);
         if (allLocal)
         {
            evt.menu.BeamableAppendAction($"{ContentManagerContants.ContentListDeleteItems} ({items.Count})", (Action<Vector2>)((pos) =>
            {
               ContentVisualElement_OnItemDelete(items.ToArray());
            }));
         }

         var modifiedOrServerOnly = items.Where(i =>
            i.LocalStatus == HostStatus.NOT_AVAILABLE || i.Status == ContentModificationStatus.MODIFIED).ToList();
         if (modifiedOrServerOnly.Count > 0)
         {
            evt.menu.BeamableAppendAction($"{ContentManagerContants.ContentListDownloadItems} ({modifiedOrServerOnly.Count})",
               (Action<Vector2>) ((pos) => { ContentVisualElement_OnDownloadMany(modifiedOrServerOnly); }));
         }

      }


      private void ContentVisualElement_OnContextMenuOpen( ContextualMenuPopulateEvent evt)
      {
         switch (Model.SelectedContents.Count)
         {
            case 0:
               AddCreateItemMenu(evt);
               break;
            case 1:
               AddCreateItemMenu(evt);

               ShowContextMenuForSingle(evt, Model.SelectedContents.FirstOrDefault());
               break;
            default:
               ShowContextMenuForMany(evt, Model.SelectedContents.ToList());
               break;
         }

      }

      private void ContentVisualElement_OnItemDelete(params ContentItemDescriptor[] contentItemDescriptors)
      {
         // Get the bounds of the entire visual element
         VisualElement centerConfirmationPopupWithinMe = this;

         // Center the "Are you Sure?" prompt within
         var confirmationPopupWindow = BeamablePopupWindow.ShowConfirmationPopup(centerConfirmationPopupWithinMe);

         ConfirmationPopupVisualElement confirmationPopupVisualElement =
            (ConfirmationPopupVisualElement) confirmationPopupWindow.ContentElement;

         confirmationPopupVisualElement.OnOKButtonClicked += () =>
         {
            confirmationPopupWindow.Close();
            foreach (var toDelete in contentItemDescriptors)
            {
               OnItemDelete?.Invoke(toDelete);
            }
         };

         confirmationPopupVisualElement.OnCancelButtonClicked += () =>
         {
            confirmationPopupWindow.Close();
         };
      }


      private void ContentVisualElement_OnItemRenameGestureBegin(ContentItemDescriptor contentItemDescriptor)
      {
         ContentVisualElement contentVisualElement = GetVisualItemByData(contentItemDescriptor);
         contentVisualElement.RenameGestureBegin();
      }

      private void ContentVisualElement_OnDownloadSingle(ContentItemDescriptor contentItemDescriptor)
      {
         OnItemDownload?.Invoke(new List<ContentItemDescriptor>{contentItemDescriptor});
      }
      private void ContentVisualElement_OnDownloadMany(List<ContentItemDescriptor> contentItemDescriptors)
      {
         OnItemDownload?.Invoke(contentItemDescriptors);
      }

      /// <summary>
      /// Handles double-click of an <see cref="ContentItemDescriptor"/>
      /// </summary>
      private void ListView_OnItemChosen(object obj)
      {
         ContentItemDescriptor contentItemDescriptor = (ContentItemDescriptor)obj;

         Debug.Log("ListView_OnItemChosen : " + contentItemDescriptor.AssetPath);

         SelectItemInInspectorWindow(contentItemDescriptor);
         PingItemInProjectWindow(contentItemDescriptor);
      }


      /// <summary>
      /// Handles single-click of an <see cref="ContentItemDescriptor"/>
      /// </summary>
      private void ListView_OnSelectionChanged(List<object> objs)
      {
         var contentItemDescriptors = objs.Cast<ContentItemDescriptor>().ToArray();

         SelectItemInInspectorWindow(contentItemDescriptors);
         OnSelectionChanged?.Invoke(contentItemDescriptors.ToList());
      }


   }
}