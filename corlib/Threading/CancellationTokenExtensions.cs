using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;

namespace Corlib.Threading {

    public static class CancellationTokenExtensions {

        /// <summary>
        /// Returns the values from the source observable sequence until the cancellation token fires
        /// </summary>
        /// <param name="sequence">Source sequence to propagate elements for</param>
        /// <param name="cancellationToken"></param>
        /// <returns>An observable sequence containing the elements of the source sequence up to the firing of the cancellationToken</returns>
        public static IObservable<T> TakeUntil<T> (this IObservable<T> sequence, CancellationToken cancellationToken) {
            if (cancellationToken.IsCancellationRequested)
                return Observable.Empty<T> ();
            else if (!cancellationToken.CanBeCanceled)
                return sequence;
            // need to materialize the cancellation stream so OncCompleted is a "value"
            // as required by the TakeUntil operator
            else
                return sequence.TakeUntil (cancellationToken.ToObservable (true).Materialize ());
        }

        /// <summary>
        /// Converts a CancellationToken to an observable stream where the stream errors (or completes) when the token is signaled
        /// </summary>
        /// <param name="cancellationToken">token to convert</param>
        /// <param name="onCompleted">true to close the stream when the token is signaled, false (default) to send OnError with 
        /// a <see cref="OperationCanceledException"/> when the token fires</param>
        /// <returns>a stream that errors (or completes) when the token is signaled</returns>
        public static IObservable<Unit> ToObservable (this CancellationToken cancellationToken, bool onCompleted = false) {
            if (cancellationToken.IsCancellationRequested) {
                if (onCompleted)
                    return Observable.Empty<Unit> ();
                else
                    return Observable.Throw<Unit> (new OperationCanceledException ());
            }
            else if (!cancellationToken.CanBeCanceled)
                return Observable.Never<Unit> ();
            else
                if (onCompleted)
                    return Observable.Create<Unit> (observer =>
                        cancellationToken.Register (observer.OnCompleted));
                else
                    return Observable.Create<Unit> (observer =>
                        cancellationToken.Register (() =>
                            observer.OnError (new OperationCanceledException ())));
            // note that CancellationToken.Register() returns an IDisposable which is disposed of
            // when the result observable is unsubscribed from
        }

        /// <summary>
        /// Converts the observable sequence in to a CancellationToken
        /// </summary>
        /// <param name="stream">observble stream to convert</param>
        /// <param name="tokens">optional addtional tokens to link to the token result</param>
        /// <returns>A cancellation token source that is canceled when the stream completed, or has an error</returns>
        public static CancellationTokenSource ToCancellationTokenSource<T> (this IObservable<T> stream, params CancellationToken[] tokens) {
            CancellationTokenSource cancellationTokenSource = null != tokens && tokens.Length > 0 ?
                CancellationTokenSource.CreateLinkedTokenSource (tokens) :
                new CancellationTokenSource ();

            var subscription = stream.Finally (cancellationTokenSource.Cancel).Subscribe ();
            cancellationTokenSource.Token.Register (subscription.Dispose);

            return cancellationTokenSource;
        }
    }
}