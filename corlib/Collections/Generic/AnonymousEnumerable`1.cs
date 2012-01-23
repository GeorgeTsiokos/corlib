using System;
using System.Collections;
using System.Collections.Generic;

namespace CorLib.Collections.Generic {

    public sealed class AnonymousEnumerable<T> : IEnumerable<T> {

        readonly Func<IEnumerator<T>> _getEnumerator;

        /// <summary>
        /// Initializes a new instance of the AnonymousEnumerable class.
        /// </summary>
        /// <param name="getEnumerator"></param>
        public AnonymousEnumerable (Func<IEnumerator<T>> getEnumerator) {
            _getEnumerator = getEnumerator;
        }

        public IEnumerator<T> GetEnumerator () {
            return _getEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator () {
            return GetEnumerator ();
        }
    }
}