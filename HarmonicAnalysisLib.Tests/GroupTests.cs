using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Numerics;
using HarmonicAnalysisLib;

namespace HarmonicAnalysisLib.Tests;
[TestClass()]
public class GroupTests
{
    private Group group;

    [TestInitialize]
    public void Setup()
    {
        group = new Group
        {
            ChordIndex = 1,
            Position = VectorConverter.CreateVector(0, 0, 0),
            Type = GroupType.Group,
            Tone = null,
        };
        var subgroup = new Group
        {
            ChordIndex = 1,
            Position = VectorConverter.CreateVector(0, 0, 0),
            Type = GroupType.Tone,
            Tone = new Pitch(0, 0),
        };
        var child = new Child(subgroup, VectorConverter.CreateVector(0, 0, 0));
        group.Children.Add(child);
    }

    [TestMethod()]
    [ExpectedException(typeof(ArgumentException))]
    public void ValidateGroupType_InvalidType_ThrowsException()
    {
        group.Type = GroupType.Tone;
        group.ValidateGroupType(GroupType.Group, GroupType.Frame);
    }

    [TestMethod()]
    public void ValidateGroupType_ValidType_DoesNotThrowException()
    {
        group.Type = GroupType.Group;
        group.ValidateGroupType(GroupType.Group, GroupType.Frame);
    }
    [TestMethod()]
    public void IsVerticalGroupOrGroup_WithVerticalGroup_ReturnsTrue()
    {
        group.Type = GroupType.VerticalGroup;
        var result = group.IsVerticalGroupOrGroup(1);
        Assert.IsTrue(result);
    }

    [TestMethod()]
    public void IsVerticalGroupOrGroup_WithGroupAndMatchingChildCount_ReturnsTrue()
    {
        group.Type = GroupType.Group;
        var result = group.IsVerticalGroupOrGroup(1);
        Assert.IsTrue(result);
    }

    [TestMethod()]
    public void IsVerticalGroupOrGroup_WithGroupAndNonMatchingChildCount_ReturnsFalse()
    {
        group.Type = GroupType.Group;
        var result = group.IsVerticalGroupOrGroup(2);
        Assert.IsFalse(result);
    }

    [TestMethod()]
    [ExpectedException(typeof(ArgumentException))]
    public void FrameToBounds_InvalidGroupType_ThrowsException()
    {
        Assert.AreEqual(group.Type, GroupType.Group);
        group.Bounds = new Vector<int>[] { VectorConverter.CreateVector(0, 0, 0), VectorConverter.CreateVector(1, 1, 1) };
        group.FrameToBounds(out _, out _);
    }

    [TestMethod()]
    public void FrameToBounds_ValidFrameType_ReturnsBounds()
    {
        group.Type = GroupType.Frame;
        group.Bounds = new Vector<int>[] { VectorConverter.CreateVector(0, 0, 0), VectorConverter.CreateVector(1, 1, 1) };
        group.FrameToBounds(out var lower, out var upper);
        Assert.AreEqual(VectorConverter.CreateVector(0, 0, 0), lower);
        Assert.AreEqual(VectorConverter.CreateVector(1, 1, 1), upper);
    }
}
