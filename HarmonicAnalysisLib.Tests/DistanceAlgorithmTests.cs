using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using HarmonicAnalysisLib;

namespace HarmonicAnalysisLib.Tests
{
    [TestClass()]
    public class DistanceAlgorithmTests
    {
        Manager manager;
        [TestInitialize]
        public void Setup()
        {
            manager = new Manager();
            manager.VerifyMethodOrder(0);
            PitchPattern[] chords = GenChords();
            manager.PitchPatternToTones_old(chords);
            manager.TonesToGroups(chords);
            manager.GroupsToVertGroups(chords);
            manager.TonesToFrames(chords);
            var frames = manager.GroupsToFrames_old(chords);
            manager.GroupsToHorizGroups(chords);
        }

        private static PitchPattern[] GenChords()
        {
            return new PitchPattern[]
                        {
                new PitchPattern
                {
                    Pitches = {
                        new Pitch(0, 0),
                        new Pitch(19, 1),
                        new Pitch(28, 4),
                    },
                },
                new PitchPattern
                {
                    Pitches = {
                        new Pitch(12, 0),
                        new Pitch(31, 1),
                        new Pitch(50, 2),
                    },
                },
                        };
        }

        [TestMethod()]
        public void Pairwise_ValidInput()
        {
            foreach (var group in manager.Groups.Where(g => g.Children.Count == 1))
            {
                Assert.AreEqual(group.Distance.Pairwise, 0);
            }
            foreach (var group in manager.Groups.Where(g => g.Type == GroupType.Group && g.Children.Count == 3))
            {
                var fund = group.Position;
                double pairwise = VectorConverter.VectorToPitch(group.Children[2].Group.Position - fund) + VectorConverter.VectorToPitch(group.Children[1].Group.Position - fund);
                Assert.AreEqual(group.Distance.Pairwise, pairwise);
            }
        }
        [TestMethod()]
        public void Average_ValidInput()
        {
            foreach (var group in manager.Groups.Where(g => g.Children.Count == 1))
            {
                Assert.AreEqual(group.Distance.Average, 0);
            }
            foreach (var group in manager.Groups.Where(g => g.Type == GroupType.Group && g.Children.Count == 2))
            {
                Assert.AreEqual(group.Distance.Average, group.Distance.Pairwise);
            }
            foreach (var group in manager.Groups.Where(g => g.Type == GroupType.Group && g.Children.Count == 3))
            {
                var fund = group.Position;
                double average = (2 * group.Children.Sum(c => VectorConverter.VectorToPitch(c.Position - fund))) / (double)group.Children.Count;
                Assert.AreEqual(group.Distance.Average, average);
            }
        }
        [TestMethod()]
        public void Weighted_ValidInput()
        {
            foreach (var group in manager.Groups.Where(g => g.Children.Count == 1))
            {
                Assert.AreEqual(group.Distance.Weighted, 0);
            }
            foreach (var group in manager.Groups)
            {
                //Assert.IsTrue(group.Distance.Pairwise <= group.Distance.Weighted);
                Assert.IsTrue(group.Distance.Average <= group.Distance.Weighted);
            }
        }
        [TestMethod()]
        public void CompoundPairwise_ValidInput()
        {
            foreach (var group in manager.Groups.Where(g => g.Type == GroupType.HorizontalGroup &&
                g.Children[0].Group.Type == GroupType.Group && g.Children[1].Group.Type == GroupType.Group &&
                g.Children[0].Group.Children.Count == 3 && g.Children[1].Group.Children.Count == 3))
            {
                var fund = group.Position;
                double pairwise = VectorConverter.VectorToPitch(group.Children[0].Group.Position - fund) + VectorConverter.VectorToPitch(group.Children[1].Group.Position - fund);
                Assert.AreEqual(group.Distance.Pairwise, pairwise);
                double compoundPairwise = pairwise + group.Children[0].Group.Distance.Pairwise + group.Children[1].Group.Distance.Pairwise;
                Assert.AreEqual(group.Distance.CompoundPairwise, compoundPairwise);
            }
        }
        [TestMethod()]
        public void CompoundAverage_ValidInput()
        {
            // VerticalGroup
            foreach (var group in manager.Groups.Where(g => g.Type == GroupType.VerticalGroup &&
                g.Children[0].Group.Type == GroupType.Group && g.Children[1].Group.Type == GroupType.Group &&
                g.Children[0].Group.Children.Count > 1 && g.Children[1].Group.Children.Count > 1))
            {
                var fund = group.Position;
                double average = VectorConverter.VectorToPitch(group.Children[0].Group.Position - fund) + VectorConverter.VectorToPitch(group.Children[1].Group.Position - fund);
                Assert.AreEqual(group.Distance.Average, average);
                double compoundAverage = average + group.Children.Sum(c => c.Group.Distance.Average);
                Assert.AreEqual(group.Distance.CompoundAverage, compoundAverage);
            }
            // HorizontalGroup
            foreach (var group in manager.Groups.Where(g => g.Type == GroupType.HorizontalGroup &&
                g.Children[0].Group.Type == GroupType.Group && g.Children[1].Group.Type == GroupType.Group &&
                g.Children[0].Group.Children.Count == 3 && g.Children[1].Group.Children.Count == 3))
            {
                var fund = group.Position;
                double average = VectorConverter.VectorToPitch(group.Children[0].Group.Position - fund) + VectorConverter.VectorToPitch(group.Children[1].Group.Position - fund);
                Assert.AreEqual(group.Distance.Average, average);
                double compoundAverage = average + group.Children.Sum(c => c.Group.Distance.Average);
                Assert.AreEqual(group.Distance.CompoundAverage, compoundAverage);
            }
        }
        [TestMethod()]
        public void CompoundWeighted_ValidInput()
        {
            // VerticalGroup
            foreach (var group in manager.Groups.Where(g => g.Type == GroupType.VerticalGroup &&
                g.Children[0].Group.Type == GroupType.Group && g.Children[1].Group.Type == GroupType.Group &&
                g.Children[0].Group.Children.Count > 1 && g.Children[1].Group.Children.Count > 1))
            {
                var fund = group.Position;
                double compoundAverage = group.Distance.Weighted + group.Children.Sum(c => c.Group.Distance.Weighted);
                Assert.AreEqual(group.Distance.CompoundWeighted, compoundAverage);
            }
            // HorizontalGroup
            foreach (var group in manager.Groups.Where(g => g.Type == GroupType.HorizontalGroup &&
                g.Children[0].Group.Type == GroupType.Group && g.Children[1].Group.Type == GroupType.Group &&
                g.Children[0].Group.Children.Count == 3 && g.Children[1].Group.Children.Count == 3))
            {
                var fund = group.Position;
                double compoundAverage = group.Distance.Weighted + group.Children.Sum(c => c.Group.Distance.Weighted);
                Assert.AreEqual(group.Distance.CompoundWeighted, compoundAverage);
            }
        }
    }
}
