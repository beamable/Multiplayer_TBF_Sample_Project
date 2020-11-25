using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common.Pooling;
using Beamable.Serialization.SmallerJSON;

namespace Beamable.Common.Api.Inventory
{
   public abstract class AbsInventoryApi : IInventoryApi
   {
      public const string SERVICE_OBJECT = "/object/inventory";

      public IBeamableRequester Requester { get; }
      public IUserContext UserContext { get; }

      public AbsInventoryApi(IBeamableRequester requester, IUserContext userContext)
      {
         Requester = requester;
         UserContext = userContext;
      }

      public Promise<Unit> AddItem(string contentId, Dictionary<string, string> properties, string transaction = null)
      {
         var dbid = UserContext.UserId;
         using (var pooledBuilder = StringBuilderPool.StaticPool.Spawn())
         {
            var dict = new ArrayDict();
            dict.Add("transaction", transaction ?? Guid.NewGuid().ToString());
            dict.Add("newItems", new [] { new ArrayDict
            {
               {"contentId", contentId},
               {"properties", properties.Select(kvp => new ArrayDict
               {
                  {"name", kvp.Key},
                  {"value", kvp.Value}
               }).ToArray()}
            }});
            var json = Json.Serialize(dict, pooledBuilder.Builder);
            return Requester.Request<EmptyResponse>(Method.PUT, $"{SERVICE_OBJECT}/{dbid}", json).ToUnit();
         }
      }

      public Promise<Unit> SetCurrency(string currencyId, long amount, string transaction)
      {
         return SetCurrencies(new Dictionary<string, long>
         {
            {currencyId, amount}
         }, transaction);
      }

      public Promise<Unit> AddCurrency(string currencyId, long amount, string transaction = null)
      {
         return AddCurrencies(new Dictionary<string, long>
         {
            {currencyId, amount}
         }, transaction);
      }

      public Promise<Unit> SetCurrencies(Dictionary<string, long> currencyIdsToAmount, string transaction = null)
      {
         return GetCurrencies(currencyIdsToAmount.Keys.ToArray()).FlatMap(existingAmounts =>
         {
            var deltas = new Dictionary<string, long>();
            foreach (var kvp in currencyIdsToAmount)
            {
               var delta = kvp.Value;
               if (existingAmounts.TryGetValue(kvp.Key, out var existing))
               {
                  delta = kvp.Value - existing;
               }

               if (deltas.ContainsKey(kvp.Key))
               {
                  deltas[kvp.Key] = delta;
               }
               else
               {
                  deltas.Add(kvp.Key, delta);
               }
            }

            return AddCurrencies(deltas, transaction);
         });
      }

      public Promise<Unit> AddCurrencies(Dictionary<string, long> currencyIdsToAmount, string transaction = null)
      {
         var dbid = UserContext.UserId;

         // the default json serializer won't work for the dictionary, so lets do it ourselves...
         using (var pooledBuilder = StringBuilderPool.StaticPool.Spawn())
         {
            var dict = new ArrayDict();
            dict.Add("transaction", transaction ?? Guid.NewGuid().ToString());
            dict.Add("currencies", currencyIdsToAmount);
            var json = Json.Serialize(dict, pooledBuilder.Builder);
            return Requester.Request<EmptyResponse>(Method.PUT, $"{SERVICE_OBJECT}/{dbid}", json).ToUnit();

         }
      }

      public Promise<Dictionary<string, long>> GetCurrencies(string[] currencyIds)
      {
         var dbid = UserContext.UserId;
         return Requester.Request<GetInventoryResponse>(Method.GET, $"{SERVICE_OBJECT}/{dbid}").Map(view =>
         {
            return view.currencies.ToDictionary(v => v.id, v => v.amount);
         });
      }

      public Promise<long> GetCurrency(string currencyId)
      {
         return GetCurrencies(new []{currencyId}).Map(all => {
            if (!all.TryGetValue(currencyId, out var result))
            {
               result = 0;
            }

            return result;
         });
      }

      public abstract Promise<InventoryView> GetCurrent(string scope = "");
      //public abstract InventoryView GetLatest(string scope = "");
   }
}