using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace CorLib {

    public static class ObservableExtensions {

        /// <summary>
        /// Takes a single value from the sequence as an <see cref="IAsyncResult{T}"/>
        /// </summary>
        /// <param name="sequence">sequence to take one value from</param>
        /// <param name="asyncCallback">APM callback</param>
        /// <param name="state">APM state</param>
        /// <returns>an async result representing the single value from the sequence</returns>
        /// <remarks>if the sequence errors, returns a value, or completes empty, the async result signals</remarks>
        /// <exception cref="ArgumentNullException">thrown when sequence is null</exception>
        public static IAsyncResult<T> AsAsyncResult<T> (this IObservable<T> sequence, AsyncCallback asyncCallback, object state) {
            if (sequence == null)
                throw new ArgumentNullException ("sequence", "sequence is null.");
            var result = AsyncResult.Create<T> (asyncCallback, state);
            // subscription will be completed when the sequence produces a single value, errors, or completes
            sequence.Take (1).Subscribe (result);
            return result;
        }
    }
}