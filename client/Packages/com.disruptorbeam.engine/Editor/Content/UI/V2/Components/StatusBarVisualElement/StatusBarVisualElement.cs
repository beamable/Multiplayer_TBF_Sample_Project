using System;
using System.Collections.Generic;
using Beamable.Editor.Content.Models;
using Beamable.Editor.UI.Buss;
using UnityEngine;
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
   public class StatusBarVisualElement : ContentManagerComponent
   {
      public new class UxmlFactory : UxmlFactory<StatusBarVisualElement, UxmlTraits> { }
      public new class UxmlTraits : VisualElement.UxmlTraits
      {
         UxmlStringAttributeDescription customText = new UxmlStringAttributeDescription { name = "custom-text", defaultValue = "nada" };

         public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
         {
            get { yield break; }
         }
         public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
         {
            base.Init(ve, bag, cc);
            var self = ve as StatusBarVisualElement;
         }
      }

//      private VisualElement _mainVisualElement;
//      private Image _testImage01;
//      private Image _testImage02;
//      private Image _testImage03;
//      private VisualElement _testVisualElement04;
//      private VisualElement _testVisualElement05;
//      private VisualElement _testVisualElement06;
      private VisualElement _statusIcon;
      private VisualElement _statusColorBar;
      private Label _statusDescriptionLabel;
      private Label _statusDespLabel2, _statusDespLabel3, _statusDespLabel4;
      private CountVisualElement _countElement1, _countElement2, _countElement3;
      private Button _viewButton;

      private string _statusClassName = "current"; // current, modified,

      public string Text { set; get; }

      public ContentDataModel Model { get; set; }

      public StatusBarVisualElement() : base(nameof(StatusBarVisualElement))
      {

      }

      public override void Refresh()
      {
         base.Refresh();

         _statusIcon = Root.Q<VisualElement>("status-icon");
         _statusColorBar = Root.Q<VisualElement>("status-color-bar");
         _statusDescriptionLabel = Root.Q<Label>("description");
         _statusDespLabel2 = Root.Q<Label>("description2");
         _statusDespLabel3 = Root.Q<Label>("description3");
         _statusDespLabel4 = Root.Q<Label>("description4");
         _countElement1 = Root.Q<CountVisualElement>("count1");
         _countElement2 = Root.Q<CountVisualElement>("count2");
         _countElement3 = Root.Q<CountVisualElement>("count3");
         _viewButton = Root.Q<Button>("view-btn");
         _viewButton.clickable.clicked += HandleViewOnClick;


         Model.OnItemEnriched += Model_OnItemEnriched;
         RefreshStatus();
//         _mainVisualElement = Root.Q<VisualElement>("mainVisualElement");

//         _testImage01 = Root.Q<Image>("testImage01");
//         _testImage02 = Root.Q<Image>("testImage02");
//         _testImage03 = Root.Q<Image>("testImage03");
//         _testVisualElement04 = Root.Q<VisualElement>("testVisualElement04");
//         _testVisualElement05 = Root.Q<VisualElement>("testVisualElement05");
//         _testVisualElement06 = Root.Q<VisualElement>("testVisualElement06");
//
//         var iconPath01 = "SampleImage";
//         var iconAsset01 = Resources.Load<Texture2D>(iconPath01);
//         _testImage01.image = iconAsset01;
//
//         var iconPath02 = "SampleImage";
//         var iconAsset02 = Resources.Load<Texture2D>(iconPath02);
//         _testImage02.image = iconAsset02;
//
//         var iconPath03 = "SampleImage";
//         var iconAsset03 = Resources.Load<Texture2D>(iconPath03);
//         _testImage03.image = iconAsset03;

      }

      private void Model_OnItemEnriched(ContentItemDescriptor obj)
      {
         RefreshStatus();
      }

      private void RefreshStatus()
      {
         // Set status
         var anyDeleted = Model.CountDeleted() > 0;
         var anyCreated = Model.CountCreated() > 0;
         var anyModified = Model.CountModified() > 0;

         // TODO: Validation error to be added
         if (anyCreated || anyModified || anyDeleted)
            _statusClassName = "modified";
         else
            _statusClassName = "current";

         switch (_statusClassName)
         {
            case "current":
               _statusDescriptionLabel.text = "All update was upload to the cloud. ";
               _countElement1.visible = false;
               _statusDespLabel2.text = "";
               _countElement2.visible = false;
               _statusDespLabel3.text = "";
               _countElement3.visible = false;
               _statusDespLabel4.text = "";
               break;
            case "modified":
               _statusDescriptionLabel.text = "";
               _countElement1.visible = true;
               _countElement1.SetValue(Model.CountCreated());
               _statusDespLabel2.text = "created,";
               _countElement1.visible = true;
               _countElement2.SetValue(Model.CountModified());
               _statusDespLabel3.text = "modified,";
               _countElement3.visible = true;
               _countElement3.SetValue(Model.CountDeleted());
               _statusDespLabel4.text = "deleted";
               break;
            case "error":
               _statusDescriptionLabel.text = "";
               _countElement1.visible = true;
               _countElement1.SetValue(10);
               _statusDespLabel2.text = "validation error, please check before publishing.";
               _countElement2.visible = false;
               _statusDespLabel3.text = "";
               _countElement3.visible = false;
               _statusDespLabel4.text = "";
               break;
         }
         //_statusIcon.RemoveFromClassList(_statusClassName);
         _statusIcon.AddToClassList(_statusClassName);
         //_statusColorBar.RemoveFromClassList(_statusClassName);
         _statusColorBar.AddToClassList(_statusClassName);
         //_viewButton.RemoveFromClassList(_statusClassName);
         _viewButton.AddToClassList(_statusClassName);
      }

      private void HandleViewOnClick()
      {
         switch (_statusClassName)
         {
            case "modified":
               // not_modified ie. sync as false
               Model.ToggleStatusFilter(ContentModificationStatus.LOCAL_ONLY, true);
               Model.ToggleStatusFilter(ContentModificationStatus.MODIFIED, true);
               Model.ToggleStatusFilter(ContentModificationStatus.SERVER_ONLY, true);
               Model.ToggleStatusFilter(ContentModificationStatus.NOT_AVAILABLE_ANYWHERE, false);
               Model.ToggleStatusFilter(ContentModificationStatus.NOT_MODIFIED, false);
               break;
            case "error":
               // TODO: set later
               break;
         }
      }
   }
}