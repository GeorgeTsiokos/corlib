using System;
using System.Threading;
using System.Threading.Tasks;
using Corlib.Internal;

namespace Corlib.Threading.Tasks {

    public static class WaitHandleExtensions {

        /// <summary>
        /// Converts a <see cref="WaitHandle"/> into a disposable <see cref="Task"/>
        /// </summary>
        /// <param name="waitHandle">the operating-system specfic object to watch</param>
        /// <param name="timeout">optional timeout</param>
        /// <param name="asyncCallback">optional callback to call when the waitHandle signals</param>
        /// <param name="state">optional state to pass to the callback</param>
        /// <returns>An encapulted task - call Dispose on the result to unregister from the waitHandle's signal and cancel the task</returns>
        /// <remarks>Calling dispose on the task before it completes will result in an exception</remarks>
        public static IDisposable<Task> ToTask (this WaitHandle waitHandle, TimeSpan? timeout = null, AsyncCallback asyncCallback = null, object state = null) {
            var tcs = null == state ?
                new TaskCompletionSource<object> () :
                new TaskCompletionSource<object> (state);

            var registeredWaitHandle = ThreadPool.UnsafeRegisterWaitForSingleObject (
                waitHandle,
                (o, timedOut) => {
                    if (timedOut)
                        tcs.TrySetException (new TimeoutException ());
                    else
                        tcs.TrySetResult (o);
                },
                state,
                timeout.AsThreadingTimeout (),
                true);

            Action unregister = () => registeredWaitHandle.Unregister (waitHandle);
            Task task = tcs.Task.ContinueWith (_ => unregister ());
            IAsyncResult asyncResult = task;
            task = null == asyncCallback ?
                tcs.Task :
                tcs.Task.ContinueWith (_ => asyncCallback (asyncResult));

            return new DisposableValue<Task> (task, () => {
                unregister ();
                tcs.TrySetCanceled ();
            });
        }
    }
}