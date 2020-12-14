using System;
using System.Collections.Generic;
using System.Linq;

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
      Promise<Unit> DeleteItem(string contentId, long itemId, string transaction = null);

      Promise<Unit> UpdateItem(string contentId, long itemId, Dictionary<string, string> properties,
          string transaction = null);

      Promise<Unit> Update(Action<InventoryUpdateBuilder> action, string transaction = null);
      Promise<Unit> Update(InventoryUpdateBuilder builder, string transaction = null);
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
        public string scope;
        public List<Currency> currencies;
        public List<ItemGroup> items;

        private HashSet<string> _scopes;
        public HashSet<string> Scopes {
            get {
                if (_scopes == null) {
                    if (!string.IsNullOrEmpty(scope))
                        _scopes = new HashSet<string>(scope.Split(','));
                    else
                        _scopes = new HashSet<string>();
                }

                return _scopes;
            }
        }

        public HashSet<string> GetNotifyScopes()
        {
            var notifyScopes = new HashSet<string>();
            notifyScopes.UnionWith(currencies.Select(currency => currency.id));
            notifyScopes.UnionWith(items.Select(item => item.id));
            notifyScopes.UnionWith(Scopes);

            return ResolveAllScopes(notifyScopes);
        }

        private HashSet<string> ResolveAllScopes(IEnumerable<string> notifyScopes)
        {
            var resolved = new HashSet<string>();

            foreach (string notifyScope in notifyScopes)
            {
                var newScopes = ResolveScope(notifyScope);
                resolved.UnionWith(newScopes);
            }

            return resolved;
        }

        private HashSet<string> ResolveScope(string notifyScope)
        {
            var result = new HashSet<string>();
            string[] slicedScopes = notifyScope.Split('.');

            foreach (string slicedScope in slicedScopes)
            {
                if (result.Count == 0)
                {
                    result.Add(slicedScope);
                }
                else
                {
                    string newScope = string.Join(".", result.Last(), slicedScope);
                    result.Add(newScope);
                }
            }

            return result;
        }

        private HashSet<string> ResolveMergeScopes(InventoryView view)
        {
            var resolved = new HashSet<string>();
            var scopes = Scopes;

            var scopesLookup = new HashSet<string>();
            scopesLookup.UnionWith(scopes);
            scopesLookup.UnionWith(scopes.Select(s => $"{s}."));

            resolved.UnionWith(scopes);
            resolved.UnionWith(view.currencies.Keys.Where(scopesLookup.Contains));
            resolved.UnionWith(view.items.Keys.Where(scopesLookup.Contains));
            resolved.UnionWith(currencies.Select(currency => currency.id));
            resolved.UnionWith(items.Select(item => item.id));

            return resolved;
        }

        public void MergeView(InventoryView view)
        {
            var relevantScopes = ResolveMergeScopes(view);
            foreach(var contentId in view.currencies.Keys.ToList().Where(relevantScopes.Contains))
            {
                view.currencies.Remove(contentId);
            }

            foreach(var contentId in view.items.Keys.ToList().Where(relevantScopes.Contains))
            {
                view.items.Remove(contentId);
            }

            foreach (var currency in currencies)
            {
                view.currencies[currency.id] = currency.amount;
            }

            foreach (var itemGroup in items)
            {
                var itemViews = itemGroup.items.Select(item =>
                {
                    ItemView itemView = new ItemView();
                    itemView.id = item.id;
                    itemView.properties = item.properties.ToDictionary(p => p.name, p => p.value);

                    return itemView;
                });

                List<ItemView> itemList = new List<ItemView>(itemViews);
                view.items[itemGroup.id] = itemList;
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