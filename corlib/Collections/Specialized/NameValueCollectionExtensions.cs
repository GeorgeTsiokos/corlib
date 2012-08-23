using System;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;

namespace Corlib.Collections.Specialized {

    public static class NameValueCollectionExtensions {

        /// <summary>
        /// Steps through the specified keys looking for the first key where a value is present and not equal to null
        /// </summary>
        /// <param name="nameValueCollection">source</param>
        /// <param name="args">zero or more keys</param>
        /// <returns>the first value not equal to null, or null if no values were found</returns>
        public static string Coalesce (this NameValueCollection nameValueCollection, params string[] args) {
            Contract.Requires (null != nameValueCollection);
            Contract.Requires (null != args);

            foreach (var key in args) {
                var result = nameValueCollection[key];
                if (null != result)
                    return result;
            }

            return null;
        }
    }
}