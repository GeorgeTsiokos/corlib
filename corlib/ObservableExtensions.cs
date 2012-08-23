using System;
using System.Diagnostics.Contracts;
using System.Reactive;
using Corlib.Threading;

namespace Corlib {

    public static class ObservableExtensions {

        /// <summary>
        /// Converts the <see cref="IAsyncResult"/> to an obserable sequence
        /// </summary>
        /// <param name="asyncResult">The asynchrous operation</param>
        /// <param name="timeout">Optional timeout</param>
        /// <returns>An observable sequence</returns>
        public static IObservable<Unit> ToObservable (this IAsyncResult asyncResult, TimeSpan? timeout = null) {
            Contract.Requires (null != asyncResult, "null == asyncResult");
            Contract.Requires (null != asyncResult.AsyncWaitHandle, "null == asyncResult.AsyncWaitHandle");
            return asyncResult.AsyncWaitHandle.ToObservable (true, timeout);
        }
    }
}