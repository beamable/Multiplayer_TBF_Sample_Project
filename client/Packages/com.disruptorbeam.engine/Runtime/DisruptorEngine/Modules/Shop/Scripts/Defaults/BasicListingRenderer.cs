using Beamable.Api.Payments;
using TMPro;
using UnityEngine;
using System;
using Beamable.Content;
using Beamable.Signals;

namespace Beamable.Modules.Shop.Defaults
{
   public class BasicListingRenderer : ListingRenderer
   {
      public TextMeshProUGUI Title;
      public TextMeshProUGUI Description;
      public TextMeshProUGUI ButtonText;
      public GameObject ObtainLayout;

      private PlayerListingView listing;
      private PlayerStoreView store;

      public override async void RenderListing (PlayerStoreView store, PlayerListingView listing)
      {
         var config = ShopConfiguration.Instance;

         // Basic info
         this.listing = listing;
         this.store = store;
         if (listing.offer.titles.Count > 0)
         {
            Title.text = listing.offer.titles[0];
         }

         if (listing.offer.descriptions.Count > 0)
         {
            Description.text = listing.offer.descriptions[0];
         }

         // Obtain
         var obtainRenderer = Instantiate(config.ObtainRenderer, ObtainLayout.transform);
         obtainRenderer.RenderObtain(listing);

         // RMT Price
         var de = await API.Instance;
         if (listing.offer.price.type == "sku")
         {
            var paymentDelegate = await de.PaymentDelegate;
            ButtonText.text = paymentDelegate.GetLocalizedPrice(listing.offer.price.symbol);
         }
         else
         {
            var contentRef = new ContentRef<CurrencyContent>();
            contentRef.Id = listing.offer.price.symbol;
            var currency = await de.ContentService.GetContent<CurrencyContent>(contentRef);
            ButtonText.text = listing.offer.price.amount + " " + currency.name;
         }
      }

      public async void Buy()
      {
         var de = await API.Instance;
         var paymentDelegate = await de.PaymentDelegate;

         switch (listing.offer.price.type)
         {
            case "sku":
               await paymentDelegate.StartPurchase($"{listing.symbol}:{store.symbol}", listing.offer.price.symbol);
               break;
            case "currency":
               await de.Commerce.Purchase(store.symbol, listing.symbol);
               break;
            default:
               throw new Exception("Unknown price type: " + listing.offer.price.type);
         }
         DeSignalTower.ForAll<ShopSignals>(s => s.OnPurchase?.Invoke(listing));
      }
   }
}