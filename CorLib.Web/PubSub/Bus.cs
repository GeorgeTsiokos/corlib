using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CorLib.Reactive;
using System.Reactive;

namespace CorLib.PubSub {

    public sealed class Bus {

        static readonly Lazy<Bus> _instance = new Lazy<Bus> (true);
        readonly Func<string, ISubject<Topic>> _factory;

        public Bus (IEqualityComparer<string> topicComparer) {
            _factory = CreateTopic;
            _factory = _factory.CacheSubject (topicComparer);
        }

        public Bus ()
            : this (StringComparer.OrdinalIgnoreCase) {
        }

        public static Bus Default {
            get {
                return _instance.Value;
            }
        }

        public Topic this[string name] {
            get {
                return _factory (name).First ();
            }
            set {
                _factory (name).OnNext (value);
            }
        }

        public void Publish (string name, TopicHttpResponse topicResponse) {
            this[name].Stream.OnNext (topicResponse);
        }

        public IDisposable Next (string name, IObservable<Unit> cancellationStream, IObserver<TopicHttpResponse> observer) {
            return this[name].Stream.TakeUntil (cancellationStream).Take (1).Subscribe (observer);
        }

        public IDisposable Subscribe (string name, IObservable<Unit> cancellationStream, IObserver<TopicHttpResponse> observer) {
            return this[name].Stream.TakeUntil (cancellationStream).Subscribe (observer);
        }

        public void OnNext (Topic topic) {
            var existing = _factory (topic.Name);
            existing.OnNext (topic);
        }

        public void OnCompleted (Topic topic) {
            _factory (topic.Name).OnCompleted ();
        }

        public void OnCompleted (string topic) {
            _factory (topic).OnCompleted ();
        }

        static ISubject<Topic> CreateTopic (string name) {
            var topic = new Topic (
                            name,
                            new Subject<TopicHttpResponse> ());

            return new BehaviorSubject<Topic> (topic);
        }
    }
}