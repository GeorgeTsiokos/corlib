using System;
using System.Collections.Concurrent;
using System.Threading;

namespace CorLib.Internal.Collections.Generic {

    internal sealed class RefCountDisposable<T> : IDisposable<T> {
        IProducerConsumerCollection<IDisposable<T>> _collection;
        T _value;
        int _count;

        public RefCountDisposable (IProducerConsumerCollection<IDisposable<T>> collection, T value) {
            if (collection == null)
                throw new ArgumentNullException ("collection", "collection is null.");
            _collection = collection;
            _value = value;
        }

        public T Value {
            get {
                Interlocked.Increment (ref _count);
                return _value;
            }
        }

        public void Dispose () {
            if (0 == Interlocked.Decrement (ref _count)) {
                var collection = Interlocked.Exchange (ref _collection, null);
                if (null != collection)
                    collection.TryAdd (this);
            }
        }
    }
}