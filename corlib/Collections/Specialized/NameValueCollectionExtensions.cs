using System;
using System.Collections.Specialized;

namespace CorLib.Collections.Specialized {

    public static class NameValueCollectionExtensions {

        public static string Get (this NameValueCollection nameValueCollection, params string[] args) {
            //TODO: use contract
            foreach (var key in args) {
                var result = nameValueCollection[key];
                if (null != result)
                    return result;
            }
            return null;
        }
    }
}