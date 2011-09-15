using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using CorLib.Reactive;

namespace CorLib.IO {

    public static class ObservableExtensions {

        public static IObservable<IObservable<Unit>> WriteAsync (this IObservable<byte[]> sequence, Stream stream) {
            return sequence.Select (bytes =>
                new Tuple<byte[], int, int> (bytes, 0, bytes.Length)).WriteAsync (stream);
        }

        public static IObservable<IObservable<Unit>> WriteAsync (this IObservable<Tuple<byte[], int>> sequence, Stream stream) {
            var sequence_ = sequence.Select (value =>
                new Tuple<byte[], int, int> (value.Item1, 0, value.Item2));
            return sequence_.WriteAsync (stream);
        }

        public static IObservable<IObservable<Unit>> WriteAsync (this IObservable<Tuple<byte[], int, int>> sequence, Stream stream) {
            var write = Observable.FromAsyncPattern<byte[], int, int> (
                stream.BeginWrite,
                stream.EndWrite);

            return sequence.Select (value =>
                write (value.Item1, value.Item2, value.Item3));
        }

        public static IObservable<IObservable<Unit>> WriteAsync (this IObservable<Tuple<IDisposable<byte[]>, int>> sequence, Stream stream) {
            var sequence_ = sequence.Select (value =>
                new Tuple<IDisposable<byte[]>, int, int> (value.Item1, 0, value.Item2));
            return sequence_.WriteAsync (stream);
        }

        public static IObservable<IObservable<Unit>> WriteAsync (this IObservable<Tuple<IDisposable<byte[]>, int, int>> sequence, Stream stream) {
            var write = Observable.FromAsyncPattern<byte[], int, int> (
                stream.BeginWrite,
                stream.EndWrite);

            return sequence.Select (value =>
                write (value.Item1.Value, value.Item2, value.Item3).Select (_ => {
                    value.Item1.Dispose ();
                    return _;
                }));
        }
    }
}