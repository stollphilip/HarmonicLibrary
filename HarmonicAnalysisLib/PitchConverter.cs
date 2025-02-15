using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisLib
{
    public class PitchConverter
    {
        public static Vector<int> GlbToLub(Vector<int> glb)
        {
            return VectorConverter.CreateVector(0, glb[1] + 3, glb[2] + 2);
        }
        public static Vector<int> GreatestLowerBound(List<Vector<int>> positions)
        {
            return VectorConverter.CreateVector(positions.Min(p => p[0]), positions.Min(p => p[1]), positions.Min(p => p[2]));
        }
        public static Vector<int> GreatestLowerBound(List<List<IPitch>> pitches)
        {
            switch (pitches.Count)
            {
                case 0:
                    return Vector<int>.AllBitsSet;
                default:
                    var x = int.MaxValue;
                    var y = int.MaxValue;
                    var z = int.MaxValue;
                    foreach (var pitch in pitches)
                    {
                        foreach (var p in pitch)
                        {
                            if (p.Position[0] < x)
                            {
                                x = p.Position[0];
                            }
                            if (p.Position[1] < y)
                            {
                                y = p.Position[1];
                            }
                            if (p.Position[2] < z)
                            {
                                z = p.Position[2];
                            }
                        }
                    }
                    return VectorConverter.CreateVector(x, y, z);
            }
        }
        public static Vector<int> GreatestLowerBound(List<List<IPitch>> pitches, List<List<IPitch>> pitches2)
        {
            var a = GreatestLowerBound(pitches);
            var b = GreatestLowerBound(pitches2);
            return VectorConverter.CreateVector(
                Math.Min(a[0], b[0]),
                Math.Min(a[1], b[1]),
                Math.Min(a[2], b[2]));
        }
        public static Vector<int> GreatestLowerBound(IPitch pitch1, IPitch pitch2)
        {
            return VectorConverter.CreateVector(
                Math.Min(pitch1.Position[0], pitch2.Position[0]),
                Math.Min(pitch1.Position[1], pitch2.Position[1]),
                Math.Min(pitch1.Position[2], pitch2.Position[2]));
        }
        public static Vector<int> GreatestLowerBound(Vector<int> pitch1, Vector<int> pitch2)
        {
            return VectorConverter.CreateVector(
                Math.Min(pitch1[0], pitch2[0]),
                Math.Min(pitch1[1], pitch2[1]),
                Math.Min(pitch1[2], pitch2[2]));
        }
        public static Vector<int> LeastUpperBound(List<List<IPitch>> pitches)
        {
            switch (pitches.Count)
            {
                case 0:
                    return Vector<int>.AllBitsSet;
                default:
                    int x = int.MinValue;
                    int y = int.MinValue;
                    int z = int.MinValue;
                    foreach (var pitch in pitches)
                    {
                        foreach (var a in pitch)
                        {
                            if (a.Position[0] > x)
                            {
                                x = a.Position[0];
                            }
                            if (a.Position[1] > y)
                            {
                                y = a.Position[1];
                            }
                            if (a.Position[2] > z)
                            {
                                z = a.Position[2];
                            }
                        }
                    }
                    return VectorConverter.CreateVector(x, y, z);
            }
        }
        public static Vector<int> LeastUpperBound(List<List<IPitch>> pitches, List<List<IPitch>> pitches2)
        {
            var a = LeastUpperBound(pitches);
            var b = LeastUpperBound(pitches2);
            return VectorConverter.CreateVector(
                Math.Max(a[0], b[0]),
                Math.Max(a[1], b[1]),
                Math.Max(a[2], b[2]));
        }
        public static Vector<int> GreatestLowerBound(List<IPitch> pitches)
        {
            switch (pitches.Count)
            {
                case 0:
                    return Vector<int>.AllBitsSet;
                case 1:
                    return pitches[0].Position;
                default:
                    var x = pitches.Min(p => p.Position[0]);
                    var y = pitches.Min(p => p.Position[1]);
                    var z = pitches.Min(p => p.Position[2]);
                    return VectorConverter.CreateVector(x, y, z);
            }
        }
        public static Vector<int> LeastUpperBound(List<IPitch> pitches)
        {
            switch (pitches.Count)
            {
                case 0:
                    return Vector<int>.AllBitsSet;
                case 1:
                    return pitches[0].Position;
                default:
                    var x = pitches.Max(p => p.Position[0]);
                    var y = pitches.Max(p => p.Position[1]);
                    var z = pitches.Max(p => p.Position[2]);
                    return VectorConverter.CreateVector(x, y, z);
            }
        }
        /// <summary>
        /// convert chord to all possible shapes of the chord
        /// </summary>
        /// <remarks>
        /// Index 12 normalized lower left corner positions. Normalize tones with respect to corner.
        /// A shape is valid when min y value = corner y and z values start at corner z and form a contiguous series.
        /// </remarks>
        /// <param name="chords">chord progression</param>
        public static List<List<IPitch>> ChordToShapes(PitchPattern chord)
        {
            // I believe ChordToShapes works correctly
            // from TryAlias.csproj
            var shapes = new List<List<IPitch>>();
            // value of x to be determined later
            int x = 0;
            for (int z = 0; z < 3; ++z)
            {
                for (int y = 0; y < 4; ++y)
                {
                    var position = VectorConverter.CreateVector(x, y, z);
                    var tones = chord.Pitches.Select(t => 
                        new Pitch(t.PitchHeight, t.FifthHeight, t.Position + VectorConverter.Normalize(t.Position, position)))
                        .Cast<IPitch>().ToList();

                    bool validYs = tones.Min(t => t.Position[1]) == y;
                    // valid : Zs = {0}, {0, 1}, {0, 1, 2}
                    var Zs = tones.Select(t => t.Position[2]).Distinct().OrderBy(z => z).ToList();
                    //bool validZs = Enumerable.Range(0, Zs.Count).All(i => Zs[i] == z + i);
                    //
                    bool validZs= tones.Min(t => t.Position[2]) == z;
                    validZs = tones.Min(t => t.Position[2]) == z;
                    if (validYs && validZs)
                    {
                        shapes.Add(tones);

                        x = tones.Min(t => t.Position[0]);
                        position = VectorConverter.CreateVector(x, y, z);
                        for (int i = 0; i < chord.Pitches.Count; ++i)
                        {
                            var chordTone = chord.Pitches[i];
                            var tone = tones[i];
                            //Debug.WriteLine($"{chordTone.Position[0],2} {chordTone.Position[1],2} {chordTone.Position[2],2}  {tone.Position[0],2} {tone.Position[1],2} {tone.Position[2],2}     {12 * tone.Position[0] + 19 * tone.Position[1] + 28 * tone.Position[2]}");
                        }
                        //Debug.WriteLine(null);

                    }
                    Debug.Assert(VectorConverter.IsNormalized(position));
                }
            }
            return shapes;
        }
    }
}
