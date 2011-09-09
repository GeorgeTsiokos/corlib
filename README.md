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
```csharp
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Web;
using CorLib;

public abstract class HttpObservableAsyncHandler : IHttpAsyncHandler {

    public virtual bool IsReusable {
        get { return false; }
    }

    public abstract IObservable<Unit> ProcessRequestAsync (HttpContext context);

    void IHttpHandler.ProcessRequest (HttpContext context) {
        ProcessRequestAsync (context).ForEach (_ => { });
    }

    IAsyncResult IHttpAsyncHandler.BeginProcessRequest (HttpContext context, AsyncCallback cb, object extraData) {
        return Observable.Defer<Unit> (() => 
            ProcessRequestAsync (context)).AsAsyncResult (cb, extraData);
    }

    void IHttpAsyncHandler.EndProcessRequest (IAsyncResult result) {
        var ar = result as IAsyncResult<Unit>;
        if (null == ar)
            throw new ArgumentNullException ("result");

        ar.AsyncWaitHandle.WaitOne ();
        ar.ThrowIfExceptionEncountered ();
    }
}
```