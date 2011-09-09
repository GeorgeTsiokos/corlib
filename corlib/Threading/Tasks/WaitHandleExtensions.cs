using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CorLib.Threading.Tasks {

    public static class WaitHandleExtensions {

        public static Task AsTask (this WaitHandle waitHandle, TimeSpan? timeout = null) {
            var tcs = new TaskCompletionSource<object> ();
            ThreadPool.UnsafeRegisterWaitForSingleObject (
                waitHandle,
                (o, b) => {
                    if (b)
                        tcs.TrySetException (new TimeoutException ());
                    else
                        tcs.TrySetResult (null);
                },
                null,
                timeout.AsThreadingTimeout (),
                true);
            return tcs.Task;
        }
    }
}