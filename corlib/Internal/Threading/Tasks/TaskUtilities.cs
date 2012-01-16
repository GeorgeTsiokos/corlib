using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CorLib.Internal.Threading.Tasks {
    public static class TaskUtilities {

        public static Task<T> CreateException<T> (Exception exception) {
            TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T> ();
            taskCompletionSource.SetException (exception);
            return taskCompletionSource.Task;
        }
    }
}
