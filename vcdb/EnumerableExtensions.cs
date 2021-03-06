﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace vcdb
{
    public static class EnumerableExtensions
    {
        public static async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TInput, TKey, TValue>(
            this Task<IEnumerable<TInput>> input,
            Func<TInput, TKey> keySelector,
            Func<TInput, TValue> valueSelector)
        {
            var dictionary = new Dictionary<TKey, TValue>();

            foreach (var item in await input)
            {
                dictionary.Add(keySelector(item), valueSelector(item));
            }

            return dictionary;
        }

        public static async Task<Dictionary<TKey, TValue>> ToDictionaryAsync<TInput, TKey, TValue>(
            this IEnumerable<TInput> input,
            Func<TInput, TKey> keySelector,
            Func<TInput, Task<TValue>> valueSelector)
        {
            var dictionary = new Dictionary<TKey, TValue>();

            foreach (var item in input)
            {
                dictionary.Add(keySelector(item), await valueSelector(item));
            }

            return dictionary;
        }

        public static TValue ItemOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            return dict.ContainsKey(key)
                ? dict[key]
                : default;
        }

        public static NamedItem<TKey, TValue> AsNamedItem<TKey, TValue>(this KeyValuePair<TKey, TValue> pair)
        {
            return new NamedItem<TKey, TValue>(pair);
        }

        public static NamedItem<TKey, TValue> GetNamedItem<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            return dict.ContainsKey(key)
                ? new NamedItem<TKey, TValue>(key, dict[key])
                : null;
        }

        public static IDictionary<TKey, TValue> OrEmpty<TKey, TValue>(this IDictionary<TKey, TValue> current)
        {
            return current ?? new Dictionary<TKey, TValue>();
        }

        public static IReadOnlyCollection<TValue> OrEmptyCollection<TValue>(this IReadOnlyCollection<TValue> current)
        {
            return current ?? new TValue[0];
        }

        public static HashSet<TValue> ToHashSet<TItem, TValue>(this IEnumerable<TItem> items, Func<TItem, TValue> selector)
        {
            return items.Select(selector).ToHashSet();
        }

        public static async Task<TValue[]> ToArrayAsync<TValue>(
            this Task<IEnumerable<TValue>> input)
        {
            return (await input).ToArray();
        }
    }
}
