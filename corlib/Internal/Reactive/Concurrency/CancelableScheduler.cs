using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;

namespace Corlib.Reactive.Concurrency {

    internal sealed class CancelableScheduler : IScheduler {

        readonly IScheduler _scheduler;
        CancellationCheckMode _mode;
        CancellationToken _cancellationToken;

        public CancelableScheduler (IScheduler scheduler, CancellationCheckMode mode, CancellationToken cancellationToken) {
            _scheduler = scheduler;
            _mode = mode;
            _cancellationToken = cancellationToken;
        }

        public DateTimeOffset Now {
            get {
                ThrowIfCancellationRequestedAndHasFlag (CancellationCheckMode.OnNow);
                return _scheduler.Now;
            }
        }

        public IDisposable Schedule<TState> (TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action) {
            ThrowIfCancellationRequestedAndHasFlag (CancellationCheckMode.OnSchedule);

            if (_mode.HasFlag (CancellationCheckMode.OnExecute))
                return _scheduler.Schedule (state, dueTime, (scheduler, state2) => CheckForCancellation (state2, action));
            else
                return _scheduler.Schedule (state, dueTime, action);
        }

        public IDisposable Schedule<TState> (TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action) {
            ThrowIfCancellationRequestedAndHasFlag (CancellationCheckMode.OnSchedule);

            if (_mode.HasFlag (CancellationCheckMode.OnExecute))
                return _scheduler.Schedule (state, dueTime, (scheduler, state2) => CheckForCancellation (state2, action));
            else
                return _scheduler.Schedule (state, dueTime, action);
        }

        public IDisposable Schedule<TState> (TState state, Func<IScheduler, TState, IDisposable> action) {
            ThrowIfCancellationRequestedAndHasFlag (CancellationCheckMode.OnSchedule);

            if (_mode.HasFlag (CancellationCheckMode.OnExecute))
                return _scheduler.Schedule (() => CheckForCancellation (state, action));
            else
                return _scheduler.Schedule (state, action);
        }

        IDisposable CheckForCancellation<TState> (TState state, Func<IScheduler, TState, IDisposable> action) {
            if (_cancellationToken.IsCancellationRequested)
                return Disposable.Empty;
            else
                return action (this, state);
        }

        void ThrowIfCancellationRequestedAndHasFlag (CancellationCheckMode mode) {
            if (_mode.HasFlag (mode))
                _cancellationToken.ThrowIfCancellationRequested ();
        }
    }
}