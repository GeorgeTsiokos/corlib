![corlib](corlib.png)
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
public abstract class HttpObservableAsyncHandler : IHttpAsyncHandler {

    public virtual bool IsReusable {
        get { return false; }
    }

    public virtual void ProcessRequest (HttpContext context) {
        ProcessRequestAsync (context).ForEach (_ => { });
    }

    public abstract IObservable<Unit> ProcessRequestAsync (HttpContext context);

    IAsyncResult IHttpAsyncHandler.BeginProcessRequest (HttpContext context, AsyncCallback cb, object extraData) {
        try {
            return ProcessRequestAsync (context).AsAsyncResult (cb, extraData);
        }
        catch (Exception exception) {
            return Observable.Throw<Unit> (exception).AsAsyncResult (cb, state);
        }
    }

    void IHttpAsyncHandler.EndProcessRequest (IAsyncResult result) {
        if (null == result) {
            throw new ArgumentNullException ("result");
        }
        var ar = result as IAsyncResult<Unit>;
        ar.AsyncWaitHandle.WaitOne ();
        ar.ThrowIfExceptionEncountered ();
    }
}
```