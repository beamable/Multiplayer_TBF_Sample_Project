using Beamable.Common.Content;

namespace Beamable.Content
{
   [ContentType("skus")]
   [System.Serializable]
   public class SKUContent : ContentObject
   {
      public string description;
      public int realPrice;
      public SKUProductIds productIds;
   }

   [System.Serializable]
   public class SKUProductIds
   {
      public string googleplay;
      public string itunes;
   }

   [System.Serializable]
   public class SKURef : ContentRef<SKUContent>
   {

   }
}