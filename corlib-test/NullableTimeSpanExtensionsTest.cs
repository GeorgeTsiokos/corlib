using System;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CorLib;

namespace CorLib {
    [TestClass]
    public class NullableTimeSpanExtensionsTest {

        [TestMethod]
        public void NullableTimeSpanAsTimeoutNullValueEqualsTimeoutInfinite () {
            TimeSpan? value = null;
            Assert.AreEqual (Timeout.Infinite, value.AsThreadingTimeout ());
        }

        [TestMethod]
        public void NullableTimeSpanAsTimeoutTimeSpanZeroIsZero () {
            TimeSpan? value = TimeSpan.Zero;
            Assert.AreEqual (0, value.AsThreadingTimeout ());
        }

        [TestMethod]
        public void NullableTimeSpanMaxValueBecomesIntMaxValue () {
            TimeSpan? value = TimeSpan.MaxValue;
            Assert.AreEqual (int.MaxValue, value.AsThreadingTimeout ());
        }
    }
}