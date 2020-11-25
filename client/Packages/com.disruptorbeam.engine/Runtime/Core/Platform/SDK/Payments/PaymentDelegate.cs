using UnityEngine;
using System;
using Beamable.Common;

namespace Beamable.Platform.SDK.Payments
{
   public interface PaymentDelegate
   {
      Promise<Unit> Initialize();

      /// <summary>
      /// Fetches the localized string from the provider
      /// <param name="purchaseSymbol">
      /// The purchase symbol for the item. This will end up being the skuSymbol for the offer.
      /// </param>
      /// </summary>
      string GetLocalizedPrice(string purchaseSymbol);

      /// <summary>
      /// Start the Purchase process
      /// This makes a call to platform to start the transaction process. Then it passes off to provider to do the
      /// purchase. Note: This method will always assume success in the Unity Editor with NO actual payment occuring.
      /// </summary>
      /// <param name="listingSymbol">Symbol of the listing the offer is a part of.</param>
      /// <param name="skuSymbol">The SKU we should use as the price for the offer.</param>
      /// <param name="success">Action to take if the payment is successful.</param>
      /// <param name="fail">Action to take if the payment fails.</param>
      /// <param name="cancelled">Action to take if the payment is cancelled.</param>
      /// DEPRECATED: Use StartPurchase
      void startPurchase(
         string listingSymbol,
         string skuSymbol,
         Action<CompletedTransaction> success,
         Action<ErrorCode> fail,
         Action cancelled
      );

      Promise<CompletedTransaction> StartPurchase(string listingSymbol, string skuSymbol);
   }
}
