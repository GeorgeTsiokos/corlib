using System;
using System.Threading;
using System.Threading.Tasks;

namespace CorLib.Threading.Tasks {

    public static class AsyncResultExtensions {

        public static Task AsTask (this IAsyncResult asyncResult, TimeSpan? timeout = null) {
            return asyncResult.AsyncWaitHandle.AsTask (timeout);
        }

        public static Task<T> AsTask<T> (this IAsyncResult<T> asyncResult, TimeSpan? timeout = null) {
            var tcs = new TaskCompletionSource<T> ();
            ThreadPool.UnsafeRegisterWaitForSingleObject (
                asyncResult.AsyncWaitHandle,
                (o, b) => {
                    if (b)
                        tcs.SetException (new TimeoutException ());
                    else {
                        try {
                            if (null != asyncResult.AggregateException)
                                tcs.SetException (asyncResult.AggregateException);
                            else
                                if (asyncResult.HasValue)
                                    tcs.SetResult (asyncResult.Result);
                                else
                                    tcs.SetCanceled ();
                        }
                        catch (Exception exception) {
                            tcs.TrySetException (exception);
                        }
                    }
                },
                null,
                timeout.AsThreadingTimeout (),
                true);
            return tcs.Task;
        }
    }
}