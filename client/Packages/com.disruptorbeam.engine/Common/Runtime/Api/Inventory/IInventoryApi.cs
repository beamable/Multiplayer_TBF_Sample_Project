using System;
using System.Collections.Generic;

namespace Beamable.Common.Api.Inventory
{
   public interface IInventoryApi : ISupportsGet<InventoryView>
   {
      Promise<Unit> SetCurrency(string currencyId, long amount, string transaction=null);

      Promise<Unit> AddCurrency(string currencyId, long amount, string transaction=null);

      Promise<Unit> SetCurrencies(Dictionary<string, long> currencyIdsToAmount, string transaction = null);
      Promise<Unit> AddCurrencies(Dictionary<string, long> currencyIdsToAmount, string transaction = null);

      Promise<Dictionary<string, long>> GetCurrencies(string[] currencyIds);

      Promise<long> GetCurrency(string currencyId);
      Promise<Unit> AddItem(string contentId, Dictionary<string, string> properties, string transaction = null);
   }

   [System.Serializable]
   public class GetInventoryResponse
   {
      public List<Currency> currencies;
   }
   [System.Serializable]
   public class InventoryUpdateRequest
   {
      public string transaction; // will be set by api
      public Dictionary<string, long> currencies;
   }

   [Serializable]
   public class InventoryResponse
   {
      public List<Currency> currencies;
      public List<ItemGroup> items;

      public void MergeView(InventoryView view)
      {
         foreach (var currency in currencies)
         {
            view.currencies[currency.id] = currency.amount;
         }
         view.items.Clear();
         foreach (var itemGroup in items)
         {
            List<ItemView> itemList;
            if (!view.items.TryGetValue(itemGroup.id, out itemList))
            {
               itemList = new List<ItemView>();
               view.items[itemGroup.id] = itemList;
            }

            foreach (var item in itemGroup.items)
            {
               ItemView itemView = new ItemView();
               itemView.id = item.id;
               itemView.properties = new Dictionary<string, string>();
               foreach (var property in item.properties)
               {
                  itemView.properties[property.name] = property.value;
               }
               itemList.Add(itemView);
            }
         }
      }
   }

   [Serializable]
   public class Currency
   {
      public string id;
      public long amount;
   }

   [Serializable]
   public class Item
   {
      public string id;
      public List<ItemProperty> properties;
   }

   [Serializable]
   public class ItemGroup
   {
      public string id;
      public List<Item> items;
   }

   [Serializable]
   public class ItemProperty
   {
      public string name;
      public string value;
   }

   public class InventoryView
   {
      public Dictionary<string, long> currencies = new Dictionary<string, long>();
      public Dictionary<string, List<ItemView>> items = new Dictionary<string, List<ItemView>>();
   }

   public class ItemView
   {
      public string id;
      public Dictionary<string, string> properties;
   }
}