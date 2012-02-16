using System;

namespace CorLib.Internal {

    internal sealed class DisposableValue<T> : IDisposable<T> {

        T _value;
        readonly Action _dispose;

        public DisposableValue (T value, Action dispose) {
            _value = value;
            _dispose = dispose;
        }

        public DisposableValue (T value, IDisposable dispose)
            : this (value, () => dispose.TryDispose ()) {
        }

        public T Value {
            get { return _value; }
        }

        public void Dispose () {
            _dispose ();
        }
    }
}