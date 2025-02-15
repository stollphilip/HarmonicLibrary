using Microsoft.VisualStudio.TestTools.UnitTesting;
using HarmonicAnalysisLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisLib.Tests;
[TestClass()]
public class PitchConverterTests
{
    [TestMethod()]
    public void GlbToLubTest()
    {
        var glb = VectorConverter.CreateVector(1, 2, 3);
        var result = PitchConverter.GlbToLub(glb);
        var expected = VectorConverter.CreateVector(0, 5, 5);
        Assert.AreEqual(expected, result);
    }

    [TestMethod()]
    public void GreatestLowerBound_VectorList_Test()
    {
        var positions = new List<Vector<int>>()
        {
            VectorConverter.CreateVector(1, 2, 3),
            VectorConverter.CreateVector(4, 5, 6),
            VectorConverter.CreateVector(0, 1, 2)
        };
        var result = PitchConverter.GreatestLowerBound(positions);
        var expected = VectorConverter.CreateVector(0, 1, 2);
        Assert.AreEqual(expected, result);
    }

    [TestMethod()]
    public void GreatestLowerBound_IPitchListList_Test()
    {
        var pitches = new List<List<IPitch>>()
        {
            new List<IPitch>() { new Pitch(1, 2, VectorConverter.CreateVector(1, 2, 3)) },
            new List<IPitch>() { new Pitch(1, 2, VectorConverter.CreateVector(4, 5, 6)) },
            new List<IPitch>() { new Pitch(4, 5, VectorConverter.CreateVector(0, 1, 2)) }
        };
        var result = PitchConverter.GreatestLowerBound(pitches);
        var expected = VectorConverter.CreateVector(0, 1, 2);
        Assert.AreEqual(expected, result);
    }

    [TestMethod()]
    public void GreatestLowerBound_IPitchListList_TwoLists_Test()
    {
        var pitches1 = new List<List<IPitch>>()
        {
            new List<IPitch>() { new Pitch(1, 2, VectorConverter.CreateVector(3, 4, 5)) },
            new List<IPitch>() { new Pitch(1, 2, VectorConverter.CreateVector(1, 2, 3)) }
        };
        var pitches2 = new List<List<IPitch>>()
        {
            new List<IPitch>() { new Pitch(4, 5, VectorConverter.CreateVector(2, 3, 4)) },
            new List<IPitch>() { new Pitch(4, 5, VectorConverter.CreateVector(0, 1, 2)) }
        };
        var result = PitchConverter.GreatestLowerBound(pitches1, pitches2);
        var expected = VectorConverter.CreateVector(0, 1, 2);
        Assert.AreEqual(expected, result);
    }

    [TestMethod()]
    public void GreatestLowerBound_IPitch_Test()
    {
        var pitch1 = new Pitch(1, 2, VectorConverter.CreateVector(1, 2, 3));
        var pitch2 = new Pitch(4, 5, VectorConverter.CreateVector(0, 1, 2));
        var result = PitchConverter.GreatestLowerBound(pitch1, pitch2);
        var expected = VectorConverter.CreateVector(0, 1, 2);
        Assert.AreEqual(expected, result);
    }

    [TestMethod()]
    public void GreatestLowerBound_Vector_Test()
    {
        var pitch1 = VectorConverter.CreateVector(1, 2, 3);
        var pitch2 = VectorConverter.CreateVector(0, 1, 2);
        var result = PitchConverter.GreatestLowerBound(pitch1, pitch2);
        var expected = VectorConverter.CreateVector(0, 1, 2);
        Assert.AreEqual(expected, result);
    }

    [TestMethod()]
    public void LeastUpperBound_IPitchListList_Test()
    {
        var pitches = new List<List<IPitch>>()
        {
            new List<IPitch>() { new Pitch(1, 2, VectorConverter.CreateVector(1, 2, 3)) },
            new List<IPitch>() { new Pitch(4, 5, VectorConverter.CreateVector(0, 1, 2)) }
        };
        var result = PitchConverter.LeastUpperBound(pitches);
        var expected = VectorConverter.CreateVector(1, 2, 3);
        Assert.AreEqual(expected, result);
    }

    [TestMethod()]
    public void LeastUpperBound_IPitchListList_TwoLists_Test()
    {
        var pitches1 = new List<List<IPitch>>()
        {
            new List<IPitch>() { new Pitch(1, 2, VectorConverter.CreateVector(1, 2, 3)) },
            new List<IPitch>() { new Pitch(1, 2, VectorConverter.CreateVector(-1, 0, 1)) }
        };
        var pitches2 = new List<List<IPitch>>()
        {
            new List<IPitch>() { new Pitch(4, 5, VectorConverter.CreateVector(0, 1, 2)) },
            new List<IPitch>() { new Pitch(4, 5, VectorConverter.CreateVector(-2, -1, 0)) }
        };
        var result = PitchConverter.LeastUpperBound(pitches1, pitches2);
        var expected = VectorConverter.CreateVector(1, 2, 3);
        Assert.AreEqual(expected, result);
    }

    [TestMethod()]
    public void GreatestLowerBound_IPitchList_Test()
    {
        var pitches = new List<IPitch>()
        {
            new Pitch(1, 2, VectorConverter.CreateVector(1, 2, 3)),
            new Pitch(4, 5, VectorConverter.CreateVector(0, 1, 2))
        };
        var result = PitchConverter.GreatestLowerBound(pitches);
        var expected = VectorConverter.CreateVector(0, 1, 2);
        Assert.AreEqual(expected, result);
    }

    [TestMethod()]
    public void LeastUpperBound_IPitchList_Test()
    {
        var pitches = new List<IPitch>()
        {
            new Pitch(1, 2, VectorConverter.CreateVector(1, 2, 3)),
            new Pitch(4, 5, VectorConverter.CreateVector(0, 1, 2))
        };
        var result = PitchConverter.LeastUpperBound(pitches);
        var expected = VectorConverter.CreateVector(1, 2, 3);
        Assert.AreEqual(expected, result);
    }

    //TODO: write ChordToShapes_Test
    [TestMethod()]
    public void ChordToShapes_Test()
    {
        var chord = new PitchPattern()
        {
            Pitches = new List<IPitch>()
            {
                new Pitch(1, 2, VectorConverter.CreateVector(1, 2, 0)),
                new Pitch(4, 5, VectorConverter.CreateVector(0, 1, 0))
            }
        };
        var result = PitchConverter.ChordToShapes(chord);
        Assert.IsTrue(result.Count > 0);
    }
}
