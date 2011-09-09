using System;
using System.Reactive.Concurrency;
using System.Threading;

namespace CorLib.Reactive.Concurrency {

    public static class SchedulerExtensions {

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