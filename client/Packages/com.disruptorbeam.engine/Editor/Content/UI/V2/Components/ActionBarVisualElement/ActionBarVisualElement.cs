using Beamable.Editor.Content.Models;
using Beamable.Editor.Content;
using UnityEngine;
using Beamable.Editor.UI.Buss.Components;
using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Content.Components
{

   public class ActionBarVisualElement : ContentManagerComponent
   {
      public new class UxmlFactory : UxmlFactory<ActionBarVisualElement, UxmlTraits>
      {
      }

      public new class UxmlTraits : VisualElement.UxmlTraits
      {
         UxmlStringAttributeDescription customText = new UxmlStringAttributeDescription
            {name = "custom-text", defaultValue = "nada"};

         public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
         {
            get { yield break; }
         }

         public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
         {
            base.Init(ve, bag, cc);
            var self = ve as ActionBarVisualElement;
         }
      }

      public event Action OnAddItemButtonClicked;
      public event Action OnValidateButtonClicked;
      public event Action OnPublishButtonClicked;
      public event Action OnDownloadButtonClicked;
      public event Action OnRefreshButtonClicked;
      public event Action OnDocsButtonClicked;

      public ContentDataModel Model { get; internal set; }

      private SearchBarVisualElement _searchBar;
      private Button _createNewButton, _validateButton, _publishButton, _downloadButton;
      private Button _tagButton, _typeButton, _statusButton, _refreshButton, _docsButton;

      public ActionBarVisualElement() : base(nameof(ActionBarVisualElement))
      {
      }

      public override void Refresh()
      {
         base.Refresh();

         //Buttons (Left To Right in UX)

         _createNewButton = Root.Q<Button>("createNewButton");
         _createNewButton.clickable.clicked += () => { CreateNewButton_OnClicked(_createNewButton.worldBound); };

         _validateButton = Root.Q<Button>("validateButton");
         _validateButton.clickable.clicked += () => { OnValidateButtonClicked?.Invoke(); };
         _publishButton = Root.Q<Button>("publishButton");
         _publishButton.clickable.clicked += () => { OnPublishButtonClicked?.Invoke(); };
         _downloadButton = Root.Q<Button>("downloadButton");
         _downloadButton.clickable.clicked += () => { OnDownloadButtonClicked?.Invoke(); };

         _tagButton = Root.Q<Button>("tagButton");
         _tagButton.clickable.clicked += () => { TagButton_OnClicked(_tagButton.worldBound); };

         _typeButton = Root.Q<Button>("typeButton");
         _typeButton.clickable.clicked += () => { TypeButton_OnClicked(_typeButton.worldBound); };

         _statusButton = Root.Q<Button>("statusButton");
         _statusButton.clickable.clicked += () => { StatusButton_OnClicked(_statusButton.worldBound); };

         _refreshButton = Root.Q<Button>("refreshButton");
         _refreshButton.clickable.clicked += () => { OnRefreshButtonClicked?.Invoke(); };


         _docsButton = Root.Q<Button>("docsButton");
         _docsButton.clickable.clicked += () => { OnDocsButtonClicked?.Invoke(); };
         _searchBar = Root.Q<SearchBarVisualElement>();
         Model.OnQueryUpdated += (query, force) =>
         {
            var existing = force
               ? null
               : _searchBar.Value;

            var filterString = query?.ToString(existing) ?? "";
            _searchBar.SetValueWithoutNotify(filterString);
         };
         _searchBar.OnSearchChanged += SearchBar_OnSearchChanged;
      }

      private void SearchBar_OnSearchChanged(string obj)
      {
         var query = EditorContentQuery.Parse(obj);
         Model.SetFilter(query);
      }

      private void CreateNewButton_OnClicked(Rect visualElementBounds)
      {
         Rect popupWindowRect = BeamablePopupWindow.GetLowerLeftOfBounds(visualElementBounds);

         var content = new CreateNewPopupVisualElement();
         content.Model = Model;
         var wnd = BeamablePopupWindow.ShowDropdown(ContentManagerContants.CreateNewPopupWindowTitle,
            popupWindowRect, ContentManagerContants.CreateNewPopupWindowSize, content);

         content.OnAddItemButtonClicked += () =>
         {
            OnAddItemButtonClicked?.Invoke();
            wnd.Close();
         };

      }

      private void TagButton_OnClicked(Rect visualElementBounds)
      {
         Rect popupWindowRect = BeamablePopupWindow.GetLowerLeftOfBounds(visualElementBounds);
         var content = new TagFilterPopupVisualElement();
         content.Model = Model;
         var longest = "";
         foreach (var name in Model.GetAllTags())
         {
            if (name.Length > longest.Length)
            {
               longest = name;
            }
         }

         var width = Mathf.Max(120, longest.Length * 7 + 30);
         var wnd = BeamablePopupWindow.ShowDropdown("Filter Tag", popupWindowRect, new Vector2(width, 200), content);


         //content.OnSelected += (wrapper, name) =>
         //{
         //    wnd.Close();
         //    EditorUtility.SetDirty(Model.Sheet);
         //    wrapper.Create(name);
         //    _styleObjectElement.Refresh();
         //    VariableAddOrRemoved?.Invoke();
         //};

         content.Refresh();

      }


      private void TypeButton_OnClicked(Rect visualElementBounds)
      {
         Rect popupWindowRect = BeamablePopupWindow.GetLowerLeftOfBounds(visualElementBounds);

         var content = new TypeFilterPopupVisualElement();
         content.Model = Model;

         var longest = "";
         foreach (var name in Model.GetContentTypes().Select(t => t.TypeName))
         {
            if (name.Length > longest.Length)
            {
               longest = name;
            }
         }

         var width = Mathf.Max(120, longest.Length * 7 + 30);
         var wnd = BeamablePopupWindow.ShowDropdown("Filter Tag", popupWindowRect, new Vector2(width, 200), content);

         //content.OnSelected += (wrapper, name) =>
         //{
         //    wnd.Close();
         //    EditorUtility.SetDirty(Model.Sheet);
         //    wrapper.Create(name);
         //    _styleObjectElement.Refresh();
         //    VariableAddOrRemoved?.Invoke();
         //};

         content.Refresh();
      }

      private void StatusButton_OnClicked(Rect visualElementBounds)
      {
         Rect popupWindowRect = BeamablePopupWindow.GetLowerLeftOfBounds(visualElementBounds);

         var content = new StatusFilterPopupVisualElement();
         content.Model = Model;
         var wnd = BeamablePopupWindow.ShowDropdown("Filter Tag", popupWindowRect, new Vector2(110, 110), content);

         //content.OnSelected += (wrapper, name) =>
         //{
         //    wnd.Close();
         //    EditorUtility.SetDirty(Model.Sheet);
         //    wrapper.Create(name);
         //    _styleObjectElement.Refresh();
         //    VariableAddOrRemoved?.Invoke();
         //};

         content.Refresh();

      }
   }
}