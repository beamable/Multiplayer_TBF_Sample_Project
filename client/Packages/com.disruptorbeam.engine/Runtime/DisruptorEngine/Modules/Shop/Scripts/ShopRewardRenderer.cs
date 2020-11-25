using Beamable.UI.Scripts;
using Beamable.Platform.SDK.Payments;
using Beamable.Signals;
using UnityEngine;

namespace Beamable.Modules.Shop
{
   public class ShopRewardRenderer : MenuBase
   {
      public ObtainRenderer ObtainRenderer;
      public GameObject Frame;

      public PlayerListingView Listing;

      void Start()
      {
         Frame.SetActive(false);
      }

      public override void OnOpened()
      {
         Frame.SetActive(true);

         ObtainRenderer.RenderObtain(Listing);
      }
   }
}