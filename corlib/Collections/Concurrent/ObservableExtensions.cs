using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using CorLib.Diagnostics;
using CorLib.Reactive;
using CorLib.Threading;

#if EXPERIMENTAL
namespace CorLib.Collections.Concurrent {

    [Experimental (State = ExperimentalState.RequiresFeedback)]
    public static class ObservableExtensions {

        public static IDisposable Subscribe<T> (this IObservable<T> sequence, IProducerConsumerCollection<T> collection) {
            var subscription = new SingleAssignmentDisposable ();
            subscription.Disposable = sequence.Subscribe (value => {
                if (!collection.TryAdd (value))
                    subscription.Dispose ();
            });
            return subscription;
        }

        public static IObservable<T> ToObservable<T> (this IEnumerable<IProducerConsumerCollection<T>> collections, ProducerConsumerCollectionObservableOptions options) {
            //TODO: divide the option values across the number of collections
            List<IObservable<T>> results = new List<IObservable<T>> ();
            foreach (var collection in collections)
                results.Add (collection.ToObservable<T> (options));
            return Observable.Concat (results);
        }

        public static IObservable<T> ToObservable<T> (this IProducerConsumerCollection<T> collection) {
            return ToObservable (collection, new ProducerConsumerCollectionObservableOptions ());
        }

        [Experimental]
        public static IObservable<T> ToObservable<T> (this IProducerConsumerCollection<T> collection, ProducerConsumerCollectionObservableOptions options) {
            var cts = new CancellationTokenSource ();
            var globalToken = cts.Token;

            return Observable.Create<T> (observer => {

                var blockingCollectionSequence = options.BoundedCapacityChanged.Select (value =>
                    value.HasValue ? new BlockingCollection<T> (collection, value.Value) :
                    new BlockingCollection<T> (collection));
                var minDegreeOfParallelismSequence = options.MinDegreeOfParallelismChanged;
                var blockingWorkerSchedulerSequence = options.BlockingWorkerSchedulerChanged;

                /* Restart the blocking workers when the following properties change
                 * BoundedCapacity
                 * MinDegreeOfParallelism
                 * BlockingWorkerScheduler */
                var blockingWorkerSequence = blockingCollectionSequence.CombineLatest (minDegreeOfParallelismSequence, (bc, mdop) =>
                    new { bc, mdop }).CombineLatest (blockingWorkerSchedulerSequence, (a, hs) =>
                        new { bc = a.bc, mdop = a.mdop, hs = hs }).TakeUntil (globalToken).Publish ().RefCount ();

                blockingWorkerSequence.Subscribe (state => {
                    int minWorkerCount = 0;
                    var token = blockingWorkerSequence.Take (1).AsCancellationToken ().First ();

                    while (minWorkerCount++ < state.mdop) {
                        state.hs.Schedule (() => {
                            foreach (T item in state.bc.GetConsumingEnumerable (token))
                                observer.OnNext (item);
                        });
                    }
                });

                var workerCountSequence = minDegreeOfParallelismSequence;
                var millisecondsTimeoutSequence = options.WorkerTimeoutChanged.Select (value => Convert.ToInt32 (value.TotalMilliseconds));
                var timeoutSchedulerSequence = options.LightSchedulerChanged;
                var timeoutWorkerSequence = workerCountSequence.CombineLatest (blockingCollectionSequence, (wc, bc) =>
                    new { wc, bc }).CombineLatest (millisecondsTimeoutSequence, (a, wt) =>
                        new { wc = a.wc, bc = a.bc, wt }).CombineLatest (timeoutSchedulerSequence, (b, ts) =>
                            new { wc = b.wc, bc = b.bc, wt = b.wt, ts = ts }).TakeUntil (globalToken).Publish ().RefCount ();

                timeoutWorkerSequence.Subscribe (state => {
                    int workerCount = state.wc;
                    var token = timeoutWorkerSequence.Take (1).AsCancellationToken ().First ();

                    Action worker = () => {
                        T item;
                        try {
                            while (state.bc.TryTake (out item, state.wt, token))
                                observer.OnNext (item);
                        }
                        finally {
                            Interlocked.Decrement (ref workerCount);
                        }
                    };

                    Action addWorker = () => {
                        Interlocked.Increment (ref workerCount);
                        state.ts.Schedule (worker);
                    };

                    addWorker ();

                    var blockingCollection = state.bc;
                    var taskScheduler = state.ts;
                    int workerCreationTimeout = 50;
                    TimeSpan workerCreationInterval = TimeSpan.FromSeconds (workerCount * 30);
                    int workerCreationPeriod = 10;
                    int maxDegreeOfParallelism = options.MaxDegreeOfParallelism;
                    Action workerFactory = null;

                    workerFactory = () => {
                        try {
                            if (workerCount >= maxDegreeOfParallelism)
                                return;

                            T item;
                            for (int i = 0; i < workerCreationPeriod; i++) {
                                if (!blockingCollection.TryTake (out item, workerCreationTimeout, token))
                                    return;
                                observer.OnNext (item);
                            }
                            addWorker ();
                        }
                        finally {
                            taskScheduler.Schedule (workerCreationInterval, workerFactory);
                        }
                    };
                    taskScheduler.Schedule (workerCreationInterval, workerFactory);
                });

                return new CancellationDisposable (cts);
            }).Using (cts);
        }
    }
}
#endif