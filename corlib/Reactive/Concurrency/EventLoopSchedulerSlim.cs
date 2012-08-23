using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using Corlib.Diagnostics;
using Corlib.Threading;

namespace Corlib.Reactive.Concurrency {

    /// <summary>
    /// Represents an object that schedules units of work to execute sequentially
    /// </summary>
    [DebuggerNonUserCode]
    [DebuggerDisplay ("Count = {Count}")]
    public class EventLoopSchedulerSlim : IScheduler {

        readonly Gate _gate = new Gate ();
        readonly ConcurrentQueue<Action> _queue = new ConcurrentQueue<Action> ();
        readonly IScheduler _scheduler;
        readonly Action<Exception> _exceptionHandler;

        /// <summary>
        /// Creates a new instance with the specified scheduler and unhandled exception handler
        /// </summary>
        /// <param name="scheduler">the scheduler to schedule the execution loop on</param>
        /// <param name="unhandledException">excheption scheduler</param>
        public EventLoopSchedulerSlim (IScheduler scheduler, Action<Exception> unhandledException) {
            _scheduler = scheduler;
            _exceptionHandler = ExceptionLogger.Wrap (typeof (EventLoopSchedulerSlim), unhandledException);
        }

        /// <summary>
        /// Creates a new instance with the specified scheduler and unhandled exception handler
        /// </summary>
        /// <param name="scheduler">the scheduler to schedule the execution loop on</param>
        public EventLoopSchedulerSlim (IScheduler scheduler) {
            _scheduler = scheduler;
            _exceptionHandler = ExceptionHandler.Default.GetHandler<EventLoopSchedulerSlim> ();
        }

        /// <summary>
        /// Creates a new instance using the thread pool to schedule items to execute sequentially
        /// </summary>
        public EventLoopSchedulerSlim ()
            : this (Scheduler.ThreadPool) {
        }

        /// <summary>
        /// Gets the scheduler's notion of current time
        /// </summary>
        public DateTimeOffset Now { get { return _scheduler.Now; } }

        /// <summary>
        /// Gets the number of outstanding work items
        /// </summary>
        public int Count { get { return _queue.Count; } }

        public IDisposable Schedule<TState> (TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action) {
            return _scheduler.Schedule (state, dueTime, (scheduler, state2) =>
                ScheduleFunc (state2, action));
        }

        public IDisposable Schedule<TState> (TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action) {
            return _scheduler.Schedule (state, dueTime, (scheduler, state2) =>
                ScheduleFunc (state2, action));
        }

        public IDisposable Schedule<TState> (TState state, Func<IScheduler, TState, IDisposable> action) {
            return ScheduleFunc (state, action);
        }

        IDisposable ScheduleFunc<TState> (TState state, Func<IScheduler, TState, IDisposable> action) {
            var result = new SingleAssignmentDisposable ();

            _queue.Enqueue (() => {
                if (!result.IsDisposed)
                    result.Disposable = action (this, state);
            });

            if (_gate.TryOpen ())
                _scheduler.Schedule (Loop);

            return result;
        }

        void Loop () {
            Action action;
            while (_queue.TryDequeue (out action))
                try {
                    action ();
                }
                catch (Exception exception) {
                    _exceptionHandler (exception);
                }

            _gate.Close ();
        }
    }
}