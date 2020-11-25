using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Beamable.Common;
using Beamable.Common.Content;
using Beamable.Content.Validation;
using UnityEngine;

namespace Beamable.Content
{
   [ContentType("listings")]
   [System.Serializable]
   public class ListingContent : ContentObject
   {
      public ListingPrice price;
      public ListingOffer offer;
      public OptionalPeriod activePeriod;
      public OptionalInt purchaseLimit;
      public OptionalStats playerStatRequirements;
      public OptionalOffers offerRequirements;
      public OptionalDict clientData;
      public OptionalInt activeDurationSeconds;
      public OptionalInt activeDurationCoolDownSeconds;
      public OptionalInt activeDurationPurchaseLimit;
      public OptionalString buttonText;
   }

   [System.Serializable]
   public class ListingOffer
   {
      public List<string> titles;
      public List<string> descriptions;
      public List<OfferObtainCurrency> obtainCurrency;
      public List<OfferObtainItem> obtainItems;
   }

   [System.Serializable]
   [Agnostic]
   public class OfferObtainCurrency
   {
      public string symbol;
      public int amount;
   }

   public static class OfferObtainCurrencyExtensions
   {
      public static Promise<Dictionary<string, Sprite>> ResolveAllIcons(this List<OfferObtainCurrency> self)
      {
         List<Promise<CurrencyContent>> toContentPromises = self
            .Select(x => x.symbol)
            .Distinct()
            .Select(x => new CurrencyRef {Id = x}.Resolve())
            .ToList();

         return Promise.Sequence(toContentPromises)
            .Map(contentSet => contentSet.ToDictionary(
               content => content.Id,
               content => Common.PromiseExtensions.ToPromise(content.Icon.LoadAssetAsync().Task))
            ).FlatMap(dict =>
               Promise
                  .Sequence(dict.Values.ToList())
                  .Map(_ => dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.GetResult()))
            );
      }
   }

   [System.Serializable]
   public class OfferObtainItem
   {
      public string contentId;
      public List<OfferObtainItemProperty> properties;
   }

   [System.Serializable]
   public class OfferObtainItemProperty
   {
      public string name;
      public string value;
   }

   [System.Serializable]
   public class ListingPrice
   {
      public string type;
      [MustReferenceContent(false, typeof(CurrencyContent), typeof(SKUContent))]
      public string symbol;
      public int amount;
   }

   [System.Serializable]
   public class ActivePeriod
   {
        public string start;
        public string end;
   }

    [System.Serializable]
    public class StatRequirement
    {
       public string stat;
       public string constraint;
       public int value;
    }

    [System.Serializable]
    public class OfferRequirement
    {
        public string offerSymbol;
        public OfferConstraint purchases;
    }

    [System.Serializable]
    public class ContentDictionary
    {
        public List<KVPair> keyValues;
    }

    [System.Serializable]
    public class OfferConstraint
    {
        public string constraint;
        public int value;
    }

    [System.Serializable]
   public class ListingRef : ContentRef<ListingContent>
   {

   }

   [System.Serializable]
   public class ListingLink : ContentLink<ListingContent>
   {

   }
   [System.Serializable]
   public class OptionalColor : Optional<Color>
   {
      public static OptionalColor From(Color color)
      {
         return new OptionalColor {HasValue = true, Value = color};
      }
   }

   [System.Serializable]
   public class OptionalPeriod : Optional<ActivePeriod> { }

   [System.Serializable]
   public class OptionalStats : Optional<List<StatRequirement>> { }

   [System.Serializable]
   public class OptionalOffers : Optional<List<OfferRequirement>> { }

   [System.Serializable]
   public class OptionalDict : Optional<ContentDictionary> { }
}
