using System;
using System.Collections.Generic;
using System.Linq;
using Beamable.Common.Pooling;
using Beamable.Serialization.SmallerJSON;

namespace Beamable.Common.Api.Inventory
{
    public abstract class AbsInventoryApi : IInventoryApi
    {
        public const string SERVICE_OBJECT = "object/inventory";

        public IBeamableRequester Requester { get; }
        public IUserContext UserContext { get; }

        public AbsInventoryApi(IBeamableRequester requester, IUserContext userContext)
        {
            Requester = requester;
            UserContext = userContext;
        }

        public Promise<Unit> AddItem(string contentId, Dictionary<string, string> properties, string transaction = null)
        {
            return Update(builder => { builder.AddItem(contentId, properties); }, transaction);
        }

        public Promise<Unit> DeleteItem(string contentId, long itemId, string transaction = null)
        {
            return Update(builder => { builder.DeleteItem(contentId, itemId); }, transaction);
        }

        public Promise<Unit> UpdateItem(string contentId, long itemId, Dictionary<string, string> properties,
            string transaction = null)
        {
            return Update(builder => { builder.UpdateItem(contentId, itemId, properties); }, transaction);
        }

        public Promise<Unit> AddCurrency(string currencyId, long amount, string transaction = null)
        {
            return Update(builder => { builder.CurrencyChange(currencyId, amount); }, transaction);
        }

        public Promise<Unit> AddCurrencies(Dictionary<string, long> currencyIdsToAmount, string transaction = null)
        {
            return Update(builder =>
            {
                foreach (var currency in currencyIdsToAmount)
                {
                    string currencyId = currency.Key;
                    long amount = currency.Value;

                    builder.CurrencyChange(currencyId, amount);
                }
            }, transaction);
        }

        public Promise<Unit> SetCurrency(string currencyId, long amount, string transaction = null)
        {
            return SetCurrencies(new Dictionary<string, long>
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

        public Promise<Dictionary<string, long>> GetCurrencies(string[] currencyIds)
        {
            string scopes;
            if (currencyIds == null || currencyIds.Count() == 0)
            {
                scopes = "currency";
            }
            else
            {
                scopes = string.Join(",", currencyIds);
            }

            return Requester.Request<InventoryResponse>(Method.GET, CreateRefreshUrl(scopes)).Map(view =>
            {
                return view.currencies.ToDictionary(v => v.id, v => v.amount);
            });
        }

        protected string CreateRefreshUrl(string scope)
        {
            var queryArgs = "";
            if (!string.IsNullOrEmpty(scope))
            {
                queryArgs = $"?scope={scope}";
            }

            return $"/{SERVICE_OBJECT}/{UserContext.UserId}{queryArgs}";
        }

        public Promise<long> GetCurrency(string currencyId)
        {
            return GetCurrencies(new[] {currencyId}).Map(all =>
            {
                if (!all.TryGetValue(currencyId, out var result))
                {
                    result = 0;
                }

                return result;
            });
        }

        public Promise<Unit> Update(Action<InventoryUpdateBuilder> action, string transaction = null)
        {
            var builder = new InventoryUpdateBuilder();
            action.Invoke(builder);

            return Update(builder, transaction);
        }


        public Promise<Unit> Update(InventoryUpdateBuilder builder, string transaction = null)
        {
            if (builder.IsEmpty)
            {
                return Promise<Unit>.Successful(PromiseBase.Unit);
            }

            using (var pooledBuilder = StringBuilderPool.StaticPool.Spawn())
            {
                var dict = new ArrayDict();
                dict.Add("transaction", transaction ?? Guid.NewGuid().ToString());

                if (builder.currencies != null && builder.currencies.Count > 0)
                {
                    dict.Add("currencies", builder.currencies);
                }

                if (builder.newItems != null && builder.newItems.Count > 0)
                {
                    var newItems = builder.newItems.Select(newItem => new ArrayDict
                    {
                        {"contentId", newItem.contentId},
                        {
                            "properties", newItem.properties.Select(kvp => new ArrayDict
                            {
                                {"name", kvp.Key},
                                {"value", kvp.Value}
                            }).ToArray()
                        }
                    }).ToArray();

                    dict.Add("newItems", newItems);
                }

                if (builder.deleteItems != null && builder.deleteItems.Count > 0)
                {
                    var deleteItems = builder.deleteItems.Select(deleteItem => new ArrayDict
                    {
                        {"contentId", deleteItem.contentId},
                        {"id", deleteItem.itemId}
                    }).ToArray();

                    dict.Add("deleteItems", deleteItems);
                }

                if (builder.updateItems != null && builder.updateItems.Count > 0)
                {
                    var updateItems = builder.updateItems.Select(updateItem => new ArrayDict
                    {
                        {"contentId", updateItem.contentId},
                        {"id", updateItem.itemId},
                        {
                            "properties", updateItem.properties.Select(kvp => new ArrayDict
                            {
                                {"name", kvp.Key},
                                {"value", kvp.Value}
                            }).ToArray()
                        }
                    }).ToArray();

                    dict.Add("updateItems", updateItems);
                }

                var json = Json.Serialize(dict, pooledBuilder.Builder);
                return Requester.Request<EmptyResponse>(Method.PUT, CreateRefreshUrl(null), json).ToUnit();
            }
        }


        public abstract Promise<InventoryView> GetCurrent(string scope = "");
        //public abstract InventoryView GetLatest(string scope = "");
    }
}