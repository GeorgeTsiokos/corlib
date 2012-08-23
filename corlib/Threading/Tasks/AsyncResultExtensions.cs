using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Corlib.Threading.Tasks {

    /// <summary>
    /// Extension methods for <see cref="IAsyncResult"/>
    /// </summary>
    public static class AsyncResultExtensions {

        /// <summary>
        /// Converts a <see cref="IAsyncResult"/> in to a disposable task
        /// </summary>
        /// <remarks>Calling dispose releases the resources held to create
        /// the task (the task will no longer timeout or complete)</remarks>
        /// <param name="asyncResult">the object to convert</param>
        /// <param name="timeout">an optional timeout</param>
        /// <param name="asyncCallback">optional callback when the
        /// asynchrounous operation completes</param>
        /// <returns>a disposable task</returns>
        public static IDisposable<Task> ToTask (this IAsyncResult asyncResult, TimeSpan? timeout = null, AsyncCallback asyncCallback = null) {
            Contract.Requires (null != asyncResult);
            Contract.Requires (null != asyncResult.AsyncWaitHandle);

            return asyncResult.AsyncWaitHandle.ToTask (
                timeout, 
                asyncCallback, 
                asyncResult.AsyncState);
        }
    }
}