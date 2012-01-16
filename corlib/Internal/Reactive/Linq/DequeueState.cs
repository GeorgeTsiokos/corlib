using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using CorLib.Collections.Concurrent;

namespace CorLib.Internal.Reactive.Linq {

    internal sealed class DequeueState {

        readonly ProducerConsumerCollectionObservableOptions _options;
        int _workers;

        public DequeueState (ProducerConsumerCollectionObservableOptions options, int workers) {
            _options = options;
            _workers = workers < 1 ? 1 : workers;
        }

        public IObservable<IScheduler> LightScheduler {
            get { return _options.LightSchedulerChanged; }
        }

        public IObservable<IScheduler> HeavyScheduler {
            get { return _options.HeavySchedulerChanged; }
        }

        public IObservable<int> MinDegreeOfParallelism {
            get { return _options.MinDegreeOfParallelismChanged.Select (value => 
                value / _workers); }
        }

        public IObservable<int> MaxDegreeOfParallelism {
            get { return _options.MaxDegreeOfParallelismChanged.Select (value => 
                value / _workers); }
        }


        public IObservable<int?> BoundedCapacity {
            get {
                return _options.BoundedCapacityChanged.Select (value =>
                    value.HasValue ? value.Value / _workers : value);
            }
        }
    }
}
