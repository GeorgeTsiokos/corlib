using System;
using System.Diagnostics;

namespace CorLib.Diagnostics {

    public static class TraceSourceExtensions {

        public static void TraceException (this TraceSource traceSource, Exception exception, TraceEventType traceEventType, int id) {
            if (traceSource == null)
                throw new ArgumentNullException ("traceSource", "traceSource is null.");
            if (exception == null)
                throw new ArgumentNullException ("exception", "exception is null.");
            traceSource.TraceData (traceEventType, id, exception.ToString ());
        }

        public static void TraceException (this TraceSource traceSource, Exception exception, TraceEventType traceEventType) {
            TraceException (traceSource, exception, traceEventType, 0);
        }

        public static void TraceException (this TraceSource traceSource, Exception exception) {
            TraceException (traceSource, exception, TraceEventType.Error, 0);
        }
    }
}