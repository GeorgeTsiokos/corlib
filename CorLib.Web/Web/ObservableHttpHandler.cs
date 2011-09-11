using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Web;

namespace CorLib.Web {

    /// <summary>
    /// Defines the base implementation of Rx HTTP asynchronous handler objects
    /// </summary>
    public abstract class ObservableHttpHandler : IHttpAsyncHandler {

        /// <summary>
        /// Async handler implementation
        /// </summary>
        /// <param name="context">current HTTP context</param>
        /// <returns>an observable sequence that signals completion or an error</returns>
        protected abstract IObservable<Unit> ProcessRequestAsync (HttpContext context);

        /// <remarks>deferrs exceptions to End APM method</remarks>
        IAsyncResult IHttpAsyncHandler.BeginProcessRequest (HttpContext context, AsyncCallback cb, object extraData) {
            IObservable<Unit> result;
            try {
                result = ProcessRequestAsync (context);
            }
            catch (Exception exception) {
                result = Observable.Throw<Unit> (exception);
            }
            return result.AsAsyncResult<Unit> (cb, extraData);
        }

        void IHttpAsyncHandler.EndProcessRequest (IAsyncResult result) {
            IAsyncResult<Unit> ar = result as IAsyncResult<Unit>;
            if (null == ar)
                throw new ArgumentException ("result");
            ar.AsyncWaitHandle.WaitOne ();
            ar.ThrowIfExceptionEncountered ();
        }

        bool IHttpHandler.IsReusable {
            get { return true; }
        }

        void IHttpHandler.ProcessRequest (HttpContext context) {
            ProcessRequestAsync (context).ForEach (_ => { });
        }
    }
}