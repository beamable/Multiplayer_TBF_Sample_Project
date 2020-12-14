using Beamable.Content;
using Beamable.Api.Payments;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Modules.Shop.Defaults
{
    public class ObtainSummaryListItemRenderer : MonoBehaviour
    {
        public RawImage Icon;
        public TextMeshProUGUI Name;

        public async void RenderObtainItem(ObtainItem data)
        {
            Name.text = data.contentId.Split('.')[1];

            var contentRef = new ItemRef();
            contentRef.Id = data.contentId;
            var item = await contentRef.Resolve();
            var icon = await item.Icon.LoadAssetAsync().Task;
            Icon.texture = icon.texture;
        }
    }
}