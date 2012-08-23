using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Corlib.Threading {

    [TestClass]
    public class AtomicUInt64Test {

        [TestMethod]
        public void AtomicUInt64CreateStartsAtZero () {
            AtomicUInt64 target = AtomicUInt64.Create ();
            Assert.AreEqual ((ulong)0, target.Value);
        }

        [TestMethod]
        public void AtomicUInt64StartsAtDefaultValue () {
            AtomicUInt64 target = new AtomicUInt64 ();
            Assert.AreEqual (AtomicUInt64.Info.DefaultValue, target.Value);
        }

        [TestMethod]
        public void AtomicUInt64ResetsToZero () {
            AtomicUInt64 target = new AtomicUInt64 () { 
                Value = 18446744073709551615
            };
            Assert.AreEqual (18446744073709551615, target.Value);
            Assert.AreEqual ((ulong)0, target.IncrementAndReturn ());
        }
    }
}