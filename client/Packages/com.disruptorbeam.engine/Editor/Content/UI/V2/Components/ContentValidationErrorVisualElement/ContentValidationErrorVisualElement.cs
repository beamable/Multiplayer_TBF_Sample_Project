using System.Collections.Generic;
using Beamable.Content.Validation;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif
namespace Beamable.Editor.Content.Components
{
   public class ContentValidationErrorVisualElement : ContentManagerComponent
   {
      public new class UxmlFactory : UxmlFactory<ContentValidationErrorVisualElement, UxmlTraits>
      {
      }

      public override void Refresh()
      {
         base.Refresh();
         
         var contentId = Root.Q<Label>("contentId");
         var count = Root.Q<CountVisualElement>();
         count.SetValue(ExceptionCollection.Exceptions.Count);
         contentId.text = ExceptionCollection.Content.Id;
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
            var self = ve as ContentValidationErrorVisualElement;
         }
      }

      public ContentExceptionCollection ExceptionCollection { get; set; }

      public ContentValidationErrorVisualElement() : base(nameof(ContentValidationErrorVisualElement))
      {
      }
   }
}