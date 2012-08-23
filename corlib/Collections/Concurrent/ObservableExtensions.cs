using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using Corlib.Threading;

namespace Corlib.Collections.Concurrent {

    public static class ObservableExtensions {

        public static IDisposable Subscribe<TKey, TValue> (this IObservable<TValue> sequence, ConcurrentDictionary<TKey, TValue> dictionary, Func<TValue, TKey> keySelector) {
            return Subscribe<TValue, TKey, TValue> (sequence, dictionary, keySelector, (key, value) => value);
        }

        public static IDisposable Subscribe<T, TKey, TValue> (this IObservable<T> sequence, ConcurrentDictionary<TKey, TValue> dictionary, Func<T, TKey> keySelector, Func<TKey, T, TValue> valueSelector, Func<T, bool> addOrUpdate = null, Func<T, bool> remove = null, Func<TKey, TValue, TValue> updateValueFactory = null) {
            Contract.Requires (sequence != null, "sequence is null.");
            Contract.Requires (dictionary != null, "dictionary is null.");
            Contract.Requires (keySelector != null, "keySelector is null.");
            Contract.Requires (valueSelector != null, "valueSelector is null.");

            if (null == updateValueFactory)
                updateValueFactory = (key, value) => value;

            if (null == addOrUpdate)
                if (null == remove)
                    return sequence.Subscribe (item =>
                        dictionary.AddOrUpdate (keySelector (item), key_ => valueSelector (key_, item), updateValueFactory)
                    );
                else
                    return sequence.Subscribe (item => {
                        if (remove (item))
                            dictionary.TryRemove (keySelector (item));
                        else
                            dictionary.AddOrUpdate (keySelector (item), key_ => valueSelector (key_, item), updateValueFactory);
                    }
                );
            else
                if (null == remove)
                    return sequence.Where (addOrUpdate).Subscribe (item =>
                            dictionary.AddOrUpdate (keySelector (item), key_ => valueSelector (key_, item), updateValueFactory)
                    );
                else
                    return sequence.Subscribe (item => {
                        if (addOrUpdate (item))
                            dictionary.AddOrUpdate (keySelector (item), key_ => valueSelector (key_, item), updateValueFactory);
                        else if (remove (item))
                            dictionary.TryRemove (keySelector (item));
                    });
        }

        /// <summary>
        /// Each item published to the observable sequence is added to the collection
        /// </summary>
        /// <param name="sequence">input observable sequence</param>
        /// <param name="collection">collection to add the items to</param>
        /// <returns>a <see cref="SingleAssignmentDisposable"/> to check if a subscription has ended (due to a false TryAdd) 
        /// or to unsubscribe the collection from the sequence</returns>
        /// <remarks>if the collection returns false during TryAdd the collection unsubscribes from the sequence</remarks>
        public static SingleAssignmentDisposable Subscribe<T> (this IObservable<T> sequence, IProducerConsumerCollection<T> collection) {
            var subscription = new SingleAssignmentDisposable ();
            subscription.Disposable = sequence.Subscribe (value => {
                if (!collection.TryAdd (value))
                    subscription.Dispose ();
            });
            return subscription;
        }

        /// <summary>
        /// Converts a <see cref="IProducerConsumerCollection`1"/> to an observable sequence
        /// </summary>
        /// <param name="collection">collection to convert to a sequence</param>
        /// <param name="options">timeout, poll interval, boundedCapacity, and scheduler</param>
        /// <returns>an observable sequence</returns>
        public static IObservable<T> ToObservable<T> (this IProducerConsumerCollection<T> collection, ProducerConsumerCollectionObservableOptions options) {
            Action take;
            var multipleAssignmentDisposable = new MultipleAssignmentDisposable ();

            return Observable.Create<T> (observer => {

                Action takeCompleted = observer.OnCompleted;
                var sequence = options.Sequence.Publish ();
                var cancellationToken = sequence.Take (1).ToCancellationTokenSource ().Token;

                var optionSubscription = sequence.Subscribe (value => {
                    var timeout = value.Item1;
                    var pollInterval = value.Item2;
                    var scheduler = value.Item3;
                    var boundedCapacity = value.Item4;

                    if (timeout.HasValue) {
                        var millisecondsTimeout = timeout.AsThreadingTimeout ();

                        var blockingCollection = boundedCapacity.HasValue ?
                            new BlockingCollection<T> (collection, boundedCapacity.Value) :
                            new BlockingCollection<T> (collection);

                        if (pollInterval.HasValue)
                            takeCompleted = () => {
                                if (!cancellationToken.IsCancellationRequested)
                                    multipleAssignmentDisposable.Disposable = scheduler.Schedule (pollInterval.Value, () =>
                                        Take (blockingCollection, observer, millisecondsTimeout, cancellationToken, takeCompleted));
                            };

                        take = () =>
                            multipleAssignmentDisposable.Disposable = scheduler.Schedule (() =>
                                Take (blockingCollection, observer, millisecondsTimeout, cancellationToken, takeCompleted));
                    }
                    else {
                        if (pollInterval.HasValue)
                            takeCompleted = () => {
                                if (!cancellationToken.IsCancellationRequested)
                                    multipleAssignmentDisposable.Disposable = scheduler.Schedule (pollInterval.Value, () =>
                                        Take (collection, observer, takeCompleted));
                            };

                        take = () =>
                            multipleAssignmentDisposable.Disposable = scheduler.Schedule (() =>
                                Take (collection, observer, takeCompleted));
                    }

                    multipleAssignmentDisposable.Disposable = options.Scheduler.Schedule (take);
                });

                return new CompositeDisposable (
                    sequence.Connect (),
                    multipleAssignmentDisposable,
                    optionSubscription);
            });
        }

        static void Take<T> (IProducerConsumerCollection<T> collection, IObserver<T> observer, Action takeCompleted) {
            T item;
            while (collection.TryTake (out item))
                observer.OnNext (item);

            takeCompleted ();
        }

        static void Take<T> (BlockingCollection<T> collection, IObserver<T> observer, int timeout, CancellationToken cancellationToken, Action takeCompleted) {
            T item;
            while (collection.TryTake (out item, timeout, cancellationToken))
                observer.OnNext (item);

            takeCompleted ();
        }
    }
}