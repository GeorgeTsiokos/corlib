using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;

namespace CorLib.Threading {

    public static class WaitHandleExtensions {

        /// <summary>
        /// Converts a <see cref="System.Threading.WaitHandle"/> to an observable sequence
        /// </summary>
        /// <param name="waitHandle">the value to convert</param>
        /// <param name="executeOnlyOnce">true to observe multiple signals, false to complete the stream when signaled</param>
        /// <param name="timeout">optional timeout waiting for a single</param>
        /// <returns>A sequence that completes when <paramref name="executeOnlyOnce"/> is true, or never completes, and signals when false. Returns a TimeoutException when timeout is hit.</returns>
        public static IObservable<Unit> AsObservable (this WaitHandle waitHandle, bool executeOnlyOnce, TimeSpan? timeout = null) {
            return Observable.Create<Unit> (observer => {
                RegisteredWaitHandle registration = null;
                registration = ThreadPool.UnsafeRegisterWaitForSingleObject (
                                    waitHandle,
                                    (state, timedOut) => {
                                        if ((timedOut || executeOnlyOnce) && null != registration)
                                            registration.Unregister (waitHandle);

                                        if (timedOut)
                                            observer.OnError (new TimeoutException ());
                                        else {
                                            if (executeOnlyOnce)
                                                observer.OnCompleted ();
                                            else
                                                observer.OnNext (Unit.Default);
                                        }
                                    },
                                    null,
                                    timeout.AsThreadingTimeout (),
                                    executeOnlyOnce);
                return () => registration.Unregister (waitHandle);
            });
        }
    }
}