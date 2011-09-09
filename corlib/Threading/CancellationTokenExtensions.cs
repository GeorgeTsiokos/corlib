using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using CorLib.Reactive;
using CorLib.Diagnostics;
using CorLib.Reactive;

namespace CorLib.Threading {

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
            // need to materialize the cancellation stream so OncCompleted is a "value" as required by the TakeUntil operator
            else return sequence.TakeUntil (cancellationToken.AsCancellationStream ().Materialize ());
        }

        /// <summary>
        /// Converts a CancellationToken in to an observable stream where the stream completes when the token is canceled
        /// </summary>
        /// <param name="cancellationToken">token to convert</param>
        /// <returns>OnCompleted when token fires</returns>
        /// <remarks>OnNext and OnError never fire, no exceptions are thrown</remarks>
        public static IObservable<Unit> AsCancellationStream (this CancellationToken cancellationToken) {
            if (cancellationToken.IsCancellationRequested)
                return Observable.Empty<Unit> ();
            else if (!cancellationToken.CanBeCanceled)
                return Observable.Never<Unit> ();
            else
                return Observable.Create<Unit> (observer =>
                    cancellationToken.Register (observer.OnCompleted));
        }

#if EXPERIMENTAL
        [ExperimentalAttribute (State = ExperimentalState.RequiresAdditionalTesting)]
        public static IObservable<CancellationToken> AsCancellationToken<T> (this IObservable<T> cancellationStream) {
            return AsCancellationToken<T> (cancellationStream, CancellationToken.None);
        }

        [ExperimentalAttribute (State = ExperimentalState.RequiresAdditionalTesting)]
        public static IObservable<CancellationToken> AsCancellationToken<T> (this IObservable<T> cancellationStream, params CancellationToken[] linkedTokens) {
            if (cancellationStream == null)
                throw new ArgumentNullException ("cancellationStream", "cancellationStream is null.");

            var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource (
                linkedTokens);

            var onlyOnErrorAndCompletedSequence = cancellationStream.IgnoreElements ();
            
            // Finally2 will propagate exceptions through the result stream
            var signalCancelOnErrorOrCompleted = onlyOnErrorAndCompletedSequence.Finally2 (
                cancellationTokenSource.Cancel);
            
            var disposeOfCtsWhenCompleted = signalCancelOnErrorOrCompleted.Using (
                cancellationTokenSource);
            
            var convertToCancellationTokenSequence = disposeOfCtsWhenCompleted.Select (_ =>
                CancellationToken.None);
            
            return convertToCancellationTokenSequence.StartWith<CancellationToken> (
                    cancellationTokenSource.Token);
        }
#endif
    }
}