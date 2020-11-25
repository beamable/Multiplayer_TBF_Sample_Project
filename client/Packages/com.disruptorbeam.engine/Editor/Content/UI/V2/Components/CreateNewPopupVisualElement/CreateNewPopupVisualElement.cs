using UnityEngine;
using System;
using Beamable.Editor.Content.Models;
using UnityEditor.IMGUI.Controls;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Content.Components
{
   public class CreateNewPopupVisualElement : ContentManagerComponent
   {
      public event Action OnAddItemButtonClicked;
      public ContentDataModel Model { get; internal set; }

      private Button _addContentGroupButton;
      private Button _addContentButton;
      private Button _addItemButton;

      public CreateNewPopupVisualElement() : base(nameof(CreateNewPopupVisualElement)) { }

      public override void Refresh()
      {
         base.Refresh();

         _addItemButton = Root.Q<Button>("addItemButton");

         if (Model.SelectedContentTypes.Count == 1)
         {
            TreeViewItem treeViewItem = Model.SelectedContentTypes[0];
            ContentTypeTreeViewItem contentTypeTreeViewItem = (ContentTypeTreeViewItem)treeViewItem;
            Type type = contentTypeTreeViewItem.TypeDescriptor.ContentType;

            _addItemButton.SetEnabled(true);
            _addItemButton.text = string.Format(ContentManagerContants.CreateNewPopupAddButtonEnabledText,
               contentTypeTreeViewItem.TypeDescriptor.ContentType.Name);

         }
         else
         {
            _addItemButton.SetEnabled(false);
            _addItemButton.text = ContentManagerContants.CreateNewPopupAddButtonDisabledText;
         }
         
         _addItemButton.clickable.clicked += () =>
         {
            AddItemButton_OnClicked();
         };
      }

      private void AddItemButton_OnClicked()
      {
         OnAddItemButtonClicked?.Invoke();
      }
   }
}