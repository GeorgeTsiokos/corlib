using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CorLib.Threading {

    [TestClass]
    public class GateTest {

        [TestMethod]
        public void OpenClose () {
            var gate = new Gate ();
            Assert.IsTrue (!gate.IsOpened);
            for (int i = 0; i < 3; i++) {
                gate.Open ();
                gate.Close ();
            }
            Assert.IsTrue (!gate.IsOpened);
        }

        [TestMethod]
        public void CloseOpen () {
            var gate = new Gate (true);
            Assert.IsTrue (gate.IsOpened);
            for (int i = 0; i < 3; i++) {
                gate.Close ();
                gate.Open ();
            }
            Assert.IsTrue (gate.IsOpened);
        }
    }
}