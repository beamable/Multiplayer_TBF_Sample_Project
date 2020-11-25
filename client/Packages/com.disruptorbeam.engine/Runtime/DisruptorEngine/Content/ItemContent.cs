using Beamable.Common.Content;
using UnityEngine.AddressableAssets;

namespace Beamable.Content
{
    [ContentType("items")]
    [System.Serializable]
    public class ItemContent : ContentObject
    {
        public AssetReferenceSprite Icon;
        public ClientPermissions clientPermission;
    }

    [System.Serializable]
    public class ItemRef : ContentRef<ItemContent>
    {

    }
}