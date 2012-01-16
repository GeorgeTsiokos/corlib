using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using CorLib.Collections.Concurrent;

namespace CorLib.IO {

    public static class StreamExtensions {

        public static IObservable<byte[]> ReadAsync (this Stream stream, int bufferSize) {
            return stream.ReadAsync (bufferSize, true, false).Select (value => {
                byte[] result = new byte[value.Item2];
                using (IDisposable<byte[]> source = value.Item1) {
                    Array.Copy (source.Value, 0, result, 0, value.Item2);
                }
                return result;
            });
        }

        public static IObservable<Tuple<IDisposable<byte[]>, int>> ReadAsync (this Stream stream, int bufferSize, bool waitForObserver, bool refCount) {
            return ReadAsync (stream, bufferSize, waitForObserver, refCount, new ConcurrentBag<IDisposable<byte[]>> ());
        }

        public static IObservable<Tuple<IDisposable<byte[]>, int>> ReadAsync (this Stream stream, int bufferSize, bool waitForObserver, bool refCount, IProducerConsumerCollection<IDisposable<byte[]>> cache) {
            var read = Observable.FromAsyncPattern<byte[], int, int, int> (stream.BeginRead, stream.EndRead);

            return Observable.Create<Tuple<IDisposable<byte[]>, int>> (observer => {
                var subscription = new BooleanDisposable ();
                Action<Exception> onError = error => {
                    observer.OnError (error);
                    subscription.Dispose ();
                };

                Action loop = null;
                Action<IDisposable<byte[]>, int> engine;
                if (waitForObserver)
                    engine = (buffer, bytesRead) => {
                        // OnNext is called first
                        observer.OnNext (new Tuple<IDisposable<byte[]>, int> (buffer, bytesRead));
                        // then we begin async read
                        if (!subscription.IsDisposed)
                            loop ();
                    };
                else
                    engine = (buffer, bytesRead) => {
                        // begin async read first
                        if (!subscription.IsDisposed)
                            loop ();
                        //then call OnNext
                        observer.OnNext (new Tuple<IDisposable<byte[]>, int> (buffer, bytesRead));
                    };

                loop = () => {
                    var buffer = cache.TakeOrCreate (refCount, () =>
                        new byte[bufferSize]);
                    read (buffer.Value, 0, bufferSize).Subscribe (bytesRead => {
                        if (0 == bytesRead)
                            observer.OnCompleted ();
                        else
                            engine (buffer, bytesRead);
                    }, onError);
                };

                loop ();

                return subscription;
            });
        }
    }
}