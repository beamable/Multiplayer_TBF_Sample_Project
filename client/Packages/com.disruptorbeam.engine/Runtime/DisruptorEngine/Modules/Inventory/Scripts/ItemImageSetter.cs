
using Beamable.Content;
using Beamable.Modules.Inventory.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Beamable.Modules.Inventory
{
    public class ItemImageSetter : MonoBehaviour
    {
        public Image Image;
        public ItemRef Item;

        private void Start()
        {
            Refresh();
        }

        public void SetItem(ItemRef item)
        {
            Item = item;
            Refresh();
        }

        public void SetItem(InventoryEventArgs args)
        {
            Item = args.Group.ItemRef;
            Refresh();
        }

        public void Refresh()
        {
            if (Image == null || Item == null) return;
            Item.Resolve().Then(async content =>
            {
                if (content.Icon == null) return;
                var sprite = await content.Icon.LoadAssetAsync().Task;
                Image.sprite = sprite;
            }).Error(err => Debug.LogError(err));
        }

    }
}
