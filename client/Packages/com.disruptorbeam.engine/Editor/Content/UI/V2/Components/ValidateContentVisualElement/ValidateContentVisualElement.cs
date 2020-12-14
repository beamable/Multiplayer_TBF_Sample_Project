
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common.Content.Validation;
using Beamable.Content.Validation;
using Beamable.Editor.Content.Models;
using Beamable.Editor.UI.Components;
using UnityEngine;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.Content.Components
{
   public class ValidateContentVisualElement : ContentManagerComponent
   {

      public ContentDataModel DataModel { get; set; }

      public event Action OnCancelled;
      public event Action OnClosed;
      private LoadingBarVisualElement _progressBar;
      private Label _messageLbl;
      private Button _detailButton;

      private CountVisualElement _errorObjectCountElement;
      private CountVisualElement _errorCountElement;

      private ListView _listView;
      private IList _listSource = new List<ContentExceptionCollection>();
      private VisualElement _errorContainer;
      private Button _okayButton;
      private Button _cancelButton;

      public ValidateContentVisualElement() : base(nameof(ValidateContentVisualElement))
      {
      }

      public override void Refresh()
      {
         base.Refresh();
         _progressBar = Root.Q<LoadingBarVisualElement>();
         _progressBar.Value = 0;

         _messageLbl = Root.Q<Label>("message");
         _messageLbl.text = ContentManagerContants.ValidateStartMessage;

         // _detailButton = Root.Q<Button>("detailBtn");
         // _detailButton.clickable.clicked += DetailButton_OnClicked;

         _errorObjectCountElement = Root.Q<CountVisualElement>("errorObjectCount");
         _errorCountElement = Root.Q<CountVisualElement>("errorCount");
         UpdateErrorCount();

         _okayButton = Root.Q<Button>("okayBtn");
         _okayButton.text = ContentManagerContants.ValidateButtonStartText;
         _okayButton.clickable.clicked += OkayButton_OnClicked;
         _okayButton.SetEnabled(false);


         _cancelButton = Root.Q<Button>("cancelBtn");
         _cancelButton.clickable.clicked += CancelButton_OnClicked;

         _errorContainer = Root.Q<VisualElement>("errorContainer");
         _listView = new ListView(_listSource, 24, CreateListItem, BindListItem);
         _errorContainer.Add(_listView);
      }

      private void UpdateErrorCount()
      {
         _errorObjectCountElement.SetValue(_listSource.Count);

         int totalErrorCount = 0;
         foreach (var exceptionObj in _listSource)
         {
            var exceptionCollection = exceptionObj as ContentExceptionCollection;
            totalErrorCount += exceptionCollection.Exceptions.Count();
         }
         _errorCountElement.SetValue(totalErrorCount);
      }

      private void DetailButton_OnClicked()
      {
         DataModel.ToggleValidationFilter(ContentValidationStatus.INVALID, true);
      }

      private void OkayButton_OnClicked()
      {
         if (_listSource.Count != 0)
         {
            // TODO: set the filter to show the invalid content.
            DataModel.SetFilter("valid:n");
         }

         OnClosed?.Invoke();
      }

      private void CancelButton_OnClicked()
      {
         OnCancelled?.Invoke();
      }

      ContentValidationErrorVisualElement CreateListItem()
      {
         return new ContentValidationErrorVisualElement();
      }

      void BindListItem(VisualElement element, int index)
      {
         var view = element as ContentValidationErrorVisualElement;
         var data = _listSource[index] as ContentExceptionCollection;

         if (view == null || data == null)
         {
            Debug.LogWarning("Validation ListView binding content incorrectly.");
            return;
         }

         view.ExceptionCollection = data;
         view.Refresh();
      }

      public void SetProgress(float progress)
      {
         Debug.Log("validation progress " + progress);
         _progressBar.Value = progress;
      }

      public void HandleValidationErrors(ContentExceptionCollection errors)
      {
         _messageLbl.text = ContentManagerContants.ValidateProgressMessage;

         _listSource.Add(errors);
         _listView.Refresh();
      }

      public void HandleFinished()
      {
         _messageLbl.text = ContentManagerContants.ValidationCompleteMessage;

         _okayButton.text = _listSource.Count > 0
            ? ContentManagerContants.ValidateButtonDoneWithErrorsText
            : ContentManagerContants.ValidateButtonDoneWithoutErrorsText;

         UpdateErrorCount();

         _okayButton.SetEnabled(true);

      }
   }
}