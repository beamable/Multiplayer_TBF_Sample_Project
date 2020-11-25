using System;
using Beamable.Common;
using Beamable.Content;
using Beamable.Content.Validation;
using Beamable.Platform.SDK;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
#endif

namespace Beamable.Editor.Content.UI.ProjectView
{

   public abstract class ContentItemVisualElement : VisualElement
   {
      public abstract void Refresh();
      public abstract Promise<string> BeginNaming();
   }

   public delegate void ContentItemVisualElementEvent();

   public class ContentItemVisualElement<TContent> : ContentItemVisualElement where TContent: ContentObject, new()
   {
      private const string Asset_UXML_ContentItemElement =
         "Packages/com.disruptorbeam.engine/Editor/Content/UI/ProjectView/contentItemElement.uxml";

      private const string Asset_USS_ContentItemElement =
         "Packages/com.disruptorbeam.engine/Editor/Content/UI/ProjectView/contentItemElement.uss";


      private readonly VisualElement _main;
      private readonly Label _title;
      private readonly TextField _textfield;
      private readonly VisualElement _statusBar;
      private readonly VisualElement _statusIcon;

      public TContent Content { get; set; }
      private TContent _lastContent;

      public event ContentItemVisualElementEvent OnClicked;

      private bool IsNaming => _namingPromise != null;

      private Promise<string> _namingPromise;
      private Promise<EditorAPI> _de;
      private string _statusClassName = "current";

      public ContentItemVisualElement()
      {
         var treeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(Asset_UXML_ContentItemElement);
         _main = treeAsset.CloneTree();

         _title = _main.Q<Label>();
         _textfield = _main.Q<TextField>();
         _statusIcon = _main.Q<VisualElement>(name = "status-icon");
         //_statusBar = _main.Q<VisualElement>(name = "status-bar");

         this.AddStyleSheet(Asset_USS_ContentItemElement);
         Add(_main);

         _main.RegisterCallback<MouseDownEvent>(HandleOnClicked);

         _textfield.RegisterCallback<BlurEvent>(SetName);
         _textfield.RegisterCallback<KeyUpEvent>(evt =>
         {
            CheckName();
         });
         _textfield.RegisterCallback<KeyDownEvent>(evt =>
            {
               if (evt.keyCode == KeyCode.Return)
               {
                  SetName(evt);
               }
            }
         );
         _de = EditorAPI.Instance;
         _de.Then(de => { de.ContentIO.OnSelectionChanged += SelectionCallback; });

         _main.Q<Button>("edit").clickable.clicked += () => {
             var selection = _lastContent;
             if (selection == null)
             {
                 selection = Content;
             }
             BeginNaming().Then(name =>
             {
                 _de.Then(de =>
                 {
                     de.ContentIO.Rename(selection, name);
                 });
             });
             Refresh();
         };

          _main.Q<Button>("deleted").clickable.clicked += () => {
              _de.Then(de =>
             {
                 Debug.Log(_lastContent);
                 if (Content != null)
                 {
                     de.ContentIO.Delete(Content);
                     Refresh();
                 }
             });
          };

          Refresh();
      }

      public void CheckName()
      {
         var name = _textfield.value;
         if (ContentNameValidationException.HasNameValidationErrors(Content, name, out var errors))
         {
            foreach (var error in errors)
            {
               var replaceWith = error.InvalidChar == ' ' ? "_" : "";
               name = name.Replace(error.InvalidChar.ToString(), replaceWith);
            }
         }

         _textfield.value = name;
      }
      public override Promise<string> BeginNaming()
      {
         if (_namingPromise != null)
         {
            return _namingPromise;
         }

         _namingPromise = new Promise<string>();
         Refresh();
         return _namingPromise.Error(err =>
         {
            Refresh();
         });
      }

      private void SelectionCallback(ContentObject selectionAsContent)
      {
         var active = selectionAsContent != null && selectionAsContent.Id.Equals(Content.Id);

         if (active)
         {
            AddToClassList("selected");
         }
         else
         {
            RemoveFromClassList("selected");
         }
      }


      private void OnContentUpdated(ContentObject content)
      {
         UpdateStatus();
         UpdateName();
         _main.MarkDirtyRepaint();
      }

      public override void Refresh()
      {
         UpdateName();
         UpdateStatus();


         if (_lastContent != null)
         {
            _lastContent.OnChanged -= OnContentUpdated;
         }

         if (Content != null)
         {
            Content.OnChanged += OnContentUpdated;
         }
         _lastContent = Content;

         if (IsNaming)
         {
            _textfield.RemoveFromClassList("hide");
            _title.AddToClassList("hide");

             _textfield.BeamableFocus();
         }
         else
         {
            _title.RemoveFromClassList("hide");
            _textfield.AddToClassList("hide");
         }
      }

      private void HandleOnClicked(MouseDownEvent evt)
      {
         OnClicked?.Invoke();
         evt.StopPropagation();
      }

      private void UpdateName()
      {
         var contentName = Content != null ? Content.ContentName : "not found";
         name = Content != null ? $"content_{Content.Id}" : "content_null";

         _title.text = contentName;
         _textfield.value = contentName;
      }

      private void UpdateStatus()
      {
         if (Content == null)
         {
            return;
         }

         _de.Then(de =>
         {
            _statusIcon.RemoveFromClassList(_statusClassName);
            //_statusBar.RemoveFromClassList(_statusClassName);
            de.ContentIO.GetStatus(Content).Then(status =>
            {
               _statusClassName = status.ToString().ToLowerInvariant();
               _statusIcon.AddToClassList(_statusClassName);
               //_statusBar.AddToClassList(_statusClassName);
            });
         });

      }

      private void SetName(EventBase evt)
      {
         if (_namingPromise == null)
         {
            return;
         }

         var promise = _namingPromise;
         _namingPromise = null;


         try
         {
            if (string.IsNullOrEmpty(_textfield.value))
            {
               throw new Exception("Content name cant be empty");
            }
            promise.CompleteSuccess(_textfield.value);
            Refresh();
         }
         catch (Exception ex)
         {
            Debug.LogWarning(ex);
            promise.CompleteError(new ErrorCode(0, GameSystem.GAME_CLIENT));
         }
      }
   }
}