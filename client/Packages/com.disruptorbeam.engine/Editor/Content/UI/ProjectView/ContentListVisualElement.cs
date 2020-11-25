using System;
using System.Collections.Generic;
using Beamable.Content;
using UnityEditor;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace Beamable.Editor.Content.UI.ProjectView
{
   public class ContentListVisualElement : VisualElement
   {
      private const string Asset_UXML_ContentListElement =
         "Packages/com.disruptorbeam.engine/Editor/Content/UI/ProjectView/contentListElement.uxml";

      private const string Asset_USS_ContentListElement =
         "Packages/com.disruptorbeam.engine/Editor/Content/UI/ProjectView/contentListElement.uss";

      private VisualTreeAsset _treeAsset;
      private VisualElement _typeList;
      private TextField _searchField;

      private Dictionary<Type, ContentGroupVisualElement> _typeToGroup = new Dictionary<Type, ContentGroupVisualElement>();
      private ContentQuery _query;

      public ContentListVisualElement()
      {
         _treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Asset_UXML_ContentListElement);
         var mainNode = _treeAsset.CloneTree();

         _typeList = mainNode.Q<VisualElement>(name = "content-type-list");
         _searchField = mainNode.Q<TextField>(name = "filter-text");

         this.AddStyleSheet(Asset_USS_ContentListElement);
         Add(mainNode);

         _typeList.RegisterCallback<MouseUpEvent>(HandleRightClick);
         _typeList.RegisterCallback<MouseDownEvent>(HandleLeftClick);
         _searchField.RegisterCallback<KeyDownEvent>(TriggerFromKeyboard);
         _searchField.RegisterCallback<BlurEvent>(TriggerFromBlur);
         Refresh();
      }

      private void TriggerFromBlur(BlurEvent evt)
      {
         TriggerSearch();
      }

      private void TriggerFromKeyboard(KeyDownEvent evt)
      {
         if (evt.keyCode == KeyCode.Return)
         {
            TriggerSearch();
         }
      }

      private void TriggerSearch()
      {
         var filter = _searchField.value;
         _query = ContentQuery.Parse(filter);
         FilterRefresh();
      }

      private void HandleLeftClick(MouseDownEvent evt)
      {
         if (evt.button != (int)MouseButton.LeftMouse)
            return;

         Selection.SetActiveObjectWithContext(null, null);
      }

      private async void HandleRightClick(MouseUpEvent evt)
      {
         if (evt.button != (int)MouseButton.RightMouse)
            return;

         var targetElement = evt.target as VisualElement;
         if (targetElement == null)
            return;

         var de = await EditorAPI.Instance;

         var menu = new GenericMenu();

         foreach (var contentType in de.ContentIO.GetContentTypes())
         {
            var typeName = ContentObject.GetContentTypeName(contentType);
            menu.AddItem(new GUIContent($"Content/{typeName}"), false, () =>
            {
               var group = _typeToGroup[contentType];
               group.CreateNew();
            });
         }

         menu.AddSeparator("");

         // do we have a selection?
         var selection = Selection.activeObject as ContentObject;
         if (selection != null)
         {
            menu.AddItem(new GUIContent("Rename"), false, () =>
            {
               var item = _typeList.Q<ContentItemVisualElement>(name = $"content_{selection.Id}");
               item.BeginNaming().Then(name =>
               {

                  de.ContentIO.Rename(selection, name);
               });
               item.Refresh();
            });
            menu.AddItem(new GUIContent("Delete"), false, () =>
            {
               var group = _typeToGroup[selection.GetType()];
               group.Delete(selection);
            });
         }
         else
         {
            menu.AddDisabledItem(new GUIContent("Rename"));
            menu.AddDisabledItem(new GUIContent("Delete"));
         }


         // Get position of menu on top of target element.
         var menuPosition = evt.localMousePosition + new Vector2(0, -10);
         menuPosition = this.LocalToWorld(menuPosition);
         var menuRect = new Rect(menuPosition, Vector2.zero);

         menu.DropDown(menuRect);
      }

      public async void FilterRefresh()
      {
         var de = await EditorAPI.Instance;
         var contentTypes = de.ContentIO.GetContentTypes();
         _typeToGroup.Clear();
         _typeList.Clear();
         foreach (var contentType in contentTypes)
         {
            if (!(_query?.AcceptType(contentType) ?? true))
            {
               continue;
            }

            var type = typeof(ContentGroupVisualElement<>).MakeGenericType(contentType);
            var groupInstance = Activator.CreateInstance(type) as ContentGroupVisualElement;
            groupInstance.ContentQuery = _query;
            groupInstance.Refresh();
            _typeToGroup.Add(contentType, groupInstance);
            _typeList.Add(groupInstance);
         }
      }

      public async void Refresh()
      {
         var de = await EditorAPI.Instance;
         _ = de.ContentIO.FetchManifest();
         FilterRefresh();
      }
   }
}