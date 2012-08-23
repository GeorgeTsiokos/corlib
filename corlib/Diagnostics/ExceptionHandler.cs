using System;
using System.Reactive.Disposables;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Corlib.Diagnostics {

    public class ExceptionHandler {

        static readonly Lazy<ExceptionHandler> __default = new Lazy<ExceptionHandler> (Create);
        static readonly ThreadLocal<ExceptionHandler> __current = new ThreadLocal<ExceptionHandler> (() => Default);
        readonly List<Func<Exception, bool>> _handlers = new List<Func<Exception, bool>> ();

        static internal ExceptionHandler Create () {
            return new ExceptionHandler ();
        }

        public static ExceptionHandler Default {
            get {
                return __default.Value;
            }
        }

        public static ExceptionHandler Current {
            get {
                if (!__current.IsValueCreated)
                    return Default;
                return __current.Value;
            }
            set {
                if (value == null)
                    value = Default;
                __current.Value = value;
            }
        }

        private ExceptionHandler () {
        }

        public Action<Exception> GetHandler<T> () {
            var logger = ExceptionLogger.GetDefault<T> ();
            var handler = GetHandler ();
            return exception => {
                logger (exception);
                handler (exception);
            };
        }

        public Action<Exception> GetHandler () {
            return exception => {
                Handle (exception, GetHandlers ().Concat (new Func<Exception, bool>[] { ex => { throw ex; } }));
            };
        }

        public IDisposable Register (Func<Exception, bool> exceptionHandler) {
            if (exceptionHandler == null)
                throw new ArgumentNullException ("exceptionHandler", "exceptionHandler is null.");
            lock (_handlers)
                _handlers.Add (exceptionHandler);
            return Disposable.Create (() => UnRegister (exceptionHandler));
        }

        void UnRegister (Func<Exception, bool> exceptionHandler) {
            lock (_handlers)
                _handlers.Remove (exceptionHandler);
        }

        Func<Exception, bool>[] GetHandlers () {
            lock (_handlers)
                return _handlers.ToArray ();
        }

        static void Handle (Exception exception, IEnumerable<Func<Exception, bool>> handlers) {
            Func<Exception, bool> currentHandler = null;
            try {
                foreach (var handler in handlers) {
                    currentHandler = handler;
                    if (handler (exception))
                        return;
                }
            }
            catch (Exception handlerException) {
                exception = new AggregateException (exception, handlerException);
                Handle (exception, handlers.Where (h => currentHandler != h));
            }
        }
    }
}