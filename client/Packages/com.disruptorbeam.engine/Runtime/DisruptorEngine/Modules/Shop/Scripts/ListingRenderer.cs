using Beamable.Platform.SDK.Payments;
using UnityEngine;

namespace Beamable.Modules.Shop
{
   public abstract class ListingRenderer : MonoBehaviour
   {
      public abstract void RenderListing(PlayerStoreView store, PlayerListingView listing);
   }
}