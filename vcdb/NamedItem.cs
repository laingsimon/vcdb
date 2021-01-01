using System.Collections.Generic;

namespace vcdb
{
    /// <summary>
    /// This is a 'copy' of the KeyValuePair<TKey, TValue> type; but is modelled as a reference type rather than a value type
    /// As such it allows for the property/method that uses/returns it to return null rather than default(KeyValuePair<TKey, TValue>)
    /// </summary>
    public class NamedItem<TKey, TValue>
    {
        public static readonly NamedItem<TKey, TValue> Null = null;

        public TKey Key { get; }
        public TValue Value { get; }

        public NamedItem(KeyValuePair<TKey, TValue> pair)
        {
            Key = pair.Key;
            Value = pair.Value;
        }
    }
}
