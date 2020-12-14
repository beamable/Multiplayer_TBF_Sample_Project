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
      private Button _validationSwitchBtn, _modificationSwitchBtn;

      private VisualElement _statusIcon;
      private Label _statusDespLabel;
      private Button _statusDespBtn1, _statusDespBtn2, _statusDespBtn3, _statusDespBtn4;
      private CountVisualElement _countElement1, _countElement2, _countElement3, _countElement4;

      private string _statusClassName = "modified"; // current, modified
      private const string CSS_HIDE_ELEMENT = "hide";

      public string Text { set; get; }

      public ContentDataModel Model { get; set; }

      public StatusBarVisualElement() : base(nameof(StatusBarVisualElement))
      {

      }

      public override void Refresh()
      {
         base.Refresh();

         _statusIcon = Root.Q<VisualElement>("status-icon");
         _statusDespLabel = Root.Q<Label>("Description");
         
         _statusDespBtn1 = Root.Q<Button>("description1");
         _statusDespBtn1.clickable.clicked += HandleInvalidOnClick;
         _statusDespBtn2 = Root.Q<Button>("description2");
         _statusDespBtn2.clickable.clicked += HandleCreatedOnClick;
         _statusDespBtn3 = Root.Q<Button>("description3");
         _statusDespBtn3.clickable.clicked += HandleModifiedOnClick;
         _statusDespBtn4 = Root.Q<Button>("description4");
         _statusDespBtn4.clickable.clicked += HandleDeletedOnClick;
         _countElement1 = Root.Q<CountVisualElement>("count1");
         _countElement2 = Root.Q<CountVisualElement>("count2");
         _countElement3 = Root.Q<CountVisualElement>("count3");
         _countElement4 = Root.Q<CountVisualElement>("count4");


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
         _statusIcon.RemoveFromClassList(_statusClassName);
         
         // check any difference
         var anyInvalid = Model.CountInValid() > 0;
         var anyDeleted = Model.CountDeleted() > 0;
         var anyCreated = Model.CountCreated() > 0;
         var anyModified = Model.CountModified() > 0;

         if (anyInvalid || anyCreated || anyModified || anyDeleted)
         {
            _statusDespLabel.text = "";
            if (anyInvalid)
            {
               _countElement1.RemoveFromClassList(CSS_HIDE_ELEMENT);
               _countElement1.SetValue(Model.CountInValid());
               _statusDespBtn1.RemoveFromClassList(CSS_HIDE_ELEMENT);
            }
            else
            {
               _countElement1.AddToClassList(CSS_HIDE_ELEMENT);
               _statusDespBtn1.AddToClassList(CSS_HIDE_ELEMENT);
            }
            
            if (anyCreated)
            {
               _countElement2.RemoveFromClassList(CSS_HIDE_ELEMENT);
               _countElement2.SetValue(Model.CountCreated());
               _statusDespBtn2.RemoveFromClassList(CSS_HIDE_ELEMENT);
            }
            else
            {
               _countElement2.AddToClassList(CSS_HIDE_ELEMENT);
               _statusDespBtn2.AddToClassList(CSS_HIDE_ELEMENT);
            }
            
            if (anyModified)
            {
               _countElement3.RemoveFromClassList(CSS_HIDE_ELEMENT);
               _countElement3.SetValue(Model.CountModified());
               _statusDespBtn3.RemoveFromClassList(CSS_HIDE_ELEMENT);
            }
            else
            {
               _countElement3.AddToClassList(CSS_HIDE_ELEMENT);
               _statusDespBtn3.AddToClassList(CSS_HIDE_ELEMENT);
            }
            
            if (anyDeleted)
            {
               _countElement4.RemoveFromClassList(CSS_HIDE_ELEMENT);
               _countElement4.SetValue(Model.CountDeleted());
               _statusDespBtn4.RemoveFromClassList(CSS_HIDE_ELEMENT);
            }
            else
            {
               _countElement4.AddToClassList(CSS_HIDE_ELEMENT);
               _statusDespBtn4.AddToClassList(CSS_HIDE_ELEMENT);
            }
            _statusClassName = "modified";
         }
         else
         {
            _statusDespLabel.text = "All data has synced with server";
            _countElement1.AddToClassList(CSS_HIDE_ELEMENT);
            _statusDespBtn1.AddToClassList(CSS_HIDE_ELEMENT);
            _countElement2.AddToClassList(CSS_HIDE_ELEMENT);
            _statusDespBtn2.AddToClassList(CSS_HIDE_ELEMENT);
            _countElement3.AddToClassList(CSS_HIDE_ELEMENT);
            _statusDespBtn3.AddToClassList(CSS_HIDE_ELEMENT);
            _countElement4.AddToClassList(CSS_HIDE_ELEMENT);
            _statusDespBtn4.AddToClassList(CSS_HIDE_ELEMENT);
            _statusClassName = "current";
         }
         
         _statusIcon.AddToClassList(_statusClassName);
      }

      private void HandleInvalidOnClick()
      {
         // Show invalid
         Model.ToggleValidationFilter(ContentValidationStatus.VALID, false);
         Model.ToggleValidationFilter(ContentValidationStatus.INVALID, true);
         
         Model.ToggleStatusFilter(ContentModificationStatus.LOCAL_ONLY, true);
         Model.ToggleStatusFilter(ContentModificationStatus.MODIFIED, true);
         Model.ToggleStatusFilter(ContentModificationStatus.SERVER_ONLY, true);
      }
      
      private void HandleCreatedOnClick()
      {
         Model.ToggleValidationFilter(ContentValidationStatus.VALID, false);
         Model.ToggleValidationFilter(ContentValidationStatus.INVALID, false);
         // Show Created
         Model.ToggleStatusFilter(ContentModificationStatus.LOCAL_ONLY, true);
         Model.ToggleStatusFilter(ContentModificationStatus.MODIFIED, false);
         Model.ToggleStatusFilter(ContentModificationStatus.SERVER_ONLY, false);
      }
      
      private void HandleModifiedOnClick()
      {
         Model.ToggleValidationFilter(ContentValidationStatus.VALID, false);
         Model.ToggleValidationFilter(ContentValidationStatus.INVALID, false);
         // Show Modified
         Model.ToggleStatusFilter(ContentModificationStatus.LOCAL_ONLY, false);
         Model.ToggleStatusFilter(ContentModificationStatus.MODIFIED, true);
         Model.ToggleStatusFilter(ContentModificationStatus.SERVER_ONLY, false);
      }
      
      private void HandleDeletedOnClick()
      {
         Model.ToggleValidationFilter(ContentValidationStatus.VALID, false);
         Model.ToggleValidationFilter(ContentValidationStatus.INVALID, false);
         // Show Deleted
         Model.ToggleStatusFilter(ContentModificationStatus.LOCAL_ONLY, false);
         Model.ToggleStatusFilter(ContentModificationStatus.MODIFIED, false);
         Model.ToggleStatusFilter(ContentModificationStatus.SERVER_ONLY, true);
      }
   }
}