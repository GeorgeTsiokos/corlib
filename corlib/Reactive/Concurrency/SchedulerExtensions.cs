using System;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Threading;

namespace Corlib.Reactive.Concurrency {

    public static class SchedulerExtensions {

        public static IDisposable ScheduleTimers (this IScheduler scheduler, IObservable<Unit> resetStream, params Tuple<TimeSpan, Action>[] timers) {
            var flag = new BooleanDisposable ();
            var compositeDisposable = new CompositeDisposable ();

            Action<Tuple<TimeSpan, Action>> scheduleTimer = null;
            scheduleTimer = timer => {
                if (flag.IsDisposed)
                    return;
                IDisposable disposable = null;
                disposable = scheduler.ScheduleTimer (timer.Item1, timer.Item2, () => {
                    if (null != disposable)
                        compositeDisposable.Remove (disposable);
                    scheduleTimer (timer);
                });
                compositeDisposable.Add (disposable);
            };

            foreach (var timer in timers)
                scheduleTimer (timer);

            return new CompositeDisposable (flag, compositeDisposable, resetStream.Subscribe (_ => {
                var compositeDisposableValue = compositeDisposable;
                compositeDisposable = new CompositeDisposable ();
                compositeDisposableValue.Dispose ();
            }));
        }

        public static IDisposable ScheduleTimer (this IScheduler scheduler, TimeSpan interval, Action action, Action onComplete = null) {
            if (scheduler == null)
                throw new ArgumentNullException ("scheduler", "scheduler is null.");
            if (action == null)
                throw new ArgumentNullException ("action", "action is null.");
            interval = Scheduler.Normalize (interval);
            if (TimeSpan.Zero == interval)
                throw new ArgumentOutOfRangeException ("interval");

            int onCompleteCallCount = 0;
            Action callOnComplete = () => {
                if (Interlocked.Increment (ref onCompleteCallCount) == 1 && null != onComplete)
                    onComplete ();
            };

            var multipleAssignmentDisposable = new MultipleAssignmentDisposable ();
            Action scheduledAction = null;

            scheduledAction = () => {
                TimeSpan scheduledInterval;
                do {
                    if (multipleAssignmentDisposable.IsDisposed)
                        return;
                    var stopwatch = Stopwatch.StartNew ();
                    action ();
                    // modify the interval to account for the execution time of action
                    scheduledInterval = Scheduler.Normalize (interval - stopwatch.Elapsed);
                } while (scheduledInterval == TimeSpan.Zero);

                if (multipleAssignmentDisposable.IsDisposed)
                    return;

                multipleAssignmentDisposable.Disposable = new CompositeDisposable (
                    scheduler.Schedule (scheduledInterval, scheduledAction),
                    Disposable.Create (callOnComplete));
            };

            multipleAssignmentDisposable.Disposable = new CompositeDisposable (
                scheduler.Schedule (scheduledAction),
                Disposable.Create (callOnComplete));

            return multipleAssignmentDisposable;
        }


        /// <summary>
        /// A scheduler that throws System.OperationCanceledException exceptions after the CancellationToken is signaled
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">if token can't be canceled</exception>
        public static IScheduler WithCancellation (this IScheduler scheduler, CancellationToken cancellationToken, CancellationCheckMode cancellationCheckMode) {
            if (!cancellationToken.IsCancellationRequested && !cancellationToken.CanBeCanceled)
                throw new ArgumentOutOfRangeException ("cancellationToken", "token can not be canceled, so the scheduler can not support cancellation");

            return new CancelableScheduler (scheduler, cancellationCheckMode, cancellationToken);
        }

        /// <summary>
        /// A scheduler that throws System.OperationCanceledException exceptions after the CancellationToken is signaled
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">if token can't be canceled</exception>
        public static IScheduler WithCancellation (this IScheduler scheduler, CancellationToken cancellationToken) {
            return WithCancellation (
                scheduler,
                cancellationToken,
                CancellationCheckMode.OnNow | CancellationCheckMode.OnSchedule | CancellationCheckMode.OnExecute);
        }
    }
}