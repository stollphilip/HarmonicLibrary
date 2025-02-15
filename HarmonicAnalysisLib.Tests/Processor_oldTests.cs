using Microsoft.VisualStudio.TestTools.UnitTesting;
using HarmonicAnalysisLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace HarmonicAnalysisLib.Tests
{
    [TestClass()]
    public class Processor_oldTests
    {
        [TestMethod()]
        public void FundamentalOfPitches_ZeroPitch()
        {
            var pitches = new List<IPitch_old>();
            Assert.AreEqual(Processor_old.GreatestLowerBound(pitches), Vector<int>.AllBitsSet);
        }
        [TestMethod()]
        public void FundamentalOfPitches_OnePitch()
        {
            var pitches = new List<IPitch_old> { new Pitch_old(0, 0), };
            Assert.AreEqual(Processor_old.GreatestLowerBound(pitches), Vector<int>.Zero);
        }
        [TestMethod()]
        public void FundamentalOfPitches_MultiplePitches()
        {
            var pitches = new List<IPitch_old> {
                new Pitch_old(28, 0) { Position = new Vector<int>(new int[] {1, 2, 2, 0, 0, 0, 0, 0,})},
                new Pitch_old(28, 0) { Position = new Vector<int>(new int[] {2, 1, 2, 0, 0, 0, 0, 0,})}, 
                new Pitch_old(28, 0) { Position = new Vector<int>(new int[] {2, 2, 1, 0, 0, 0, 0, 0,})} };
            var actual = Processor_old.GreatestLowerBound(pitches);
            var expected = new Vector<int>(new int[] { 1, 1, 1, 0, 0, 0, 0, 0, });
            Assert.AreEqual(expected, actual);
        }
    }
}