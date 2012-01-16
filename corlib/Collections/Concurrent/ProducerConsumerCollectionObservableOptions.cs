using System;
using System.Diagnostics.Contracts;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace CorLib.Collections.Concurrent {

    /// <summary>
    /// Options for converting a <see cref="ProducerConsumerCollection`1"/> to an observable sequence
    /// </summary>
    public class ProducerConsumerCollectionObservableOptions {

        readonly BehaviorSubject<TimeSpan?> _pollInterval;
        readonly BehaviorSubject<TimeSpan?> _timeout;
        readonly BehaviorSubject<int?> _boundedCapacity;
        readonly BehaviorSubject<IScheduler> _scheduler;
        readonly IObservable<Tuple<TimeSpan?, TimeSpan?, IScheduler, int?>> _sequence;

        /// <summary>
        /// Creates a new instance of options
        /// </summary>
        /// <param name="pollInterval">null to disable polling, otherwise interval to poll</param>
        /// <param name="timeout">null to complete the sequence if polling is disabled, 
        /// if polling is enabled, items will be taken from the collection until the TryTake
        /// method returns false, if there is a timeout, the timeout is passed to a 
        /// <see cref="BlockingCollection`1.TryTake"/> wrapping the collection</param>
        /// <param name="boundedCapacity">only used if timeout has a value, indicating
        /// the boundedCpacity for the <see cref="BlockingCollection`1"/></param>
        /// <param name="scheduler">scheduler to schedule the dequeue for each poll if polling is enabled
        /// or, to schedule the only an only poll if polling is disabled</param>
        public ProducerConsumerCollectionObservableOptions (
            TimeSpan? pollInterval,
            TimeSpan? timeout,
            int? boundedCapacity,
            IScheduler scheduler) {

            _pollInterval = new BehaviorSubject<TimeSpan?> (pollInterval);
            _timeout = new BehaviorSubject<TimeSpan?> (timeout);
            _boundedCapacity = new BehaviorSubject<int?> (boundedCapacity);
            _scheduler = new BehaviorSubject<IScheduler> (scheduler ??
                System.Reactive.Concurrency.Scheduler.ThreadPool);

            _sequence = PollIntervalChanged.CombineLatest (TimeoutChanged, (a, b) =>
                new Tuple<TimeSpan?, TimeSpan?> (a, b)).CombineLatest (SchedulerChanged, (a, b) =>
                new Tuple<TimeSpan?, TimeSpan?, IScheduler> (a.Item1, a.Item2, b)).CombineLatest (BoundedCapacityChanged, (a, b) =>
                new Tuple<TimeSpan?, TimeSpan?, IScheduler, int?> (a.Item1, a.Item2, a.Item3, b));
        }

        /// <summary>
        /// Creates a dedicated thread with an infinite timeout to dequeue from the sequence
        /// and route to the observable sequence
        /// </summary>
        public static ProducerConsumerCollectionObservableOptions DedicatedThread {
            get {
                return new ProducerConsumerCollectionObservableOptions (
                    null,
                    TimeSpan.MaxValue,
                    null,
                    System.Reactive.Concurrency.Scheduler.NewThread);
            }
        }

        /// <summary>
        /// Uses a 500ms poll interval with a 500ms timeout, and the thread pool
        /// scheduler
        /// </summary>
        public static ProducerConsumerCollectionObservableOptions ThreadPool {
            get {
                return new ProducerConsumerCollectionObservableOptions (
                    TimeSpan.FromMilliseconds (500),
                    TimeSpan.FromMilliseconds (500),
                    null,
                    System.Reactive.Concurrency.Scheduler.ThreadPool);
            }
        }

        /// <summary>
        /// null to disable polling, otherwise interval to poll
        /// </summary>
        public TimeSpan? PollInterval {
            get { return _pollInterval.First (); }
            set { _pollInterval.OnNext (value); }
        }

        /// <summary>
        /// Signaled when PollInterval changed
        /// </summary>
        public IObservable<TimeSpan?> PollIntervalChanged {
            get { return _pollInterval.DistinctUntilChanged (); }
        }

        /// <summary>
        /// null to complete the sequence if polling is disabled, 
        /// if polling is enabled, items will be taken from the collection until the TryTake
        /// method returns false, if there is a timeout, the timeout is passed to a 
        /// <see cref="BlockingCollection`1.TryTake"/> wrapping the collection
        /// </summary>
        public TimeSpan? Timeout {
            get { return _timeout.First (); }
            set { _timeout.OnNext (value); }
        }

        /// <summary>
        /// Signaled when Timeout changed
        /// </summary>
        public IObservable<TimeSpan?> TimeoutChanged {
            get { return _timeout.DistinctUntilChanged (); }
        }

        /// <summary>
        /// Used if timeout has a value, indicating
        /// the boundedCpacity for the <see cref="BlockingCollection`1"/>
        /// </summary>
        public int? BoundedCapacity {
            get { return _boundedCapacity.First (); }
            set {
                Contract.Requires (!value.HasValue || value.Value > 0);
                _boundedCapacity.OnNext (value);
            }
        }

        /// <summary>
        /// Signaled when BoundedCapacity changed
        /// </summary>
        public IObservable<int?> BoundedCapacityChanged {
            get { return _boundedCapacity.DistinctUntilChanged (); }
        }

        /// <summary>
        /// scheduler to schedule the dequeue for each poll if polling is enabled
        /// or, to schedule the only an only poll if polling is disabled
        /// </summary>
        public IScheduler Scheduler {
            get { return _scheduler.First (); }
            set { _scheduler.OnNext (value); }
        }

        /// <summary>
        /// Signals when Scheduler changed
        /// </summary>
        public IObservable<IScheduler> SchedulerChanged {
            get { return _scheduler.DistinctUntilChanged (); }
        }

        internal IObservable<Tuple<TimeSpan?, TimeSpan?, IScheduler, int?>> Sequence {
            get {
                return _sequence;
            }
        }
    }
}