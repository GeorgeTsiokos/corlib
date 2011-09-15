using System;
using System.Collections.Concurrent;
using CorLib.Internal.Collections.Generic;

namespace CorLib.Collections.Concurrent {

    public static class ProducerConsumerCollectionObservableExtensions {
        /// <summary>
        /// Takes a disposable value from the collection, or creates one with the specified factory
        /// </summary>
        /// <param name="collection">collection to remove the disposable value from</param>
        /// <param name="factory">function to create a new value</param>
        /// <returns></returns>
        public static IDisposable<T> TakeOrCreate<T> (this IProducerConsumerCollection<IDisposable<T>> collection, bool refCount, Func<T> factory) {
            IDisposable<T> result;
            if (!collection.TryTake (out result)) {
                if (refCount)
                    result = new RefCountDisposable<T> (collection, factory ());
                else
                    result = new ImmutableDisposable<T> (collection, factory ());
            }
            return result;
        }
    }
}