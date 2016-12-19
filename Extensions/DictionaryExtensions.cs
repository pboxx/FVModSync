namespace FVModSync.Extensions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Extensions for the <see cref="IDictionary{TKey,TValue}"/> interface.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets the value for the specified <paramref name="key"/> if it exists or
        /// creates a new value via <paramref name="create"/> and adds it to the
        /// dictionary.
        /// </summary>
        /// <returns>The existing value if applicable, or the created value.</returns>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary,
                                                    TKey key,
                                                    Func<TValue> create)
        {
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
            {
                value = create();
                dictionary.Add(key, value);
            }
            return value;
        }

        /// <summary>
        /// Sets the specified <paramref name="value"/> for the specified <paramref name="key"/>
        /// whether it existed before or not.
        /// </summary>
        public static void SetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }
    }
}