using System;
using System.Diagnostics.Contracts;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CorLib.Threading.Tasks {

    public static class ObservableExtensions {

        /// <summary>
        /// Returns a task that contains the last value of the observable sequence
        /// </summary>
        /// <param name="sequence">Observable sequence to convert to a task</param>
        /// <param name="cancellationToken">Cancellation token that can be used to cancel the task, causing unsubscription from the observable sequence</param>
        /// <param name="state">The state to use as the underlying task's AsyncState</param>
        /// <param name="asyncCallback">The method to be called when the asynchronous task completes</param>
        /// <returns>A task that contains the last value of the observable sequence</returns>
        /// <remarks>Recommanded usage is sequence.Take(1).ToTask()</remarks>
        public static Task<T> ToTask<T> (this IObservable<T> sequence, CancellationToken cancellationToken = default(CancellationToken), object state = null, AsyncCallback asyncCallback = null) {
            Contract.Requires (sequence != null, "sequence == null");
            Contract.Assume (Contract.Result<Task<T>> () != null);
            Contract.Assert (default (CancellationToken) == CancellationToken.None);

            // if a callback isn't specified, route to Reactive's implementation
            if (null == asyncCallback)
                return sequence.ToTask<T> (cancellationToken, state);

            Task<T> task = null;
            // add AsyncCallback call for OnError and OnCompleted of sequence
            sequence = sequence.Do (_ => { }, ex =>
                asyncCallback (task), () =>
                asyncCallback (task));
            // convert new sequence to a Task<T> via Reactive's extension method
            task = sequence.ToTask (cancellationToken, state);

            return task;
        }
    }
}