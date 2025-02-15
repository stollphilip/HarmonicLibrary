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
    public class Manager_oldTests
    {
        [TestMethod]
        public void ProcessChords_ValidOutput()
        {
            var chords = new PitchPattern_old[]
            {
                new PitchPattern_old
                {
                    Pitches =
                    {
                        new Pitch_old(0, 0),
                        new Pitch_old(19, 1),
                        new Pitch_old(14, 2),
                        new Pitch_old(4, 4),
                    },
                },
                new PitchPattern_old
                {
                    Pitches =
                    {
                        new Pitch_old(0, 0),
                        new Pitch_old(5, -1),
                        new Pitch_old(14, 2),
                        new Pitch_old(21, 3),
                    },
                },
            };
            var manager = new Manager_old();
            manager.ProcessChords(chords);
        }
    }
}
