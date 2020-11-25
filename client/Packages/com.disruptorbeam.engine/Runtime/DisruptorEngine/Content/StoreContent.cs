using System.Collections.Generic;
using Beamable.Common.Content;

namespace Beamable.Content
{
   [ContentType("stores")]
   [System.Serializable]
   public class StoreContent : ContentObject
   {
      public string title;
      public List<ListingLink> listings;
   }

   [System.Serializable]
   public class StoreRef : ContentRef<StoreContent>
   {

   }
}