using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using CorLib.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.ComponentModel;

namespace CorLib.Reactive.Linq {

    public static class ObservableExtensions {

        public static IObservable<Tuple<A, B>> CombineLatest<A, B> (this IObservable<A> sequeceA, IObservable<B> sequenceB) {
            return sequeceA.CombineLatest<A, B, Tuple<A, B>> (sequenceB, (a, b) => new Tuple<A, B> (a, b));
        }

        /// <summary>Returns a filtered sequence where each value T is not equal to null</summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static IObservable<T> NotNull<T> (this IObservable<T> sequence) where T : class {
            Contract.Requires (sequence != null, "sequence is null.");
            return sequence.Where (item => null != item);
        }

        /// <summary>
        /// Returns a filtered sequence where each value T is not equal to null or whitespace
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static IObservable<string> NotNullOrWhitespace (this IObservable<string> sequence) {
            Contract.Requires (sequence != null, "sequence is null.");
            return sequence.Where (item => !string.IsNullOrWhiteSpace (item));
        }

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

        /// <summary>
        /// Invokes Dispose on each disposable value
        /// </summary>
        /// <typeparam name="T">value type</typeparam>
        /// <param name="sequence">source sequence of disposable values</param>
        /// <returns>the dispose result</returns>
        public static IObservable<bool> Using<T> (this IObservable<IDisposable<T>> sequence) {
            if (sequence == null)
                throw new ArgumentNullException ("sequence", "sequence is null.");
            return sequence.Select (value => value.TryDispose ());
        }


        public static IObservable<T> ContinueWith<T> (this IObservable<T> sequence, Func<IObservable<T>> nextSequence) {
            return sequence.Concat<T> (Observable.Defer<T> (nextSequence));
        }

        public static IObservable<T> ContinueWith<X, T> (this IObservable<X> sequence, Func<IObservable<T>> nextSequence) {
            return sequence.IgnoreElements ().Select (_ =>
                default (T)).ContinueWith<T> (nextSequence);
        }

        public static Func<TKey, ISubject<TValue>> CacheSubject<TKey, TValue> (this Func<TKey, ISubject<TValue>> factory) {
            return CacheSubject (factory, EqualityComparer<TKey>.Default);
        }

        public static Func<TKey, ISubject<TValue>> CacheSubject<TKey, TValue> (this Func<TKey, ISubject<TValue>> factory, IEqualityComparer<TKey> comparer) {
            var dictionary = new ConcurrentDictionary<TKey, ISubject<TValue>> (comparer);
            Func<TKey, ISubject<TValue>> factory_ = key => {
                ISubject<TValue> subject;
                try {
                    subject = factory (key);
                    subject.Finally (() => dictionary.TryRemove (key)).Subscribe ();
                }
                catch (Exception exception) {
                    //TODO: verify
                    subject = new AsyncSubject<TValue> ();
                    Observable.Throw<TValue> (exception).Subscribe (subject);
                }
                return subject;
            };
            return key => dictionary.GetOrAdd (key, factory_);
        }

        /// <summary>
        /// Detects conncurrent OnNext calls from <paramref name="sequence"/>
        /// </summary>
        /// <typeparam name="T">sequence type</typeparam>
        /// <param name="sequence">sequence to watch</param>
        /// <param name="timeSpan">window of time to detect concurrent signals</param>
        /// <returns>a sequence that completes when concurrent signals are detected</returns>
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
    }
}