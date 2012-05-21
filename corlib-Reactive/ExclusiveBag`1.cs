using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;

namespace Corlib {

    internal sealed class ExclusiveBag<T> : IExclusive<T> {

        readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource ();
        CancellationToken _token;
        readonly IScheduler _scheduler;
        readonly BlockingCollection<IExclusive<T>> _bag;

        public ExclusiveBag (Func<IDisposable<T>> disposableValueFactory, int maximumInstanceCount) {
            Contract.Requires (disposableValueFactory != null, "disposableValueFactory is null.");
            Contract.Requires (maximumInstanceCount > 0, "maximumInstanceCount < 1");

            _token = _cancellationTokenSource.Token;

            _scheduler = Scheduler.ThreadPool.WithCancellation (
                _token,
                CancellationCheckMode.OnExecute);

            _bag = new BlockingCollection<IExclusive<T>> (
                new ConcurrentStack<IExclusive<T>> ());

            for (int i = 0; i < maximumInstanceCount; i++)
                _bag.Add (
                    disposableValueFactory ().ToExclusive (),
                    _cancellationTokenSource.Token);
        }

        public IDisposable Schedule (Action<T> action) {
            _token.ThrowIfCancellationRequested ();

            var result = new MultipleAssignmentDisposable ();
            result.Disposable = _scheduler.Schedule (() => {
                var exclusive = _bag.Take (_cancellationTokenSource.Token);
                result.Disposable = exclusive.Schedule (value => {
                    try {
                        action (value);
                    }
                    finally {
                        if (_cancellationTokenSource.IsCancellationRequested)
                            exclusive.TryDispose ();
                        else
                            _bag.Add (exclusive);
                    }
                });
            });
            return result;
        }

        public void Dispose () {
            if (_cancellationTokenSource.IsCancellationRequested)
                return;

            _cancellationTokenSource.Cancel ();
            Parallel.ForEach (
                _bag.GetConsumingEnumerable (), item =>
                    item.TryDispose ());
        }
    }
}