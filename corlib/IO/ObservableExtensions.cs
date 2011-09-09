using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using CorLib.Reactive;

namespace CorLib.IO {

    public static class ObservableExtensions {

        public static IObservable<IObservable<Unit>> Write (this IObservable<byte[]> sequence, Stream stream) {
            return sequence.Select (bytes =>
                new Tuple<byte[], int, int> (bytes, 0, bytes.Length)).Write (stream);
        }

        public static IObservable<IObservable<Unit>> Write (this IObservable<Tuple<byte[], int, int>> sequence, Stream stream) {
            var write = Observable.FromAsyncPattern<byte[], int, int> (
                stream.BeginWrite,
                stream.EndWrite);
            return Observable.Create<IObservable<Unit>> (observer =>
                sequence.Subscribe (value =>
                    observer.OnNext (write (value.Item1, value.Item2, value.Item3)),
                    observer.OnError,
                    observer.OnCompleted));
        }

        public static IObservable<Tuple<IDisposable<byte[]>, int, int>> Write (this IObservable<Tuple<IDisposable<byte[]>, int, int>> sequence, Stream stream) {
            var write = Observable.FromAsyncPattern<byte[], int, int> (
                stream.BeginWrite,
                stream.EndWrite);
            return Observable.Create<Tuple<IDisposable<byte[]>, int, int>> (observer =>
                sequence.Subscribe (value =>
                    write (value.Item1.Value, value.Item2, value.Item3).Select (_ => value).Subscribe (observer.OnNext),
                    observer.OnError,
                    observer.OnCompleted));
        }
    }
}