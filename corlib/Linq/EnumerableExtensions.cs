using System.Collections.Generic;
using System.Reactive.Linq;

namespace CorLib.Linq {

    public static class EnumerableExtensions {

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