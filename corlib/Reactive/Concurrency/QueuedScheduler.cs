using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using Corlib.Threading;

namespace Corlib.Reactive.Concurrency {

    /// <summary>
    /// A scheduler where work items are queued and must be started for items to execute
    /// </summary>
    [DebuggerNonUserCode]
    [DebuggerDisplay ("Count = {Count} IsRunning = {IsRunning}")]
    public sealed class QueuedScheduler : IScheduler {

        volatile bool _direct;
        volatile bool _enabled;
        readonly Gate _gate = new Gate ();
        readonly ConcurrentQueue<object> _queue = new ConcurrentQueue<object> ();
        readonly IScheduler _scheduler;
        readonly Action<Exception> _exceptionHandler;

        public QueuedScheduler (IScheduler scheduler, Action<Exception> unhandledException) {
            _scheduler = scheduler;
            _exceptionHandler = unhandledException;
        }

        public DateTimeOffset Now {
            get { return _scheduler.Now; }
        }

        /// <summary>
        /// Gets the number of outstanding work items
        /// </summary>
        public int Count { get { return _queue.Count; } }

        public bool Enabled {
            get {
                return _enabled;
            }
            set {
                if (_enabled == value)
                    return;

                _direct = false;
                _enabled = value;

                if (value)
                    TryStartLoop ();
                else
                    _gate.TryClose ();
            }
        }

        public void Start () {
            Enabled = true;
        }

        public void Stop () {
            Enabled = false;
        }

        void TryStartLoop () {
            if (_gate.TryOpen ())
                Scheduler.ThreadPool.Schedule (Loop);
        }

        void Loop () {
            dynamic item;
            while (_enabled && _queue.TryDequeue (out item))
                try {
                    SingleAssignmentDisposable singleAssignmentDisposable = item.item1;
                    if (!singleAssignmentDisposable.IsDisposed)
                        singleAssignmentDisposable.Disposable = _scheduler.Schedule (item.Item2, item.item3);
                }
                catch (Exception exception) {
                    _exceptionHandler (exception);
                }

            _direct = _enabled;
            _gate.Close ();
        }

        public IDisposable Schedule<TState> (TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action) {
            var disposable = new MultipleAssignmentDisposable ();
            disposable.Disposable = Scheduler.ThreadPool.Schedule (
                dueTime, () => Schedule (state, (s1, s2) => action (s1, s2), disposable));
            return disposable;
        }

        public IDisposable Schedule<TState> (TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action) {
            var disposable = new MultipleAssignmentDisposable ();
            disposable.Disposable = Scheduler.ThreadPool.Schedule (
                dueTime, () => Schedule (state, (s1, s2) => action (s1, s2), disposable));
            return disposable;
        }

        public IDisposable Schedule<TState> (TState state, Func<IScheduler, TState, IDisposable> action) {
            return Schedule (state, action, new MultipleAssignmentDisposable ());
        }

        IDisposable Schedule<TState> (TState state, Func<IScheduler, TState, IDisposable> action, MultipleAssignmentDisposable disposable) {
            if (_direct)
                return _scheduler.Schedule (state, action);
            
            _queue.Enqueue (new Tuple<MultipleAssignmentDisposable, TState, Func<IScheduler, TState, IDisposable>> (
                disposable, state, action));

            TryStartLoop ();

            return disposable;
        }
    }
}