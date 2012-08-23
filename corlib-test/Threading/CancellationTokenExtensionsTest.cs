using System;
using System.Reactive.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Corlib.Threading {

    [TestClass]
    public class CancellationTokenExtensionsTest {

        [TestMethod]
        public void TakeUntilCanceledTokenIsEmptyStream () {
            CancellationTokenSource cts = new CancellationTokenSource ();
            cts.Cancel ();
            var count = 1;
            Observable.Range (0, 10).TakeUntil (cts.Token).Count ().ForEach (result =>
                count = result);
            Assert.AreEqual (0, count);
        }

        [TestMethod]
        public void TakeUntilIdentity () {
            var observableA = Observable.Range (0, 10);
            var observableB = observableA.TakeUntil (CancellationToken.None);
            var observableC = observableA.TakeUntil (new CancellationTokenSource ().Token);

            Assert.IsTrue (object.ReferenceEquals (observableA, observableB));
            Assert.IsFalse (object.ReferenceEquals (observableA, observableC));
        }

        [TestMethod]
        public void TakeUntilNotCanceled () {
            CancellationTokenSource cts = new CancellationTokenSource ();
            var count = 1;
            Observable.Range (0, 10).TakeUntil (cts.Token).Count ().ForEach (result =>
                count = result);
            Assert.AreEqual (10, count);
        }

        [TestMethod]
        public void TakeUntilCancel () {
            CancellationTokenSource cts = new CancellationTokenSource ();
            var count = 1;
            Func<bool, IObservable<int>> createSequence = cancelInTheMiddle => {
                return Observable.Range (0, 5).Concat (Observable.Create<int> (observer => {
                    if (cancelInTheMiddle)
                        cts.Cancel ();
                    observer.OnNext (6);
                    observer.OnCompleted ();
                    return () => { };
                })).Concat (Observable.Range (7, 4));
            };

            createSequence (false).TakeUntil (cts.Token).Count ().ForEach (result => 
                count = result);
            // if we don't signal the token, we have a count of 10
            Assert.AreEqual (10, count);

            createSequence (true).TakeUntil (cts.Token).Count ().ForEach (result =>
                count = result);
            // if we signal the token, we have a count of 5
            Assert.AreEqual (5, count);
        }
    }
}