using System.Collections.Generic;
using System.Diagnostics.Contracts;
using CorLib.Internal.Collections.Generic;

namespace CorLib.Collections.Generic {

    public static class DictionaryExtensions {

        /// <summary>
        /// Steps through the specified keys looking for the first key where a value is present and not equal to null
        /// </summary>
        /// <param name="dictionary">source</param>
        /// <param name="args">zero or more keys</param>
        /// <returns>the first value not equal to null, or null if no values were found</returns>
        public static TValue Coalesce<TKey, TValue> (this IDictionary<TKey, TValue> dictionary, params TKey[] args) where TValue : class {
            Contract.Requires (null != dictionary);
            Contract.Requires (null != args);

            foreach (var key in args) {
                TValue value;
                if (dictionary.TryGetValue (key, out value) && null != value)
                    return value;
            }
            return null;
        }

        /// <summary>
        /// Prevents updates to the dictionary
        /// </summary>
        /// <param name="dictionary">source</param>
        /// <returns>a dictionary incapable of mutation</returns>
        public static IDictionary<TKey, TValue> AsReadOnly<TKey, TValue> (this IDictionary<TKey, TValue> dictionary) {
            return new ReadOnlyDictionary<TKey, TValue> (dictionary);
        }
    }
}