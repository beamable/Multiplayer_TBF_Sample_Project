using System.Collections.Generic;

namespace Beamable.Common.Api.Inventory
{
    public class ItemCreateRequest
    {
        public string contentId;
        public Dictionary<string, string> properties;
    }

    public class ItemDeleteRequest
    {
        public string contentId;
        public long itemId;
    }

    public class ItemUpdateRequest
    {
        public string contentId;
        public long itemId;
        public Dictionary<string, string> properties;
    }

    public class InventoryUpdateBuilder
    {
        public readonly Dictionary<string, long> currencies;
        public readonly List<ItemCreateRequest> newItems;
        public readonly List<ItemDeleteRequest> deleteItems;
        public readonly List<ItemUpdateRequest> updateItems;

        public bool IsEmpty
        {
            get
            {
                return currencies.Count == 0 &&
                       newItems.Count == 0 &&
                       deleteItems.Count == 0 &&
                       updateItems.Count == 0;
            }
        }

        public InventoryUpdateBuilder()
        {
            currencies = new Dictionary<string, long>();
            newItems = new List<ItemCreateRequest>();
            deleteItems = new List<ItemDeleteRequest>();
            updateItems = new List<ItemUpdateRequest>();
        }

        public InventoryUpdateBuilder CurrencyChange(string contentId, long amount)
        {
            if (currencies.TryGetValue(contentId, out var currentValue))
            {
                currencies[contentId] = currentValue + amount;
            }
            else
            {
                currencies.Add(contentId, amount);
            }

            return this;
        }

        public InventoryUpdateBuilder AddItem(string contentId, Dictionary<string, string> properties)
        {
            newItems.Add(new ItemCreateRequest
            {
                contentId = contentId,
                properties = properties
            });

            return this;
        }

        public InventoryUpdateBuilder DeleteItem(string contentId, long itemId)
        {
            deleteItems.Add(new ItemDeleteRequest
            {
                contentId = contentId,
                itemId = itemId
            });

            return this;
        }

        public InventoryUpdateBuilder UpdateItem(string contentId, long itemId, Dictionary<string, string> properties)
        {
            updateItems.Add(new ItemUpdateRequest
            {
                contentId = contentId,
                itemId = itemId,
                properties = properties
            });

            return this;
        }
    }
}