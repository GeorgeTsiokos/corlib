using System;
using System.Collections.Generic;

namespace CorLib.Internal.Collections.Generic {

    internal class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue> {

        readonly IDictionary<TKey, TValue> _0;

        public ReadOnlyDictionary (IDictionary<TKey, TValue> value) {
            _0 = value;
        }

        public void Add (TKey key, TValue value) {
            throw new NotSupportedException ();
        }

        public bool ContainsKey (TKey key) {
            return _0.ContainsKey (key);
        }

        public ICollection<TKey> Keys {
            get { return _0.Keys; }
        }

        public bool Remove (TKey key) {
            throw new NotSupportedException ();
        }

        public bool TryGetValue (TKey key, out TValue value) {
            return _0.TryGetValue (key, out value);
        }

        public ICollection<TValue> Values {
            get { return _0.Values; }
        }

        public TValue this[TKey key] {
            get {
                return _0[key];
            }
            set {
                throw new NotSupportedException ();
            }
        }

        public void Add (KeyValuePair<TKey, TValue> item) {
            throw new NotSupportedException ();
        }

        public void Clear () {
            throw new NotSupportedException ();
        }

        public bool Contains (KeyValuePair<TKey, TValue> item) {
            return _0.Contains (item);
        }

        public void CopyTo (KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            _0.CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return _0.Count; }
        }

        public bool IsReadOnly {
            get { return true; }
        }

        public bool Remove (KeyValuePair<TKey, TValue> item) {
            throw new NotSupportedException ();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator () {
            return _0.GetEnumerator ();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
            return GetEnumerator ();
        }
    }
}
