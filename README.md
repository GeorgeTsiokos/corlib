![corlib](corlib/raw/master/inc/corlib.png)
# LICENSE
[Microsoft Reciprocal License (Ms-RL)](http://www.opensource.org/licenses/MS-RL)

## Get it on NuGet!

* [corlib](http://nuget.org/List/Packages/corlib) - main library

## Links

* [Reactive Extensions (Rx)](http://msdn.microsoft.com/en-us/data/gg577609)

To get started:

    Install-Package corlib

# Samples
## A Rx stream can represent a CancellationToken perfectly
Included in the CorLib.Threading namspace, the ToObservable extension method converts a CancellationToken
into an IObservable<Unit> stream that represents the exact same states as a CancellationToken as shown
in the extension method's implementation:

```csharp
public static IObservable<Unit> ToObservable (this CancellationToken cancellationToken, bool onCompleted = false) {
    if (cancellationToken.IsCancellationRequested) {
        if (onCompleted)
            return Observable.Empty<Unit> ();
        else
            return Observable.Throw<Unit> (new OperationCanceledException ());
    }
    else if (!cancellationToken.CanBeCanceled)
        return Observable.Never<Unit> ();
    else
        if (onCompleted)
            return Observable.Create<Unit> (observer =>
                cancellationToken.Register (observer.OnCompleted));
        else
            return Observable.Create<Unit> (observer =>
                cancellationToken.Register (() =>
                    observer.OnError (new OperationCanceledException ())));
    // note that CancellationToken.Register() returns an IDisposable which is disposed of
    // when the result observable is unsubscribed from
}
```
## Rx Simplified APM integration
Calling Observable.Defer delays exceptions thrown from the abstract method to the Rx sequence which
is subsequently picked up by the ToTask extension method who's Result property or Wait method
is called from the End APM method, will propragate the exception

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
                ProcessRequestAsync (context)).ToTask<Unit> (cb, extraData);
        }

        void IHttpAsyncHandler.EndProcessRequest (IAsyncResult result) {
            var task = (Task)result;
            task.Wait ();
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