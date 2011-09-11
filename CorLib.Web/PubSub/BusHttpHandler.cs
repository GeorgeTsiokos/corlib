using System;
using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Web;
using CorLib.Threading;
using CorLib.Web;

namespace CorLib.PubSub {

    public sealed class BusHttpHandler : ObservableHttpHandler {
        const int __bufferSize = 32768;
        readonly Bus _bus = Bus.Default;
        readonly ConcurrentBag<IDisposable<byte[]>> _buffer = new ConcurrentBag<IDisposable<byte[]>> ();

        void ProcessRequest (HttpContext context, IObserver<Unit> observer, IObservable<Unit> cancellationStream) {
            try {
                HttpRequest request = context.Request;
                string topic = request.Params["t"] ?? request.Params["topic"];
                bool chunked = request.Params["c"] == "1";

                if (string.IsNullOrWhiteSpace (topic))
                    throw new HttpException (400, "t or topic is required");

                switch (context.Request.HttpMethod) {
                    case "POST":
                        var topicResponse = TopicHttpResponse.FromHttpRequest (
                            context.Request, cancellationStream, _buffer, __bufferSize);

                        _bus.Publish (topic, topicResponse);
                        topicResponse.Publish.Subscribe (observer);
                        break;
                    case "GET":
                        var target = Observer.Create<TopicHttpResponse> (response =>
                                        response.Subscribe (context.Response, cancellationStream, chunked).Subscribe (observer)
                                        , observer.OnError);

                        if (chunked)
                            _bus.Subscribe (topic, cancellationStream, target);
                        else
                            _bus.Next (topic, cancellationStream, target);
                        break;
                    case "HEAD":
                        _bus.Next (topic, cancellationStream, Observer.Create<TopicHttpResponse> (response =>
                            response.Publish.Subscribe (observer)
                        , observer.OnError));
                        break;
                    default:
                        throw new HttpException (400, "HTTP method not supported");
                }
            }
            catch (Exception exception) {
                observer.OnError (exception);
            }
        }

        protected override IObservable<Unit> ProcessRequestAsync (HttpContext context) {
            return Observable.Create<Unit> (observer => {
                var disposable = new CancellationDisposable ();
                ProcessRequest (context, observer, disposable.Token.AsCancellationStream ());
                return disposable;
            });
        }
    }
}