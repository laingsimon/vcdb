using System.Collections.Generic;
using System.Linq;
using vcdb.Models;

namespace vcdb.Scripting
{
    public class NamedItemFinder : INamedItemFinder
    {
        public NamedItem<TKey, TValue> GetCurrentItem<TKey, TValue>(
            IDictionary<TKey, TValue> currentItems,
            KeyValuePair<TKey, TValue> requiredItem)
            where TValue : INamedItem<TKey>
        {
            return GetCurrentItem(currentItems, requiredItem.Key)
                ?? GetCurrentItemForPreviousName(currentItems, requiredItem.Value.PreviousNames);
        }

        public NamedItem<TKey, TValue> GetCurrentItem<TKey, TValue>(IDictionary<TKey, TValue> currentItems, TKey requiredKey, TKey[] previousNames)
        {
            return GetCurrentItem(currentItems, requiredKey)
                ?? GetCurrentItemForPreviousName(currentItems, previousNames);
        }

        private NamedItem<TKey, TValue> GetCurrentItem<TKey, TValue>(
            IDictionary<TKey, TValue> currentItems,
            TKey requiredItem)
        {
            var itemsWithTheSameKey = currentItems.Where(pair => pair.Key.Equals(requiredItem)).ToArray();
            return itemsWithTheSameKey.Length == 1
                ? itemsWithTheSameKey[0].AsNamedItem()
                : NamedItem<TKey, TValue>.Null;
        }

        private NamedItem<TKey, TValue> GetCurrentItemForPreviousName<TKey, TValue>(
            IDictionary<TKey, TValue> currentIndexes,
            TKey[] previousNames)
        {
            if (previousNames == null)
                return null;

            return previousNames
                .Select(previousName => GetCurrentItem(currentIndexes, previousName))
                .FirstOrDefault(current => current != null);
        }
    }
}
