using System;
using System.Collections.Concurrent;
using System.Threading;

namespace CorLib.Internal.Collections.Generic {

    internal sealed class ImmutableDisposable<T> : IDisposable<T> {
        IProducerConsumerCollection<IDisposable<T>> _collection;
        T _value;

        public ImmutableDisposable (IProducerConsumerCollection<IDisposable<T>> collection, T value) {
            if (collection == null)
                throw new ArgumentNullException ("collection", "collection is null.");
            _collection = collection;
            _value = value;
        }

        public T Value {
            get { return _value; }
        }

        public void Dispose () {
            var collection = Interlocked.Exchange (ref _collection, null);
            if (null != collection)
                collection.TryAdd (this);
        }
    }
}