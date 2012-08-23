using System;
using System.Diagnostics.Contracts;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;

namespace Corlib.Reactive.Threading.Tasks {

    /// <summary>
    /// Provides a set of static methods for converting IObservables to Tasks.
    /// </summary>
    public static class ObservableExtensions {

        /// <summary>
        /// Returns a task that contains the last value of the observable sequence
        /// </summary>
        /// <param name="sequence">Observable sequence to convert to a task</param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel the task, causing unsubscription from the observable sequence</param>
        /// <param name="state">The state to use as the underlying task's AsyncState</param>
        /// <param name="asyncCallback">The method to be called when the asynchronous task completes</param>
        /// <returns>A task that contains the last value of the observable sequence</returns>
        /// <remarks>Recommanded usage is sequence.Take(1).ToTask(...)</remarks>
        public static Task<T> ToTask<T> (this IObservable<T> sequence, AsyncCallback asyncCallback = null, object state = null, CancellationToken cancellationToken = default(CancellationToken)) {
            Contract.Requires (sequence != null);
            Contract.Assume (Contract.Result<Task<T>> () != null);

            // call Rx's implementation to convert sequence to a task
            var task = TaskObservableExtensions.ToTask (sequence, cancellationToken, state);

            // if a callback is specified, invoke it after the task completes
            if (null != asyncCallback)
                task.ContinueWith (_ =>
                    asyncCallback (task));

            return task;
        }
    }
}