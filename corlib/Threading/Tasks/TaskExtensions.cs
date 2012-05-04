using System;
using System.Diagnostics.Contracts;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;

namespace CorLib.Threading.Tasks {

    public static class TaskExtensions {

        public static bool TrySetFromTask<T> (this TaskCompletionSource<T> taskCompletionSource, Task<T> task) {
            Contract.Requires (taskCompletionSource != null, "taskCompletionSource is null.");
            Contract.Requires (task != null, "task is null.");
            
            bool result = false;
            switch (task.Status) {
                case TaskStatus.RanToCompletion:
                    result = taskCompletionSource.TrySetResult (task.Result);
                    break;
                case TaskStatus.Canceled:
                    result = taskCompletionSource.TrySetCanceled ();
                    break;
                case TaskStatus.Faulted:
                    result = taskCompletionSource.TrySetException (task.Exception.InnerExceptions);
                    break;
                default:
                    result = false;
                    break;
            }
            return result;
        }

        public static bool TrySetFromTask (this TaskCompletionSource<Unit> taskCompletionSource, Task task) {
            Contract.Requires (taskCompletionSource != null, "taskCompletionSource is null.");
            Contract.Requires (task != null, "task is null.");

            bool result = false;
            switch (task.Status) {
                case TaskStatus.RanToCompletion:
                    result = taskCompletionSource.TrySetResult (Unit.Default);
                    break;
                case TaskStatus.Canceled:
                    result = taskCompletionSource.TrySetCanceled ();
                    break;
                case TaskStatus.Faulted:
                    result = taskCompletionSource.TrySetException (task.Exception.InnerExceptions);
                    break;
                default:
                    result = false;
                    break;
            }
            return result;
        }

        public static Task UpdateTaskState (Task task, object state) {
            Contract.Requires (task != null, "task is null.");
            Contract.Requires (state != null, "state is null.");
            
            var taskCompletionSource = new TaskCompletionSource<Unit> (
                state,
                task.CreationOptions & TaskCreationOptions.AttachedToParent);

            task.ContinueWith (antecedent =>
                taskCompletionSource.TrySetFromTask (antecedent));

            return taskCompletionSource.Task;
        }

        public static Task<T> UpdateTaskState<T> (Task<T> task, object state) {
            Contract.Requires (task != null, "task is null.");
            Contract.Requires (state != null, "state is null.");

            var taskCompletionSource = new TaskCompletionSource<T> (
                state,
                task.CreationOptions & TaskCreationOptions.AttachedToParent);

            task.ContinueWith (antecedent =>
                taskCompletionSource.TrySetFromTask (antecedent));

            return taskCompletionSource.Task;
        }

        public static Task ContinueWithCallback (this Task task, AsyncCallback asyncCallback) {
            Contract.Requires (task != null, "task is null.");
            Contract.Requires (asyncCallback != null, "asyncCallback is null.");

            return task.ContinueWith (antecedent =>
                Callback (antecedent, asyncCallback));
        }

        public static Task ContinueWithCallback (this Task task, AsyncCallback asyncCallback, CancellationToken cancellationToken) {
            Contract.Requires (task != null, "task is null.");
            Contract.Requires (asyncCallback != null, "asyncCallback is null.");

            return task.ContinueWith (antecedent =>
                Callback (antecedent, asyncCallback), cancellationToken);
        }

        public static Task ContinueWithCallback (this Task task, AsyncCallback asyncCallback, TaskContinuationOptions continuationOptions) {
            Contract.Requires (task != null, "task is null.");
            Contract.Requires (asyncCallback != null, "asyncCallback is null.");

            return task.ContinueWith (antecedent =>
                Callback (antecedent, asyncCallback), continuationOptions);
        }

        public static Task ContinueWithCallback (this Task task, AsyncCallback asyncCallback, TaskScheduler scheduler) {
            Contract.Requires (task != null, "task is null.");
            Contract.Requires (asyncCallback != null, "asyncCallback is null.");
            Contract.Requires (scheduler != null, "scheduler is null.");

            return task.ContinueWith (antecedent =>
                Callback (antecedent, asyncCallback), scheduler);
        }

        public static Task ContinueWithCallback (this Task task, AsyncCallback asyncCallback, object state) {
            Contract.Requires (task != null, "task is null.");
            Contract.Requires (asyncCallback != null, "asyncCallback is null.");
            Contract.Requires (state != null, "state is null.");

            return UpdateTaskState (task, state).ContinueWithCallback (asyncCallback);
        }

        public static Task ContinueWithCallback (this Task task, AsyncCallback asyncCallback, object state, CancellationToken cancellationToken) {
            Contract.Requires (task != null, "task is null.");
            Contract.Requires (asyncCallback != null, "asyncCallback is null.");
            Contract.Requires (state != null, "state is null.");

            return UpdateTaskState (task, state).ContinueWithCallback (asyncCallback, cancellationToken);
        }

        public static Task ContinueWithCallback (this Task task, AsyncCallback asyncCallback, object state, TaskContinuationOptions continuationOptions) {
            Contract.Requires (task != null, "task is null.");
            Contract.Requires (asyncCallback != null, "asyncCallback is null.");
            Contract.Requires (state != null, "state is null.");

            return UpdateTaskState (task, state).ContinueWithCallback (asyncCallback, continuationOptions);
        }

        public static Task ContinueWithCallback (this Task task, AsyncCallback asyncCallback, object state, TaskScheduler scheduler) {
            Contract.Requires (task != null, "task is null.");
            Contract.Requires (asyncCallback != null, "asyncCallback is null.");
            Contract.Requires (state != null, "state is null.");
            Contract.Requires (scheduler != null, "scheduler is null.");

            return UpdateTaskState (task, state).ContinueWithCallback (asyncCallback, scheduler);
        }

        static void Callback (Task task, AsyncCallback asyncCallback) {
            Contract.Requires (task != null, "task is null.");
            Contract.Requires (asyncCallback != null, "asyncCallback is null.");

            asyncCallback (task);
        }
    }
}