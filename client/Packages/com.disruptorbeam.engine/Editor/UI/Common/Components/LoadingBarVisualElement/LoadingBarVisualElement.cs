
using System.Collections.Generic;
using Beamable.Editor.Content.Models;
using Beamable.Editor.UI.Buss;
using Beamable.Platform.SDK;
using UnityEditor;
#if UNITY_2018
using UnityEngine.Experimental.UIElements;
using UnityEditor.Experimental.UIElements;
#elif UNITY_2019_1_OR_NEWER
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Beamable.Editor.UI.Components
{
   public class LoadingBarVisualElement : BeamableVisualElement
   {
      public LoadingBarVisualElement() : base($"{BeamableComponentsConstants.UI_PACKAGE_PATH}/Common/Components/{nameof(LoadingBarVisualElement)}/{nameof(LoadingBarVisualElement)}")
      {
      }

      public new class UxmlFactory : UxmlFactory<LoadingBarVisualElement, UxmlTraits> { }
      public new class UxmlTraits : VisualElement.UxmlTraits
      {
//         UxmlStringAttributeDescription loadingText = new UxmlStringAttributeDescription { name = "text", defaultValue = "Loading" };

         public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
         {
            get { yield break; }
         }
         public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
         {
            base.Init(ve, bag, cc);
            var self = ve as LoadingBarVisualElement;

            self.Refresh();
         }
      }

      private float _value;
      private VisualElement _progressElement, _spaceElement;

      public float Value
      {
         get => _value;
         set
         {
            _value = value;
            UpdateSize();
         }
      }

      public override void Refresh()
      {
         base.Refresh();
         _progressElement = Root.Q<VisualElement>("progress");
         _spaceElement = Root.Q<VisualElement>("space");

         _value = 0;
         UpdateSize();
      }

      void UpdateSize()
      {
         EditorApplication.delayCall += () =>
         {
            _progressElement.style.flexGrow = _value;
            _spaceElement.style.flexGrow = 1 - _value;
            MarkDirtyRepaint();
         };
      }
   }
}