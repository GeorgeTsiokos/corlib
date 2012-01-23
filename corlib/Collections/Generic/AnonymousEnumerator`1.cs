using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections;

namespace CorLib.Collections.Generic {

    internal sealed class AnonymousEnumerator<T> : IEnumerator<T> {

        readonly Func<T> _current;
        readonly Action _dispose;
        readonly Func<bool> _moveNext;
        readonly Action _reset;

        /// <summary>
        /// Initializes a new instance of the AnonymousEnumerator class.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="dispose"></param>
        /// <param name="moveNext"></param>
        /// <param name="reset"></param>
        public AnonymousEnumerator (Func<T> current, Action dispose, Func<bool> moveNext, Action reset) {
            _current = current;
            _dispose = dispose;
            _moveNext = moveNext;
            _reset = reset;
        }

        public T Current {
            get { return _current (); }
        }

        public void Dispose () {
            _dispose ();
        }

        object IEnumerator.Current {
            get { return Current; }
        }

        public bool MoveNext () {
            return _moveNext ();
        }

        public void Reset () {
            _reset ();
        }
    }
}