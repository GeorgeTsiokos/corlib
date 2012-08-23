using System;
using System.Threading.Tasks;

namespace Corlib.Internal.Threading.Tasks {

    internal static class TaskUtilities {

        public static Task<T> CreateException<T> (Exception exception) {
            TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T> ();
            taskCompletionSource.SetException (exception);
            return taskCompletionSource.Task;
        }
    }
}
