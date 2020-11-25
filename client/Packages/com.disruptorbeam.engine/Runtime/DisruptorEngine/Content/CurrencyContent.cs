using Beamable.Common;
using Beamable.Common.Content;
using UnityEngine.AddressableAssets;

namespace Beamable.Content
{
   [ContentType("currency")]
   [System.Serializable]
   [Agnostic]
   public class CurrencyContent : ContentObject
   {
      public AssetReferenceSprite Icon;
      public ClientPermissions clientPermission;
   }

   [System.Serializable]
   [Agnostic]
   public class CurrencyRef : ContentRef<CurrencyContent>
   {

   }

   [System.Serializable]
   [Agnostic]
   public class CurrencyAmount
   {
      public int amount;
      public CurrencyRef symbol;
   }
}