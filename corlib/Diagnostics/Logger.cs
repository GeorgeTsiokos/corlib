using System;
using System.Diagnostics;
using System.Threading;

namespace CorLib.Diagnostics {

    public static class Logger {

        public static Action<string> GetDefault<T> (TraceEventType traceEventType, int id) {
            var traceSource = new ThreadLocal<TraceSource> (() =>
                TraceSourceFactory.Default.Create<T> ());

            return message =>
                traceSource.Value.TraceEvent (traceEventType, id, message);
        }

        public static Action<string> GetDefault<T> (TraceEventType traceEventType) {
            return GetDefault<T> (traceEventType, 0);
        }

        public static Action<string> GetDefault<T> () {
            return GetDefault<T> (TraceEventType.Verbose);
        }
    }
}