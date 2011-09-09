using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace CorLib {

    [TestClass]
    public class ObservableExtensionsTest {

        [TestMethod]
        public void ObservableAsAsyncResultProperties () {
            var subject = new Subject<int> ();
            var asyncResult = subject.AsAsyncResult (null, null);
            Assert.IsFalse (asyncResult.CompletedSynchronously);
            Assert.IsFalse (asyncResult.HasValue);
            Assert.IsFalse (asyncResult.IsCompleted);
            subject.OnNext (1);
            Assert.IsFalse (asyncResult.CompletedSynchronously);
            Assert.IsTrue (asyncResult.HasValue);
            Assert.IsTrue (asyncResult.IsCompleted);
        }

        [TestMethod]
        public void ObservableAsAsyncResultIsResultValid () {
            Assert.AreEqual (1, Observable.Return<int> (1).AsAsyncResult (null, null).Result);
        }

        [TestMethod]
        public void ObservableAsAsyncResultOnlyFirstValue () {
            var subject = new BehaviorSubject<int> (1);
            var asyncResult = subject.AsAsyncResult (null, null);
            Assert.AreEqual (1, asyncResult.Result);
            subject.OnNext (2);
            Assert.AreEqual (1, asyncResult.Result);
        }

        [TestMethod]
        [ExpectedException (typeof (AggregateException))]
        public void ObservableAsAsyncResultEnsureResultThrows () {
            var subject = new BehaviorSubject<int> (0);
            subject.OnError (new NotImplementedException ());
            var result = subject.AsAsyncResult (null, null).Result;
        }

        [TestMethod]
        [ExpectedException (typeof (AggregateException))]
        public void ObservableAsAsyncResultEnsureThrowIfExceptionEncountered () {
            var subject = new BehaviorSubject<int> (0);
            subject.OnError (new NotImplementedException ());
            subject.AsAsyncResult (null, null).ThrowIfExceptionEncountered ();
        }
    }
}