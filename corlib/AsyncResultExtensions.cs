using System;
using System.Reactive;
using System.Reactive.Linq;
using CorLib.Reactive;
using CorLib.Threading;

namespace CorLib {

    public static class AsyncResultExtensions {

        /// <summary>
        /// Converts an async result to an observable sequence
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asyncResult"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static IObservable<T> AsObservable<T> (this IAsyncResult<T> asyncResult, TimeSpan? timeout = null) {
            // an async result should only signal once
            const bool executeOnlyOnce = true;
            var waitHandleStream = asyncResult.AsyncWaitHandle.AsObservable (executeOnlyOnce, timeout);
            // when waithandle is signaled, stream completes, so return result and complete
            return waitHandleStream.IgnoreElementsContinueWith (() =>
                Observable.Return (asyncResult.Result));
        }

        /// <summary>
        /// Converts an async result to an observable with an optional timeout
        /// </summary>
        /// <param name="asyncResult">the async result to ovserve</param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static IObservable<Unit> AsObservable (this IAsyncResult asyncResult, TimeSpan? timeout = null) {
            // an async result should only signal once
            const bool executeOnlyOnce = true;
            // return the stream from the waithandle
            return asyncResult.AsyncWaitHandle.AsObservable (executeOnlyOnce, timeout);
        }
    }
}