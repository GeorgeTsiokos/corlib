using System.Collections.Generic;
using System.Reactive.Linq;
using System.Linq;
using System.Diagnostics.Contracts;
using System;

namespace Corlib.Linq {

    public static class EnumerableExtensions {

        /// <summary>
        /// Identifies if the sequence is sorted ascending or decending
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence"></param>
        /// <param name="comparer"></param>
        /// <returns>NULL (or Nothing) when the sequence is empty or unsorted, true if ascending, false if decending</returns>
        public static bool? IsSortedAscending<T> (this IEnumerable<T> sequence, IComparer<T> comparer = null) {
            Contract.Requires (null != sequence);

            if (null == comparer)
                comparer = Comparer<T>.Default;
            var enumerator = sequence.GetEnumerator ();
            if (!enumerator.MoveNext ())
                return null;

            bool? ascending = null;
            var previous = enumerator.Current;
            while (enumerator.MoveNext ()) {
                var current = enumerator.Current;
                // compare current with previous
                switch (comparer.Compare (previous, current)) {
                    case -1:
                        // if we've set ascending
                        if (ascending.HasValue) {
                            // and it's true
                            if (ascending.Value)
                                // it's not sorted
                                return null;
                        }
                        else
                            // set decending
                            ascending = false;
                        break;
                    case 0:
                        // duplicate value - ignore
                        break;
                    case 1:
                        // if we've set ascending
                        if (ascending.HasValue) {
                            // and it's decending
                            if (!ascending.Value)
                                // it's not sorted
                                return null;
                        }
                        else
                            // set acending
                            ascending = true;
                        break;
                    default:
                        // comparer returned an unepected result
                        throw new InvalidOperationException ();
                }
                // set the current equal to the previous
                previous = current;
            }
            // return ascending or decending (it's sorted)
            return ascending;
        }

        /// <summary>
        /// Return a tuple for every element in sortedSetA and sortedSetB specifying objects
        /// that belong to A but not to B, objects which are both in A and in B, and objects
        /// that belong to B but not A.
        /// </summary>
        /// <remarks>This method is an O(n+m) operation when the two sets have different members, where n 
        /// is the count of <paramref name="sortedSetA"/> and m is the count of <paramref name="sortedSetB"/>.
        /// Otherwise the operation approaches O(n+m-n) where m is the count of the larger set and n is the
        /// smaller set.
        /// </remarks>
        /// <typeparam name="T">Type of element in each set</typeparam>
        /// <param name="sortedSetA">A sorted set where each element is of type <typeparamref name="T"/></param>
        /// <param name="sortedSetB">A sorted set where each element is of type <typeparamref name="T"/></param>
        /// <param name="comparer"><see cref="IComparer"/> used to sort the sets</param>
        /// <returns>Returns A \ B (-1), the intersection of A and B (0), and B \ A (1)</returns>
        public static IEnumerable<Tuple<T, ObjectSetLocation>> Membership<T> (this IEnumerable<T> sortedSetA, IEnumerable<T> sortedSetB, IComparer<T> comparer = null) {
            Contract.Requires (sortedSetA != null);
            Contract.Requires (sortedSetB != null);

            if (null == comparer)
                comparer = Comparer<T>.Default;

            using (var enumeratorA = sortedSetA.GetEnumerator ())
            using (var enumeratorB = sortedSetB.GetEnumerator ()) {
                // identify if a has a value
                bool moveNextA = enumeratorA.MoveNext ();
                // identify if b has a value
                bool moveNextB = enumeratorB.MoveNext ();
                // default value for type T
                T currentA = default (T);
                // default value for type T
                T currentB = default (T);
                // if both collections have a value
                if (moveNextA & moveNextB) {
                    // get current value
                    currentA = enumeratorA.Current;
                    // get current value
                    currentB = enumeratorB.Current;
                    do {
                        // Compare a to b: is a < b, a == b, or a > b ?
                        int compareResult = comparer.Compare (currentA, currentB);
                        // a == b
                        if (compareResult == 0) {
                            // return both collections have the value a (a == b)
                            yield return new Tuple<T, ObjectSetLocation> (currentA, ObjectSetLocation.Intersection);

                            // if collection a can move next
                            if (moveNextA = enumeratorA.MoveNext ())
                                // get the next a
                                currentA = enumeratorA.Current;
                            // if collection b can move next
                            if (moveNextB = enumeratorB.MoveNext ())
                                // get the next b
                                currentB = enumeratorB.Current;
                        }
                        // a < b
                        else
                            if (compareResult < 0) {
                                // return only collection a has the value a
                                yield return new Tuple<T, ObjectSetLocation> (currentA, ObjectSetLocation.AButNotB);

                                // if collection a can move next
                                if (moveNextA = enumeratorA.MoveNext ())
                                    // get the next a
                                    currentA = enumeratorA.Current;
                            }
                            // a > b
                            else {
                                // return only collection b has the value b
                                yield return new Tuple<T, ObjectSetLocation> (currentA, ObjectSetLocation.BButNotA);

                                // if collection b can move next
                                if (moveNextB = enumeratorB.MoveNext ())
                                    // get the next b
                                    currentB = enumeratorB.Current;
                            }
                    }
                    while (moveNextA & moveNextB);
                }
                // if collection a has more values
                if (moveNextA) {
                    // return only collection a has the value a
                    yield return new Tuple<T, ObjectSetLocation> (currentA, ObjectSetLocation.AButNotB);
                    // if collection a can move next
                    while (enumeratorA.MoveNext ()) {
                        // return only collection a has the value a
                        yield return new Tuple<T, ObjectSetLocation> (enumeratorA.Current, ObjectSetLocation.AButNotB);
                    }
                }
                // if collection b has more values
                else
                    if (moveNextB) {
                        // return only collection b has the value b
                        yield return new Tuple<T, ObjectSetLocation> (currentA, ObjectSetLocation.BButNotA);
                        // if collection b can move next
                        while (enumeratorB.MoveNext ()) {
                            // return only collection b has the value b
                            yield return new Tuple<T, ObjectSetLocation> (enumeratorB.Current, ObjectSetLocation.BButNotA);
                        }
                    }
            }
        }

        /// <summary>Returns a filtered sequence where each value T is not equal to null</summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static IEnumerable<T> NotNull<T> (this IEnumerable<T> sequence) where T : class {
            Contract.Requires (sequence != null, "sequence is null.");
            return sequence.Where (item => null != item);
        }

        /// <summary>
        /// Returns a filtered sequence where each value T is not equal to null or whitespace
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static IEnumerable<string> NotNullOrWhitespace (this IEnumerable<string> sequence) {
            Contract.Requires (sequence != null, "sequence is null.");
            return sequence.Where (item => !string.IsNullOrWhiteSpace (item));
        }

        /// <summary>
        /// Returns a specified number of contiguous elements from the end of an enumerable sequence.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence">Source sequence.</param>
        /// <param name="count">Number of elements to take from the end of the source sequence.</param>
        /// <returns>An enumerable sequence containing the specified number of elements from the end of the source sequence.</returns>
        public static IEnumerable<T> TakeLast<T> (this IEnumerable<T> sequence, int count) {
            return sequence.ToObservable ().TakeLast (count).ToEnumerable ();
        }

        /// <summary>
        /// Bypasses a specified number of elements at the end of an enumerable sequence.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence">Source sequence.</param>
        /// <param name="count">Number of elements to bypass at the end of the source sequence.</param>
        /// <returns>An enumerable sequence containing the source sequence elements except for the bypassed ones at the end.</returns>
        public static IEnumerable<T> SkipLast<T> (this IEnumerable<T> sequence, int count) {
            return sequence.ToObservable ().SkipLast (count).ToEnumerable ();
        }
    }
}