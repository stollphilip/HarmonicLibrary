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
    public class VectorConverterTests
    {
        [TestMethod]
        public void CreateVector_ValidInput()
        {
            var vector = VectorConverter.CreateVector(1, 2, 3);
            Assert.AreEqual(vector[0], 1);
            Assert.AreEqual(vector[1], 2);
            Assert.AreEqual(vector[2], 3);
            Assert.AreEqual(vector[3], 0);
            Assert.AreEqual(vector[4], 0);
            Assert.AreEqual(vector[5], 0);
            Assert.AreEqual(vector[6], 0);
            Assert.AreEqual(vector[7], 0);
        }

        [TestMethod]
        public void ModulusOperator_ValidOutput()
        {
            for (int i = -24; i <= 24; i++)
            {
                var m = VectorConverter.ModulusOperator(i, 12);
                var q = VectorConverter.Quotient(i, 12);
                Assert.IsTrue(0 <= m && m < 12);
                Assert.AreEqual(i, m + 12 * q);
            }
        }

        [TestMethod]
        public void VectorToPitch_ValidInput()
        {
            VectorToPitch_Test(new Vector<int>(new int[] { 1, 0, 0, 0, 0, 0, 0, 0 }), 12);
            VectorToPitch_Test(new Vector<int>(new int[] { 0, 1, 0, 0, 0, 0, 0, 0 }), 19);
            VectorToPitch_Test(new Vector<int>(new int[] { 0, 0, 1, 0, 0, 0, 0, 0 }), 28);
            VectorToPitch_Test(new Vector<int>(new int[] { -1, 0, 0, 0, 0, 0, 0, 0 }), -12);
            VectorToPitch_Test(new Vector<int>(new int[] { 0, -1, 0, 0, 0, 0, 0, 0 }), -19);
            VectorToPitch_Test(new Vector<int>(new int[] { 0, 0, -1, 0, 0, 0, 0, 0 }), -28);
        }
        private void VectorToPitch_Test(Vector<int> v, int expected)
        {
            Assert.AreEqual(VectorConverter.VectorToPitch(v), expected);
        }

        [TestMethod]
        public void VectorToDistance_ValidInput()
        {
            VectorToDistance_Test(new Vector<int>(new int[] { 1, 0, 0, 0, 0, 0, 0, 0 }), 12);
            VectorToDistance_Test(new Vector<int>(new int[] { 0, 1, 0, 0, 0, 0, 0, 0 }), 19);
            VectorToDistance_Test(new Vector<int>(new int[] { 0, 0, 1, 0, 0, 0, 0, 0 }), 28);
            VectorToDistance_Test(new Vector<int>(new int[] { -1, 0, 0, 0, 0, 0, 0, 0 }), 12);
            VectorToDistance_Test(new Vector<int>(new int[] { 0, -1, 0, 0, 0, 0, 0, 0 }), 19);
            VectorToDistance_Test(new Vector<int>(new int[] { 0, 0, -1, 0, 0, 0, 0, 0 }), 28);
        }
        private void VectorToDistance_Test(Vector<int> v, int expected)
        {
            Assert.AreEqual(VectorConverter.VectorToDistance(v), expected);
        }

        [TestMethod]
        [DataRow(0, 0, 0, 0, 0)]
        [DataRow(12, 0, 1, 0, 0)]
        [DataRow(19, 1, 0, 1, 0)]
        [DataRow(31, 1, 1, 1, 0)]
        public void PitchAndFifthToVector_ValidInput(int pitchHeight, int fifthHeight, int x, int y, int z)
        {
            var vector = VectorConverter.PitchAndFifthToVector(pitchHeight, fifthHeight);
            Assert.AreEqual(vector[0], x);
            Assert.AreEqual(vector[1], y);
            Assert.AreEqual(vector[2], z);

            Assert.AreEqual(12 * vector[0] + 19 * vector[1] + 28 * vector[2], pitchHeight);
            Assert.AreEqual(vector[2], 0);
            Assert.AreEqual(vector[1], fifthHeight);
        }

        [TestMethod]
        [DataRow(-1, 0, 0, 0, 0)]
        public void PitchAndFifthToVector_InvalidInput(int pitchHeight, int fifthHeight, int x, int y, int z)
        {
            var vector = VectorConverter.PitchAndFifthToVector(pitchHeight, fifthHeight);
            Assert.AreEqual(vector, Vector<int>.AllBitsSet);
        }

        [TestMethod()]
        public void VectorToDeltaZTest_ValidInput()
        {
            var source = VectorConverter.CreateVector(0, 0, 0);
            foreach (var alias in VectorConverter.Aliases)
            {
                var target = source + alias;
                var deltaZ = VectorConverter.VectorToDeltaZ(source, target);
                var target_recovered = VectorConverter.DeltaZToVector(source, deltaZ);
                Assert.AreEqual(target, target_recovered);
            }
        }

        [TestMethod()]
        public void DeltaZToVector_ValidInput()
        {
            var source = VectorConverter.CreateVector(0, 0, 0);
            for (int deltaZ = -6; deltaZ <= 6; deltaZ++)
            {
                var target = VectorConverter.DeltaZToVector(source, deltaZ);
                var calculatedDeltaZ = VectorConverter.VectorToDeltaZ(source, target);
                Assert.AreEqual(deltaZ, calculatedDeltaZ);
            }
        }


        [TestMethod]
        [DataRow(1, 2, 1, -3, 6, 0)]
        [DataRow(1, 2, -1, 5, -2, 0)]
        public void NormalizeTo_z0_ValidInput(int x, int y, int z, int expected_x, int expected_y, int expected_z)
        {
            var vector = VectorConverter.CreateVector(x, y, z);
            var normalized = VectorConverter.NormalizeTo_z0(vector);
            Assert.AreEqual(normalized[0], expected_x);
            Assert.AreEqual(normalized[1], expected_y);
            Assert.AreEqual(normalized[2], expected_z);
        }

        [TestMethod()]
        public void NormalizeOverrideTest_ValidInput()
        {
            var target = Vector<int>.Zero;
            for (int y = target[1] - 4; y <= target[1] + 4; y++)
            {
                for (int z = target[2] - 3; z <= target[2] + 3; z++)
                {
                    var source = VectorConverter.CreateVector(0, y, z);
                    var offset = VectorConverter.Normalize(source/*, target*/);
                    var v = source + offset;
                    Assert.IsTrue(target[1] <= v[1] && v[1] < target[1] + 4);
                    Assert.IsTrue(target[2] <= v[2] && v[2] < target[2] + 3);
                }
            }
        }

        [TestMethod()]
        public void NormalizeTest_ValidInput()
        {
            var target = VectorConverter.CreateVector(0, -1, -1);
            for (int y = target[1] - 4; y <= target[1] + 4; y++)
            {
                for (int z = target[2] - 3; z <= target[2] + 3; z++)
                {
                    var source = VectorConverter.CreateVector(0, y, z);
                    var offset = VectorConverter.Normalize(source, target);
                    var v = source + offset;
                    Assert.IsTrue(target[1] <= v[1] && v[1] < target[1] + 4);
                    Assert.IsTrue(target[2] <= v[2] && v[2] < target[2] + 3);
                }
            }
        }

        [TestMethod]
        public void Compare_ValidInput()
        {
            var vector1 = VectorConverter.CreateVector(1, 2, 3);
            var vector2 = VectorConverter.CreateVector(1, 2, 3);
            var vector3 = VectorConverter.CreateVector(2, 2, 3);
            var vector4 = VectorConverter.CreateVector(1, 3, 3);
            var vector5 = VectorConverter.CreateVector(1, 2, 4);

            Assert.AreEqual(VectorConverter.Compare(vector5, vector4), 1);
            Assert.AreEqual(VectorConverter.Compare(vector4, vector5), -1);

            Assert.AreEqual(VectorConverter.Compare(vector4, vector3), 1);
            Assert.AreEqual(VectorConverter.Compare(vector3, vector4), -1);

            Assert.AreEqual(VectorConverter.Compare(vector3, vector2), 1);
            Assert.AreEqual(VectorConverter.Compare(vector2, vector3), -1);

            Assert.AreEqual(VectorConverter.Compare(vector1, vector2), 0);
        }

        [TestMethod]
        public void PositionClassEqual_ValidInput()
        {
            var vector1 = VectorConverter.CreateVector(1, 2, 3);
            var vector2 = VectorConverter.CreateVector(1, 2, 3);
            var vector3 = VectorConverter.CreateVector(2, 2, 3);
            var vector4 = VectorConverter.CreateVector(1, 3, 3);
            var vector5 = VectorConverter.CreateVector(1, 2, 4);

            Assert.IsTrue(VectorConverter.PositionClassEqual(vector1, vector2));
            Assert.IsTrue(VectorConverter.PositionClassEqual(vector1, vector3));
            Assert.IsFalse(VectorConverter.PositionClassEqual(vector1, vector4));
            Assert.IsFalse(VectorConverter.PositionClassEqual(vector1, vector5));
            Assert.IsTrue(VectorConverter.PositionClassEqual(vector1, vector3));
        }
    }
}