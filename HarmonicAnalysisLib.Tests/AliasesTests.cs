using Microsoft.VisualStudio.TestTools.UnitTesting;
using HarmonicAnalysisLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisLib.Tests
{
    [TestClass()]
    public class AliasesTests
    {
        [TestMethod]
        public void Aliases_DataIsValid()
        {
            foreach (var a in Aliases_old._aliases)
            {
                Assert.AreEqual(Converter_old.VectorToPitch(a), 0);
            }
        }
        [TestMethod]
        public void Aliases_InputInRange()
        {
            Aliases_old aliases = new Aliases_old();
            Assert.AreEqual(aliases[0], Vector<int>.Zero);
        }
        [TestMethod]
        public void Aliases_InputOutsideRange()
        {
            Aliases_old aliases = new Aliases_old();
            Assert.AreEqual(aliases[Aliases_old._range[0] - 1], Vector<int>.AllBitsSet);
            Assert.AreEqual(aliases[Aliases_old._range[1] + 1], Vector<int>.AllBitsSet);
        }
    }
}
