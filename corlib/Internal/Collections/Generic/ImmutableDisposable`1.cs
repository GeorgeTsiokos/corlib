using System;
using System.Collections.Concurrent;

namespace CorLib.Internal.Collections.Generic {

    internal sealed class ImmutableDisposable<T> : IDisposable<T> {
        readonly IProducerConsumerCollection<IDisposable<T>> _collection;
        readonly T _value;

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
            if (!_collection.TryAdd (this))
                (_value as IDisposable).TryDispose ();
        }
    }
}