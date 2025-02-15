using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisLib
{
    /// <summary>
    /// FrameStruct Library
    /// </summary>
    public class FrameStructLib
    {
        const int VerticalHorizontalMax = 2;
        const int YMax = 4;
        const int ZMax = 3;
        public static readonly Vector<int>[] Deltas = new Vector<int>[] { VectorConverter.CreateVector(0, 0, 0), VectorConverter.CreateVector(0, -1, 0), VectorConverter.CreateVector(0, 1, 0), VectorConverter.CreateVector(0, 0, -1), VectorConverter.CreateVector(0, 0, 1) };
        static readonly int DeltasMax = Deltas.Length;
        /// <summary>
        /// Initializes master FrameStruct list
        /// </summary>
        /// <remarks>FrameStruct array provides a simple, unified navigation of chord and chord pair shapes.</remarks>
        /// <param name="groups">master Group list</param>
        /// <param name="chords">chord progression</param>
        public static List<List<List<List<List<FrameStruct>>>>> InitFrameStructs(List<Group> groups, PitchPattern[] chords)
        {
            int maxChord = chords.Length;
            return Enumerable.Range(0, maxChord).Select(chordIndex =>
                        Enumerable.Range(0, VerticalHorizontalMax)
                        // the last chord has no horizontal frames, since there is no next chord
                        .Where(verticalHorizontal => !(chordIndex == maxChord - 1 && verticalHorizontal == VerticalHorizontalMax - 1)).Select(verticalHorizontal =>
                        Enumerable.Range(0, DeltasMax).Select(deltaIndex =>
                        Enumerable.Range(0, ZMax).Select(z =>
                        Enumerable.Range(0, YMax).Select(y =>
                        {

                            var p0 = VectorConverter.CreateVector(0, y, z);
                            var p1 = VectorConverter.CreateVector(0, y, z) + FrameStructLib.Deltas[deltaIndex];
                            var offset = VectorConverter.Normalize(p1);
                            Debug.Assert(VectorConverter.IsNormalized(p0));
                            Debug.Assert(VectorConverter.IsNormalized(p1 + offset));
                            GroupMap? groupMap = CreateGroupMap(groups, chordIndex, verticalHorizontal, deltaIndex, p0, p1/* + offset*/);
                            if (groupMap != null)
                            {
                                Debug.Assert(VectorConverter.PositionClassEqual(groupMap.Frame.Position, p0));
                                Debug.Assert(groupMap.Frame.Type == GroupType.Frame || VectorConverter.PositionClassEqual(groupMap.Frame.Children[0].Position, p0));
                                Debug.Assert(groupMap.Frame.Type == GroupType.Frame || VectorConverter.PositionClassEqual(groupMap.Frame.Children[1].Position, p1));
                            }
                            return new FrameStruct
                            {
                                ChordIndex = chordIndex,
                                VerticalOrHorizontal = verticalHorizontal,
                                DeltaIndex = deltaIndex,
                                z = z,
                                y = y,
                                GroupMap = groupMap,
                                // to test null GroupMap
                                //GroupMap = (random1.Next(10) == 0) ? null : groupMap,
                            };
                        }
                        ).ToList()).ToList()).ToList()).ToList()).ToList();
        }
        protected static GroupMap? CreateGroupMap(List<Group> groups, int chordIndex, int verticalHorizontal, int deltaIndex, Vector<int> child0pos, Vector<int> child1pos)
        {
            var frame = LookupFrame(groups, chordIndex, verticalHorizontal, deltaIndex, child0pos, child1pos);
            if (frame == null)
            {
                return null;
            }
            //frame.DebugWrite(0, Vector<int>.Zero);
            //TODO remove
            var groupMap = GroupMapLib.InitGroupMap(frame, groups);
            groupMap.Reset();
            while (groupMap.Next/*Next_new*/())
            {

            }
            return groupMap;
            //
            //return GroupMapLib.InitGroupMap(group, groups);
        }
        protected static Group? LookupFrame(List<Group> groups, int chordIndex, int verticalHorizontal, int deltaIndex, Vector<int> child0pos, Vector<int> child1pos)
        {
            GroupType type;
            if (verticalHorizontal == 0)
            {
                if (deltaIndex == 0)
                {
                    type = GroupType.Frame;
                }
                else
                {
                    type = GroupType.VerticalFrame;

                }
            }
            else
            {
                type = GroupType.HorizontalFrame;
            }
            var group = groups.SingleOrDefault(g => g.ChordIndex == chordIndex && g.Type == type && VectorConverter.PositionClassEqual(g.Position, child0pos) &&
            (g.Type == GroupType.Frame ||
            VectorConverter.PositionClassEqual(g.Children[0].Position, child0pos) &&
            VectorConverter.PositionClassEqual(g.Children[1].Position, child1pos)));
            return group;
        }

        /// <summary>
        /// Returns FrameStruct Iterator
        /// </summary>
        public static IEnumerable<FrameStruct> FrameStructIterator(List<List<List<List<List<FrameStruct>>>>> chords)
        {
            for (var chordIndex = 0; chordIndex < chords.Count; chordIndex++)
            {
                for (var verticalHorizontal = 0; verticalHorizontal < VerticalHorizontalMax; verticalHorizontal++)
                {
                    // last chord has no horizontal frames
                    if (chordIndex == chords.Count - 1 && verticalHorizontal == VerticalHorizontalMax - 1)
                    {
                        continue;
                    }
                    for (var deltaIndex = 0; deltaIndex < DeltasMax; deltaIndex++)
                    {
                        for (var z = 0; z < ZMax; z++)
                        {
                            for (var y = 0; y < YMax; y++)
                            {
                                var frame = chords[chordIndex][verticalHorizontal][deltaIndex][z][y];
                                yield return frame;
                            }
                        }
                    }
                }
            }
        }

        public static void ListChordShapes(List<List<List<List<List<FrameStruct>>>>> chords)
        {
            for (var chordIndex = 0; chordIndex < chords.Count; chordIndex++)
            {
                for (var verticalHorizontal = 0; verticalHorizontal < VerticalHorizontalMax; verticalHorizontal++)
                {
                    // last chord has no horizontal frames
                    if (chordIndex == chords.Count - 1 && verticalHorizontal == VerticalHorizontalMax - 1)
                    {
                        continue;
                    }
                    for (var deltaIndex = 0; deltaIndex < DeltasMax; deltaIndex++)
                    {
                        for (var z = 0; z < ZMax; z++)
                        {
                            for (var y = 0; y < YMax; y++)
                            {
                                var frame = chords[chordIndex][verticalHorizontal][deltaIndex][z][y];
                                Debug.WriteLine(frame);
                                if (frame.GroupMap is not null)
                                {
                                    if (frame.GroupMap.Snapshots.Count != 0)
                                    {
                                        frame.GroupMap.Snapshots[frame.GroupMap.SnapshotIndex].DebugWrite();
                                    }
                                    else
                                    {
                                        Debug.WriteLine("No snapshots");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// the prototype for looping through the FrameStruct array 
        /// </summary>
        /// <param name="chords">master FrameStruct list</param>
        public static void ListNeighbors(List<List<List<List<List<FrameStruct>>>>> chords)
        {
            // ListNeighbors contains the prototype for looping through the FrameStruct array 
            for (var chordIndex = 0; chordIndex < chords.Count - 1; chordIndex++)
            {
                for (var verticalHorizontal = 0; verticalHorizontal < VerticalHorizontalMax - 1; verticalHorizontal++)
                {
                    for (var deltaIndex = 0; deltaIndex < DeltasMax - 2; deltaIndex++)
                    {
                        for (var z = 0; z < ZMax; z++)
                        {
                            for (var y = 0; y < YMax; y++)
                            {
                                var frame = chords[chordIndex][verticalHorizontal][deltaIndex][z][y];
                                int ChordIndex = chordIndex;
                                var VerticalHorizontal = verticalHorizontal + 1;
                                var DeltaIndex = deltaIndex;
                                var Z = z;
                                var Y = y;
                                var FrameA = chords[ChordIndex][VerticalHorizontal][DeltaIndex][Z][Y];
                                /*
                                var p = frame.GroupMap.Frame.Position;
                                var v = VectorConverter.CreateVector(0, p[1], p[2]);
                                v = v + VectorConverter.Normalize(v);
                                */
                                Debug.WriteLine($" {FrameA}");
                            }
                        }
                    }
                }
            }
        }

        public static void ValidateGroupMaps(List<List<List<List<List<FrameStruct>>>>> chords)
        {
            foreach (var frameStruct in FrameStructIterator(chords))
            {
                if (frameStruct.GroupMap is not null)
                {
                    frameStruct.GroupMap.Validate();
                }
            }
        }
    }
}
