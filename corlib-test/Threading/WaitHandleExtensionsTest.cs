using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Corlib.Threading {

    [TestClass]
    public class WaitHandleExtensionsTest {

        [TestMethod]
        public void ToObservableTest () {
            int count = 1;
            const int expectedCount = 11;
            var fiftyMilliSeconds = 50;
            using (var autoResetEvent = new AutoResetEvent (false)) {
                // schedule setting the WaitHandle
                Scheduler.ThreadPool.Schedule (TimeSpan.FromMilliseconds (fiftyMilliSeconds), () => {
                    for (int j = 1; j < expectedCount; j++) {
                        autoResetEvent.Set ();
                        Thread.Sleep (1);
                    }
                });

                // convert a WaitHandle to an IObservable<Unit>
                var sequence = autoResetEvent.ToObservable (
                    // true to complete the sequence on the first signal
                    executeOnlyOnce: false,
                    // throws a TimeoutException when there are no signals
                    timeout: TimeSpan.FromMilliseconds (fiftyMilliSeconds * 2));
                // converts an OnError(TimeoutException) to a sequence completion event
                var timeoutToOnCompleted = sequence.Catch<Unit, TimeoutException> (e =>
                    Observable.Empty<Unit> ());
                // block thread until sequence completion and increments count 
                // each time the WaitHandle is set
                timeoutToOnCompleted.ForEach (_ => count++);
            }
            Assert.AreEqual (expectedCount, count);
        }
    }
}