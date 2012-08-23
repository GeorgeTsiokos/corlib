using System.Collections.Concurrent;
using System.Diagnostics.Contracts;

namespace Corlib.Collections.Concurrent {

    public static class ConcurrentDictionaryExtensions {

        /// <summary>
        /// Attempts to remove the value with the specified key from the dictionary
        /// </summary>
        /// <param name="dictionary">the dictionary to remove the item from</param>
        /// <param name="key">The key of the element to remove</param>
        /// <returns>true if an object was removed successfully; otherwise, false</returns>
        /// <remarks>Calls TryRemove and throws away the unused result</remarks>
        public static bool TryRemove<TKey, TValue> (this ConcurrentDictionary<TKey, TValue> dictionary, TKey key) {
            Contract.Requires (null != dictionary, "dictionary is null");
            Contract.Requires (null != key, "key is null");
            TValue unused;
            return dictionary.TryRemove (key, out unused);
        }
    }
}