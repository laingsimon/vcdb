using System.Collections.Generic;
using vcdb.Models;

namespace vcdb.Scripting
{
    public interface INamedItemFinder
    {
        NamedItem<TKey, TValue> GetCurrentItem<TKey, TValue>(
            IDictionary<TKey, TValue> currentItems, 
            KeyValuePair<TKey, TValue> requiredItem)
            where TValue : INamedItem<TKey>;

        NamedItem<TKey, TValue> GetCurrentItem<TKey, TValue>(
            IDictionary<TKey, TValue> currentItems, 
            TKey requiredKey, 
            TKey[] previousNames);
    }
}