using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using HarmonicAnalysisLib;

namespace HarmonicAnalysisLib.Tests;
[TestClass()]
public class ChildTests
{
    private Group group;
    private Child child;

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
        var subchild = new Child(subgroup, VectorConverter.CreateVector(0, 0, 0));
        group.Children.Add(subchild);
        child = new Child(group, VectorConverter.CreateVector(-4, 4, -1));
    }

    [TestMethod()]
    public void ChildWithinBounds_ValidInput()
    {
        var glb = VectorConverter.CreateVector(-4, 4, -1);
        var lub = VectorConverter.CreateVector(-15, 8, 1);
        var offset = VectorConverter.CreateVector(0, 0, 0);
        var result = child.ChildWithinBounds(glb, lub, offset, GroupType.Group);
        Assert.IsTrue(result);
    }

    [TestMethod()]
    public void ChildWithinBounds_Offset_ValidInput()
    {
        var glb = VectorConverter.CreateVector(0, 0, 0);
        var lub = VectorConverter.CreateVector(-11, 4, 2);
        var offset = VectorConverter.CreateVector(4, -4, 1);
        var result = child.ChildWithinBounds(glb, lub, offset, GroupType.Group);
        Assert.IsTrue(result);
    }

    [TestMethod()]
    public void ChildWithinBounds_InvalidInput()
    {
        var glb = VectorConverter.CreateVector(-4, 4, 0);
        var lub = VectorConverter.CreateVector(-15, 8, 2);
        var offset = VectorConverter.CreateVector(0, 0, 0);
        var result = child.ChildWithinBounds(glb, lub, offset, GroupType.Group);
        Assert.IsFalse(result);
    }

    [TestMethod()]
    public void ChildWithinBounds_Offset_InvalidInput()
    {
        var glb = VectorConverter.CreateVector(-4, 4, 0);
        var lub = VectorConverter.CreateVector(-15, 8, 2);
        var offset = VectorConverter.CreateVector(4, -4, 1);
        var result = child.ChildWithinBounds(glb, lub, offset, GroupType.Group);
        Assert.IsFalse(result);
    }

    [TestMethod()]
    public void Offset_ValidInput()
    {
        var expected = VectorConverter.CreateVector(-4, 4, -1);
        var result = child.Offset();
        Assert.AreEqual(expected, result);
    }

    [TestMethod()]
    public void Validate_ValidInput()
    {
        var result = child.Validate();
        Assert.IsTrue(result);
    }

    [TestMethod()]
    public void Validate_InvalidInput()
    {
        var group_bad = new Group
        {
            ChordIndex = 1,
            Position = VectorConverter.CreateVector(0, 0, 0),
            // invalid combinations of Type and Tone
            Type = GroupType.Tone,
            Tone = null,
        };
        var child_bad = new Child(group_bad, VectorConverter.CreateVector(-4, 4, -1));
        var result = child_bad.Validate();
        Assert.IsFalse(result);
    }

    [TestMethod()]
    public void Equals_ValidInput()
    {
        var otherChild = new Child(group, VectorConverter.CreateVector(-4, 4, -1));
        var result = child.Equals(otherChild);
        Assert.IsTrue(result);
    }

    [TestMethod()]
    public void Equals_InvalidInput()
    {
        var otherGroup = new Group
        {
            ChordIndex = 1,
            Position = VectorConverter.CreateVector(0, 0, 0),
            Type = GroupType.Group,
            Tone = null,
        };
        var otherChild = new Child(otherGroup, VectorConverter.CreateVector(4, -4, 1));
        var result = child.Equals(otherChild);
        Assert.IsFalse(result);
    }
}
