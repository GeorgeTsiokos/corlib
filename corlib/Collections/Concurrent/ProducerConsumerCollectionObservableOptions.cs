using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CorLib.Diagnostics;

#if EXPERIMENTAL
namespace CorLib.Collections.Concurrent {

    [Experimental (State = ExperimentalState.RequiresFeedback)]
    public class ProducerConsumerCollectionObservableOptions {

        readonly BehaviorSubject<IScheduler> _lightScheduler;
        readonly BehaviorSubject<IScheduler> _blockingWorkerScheduler;
        readonly BehaviorSubject<int> _minDegreeOfParallelism;
        readonly BehaviorSubject<int> _maxDegreeOfParallelism;
        readonly BehaviorSubject<TimeSpan> _workerTimeout;
        readonly BehaviorSubject<int?> _boundedCapacity;

        public ProducerConsumerCollectionObservableOptions ()
            : this (
                Scheduler.ThreadPool,
                Scheduler.NewThread,
                0,
                int.MaxValue,
                TimeSpan.FromMinutes (1),
            null) {
            //TODO: optimize processor count capture
        }

        public ProducerConsumerCollectionObservableOptions (IScheduler lightScheduler, IScheduler heavyScheduler, int minDegreeOfParallelism, int maxDegreeOfParallelism, TimeSpan workerTimeout, int? boundedCapacity) {
            _lightScheduler = new BehaviorSubject<IScheduler> (lightScheduler);
            _blockingWorkerScheduler = new BehaviorSubject<IScheduler> (heavyScheduler);
            _minDegreeOfParallelism = new BehaviorSubject<int> (minDegreeOfParallelism);
            _maxDegreeOfParallelism = new BehaviorSubject<int> (maxDegreeOfParallelism);
            _workerTimeout = new BehaviorSubject<TimeSpan> (workerTimeout);
            _boundedCapacity = new BehaviorSubject<int?> (boundedCapacity);
        }

        public IScheduler LightScheduler {
            get { return _lightScheduler.First (); }
            set {
                if (null == value)
                    throw new ArgumentNullException ();
                _lightScheduler.OnNext (value);
            }
        }

        public IObservable<IScheduler> LightSchedulerChanged {
            get { return _lightScheduler.AsObservable (); }
        }

        public IScheduler BlockingWorkerScheduler {
            get { return _blockingWorkerScheduler.First (); }
            set {
                if (null == value)
                    throw new ArgumentNullException ();
                _blockingWorkerScheduler.OnNext (value);
            }
        }

        public IObservable<IScheduler> BlockingWorkerSchedulerChanged {
            get { return _blockingWorkerScheduler.AsObservable (); }
        }

        public int MinDegreeOfParallelism {
            get { return _minDegreeOfParallelism.First (); }
            set {
                if (0 > value)
                    throw new ArgumentOutOfRangeException ();
                _minDegreeOfParallelism.OnNext (value);
            }
        }

        public IObservable<int> MinDegreeOfParallelismChanged {
            get { return _maxDegreeOfParallelism.AsObservable (); }
        }

        public int MaxDegreeOfParallelism {
            get { return _maxDegreeOfParallelism.First (); }
            set {
                if (0 > value)
                    throw new ArgumentOutOfRangeException ();
                _maxDegreeOfParallelism.OnNext (value);
            }
        }

        public IObservable<int> MaxDegreeOfParallelismChanged {
            get { return _maxDegreeOfParallelism.AsObservable (); }
        }

        public TimeSpan WorkerTimeout {
            get { return _workerTimeout.First (); }
            set {
                if (TimeSpan.Zero > value)
                    throw new ArgumentOutOfRangeException ();
                _workerTimeout.OnNext (value);
            }
        }

        public IObservable<TimeSpan> WorkerTimeoutChanged {
            get { return _workerTimeout.AsObservable (); }
        }

        public int? BoundedCapacity {
            get { return _boundedCapacity.First (); }
            set {
                if (value.HasValue && 1 > value.Value)
                    throw new ArgumentOutOfRangeException ();
                _boundedCapacity.OnNext (value);
            }
        }

        public IObservable<int?> BoundedCapacityChanged {
            get { return _boundedCapacity.AsObservable (); }
        }
    }
}
#endif