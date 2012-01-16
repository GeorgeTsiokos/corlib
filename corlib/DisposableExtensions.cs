using System;
using CorLib.Diagnostics;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics.Contracts;

namespace CorLib {

    /// <summary>
    /// IDisposable extension methods
    /// </summary>
    public static class DisposableExtensions {

        /// <summary>
        /// Attempts the call to dispose by catches exceptions
        /// </summary>
        /// <remarks>Exceptions are routed to the current exception handler</remarks>
        /// <param name="disposable">instance to dispose</param>
        public static bool TryDispose (this IDisposable disposable) {
            return TryDispose (disposable, new Lazy<Action<Exception>> (() =>
                ExceptionHandler.Current.GetHandler (), true));
        }

        /// <summary>
        /// Attempts the call to dispose for each instance by catches exceptions
        /// </summary>
        /// <remarks>Exceptions are routed to the current exception handler</remarks>
        /// <param name="disposables">instances to dispose</param>
        public static bool TryDispose (this IEnumerable<IDisposable> disposables) {
            return TryDispose (disposables, new Lazy<Action<Exception>> (() =>
                ExceptionHandler.Current.GetHandler (), true));
        }


        /// <summary>
        /// Attempts the call to dispose for each arg by catches exceptions
        /// </summary>
        /// <remarks>Exceptions are routed to the current exception handler</remarks>
        /// <param name="args">instances to dispose</param>
        public static bool TryDispose (params IDisposable[] args) {
            return TryDispose (args, new Lazy<Action<Exception>> (() =>
                ExceptionHandler.Current.GetHandler (), true));
        }

        static bool TryDispose (this IDisposable disposable, Lazy<Action<Exception>> exceptionHandler) {
            try {
                if (null != disposable) {
                    disposable.Dispose ();
                    return true;
                }
            }
            catch (Exception exception) {
                exceptionHandler.Value (exception);
            }
            return false;
        }

        static bool TryDispose (IEnumerable<IDisposable> disposables, Lazy<Action<Exception>> exceptionHandler) {
            Contract.Requires (null != disposables);
            var invokeDispose = disposables.Select (disposable => disposable.TryDispose (exceptionHandler));
            //TODO: parameterize
            var invokeInParallel = invokeDispose.AsParallel ();
            var storeResultsInArray = invokeInParallel.ToArray ();
            var allResultsAreTrue = storeResultsInArray.All (result => result);
            return allResultsAreTrue;
        }
    }
}