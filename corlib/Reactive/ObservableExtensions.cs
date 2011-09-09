using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using CorLib.Collections.Concurrent;
using CorLib.Diagnostics;

namespace CorLib.Reactive {

    public static class ObservableExtensions {

        /// <summary>Invokes a specified action after source observable sequence terminates normally or by an exception</summary>
        /// <param name="sequence">Source sequence</param>
        /// <param name="finallyAction">Action to invoke after the source observable sequence terminates</param>
        /// <returns>Source sequence with the action-invoking termination behavior applied</returns>
        /// <remarks>Propagates exceptions from the finally action through the observable sequence</remarks>
        public static IObservable<T> Finally2<T> (this IObservable<T> sequence, Action finallyAction) {
            return Observable.Create<T> (observer =>
                 sequence.Subscribe (observer.OnNext, ex => {
                     try {
                         finallyAction ();
                     }
                     catch (Exception exception) {
                         observer.OnError (new AggregateException (ex, exception));
                     }
                 }, () => {
                     try {
                         finallyAction ();
                         observer.OnCompleted ();
                     }
                     catch (Exception exception) {
                         observer.OnError (exception);
                     }
                 }));
        }

        public static IObservable<T> Using<T> (this IObservable<T> sequence, params IDisposable[] disposables) {
            return Using (
                sequence,
                new CompositeDisposable (disposables));
        }

        public static IObservable<T> Using<T> (this IObservable<T> sequence, IDisposable disposable) {
            return sequence.Finally2 (disposable.Dispose);
        }

        public static IObservable<T> ContinueWith<T> (this IObservable<T> sequence, Func<IObservable<T>> nextSequence) {
            return sequence.Concat<T> (Observable.Defer<T> (nextSequence));
        }

        public static IObservable<T> IgnoreElementsContinueWith<X, T> (this IObservable<X> sequence, Func<IObservable<T>> nextSequence) {
            return sequence.IgnoreElements ().Select (_ => default (T)).ContinueWith<T> (nextSequence);
        }

#if EXPERIMENTAL
        [ExperimentalAttribute (State = ExperimentalState.RequiresFeedback)]
        public static Func<TKey, ISubject<IObservable<TValue>>> ToKeyedSubject<TKey, TValue> (this Func<TKey, ISubject<IObservable<TValue>>> factory) {
            return ToKeyedSubject (factory, EqualityComparer<TKey>.Default);
        }

        [ExperimentalAttribute (State = ExperimentalState.RequiresFeedback)]
        public static Func<TKey, ISubject<IObservable<TValue>>> ToKeyedSubject<TKey, TValue> (this Func<TKey, ISubject<IObservable<TValue>>> factory, IEqualityComparer<TKey> comparer) {
            var dictionary = new ConcurrentDictionary<TKey, ISubject<IObservable<TValue>>> (comparer);
            Func<TKey, ISubject<IObservable<TValue>>> factory_ = key => {
                ISubject<IObservable<TValue>> subject;
                try {
                    subject = factory (key);
                }
                catch (Exception exception) {
                    subject = new BehaviorSubject<IObservable<TValue>>(Observable.Throw<TValue> (exception));
                }
                subject.Finally (() => dictionary.TryRemove (key)).Subscribe ();
                return subject;
            };
            return key => dictionary.GetOrAdd (key, key_ =>
                factory_ (key_));
        }

        /// <summary>
        /// Detects conncurrent OnNext calls from <paramref name="sequence"/>
        /// </summary>
        /// <typeparam name="T">sequence type</typeparam>
        /// <param name="sequence">sequence to watch</param>
        /// <param name="timeSpan">window of time to detect concurrent signals</param>
        /// <returns>a sequence that completes when concurrent signals are detected</returns>
        [ExperimentalAttribute (State = ExperimentalState.RequiresAdditionalTesting)]
        public static IObservable<Unit> SignalsConcurrently<T> (this IObservable<T> sequence, TimeSpan timeSpan) {
            var flag = new ManualResetEventSlim ();
            int counter = 0;
            return Observable.Create<Unit> (observer =>
                sequence.Subscribe (value => {
                    var result = Interlocked.Increment (ref counter);
                    if (1 != result) {
                        flag.Set ();
                        observer.OnCompleted ();
                    }
                    flag.Wait (timeSpan);
                    Interlocked.Decrement (ref counter);
                })).Using (flag);
        }
#endif
    }
}