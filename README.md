![corlib](corlib/raw/master/inc/corlib.png)
# LICENSE
Microsoft Reciprocal License (Ms-RL) <http://www.opensource.org/licenses/MS-RL>

## Get it on NuGet!

* [corlib](http://nuget.org/List/Packages/corlib) - main library

## Links

* [Reactive Extensions (Rx)](http://msdn.microsoft.com/en-us/data/gg577609)

To get started:

    Install-Package corlib

# Samples
## Rx Simplified APM integration
Calling Observable.Defer delays exceptions thrown from the abstract method to the Rx sequence which
is subsequently picked up by the AsAsyncResult extension method who's ThrowIfExceptionEncountered method
is called from the End APM method
```csharp
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
            return Observable.Defer<Unit> (() =>
                ProcessRequestAsync (context)).AsAsyncResult<Unit> (cb, extraData);
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
```