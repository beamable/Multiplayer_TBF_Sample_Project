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
    public class CountVisualElement:ContentManagerComponent
    {
        private Label _countLabel;

        public new class UxmlFactory : UxmlFactory<CountVisualElement, CountVisualElement.UxmlTraits>
        {
        }
        public CountVisualElement():base(nameof(CountVisualElement))
        {
        }

        public override void Refresh()
        {
            base.Refresh();
            _countLabel = Root.Q<Label>();
        }

        public void SetValue(int count)
        {
            _countLabel.text = count.ToString();
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
                var self = ve as CountVisualElement;
                self.Refresh();
            }
        }
    }
}
