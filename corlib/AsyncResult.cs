using System;
using System.Reactive;
using System.Threading;
using CorLib.Internal;

namespace CorLib {

    /// <summary>
    /// Provides static/shared methods to simplify APM integration with Rx async implementations
    /// </summary>
    public static class AsyncResult {

        static readonly Lazy<ManualResetEventSlim> __completedManualResetEvent = new Lazy<ManualResetEventSlim> (() =>
            new ManualResetEventSlim (true));
        static readonly Lazy<IAsyncResult<Unit>> __completed = new Lazy<IAsyncResult<Unit>> (() => 
            CreateCompleted (null, new object ())); // new object is requred!

        static ManualResetEventSlim CompletedManualResetEvent {
            get { return __completedManualResetEvent.Value; }
        }

        static IAsyncResult<Unit> CompletedAsyncResult {
            get { return __completed.Value; }
        }

        /// <summary>Creates a async result with the specified callback and state</summary>
        /// <param name="callback">callback as required by APM</param>
        /// <param name="asyncState">state as required by APM</param>
        /// <param name="completedSynchronously">true if operation was completed synchronously</param>
        /// <param name="isCompleted">true if operation is already completed</param>
        /// <returns>an async result for use with APM</returns>
        public static IAsyncResult<T> Create<T> (AsyncCallback callback, object asyncState, bool completedSynchronously = false, bool isCompleted = false) {
            return new AsyncResult<T> (callback, asyncState, completedSynchronously, isCompleted);
        }

        /// <summary>Creates a async result with the specified callback and state</summary>
        /// <param name="callback">callback as required by APM</param>
        /// <param name="asyncState">state as required by APM</param>
        /// <returns>an async result for use with APM</returns>
        public static IAsyncResult<T> Create<T> (AsyncCallback callback = null, object asyncState = null) {
            return new AsyncResult<T> (callback, asyncState, false, false);
        }

        /// <summary>Creates a completed async result with the specified callback and asyncState</summary>
        /// <param name="callback">callback to invoke immediately</param>
        /// <param name="asyncState">state as required by APM</param>
        /// <param name="exception">exception to raise as part of the completed async result</param>
        /// <returns>A complleted async result for use with APM</returns>
        /// <remarks>Call <see cref="IAsyncResult{T}.ThrowIfExceptionEncountered"/> to see the exception passed as well as the exception thrown by the callback, if any</remarks>
        public static IAsyncResult<Unit> CreateCompleted (AsyncCallback callback, object asyncState, Exception exception) {
            var result = new AsyncResult<Unit> (callback, asyncState, true, CompletedManualResetEvent);
            result.OnError (exception);
            result.InvokeCallback ();
            return result;
        }

        /// <summary>Creates a async result with the specified callback and state</summary>
        /// <param name="callback">callback as required by APM</param>
        /// <param name="asyncState">state as required by APM</param>
        /// <returns>an async result for use with APM</returns>
        /// <param name="value">the value associated with this completed async result</param>
        /// <returns>A completed async result for use with the APM</returns>
        public static IAsyncResult<T> CreateCompleted<T> (AsyncCallback callback, object asyncState, T value) {
            var result = new AsyncResult<T> (callback, asyncState, true, CompletedManualResetEvent, value);
            result.InvokeCallback ();
            return result;
        }

        /// <summary>Creates a completed async result with the specified callback and asyncState</summary>
        /// <param name="callback">callback to invoke immediately</param>
        /// <param name="asyncState">state as required by APM</param>
        /// <returns>A completed async result for use with APM</returns>
        /// <remarks>Call <see cref="IAsyncResult{T}.ThrowIfExceptionEncountered"/> to see the exception thrown by the callback, if any</remarks>
        public static IAsyncResult<Unit> CreateCompleted (AsyncCallback callback = null, object asyncState = null) {
            if (null == callback && null == asyncState)
                return CompletedAsyncResult;
            else {
                var result = new AsyncResult<Unit> (callback, asyncState, true, CompletedManualResetEvent);
                result.InvokeCallback ();
                return result;
            }
        }
    }
} 