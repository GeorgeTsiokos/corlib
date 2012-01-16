using System;
using System.Collections.Generic;
using System.Linq;

namespace CorLib.Collections.Generic {

    /// <summary>
    /// A collection where <see cref="WeakReference"/>s are held for each item
    /// </summary>
    public class WeakReferenceCollection<T> : ICollection<T> {

        readonly ICollection<WeakReference> _collection;

        public WeakReferenceCollection (ICollection<WeakReference> collection) {
            _collection = collection;
        }

        public WeakReferenceCollection ()
            : this (new List<WeakReference> ()) {
        }

        public void Add (T item) {
            var collection = _collection;
            Clean (collection);
            collection.Add (new WeakReference (item));
        }

        public void Clear () {
            _collection.Clear ();
        }

        public bool Contains (T item) {
            return CurrentDictionary.ContainsKey (item);
        }

        public void CopyTo (T[] array, int arrayIndex) {
            CurrentDictionary.Keys.ToArray ().CopyTo (array, arrayIndex);
        }

        public int Count {
            get {
                var collection = _collection;
                Clean (collection);
                return collection.Count;
            }
        }

        public bool IsReadOnly {
            get {
                var collection = _collection;
                Clean (collection);
                return collection.IsReadOnly;
            }
        }

        public bool Remove (T item) {
            WeakReference weakReference;
            if (CurrentDictionary.TryGetValue (item, out weakReference))
                return _collection.Remove (weakReference);

            return false;
        }

        public IEnumerator<T> GetEnumerator () {
            return CurrentDictionary.Keys.GetEnumerator ();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
            return GetEnumerator ();
        }

        protected static void Clean (ICollection<WeakReference> collection) {
            foreach (var item in collection.ToArray ())
                if (!item.IsAlive)
                    collection.Remove (item);
        }

        protected Dictionary<T, WeakReference> CurrentDictionary {
            get {
                var collection = _collection;
                Clean (collection);
                return collection.Where (reference =>
                    reference.IsAlive).ToDictionary (reference =>
                        (T)reference.Target);
            }
        }
    }
}