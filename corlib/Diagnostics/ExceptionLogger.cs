using System;
using System.Diagnostics;
using System.Threading;

namespace Corlib.Diagnostics {

    public static class ExceptionLogger {

        public static Action<Exception> GetDefault<T> (TraceEventType traceEventType, int id) {
            var traceSource = new ThreadLocal<TraceSource> (() =>
                TraceSourceFactory.Default.Create<T> ());

            return exception =>
                traceSource.Value.TraceException (exception, traceEventType, id);
        }

        public static Action<Exception> GetDefault<T> () {
            return GetDefault<T> (TraceEventType.Error, 0);
        }

        public static Action<Exception> Wrap (Type type, Action<Exception> exceptionHandler) {
            return exception => {
                try {
                    exceptionHandler (exception);
                }
                catch (Exception handlerException) {
                    TraceSourceFactory.Default.Create (type).TraceException (
                        new AggregateException (exception, handlerException), TraceEventType.Critical);
                }
            };
        }
    }
}