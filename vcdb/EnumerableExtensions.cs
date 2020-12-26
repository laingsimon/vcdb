using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace vcdb
{
    public static class EnumerableExtensions
    {
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
    }
}
