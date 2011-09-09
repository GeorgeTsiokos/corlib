using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace CorLib.Internal {

    [DebuggerNonUserCode]
    internal sealed class AsyncResult<T> : IAsyncResult<T> {

        readonly ConcurrentBag<Exception> _exceptions = new ConcurrentBag<Exception> ();
        readonly ManualResetEventSlim _manualResetEventSlim;
        T _result;
        bool _hasValue;

        public AsyncResult (AsyncCallback callback, object asyncState, bool completedSynchronously, ManualResetEventSlim manualResetEventSlim) {
            Callback = callback;
            AsyncState = asyncState;
            _manualResetEventSlim = manualResetEventSlim;
            CompletedSynchronously = completedSynchronously;
        }

        public AsyncResult (AsyncCallback callback, object asyncState, bool completedSynchronously, ManualResetEventSlim manualResetEventSlim, T value) :
            this (callback, asyncState, completedSynchronously, manualResetEventSlim) {
            OnNextWithoutCheck (value);
        }

        public AsyncResult (AsyncCallback callback, object asyncState, bool completedSynchronously, bool isCompleted) :
            this (callback, asyncState, completedSynchronously, new ManualResetEventSlim (isCompleted)) {
        }

        public void ThrowIfExceptionEncountered () {
            var exception = AggregateException;
            if (null != exception)
                throw exception;
        }

        public AsyncCallback Callback { get; private set; }
        public object AsyncState { get; private set; }
        public WaitHandle AsyncWaitHandle { get { return _manualResetEventSlim.WaitHandle; } }
        public bool CompletedSynchronously { get; private set; }
        public bool IsCompleted { get { return _manualResetEventSlim.IsSet; } }
        public bool HasValue { get { return _manualResetEventSlim.IsSet && _hasValue; } }

        public T Result {
            get {
                _manualResetEventSlim.Wait ();
                ThrowIfExceptionEncountered ();
                if (!_hasValue)
                    throw new InvalidOperationException ("async result has not been set");
                return _result;
            }
        }

        public AggregateException AggregateException {
            get {
                if (0 == _exceptions.Count)
                    return null;
                else
                    return new AggregateException (_exceptions);
            }
        }

        public void Complete (bool completedSynchronously) {
            CompletedSynchronously = completedSynchronously;
            OnCompleted ();
        }

        public void OnCompleted () {
            _manualResetEventSlim.Set ();
            InvokeCallback ();
        }

        internal void InvokeCallback () {
            if (null != Callback)
                try {
                    Callback (this);
                }
                catch (Exception exception) {
                    _exceptions.Add (exception);
                }
        }

        public void OnError (Exception error) {
            _exceptions.Add (error);
            OnCompleted ();
        }

        public void OnNext (T value) {
            if (_manualResetEventSlim.IsSet)
                return;
            OnNextWithoutCheck (value);
        }

        void OnNextWithoutCheck (T value) {
            _hasValue = true;
            _result = value;
        }

        public bool TryGetValue (out T value) {
            var result = _manualResetEventSlim.IsSet && _hasValue;
            if (result)
                value = _result;
            else
                value = default (T);
            return result;
        }
    }
}