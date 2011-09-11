using System;
using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Web;
using CorLib.IO;

namespace CorLib.PubSub {

    public sealed class TopicHttpResponse {
        const string __contentLengthHeader = "Content-Length";
        const string __transferEncoding = "Transfer-Encoding";
        readonly string _contentLength;
        readonly string _contentType;
        readonly IObservable<Tuple<IDisposable<byte[]>, int>> _stream;

        public TopicHttpResponse (string contentLength, string contentType, IObservable<Tuple<IDisposable<byte[]>, int>> stream) {
            _contentLength = contentLength;
            _contentType = contentType;
            _stream = stream;
            Publish = stream.IgnoreElements ().Select (_ => new Unit ());
        }

        public static TopicHttpResponse FromHttpRequest (HttpRequest request, IObservable<Unit> cancellationStream, IProducerConsumerCollection<IDisposable<byte[]>> cache, int bufferSize) {
            var stream = request.GetBufferlessInputStream ().ReadAsync (
                bufferSize, false, cache).TakeUntil (
                cancellationStream).Publish ().RefCount ();

            return new TopicHttpResponse (
                request.Headers[__contentLengthHeader],
                request.ContentType,
                stream);
        }

        public IObservable<Unit> Publish { get; private set; }

        public IObservable<Unit> Subscribe (HttpResponse response, IObservable<Unit> cancellationStream, bool chunked) {
            response.Buffer = false;
            response.BufferOutput = false;

            if (chunked)
                response.AddHeader (__transferEncoding, "chunked");
            else {
                if (null != _contentLength)
                    response.AddHeader (__contentLengthHeader, _contentLength);
            }
            response.ContentType = _contentType;

            var result = _stream.WriteAsync (
                response.OutputStream).TakeUntil (
                cancellationStream).IgnoreElements ().Select (_ =>
                    new Unit ());

            if (chunked)
                return result.Concat (cancellationStream);
            else
                return result;
        }
    }
}