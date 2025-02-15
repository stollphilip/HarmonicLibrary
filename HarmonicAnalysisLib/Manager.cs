using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HarmonicAnalysisCommonLib;

namespace HarmonicAnalysisLib
{
    public class Manager
    {
        /// <summary>
        /// All Groups are stored in this list.
        /// </summary>
        public List<Group> Groups { get; set; } = new List<Group>();

        /// <summary>
        /// FrameStruct array provides a simple, unified navigation of chord and chord pair shapes.
        /// </summary>
        public List<List<List<List<List<FrameStruct>>>>> ChordShapes /* [ChordIndex][VerticalHorizontal][DeltaIndex][z][y] */ { get; set; } = new List<List<List<List<List<FrameStruct>>>>>();

        /// <summary>
        /// Contains the analysis of the chord progression.
        /// </summary>
        public List<SegmentLite>? SegmentLites { get; protected set; }

        private int _step = 0;

        public void Process(PitchPattern[] chords)
        {
            // Process can be called multiple times. Reset each time.
            Reset();
            ResetMethodOrder();
            VerifyMethodOrder(0);
            ProcessChords(chords);
        }
        // add test for this method
        public void ProcessChords(PitchPattern[] chords)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            PitchPatternToTones(chords);
            stopWatch.Stop();
            Debug.WriteLine($"PitchPatternToTones {stopWatch.Elapsed.Milliseconds}");

            stopWatch.Reset();
            stopWatch.Start();
            TonesToGroups(chords);
            stopWatch.Stop();
            Debug.WriteLine($"TonesToGroups_new {stopWatch.Elapsed.Milliseconds}");

            stopWatch.Reset();
            stopWatch.Start();
            GroupsToVertGroups(chords);
            stopWatch.Stop();
            Debug.WriteLine($"GroupsToVertGroups {stopWatch.Elapsed.Milliseconds}");

            stopWatch.Reset();
            stopWatch.Start();
            TonesToFrames(chords);
            stopWatch.Stop();
            Debug.WriteLine($"TonesToFrames {stopWatch.Elapsed.Milliseconds}");

            stopWatch.Reset();
            stopWatch.Start();
            GroupsToHorizGroups(chords);
            stopWatch.Stop();
            Debug.WriteLine($"GroupsToHorizGroups {stopWatch.Elapsed.Milliseconds}");

            stopWatch.Reset();
            stopWatch.Start();
            FrameToVerticalFrame(chords);
            stopWatch.Stop();
            Debug.WriteLine($"FrameToVerticalFrame {stopWatch.Elapsed.Milliseconds}");

            stopWatch.Reset();
            stopWatch.Start();
            FrameToHorizFrame(chords);
            stopWatch.Stop();
            Debug.WriteLine($"FrameToHorizFrame {stopWatch.Elapsed.Milliseconds}");

            stopWatch.Reset();
            stopWatch.Start();
            ChordShapes = FrameStructLib.InitFrameStructs(Groups, chords);
            //HorizFramesToHorizGroupMaps(chords);
            stopWatch.Stop();
            Debug.WriteLine($"InitFrameStructs {stopWatch.Elapsed.Milliseconds}");

            stopWatch.Reset();
            stopWatch.Start();
            SegmentLib2.CallDijkstra(ChordShapes, chords);
            stopWatch.Stop();
            Debug.WriteLine($"CallDijkstra {stopWatch.Elapsed.Milliseconds}");

            stopWatch.Reset();
            stopWatch.Start();
            SegmentLites = SegmentLib.CallShortestPath_new(ChordShapes, 35);
            stopWatch.Stop();
            Debug.WriteLine($"CallShortestPath_new {stopWatch.Elapsed.Milliseconds}");

            //ShortestPath(chords, DistanceType.Pairwise);
            //SegmentLites = SegmentLib.CallShortestPath(ChordShapes);

            //HorizontalFramePathToGroupMap(chords);
        }

        /// <summary>
        /// convert PitchPattern to Tones
        /// </summary>
        /// <param name="chords">chord progression</param>
        public void PitchPatternToTones(PitchPattern[] chords)
        {
            // this is the only method for creating Tones
            VerifyMethodOrder(1);
            for (var iChord = 0; iChord < chords.Length; ++iChord)
            {
                var chord = chords[iChord];
                foreach (var pitch in chord.Pitches)
                {
                    //var vector = VectorConverter.PitchAndFifthToVector(pitch.PitchHeight, pitch.FifthHeight);
                    //--------------------------------------------------------
                    // The object model is:
                    //  Tone
                    //      Child
                    //          (none)
                    //      Tone : IPitch
                    var group = TonesToGroups(pitch, iChord);
                    //Debug.Assert(group.Tone.Position[2] == 0);
                    group.Tone.FifthHeight = group.Position[1];
                    group.Tone.Position = group.Tone.Position + VectorConverter.Normalize(group.Tone.Position);
                    group.Position = group.Tone.Position;
                     Groups.Add(group);
                }
            }
        }

        public Group TonesToGroups(IPitch tone, int chordIndex)
        {
            var type = GroupType.Tone;
            //var vector = VectorConverter.PitchAndFifthToVector(tone.PitchHeight, tone.FifthHeight);
            return new Group
            {
                ChordIndex = chordIndex,
                Type = type,
                Position = tone.Position/*vector*/,
                Tone = tone,
            };
        }

        /// <summary>
        /// convert Tones to Groups
        /// </summary>
        /// <param name="chords">chord progression</param>
        public void TonesToGroups(PitchPattern[] chords)
        {
            // When stored, the Group Positions are normalized, and the children Positions are normalized with respect to the parent Position
            VerifyMethodOrder(2);
            for (var iChord = 0; iChord < chords.Length; ++iChord)
            {
                var chord = chords[iChord];
                // distinct Positions of Tones and Groups, projected into four 'shapes' restricted to strips y = [-3..0], [-2..1], [-1..2], [0..3] and z >= 0
                var shapes = PitchConverter.ChordToShapes(chord);
                foreach (var shape in shapes)
                {
                    Debug.Assert(VectorConverter.IsNormalized(PitchConverter.GreatestLowerBound(shape)));
                    // finding groups by indexing fundamentals is cleaner and simpler than generating set permutations, and is the approach used
                    // using only possible values of x, y, z speeds things up
                    var zValues = shape.Select(p => p.Position[2]).OrderBy(z => z).Distinct();
                    var yValues = shape.Select(p => p.Position[1]).OrderBy(z => z).Distinct();
                    var xValues = shape.Select(p => p.Position[0]).OrderBy(z => z).Distinct();
                    foreach (var z in zValues)
                    {
                        foreach (var y in yValues)
                        {
                            foreach (var x in xValues)
                            {
                                var fund = VectorConverter.CreateVector(x, y, z);

                                var list = new List<IPitch>();
                                foreach (var p in shape)
                                {
                                    var dif = p.Position - fund;
                                    if (dif[0] >= 0 && dif[1] >= 0 && dif[2] >= 0)
                                    {
                                        var pitch = new Pitch(p.PitchHeight, p.FifthHeight, p.Position);
                                        list.Add(pitch);
                                    }
                                }
                                // find the different sized groups that all have the same fundamental
                                var sizes = new List<int>();
                                if (list.Count > 1)
                                {
                                    list = list.OrderBy(p => p.PitchHeight).ToList();
                                    for (var size = 2; size <= list.Count; size++)
                                    {
                                        var take = list.Take(size).ToList();
                                        var fundamental = PitchConverter.GreatestLowerBound(take);
                                        if (fund == fundamental)
                                        {
                                            sizes.Add(size);
                                        }
                                    }
                                    var offset = VectorConverter.Normalize(fund);
                                    foreach (int size in sizes)
                                    {
                                        var pitches = list.Take(size).ToList();
                                        Debug.Assert(PitchConverter.GreatestLowerBound(pitches) == fund);
                                        var children = new List<Child>();
                                        foreach (var pitch in pitches)
                                        {
                                            var tone = Groups.Single(g => g.Type == GroupType.Tone && g.ChordIndex == iChord && g.Tone != null && g.Tone.PitchHeight == pitch.PitchHeight);
                                            var child = new Child(tone, pitch.Position + offset);
                                            children.Add(child);
                                        }
                                        //--------------------------------------------------------
                                        // The object model is:
                                        //  Group
                                        //      Child
                                        //          Tone x M
                                        var group = new Group
                                        {
                                            ChordIndex = iChord,
                                            Type = GroupType.Group,
                                            Position = fund + offset,
                                            Children = children,
                                            Tone = null,
                                        };
                                        if (!ContainsGroup(group))
                                        {
                                            // TODO: Group with single Tone
                                            if (group.Children.Count > 1)
                                            {
                                                Groups.Add(group);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// convert Groups to Vertical Groups
        /// </summary>
        /// <param name="chords">chord progression</param>
        public void GroupsToVertGroups(PitchPattern[] chords)
        {
            // When stored, the VerticalGroup Positions are normalized, and the children Positions are normalized with respect to the parent Position
            VerifyMethodOrder(3);
            for (var iChord = 0; iChord < chords.Length; ++iChord)
            {
                // distinct Positions of Tones and Groups. all PitchPattern Pitch Positions are normalized to z=0
                var chord = GroupsToPitchPattern(iChord);

                // distinct Positions of Tones and Groups, projected into four 'shapes' restricted to strips y = [-3..0], [-2..1], [-1..2], [0..3] and z >= 0
                var shapes = PitchConverter.ChordToShapes(chord);
                var glb = PitchConverter.GreatestLowerBound(shapes);
                var lub = PitchConverter.LeastUpperBound(shapes);
                for (var y = glb[1] - 3; y <= lub[1]; ++y)
                {
                    for (var x = glb[0]; x <= lub[0]; ++x)
                    {
                        var fund = VectorConverter.CreateVector(x, y, 0);
                        var offset = VectorConverter.Normalize(fund);

                        foreach (var shape in shapes)
                        {
                            // distinct Positions of Tones and Groups that are harmonically related to the fundamental
                            var list = new List<PitchTriplet>();
                            for (int i = 0; i < shape.Count - 1; i++)
                            {
                                for (int j = i + 1; j < shape.Count; j++)
                                {
                                    if (PitchConverter.GreatestLowerBound(shape[i], shape[j]) == fund)
                                    {
                                        var delta1 = shape[i].Position - fund;
                                        var delta2 = shape[j].Position - fund;
                                        if (delta1[1] <= 3 && delta1[2] <= 1 && delta2[1] <= 3 && delta2[2] <= 1)
                                        {
                                            if (shape[i] == shape[j]) throw new Exception();
                                            // Pitch1 and Pitch2 not normalized, and should be normalized before they are used
                                            list.Add(new PitchTriplet(fund, shape[i], shape[j]));
                                        }
                                    }
                                }
                            }
                            foreach (var triplet in list)
                            {
                                // each distinct Positions of the pair can have multiple Tones and Groups
                                var a = Groups.Where(g => (g.Type == GroupType.Tone || g.Type == GroupType.Group) && g.ChordIndex == iChord && g.Position == triplet.Pitch1.Position + VectorConverter.Normalize(triplet.Pitch1.Position)).ToList();
                                var b = Groups.Where(g => (g.Type == GroupType.Tone || g.Type == GroupType.Group) && g.ChordIndex == iChord && g.Position == triplet.Pitch2.Position + VectorConverter.Normalize(triplet.Pitch2.Position)).ToList();
                                for (int k = 0; k < a.Count; k++)
                                {
                                    for (int l = 0; l < b.Count; l++)
                                    {
                                        // Groups with two tones are already saved. Don't save them again.
                                        int toneCount = a[k].ToneCount() + b[l].ToneCount();
                                        if (toneCount < 3)
                                        {
                                            continue;
                                        }
                                        // Children order is normalized so Children[0].Position < Children[1].Position
                                        var child1 = new Child(a[k], triplet.Pitch1.Position + offset);
                                        var child2 = new Child(b[l], triplet.Pitch2.Position + offset);
                                        List<Child> children = VectorConverter.Compare(
                                            VectorConverter.Normalize(child1.Position), 
                                            VectorConverter.Normalize(child2.Position)) > 0 ?
                                            new List<Child> { child2, child1 } :
                                            new List<Child> { child1, child2 };
                                        // Position is normalized so 0 <= y < 4, 0 <= z < 3
                                        //--------------------------------------------------------
                                        // The object model is:
                                        //  VerticalGroup
                                        //      Child
                                        //          Group
                                        //          Group or Tone
                                        var group = new Group
                                        {
                                            ChordIndex = iChord,
                                            Type = GroupType.VerticalGroup,
                                            Position = fund + offset,
                                            Children = children,
                                            Tone = null,
                                        };
                                        // check tones' positions in the parent's coordinate system are within bounds
                                        if (!group.GroupWithinRange(3, 2, GroupType.Tone))
                                        {
                                            continue;
                                        }
                                        if (!ContainsGroup(group))
                                        {
                                            Groups.Add(group);
                                        }
                                        // TODO: temporary fix
                                        //group = new Group
                                        //{
                                        //    ChordIndex = iChord,
                                        //    Type = GroupType.VerticalGroup,
                                        //    Position = fund + offset,
                                        //    Children = new List<Child> { children[1], children[0] },
                                        //    Tone = null,
                                        //};
                                        //// check tones' positions in the parent's coordinate system are within bounds
                                        //if (!group.GroupWithinRange(3, 2, GroupType.Tone))
                                        //{
                                        //    continue;
                                        //}
                                        //if (!ContainsGroup(group))
                                        //{
                                        //    Groups.Add(group);
                                        //}
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        //public void GroupsToVertGroups_old(PitchPattern[] chords)
        //{
        //    VerifyMethodOrder(3);
        //    for (var iChord = 0; iChord < chords.Length; ++iChord)
        //    {
        //        // distinct Positions of Tones and Groups. all PitchPattern Pitch Positions are normalized to z=0
        //        var chord = GroupsToPitchPattern(iChord);

        //        // distinct Positions of Tones and Groups, projected into four 'shapes' restricted to strips y = [-3..0], [-2..1], [-1..2], [0..3] and z >= 0
        //        var shapes = PitchConverter.ChordToShapes(chord);
        //        var glb = PitchConverter.GreatestLowerBound(shapes);
        //        var lub = PitchConverter.LeastUpperBound(shapes);
        //        for (var y = glb[1] - 3; y <= lub[1]; ++y)
        //        {
        //            for (var x = glb[0]; x <= lub[0]; ++x)
        //            {
        //                var fund = VectorConverter.CreateVector(x, y, 0);
        //                var offset = VectorConverter.Normalize(fund);

        //                foreach (var shape in shapes)
        //                {
        //                    // distinct Positions of Tones and Groups that are harmonically related to the fundamental
        //                    var list = new List<IPitch>();
        //                    foreach (var p in shape)
        //                    {
        //                        var dif = p.Position - fund;
        //                        if (dif[0] >= 0 && dif[1] >= 0 && dif[2] >= 0 && dif[1] < 4 && dif[2] < 3)
        //                        {
        //                            var pitch = new Pitch(p.PitchHeight, p.FifthHeight, p.Position);
        //                            list.Add(pitch);
        //                        }
        //                    }
        //                    if (list.Count > 1)
        //                    {
        //                        list = list.OrderBy(p => p.PitchHeight).ToList();

        //                        // pairs of distinct Positions of Tones and Groups
        //                        for (var i = 0; i < list.Count - 1; ++i)
        //                        {
        //                            for (int j = i + 1; j < list.Count; j++)
        //                            {
        //                                // list is unique, so this is not necessary
        //                                if (list[i].Position == list[j].Position)
        //                                {
        //                                    continue;
        //                                }

        //                                var take = new List<IPitch> { list[i], list[j] };
        //                                var fundamental = PitchConverter.GreatestLowerBound(take);
        //                                if (fund != fundamental)
        //                                {
        //                                    continue;
        //                                }

        //                                // each distinct Positions of the pair can have multiple Tones and Groups
        //                                var a = Groups.Where(g => (g.Type == GroupType.Tone || g.Type == GroupType.Group) && g.ChordIndex == iChord && g.Position == list[i].Position).ToList();
        //                                var b = Groups.Where(g => (g.Type == GroupType.Tone || g.Type == GroupType.Group) && g.ChordIndex == iChord && g.Position == list[j].Position).ToList();
        //                                for (int k = 0; k < a.Count; k++)
        //                                {
        //                                    for (int l = 0; l < b.Count; l++)
        //                                    {
        //                                        //Debug.Write($"{k} {l} ");
        //                                        //PitchConverter.DebugWriteVector(fund);
        //                                        //Debug.Write(" : ");
        //                                        //PitchConverter.DebugWriteVector(list[i].Position);
        //                                        //Debug.Write(" : ");
        //                                        //PitchConverter.DebugWriteVector(list[j].Position);
        //                                        //Debug.Write(" : ");
        //                                        //PitchConverter.DebugWriteVector(a[k].Position);
        //                                        //Debug.Write(" : ");
        //                                        //PitchConverter.DebugWriteVector(b[l].Position);
        //                                        //Debug.WriteLine(null);

        //                                        // Groups with two tones are already saved. Don't save them again.
        //                                        int toneCount = a[k].ToneCount() + b[l].ToneCount();
        //                                        if (toneCount < 3)
        //                                        {
        //                                            continue;
        //                                        }
        //                                        // Position is normalized so 0 <= y < 4, 0 <= z < 3
        //                                        var group = new Group
        //                                        {
        //                                            ChordIndex = iChord,
        //                                            Type = GroupType.VerticalGroup,
        //                                            Position = fund + offset,
        //                                            Children = new List<Child>
        //                                            {
        //                                                new Child(a[k], list[i].Position + offset),
        //                                                new Child(b[l], list[j].Position + offset),
        //                                            },
        //                                            Tone = null,
        //                                        };
        //                                        if (!ContainsGroup(group))
        //                                        {
        //                                            Groups.Add(group);
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        /// <summary>
        /// convert Tones to Frames
        /// </summary>
        /// <param name="chords">chord progression</param>
        public void TonesToFrames(PitchPattern[] chords)
        {
            // When stored, the Frame Positions are normalized, and the children Positions are normalized with respect to the parent Position
            // generate a set of frames based on the pairwise distances between the tones
            // the alternative, taking the distance between the glb and lub of the tones, produces similar results, but are occasionally larger
            VerifyMethodOrder(4);
            for (var iChord = 0; iChord < chords.Length; ++iChord)
            {
                List<Group> frames = new List<Group>();
                for (int z = 0; z < 3; z++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        var position = VectorConverter.CreateVector(0, y, z);

                        var groups = Groups.Where(g => g.ChordIndex == iChord && g.Type == GroupType.Tone);
                        var children = groups.Select(g => new Child(g, g.Position + VectorConverter.Normalize(g.Position, position))).ToList();
                        position = VectorConverter.CreateVector(children.Min(c => c.Position[0]), y, z);

                        var bottomLeft = VectorConverter.CreateVector(children.Min(c => c.Position[0]), children.Min(c => c.Position[1]), children.Min(c => c.Position[2]));
                        //var topRight = VectorConverter.CreateVector(children.Max(c => c.Position[0]), children.Max(c => c.Position[1]), children.Max(c => c.Position[2]));
                        var topRight = VectorConverter.CreateVector(children.Max(c => c.Position[0]), bottomLeft[1] + 3, bottomLeft[2] + 2);

                        if (y != bottomLeft[1] || z != bottomLeft[2])
                        {
                            continue;
                        }
                        Debug.Assert(position == bottomLeft);
                        //--------------------------------------------------------
                        // The object model is:
                        //  Frame
                        //      Child
                        //          Tone x M
                        var frame = new Group
                        {
                            ChordIndex = iChord,
                            Type = GroupType.Frame,
                            Position = position,
                            Children = children,
                            Tone = null,
                            Bounds = [bottomLeft, topRight],
                        };
                        frames.Add(frame);
                    }
                }

                // select a set of frames based on IsCover and IsSingleComponent
                var framesEvaluated = new List<Group>();
                foreach (var frame in frames)
                {
                    int IsCover;
                    int IsSingleComponent;
                    EvaulateFrame(frame, out IsCover, out IsSingleComponent);
                    if (IsCover > 0 /*&& IsSingleComponent > 0*/)
                    {
                        framesEvaluated.Add(frame);
                    }
                }
                foreach (var frame in framesEvaluated)
                {
                    Groups.Add(frame);
                }
                //

                // select a set of frames based on the pairwise distances between the tones
                //frames = frames.OrderBy(f => f.Distance.MaxPairwiseFrame()).ToList();
                //int threshold = ThresholdFrames(frames);

                //for (int i = 0; i < frames.Count; i++)
                //{
                //    var frame = frames[i];
                //    VectorConverter.DebugWriteVector(frame.Position);
                //    var include = i <= threshold ? "*" : string.Empty;
                //    Debug.WriteLine($" : {frame.Distance.MaxPairwiseFrame(),3} {VectorConverter.VectorToDistance(frame.Bounds[1] - frame.Bounds[0]),3} {include}");
                //}

                //for (int i = 0; i <= threshold; i++)
                //{
                //    Groups.Add(frames[i]);
                //}
            }
        }
        public GroupMap EvaulateFrame(Group frame, out int IsCover, out int IsSingleComponent)
        {
            IsCover = 0;
            IsSingleComponent = 0;

            // create and initialize map
            var map = GroupMapLib.InitGroupMap(frame, Groups);

            foreach (var snapshot in map.Snapshots)
            {
                if (snapshot.IsCover)
                //if (snapshot.ToneMaps[0].All(s => s.HorizontalGroup != null))
                {
                    ++IsCover;
                }
                else
                {
                    // look no further
                    continue;
                }
                // see if graph is a single component
                //var processed = new List<Group>();
                //var groups = snapshot.ToneMaps[0].Select(s => s.HorizontalGroup).Distinct().ToList();
                //var set = new HashSet<Group>();
                //while (processed.Count < groups.Count)
                //{
                //    bool found = false;
                //    foreach (var group in groups.Where(g => processed.IndexOf(g) == -1))
                //    {
                //        var tones = new HashSet<Group>(group.ListTones());
                //        if (set.Count == 0 || set.Intersect(tones).Count() > 0)
                //        {
                //            found = true;
                //            set.UnionWith(tones);
                //            processed.Add(group);
                //        }
                //    }
                //    if (!found)
                //    {
                //        break;
                //    }
                //}
                if (snapshot.IsSingleComponent)
                //if (processed.Count == groups.Count)
                {
                    ++IsSingleComponent;
                }
            }
            return map;
        }
        /// <summary>
        /// convert Groups to Horizontal Groups
        /// </summary>
        /// <param name="chords">chord progression</param>
        public void GroupsToHorizGroups(PitchPattern[] chords)
        {
            // When stored, the HorizGroup Positions are normalized, and the children Positions are normalized with respect to the parent Position

            VerifyMethodOrder(5);
            for (var iChord = 0; iChord < chords.Length - 1; ++iChord)
            {
                var iChordNext = iChord + 1;
                // distinct Positions of Tones and Groups. all PitchPattern Pitch Positions are normalized to z=0
                var chord = GroupsToPitchPattern(iChord);
                var chordNext = GroupsToPitchPattern(iChordNext);

                // shapes of Positions of Tones and Groups, corresponding to y-min = 0, -1, -2, -3. output (nominally) 4 shapes with z=0,1
                var shapes = PitchConverter.ChordToShapes(chord);
                var shapesNext = PitchConverter.ChordToShapes(chordNext);

                var glb = PitchConverter.GreatestLowerBound(shapes, shapesNext);
                var lub = PitchConverter.LeastUpperBound(shapes, shapesNext);
                for (var y = glb[1]; y <= lub[1]; ++y)
                {
                    for (var x = glb[0]; x <= lub[0]; ++x)
                    {
                        var fund = VectorConverter.CreateVector(x, y, 0);
                        var offset = VectorConverter.Normalize(fund);
                        // don't normalize the fundamental just yet

                        for (var iShape = 0; iShape < Math.Min(shapes.Count, shapesNext.Count); iShape++)
                        {
                            var shape = shapes[iShape];
                            var shapeNext = shapesNext[iShape]; // shapesNext[iShape] is not a typo

                            // distinct Positions of Tones and Groups that are harmonically related to the fundamental
                            var list = new List<PitchTriplet>();
                            for (int i = 0; i < shape.Count; i++)
                            {
                                for (int j = 0; j < shapeNext.Count; j++)
                                {
                                    var offset_i = VectorConverter.Normalize(shape[i].Position, fund);
                                    var offset_j = VectorConverter.Normalize(shapeNext[j].Position, fund);
                                    if (PitchConverter.GreatestLowerBound(shape[i].Position + offset_i, shapeNext[j].Position + offset_j) == fund)
                                    {
                                        var delta = shape[i].Position + offset_i - fund;
                                        var deltaNext = shapeNext[j].Position + offset_j - fund;
                                        if (delta[1] <= 3 && delta[2] <= 1 && deltaNext[1] <= 3 && deltaNext[2] <= 1)
                                        {
                                            if (!list.Any(t => t.Position == fund && t.Pitch1 == shape[i] && t.Pitch2 == shapeNext[j]))
                                            {
                                                // pitch1 and pitch2 not normalized, and should be normalized before they are used
                                                list.Add(new PitchTriplet(fund, shape[i], shapeNext[j]));
                                            }
                                        }
                                    }
                                }
                            }
                            foreach (var triplet in list)
                            {
                                // each distinct Position of the pair can have multiple Tones and Groups
                                var a = Groups.Where(g => (g.Type == GroupType.Tone || g.Type == GroupType.Group) && g.ChordIndex == iChord && g.Position == triplet.Pitch1.Position + VectorConverter.Normalize(triplet.Pitch1.Position)).ToList();
                                var b = Groups.Where(g => (g.Type == GroupType.Tone || g.Type == GroupType.Group) && g.ChordIndex == iChordNext && g.Position == triplet.Pitch2.Position + VectorConverter.Normalize(triplet.Pitch2.Position)).ToList();
                                for (int k = 0; k < a.Count; k++)
                                {
                                    for (int l = 0; l < b.Count; l++)
                                    {
                                        var offset_k = VectorConverter.Normalize(a[k].Position, fund + offset);
                                        var offset_l = VectorConverter.Normalize(b[l].Position, fund + offset);
                                        // Position is normalized so 0 <= y < 4, 0 <= z < 3
                                        //--------------------------------------------------------
                                        // The object model is:
                                        //  HorizontalGroup
                                        //      Child
                                        //          Tone or Group
                                        //      Child
                                        //          Tone or Group
                                        var group = new Group
                                        {
                                            ChordIndex = iChord,
                                            Type = GroupType.HorizontalGroup,
                                            Position = fund + offset,
                                            Children = new List<Child>
                                                    {
                                                        new Child(a[k], a[k].Position + offset_k),
                                                        new Child(b[l], b[l].Position + offset_l),
                                                    },
                                            Tone = null,
                                        };
                                        // check tones' positions in the parent's coordinate system are within bounds
                                        if (!group.GroupWithinRange(4, 3, GroupType.Tone))
                                        {
                                            continue;
                                        }
                                        if (!ContainsGroup(group))
                                        {
                                            Groups.Add(group);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        readonly Vector<int>[] Deltas = new Vector<int>[] { VectorConverter.CreateVector(0, 0, 0), VectorConverter.CreateVector(0, -1, 0), VectorConverter.CreateVector(0, 1, 0), VectorConverter.CreateVector(0, 0, -1), VectorConverter.CreateVector(0, 0, 1) };

        // FrameToVerticalFrame and FrameToHorizFrame are almost the same
        /// <summary>
        /// generate a set of frame pairs based on the pairwise distances between tones from two chords
        /// </summary>
        /// <param name="chords">chord progression</param>
        public void FrameToVerticalFrame(PitchPattern[] chords)
        {
            VerifyMethodOrder(6);
            // When stored, the VerticalFrame Position is the same as the first child Frame Position, and is normalized.
            // the first child Frame Position is normalized
            // the second child Frame Position is the Frame Position plus an alias interval
            for (var iChord = 0; iChord < chords.Length; ++iChord)
            {
                var frames = new List<Group>();
                foreach (var frame in Groups.Where(g => g.ChordIndex == iChord && g.Type == GroupType.Frame ))
                {
                    foreach (var frameNext in Groups.Where(g => g.ChordIndex == iChord && g.Type == GroupType.Frame && g != frame))
                    {
                        Vector<int>? frameNextPos = NeighborFrame(frame, frameNext);
                        if (frameNextPos.HasValue)
                        {
                            var child = new Child(frame, frame.Position);
                            var childNext = new Child(frameNext, frameNextPos.Value);
                            //Debug.WriteLine($"child {VectorConverter.DebugFormatVector(child.Position)} childNext {VectorConverter.DebugFormatVector(childNext.Position)}");
                            //--------------------------------------------------------
                            // The object model is:
                            //  VerticalFrame
                            //      Child
                            //          Frame
                            //              Child
                            //                  Tone x M
                            //      Child
                            //          Frame
                            //              Child
                            //                  Tone x M
                            var group = new Group
                            {
                                ChordIndex = iChord,
                                Type = GroupType.VerticalFrame,
                                Position = frame.Position,
                                Children = [child, childNext],
                                Tone = null,
                            };
                            Debug.Assert(VectorConverter.IsNormalized(group.Position));
                            Debug.Assert(Math.Abs(group.Children[0].Position[1] - group.Children[1].Position[1]) <= 1);
                            Debug.Assert(Math.Abs(group.Children[0].Position[2] - group.Children[1].Position[2]) <= 1);
                            frames.Add(group);
                        }
                    }
                }
                // TODO: I skipped the thresholding step because the number of VerticalFrames is not large.
                // TODO: threshold has a big effect on results. need to investigate further
                for (int i = 0; i < frames.Count/*i <= threshold*/; i++)
                {
                    Debug.Assert(!ContainsGroup(frames[i]));
                    Groups.Add(frames[i]);
                }
            }
        }

        /// <summary>
        /// generate a set of frame pairs based on the pairwise distances between tones from two chords
        /// </summary>
        /// <param name="chords">chord progression</param>
        public void FrameToHorizFrame(PitchPattern[] chords)
        {
            VerifyMethodOrder(7);
            // When stored, the HorizontalFrame Position is the same as the first child Frame Position, and is normalized.
            // the first child Frame Position is normalized
            // the second child Frame Position is the Frame Position plus an alias interval
            for (var iChord = 0; iChord < chords.Length - 1; ++iChord)
            {
                var iChordNext = iChord + 1;

                var frames = new List<Group>();
                foreach (var frame in Groups.Where(g => g.ChordIndex == iChord && g.Type == GroupType.Frame))
                {
                    var y = frame.Position[1];
                    var z = frame.Position[2];
                    var children = new List<Child>();
                    foreach (var frameNext in Groups.Where(g => g.ChordIndex == iChordNext && g.Type == GroupType.Frame && g != frame))
                    {
                        Debug.Assert(VectorConverter.IsNormalized(frame.Position));
                        Debug.Assert(VectorConverter.IsNormalized(frameNext.Position));
                        Vector<int>? frameNextPos = NeighborFrame(frame, frameNext);
                        if (frameNextPos.HasValue)
                        {
                            var childNext = new Child(frameNext, frameNextPos.Value);
                            children.Add(childNext);
                        }
                    }
                    foreach (var childNext in children)
                    {
                        //--------------------------------------------------------
                        // The object model is:
                        //  HorizontalFrame
                        //      Child
                        //          Frame
                        //              Child
                        //                  Tone x M
                        //      Child
                        //          Frame
                        //              Child
                        //                  Tone x N
                        var child = new Child(frame, frame.Position);
                        var group = new Group
                        {
                            ChordIndex = iChord,
                            Type = GroupType.HorizontalFrame,
                            Position = frame.Position,
                            Children = new List<Child> { child, childNext },
                            Tone = null,
                        };
                        Debug.Assert(VectorConverter.IsNormalized(group.Position));
                        Debug.Assert(Math.Abs(group.Children[0].Position[1] - group.Children[1].Position[1]) <= 1);
                        Debug.Assert(Math.Abs(group.Children[0].Position[2] - group.Children[1].Position[2]) <= 1);
                        frames.Add(group);
                    }
                }

                // containment hierarchy
                frames = frames.OrderBy(f => f.Distance.MaxPairwiseHorizontalFrame()).ToList();
                int threshold = ThresholdHorizontalFrames(frames);

                // TODO: threshold has a big effect on results. need to investigate further
                for (int i = 0; i < frames.Count/*i <= threshold*/; i++)
                {
                    Groups.Add(frames[i]);
                }
            }
        }

        public PitchPattern GroupsToPitchPattern(int iChord)
        {
            //foreach (var group in Groups.Where(g => g.Type == GroupType.Tone || g.Type == GroupType.Group))
            //{
            //    Debug.Assert(VectorConverter.IsNormalized(group.Position));
            //}   
            var positions = Groups.Where(g => (g.Type == GroupType.Tone || g.Type == GroupType.Group) && g.ChordIndex == iChord)
                .Select(g => g.Position).Distinct().ToList();
            // by convention, all PitchPattern Pitch Positions are normalized to z=0
            var normalizedTo_z0 = positions.Select(p => VectorConverter.NormalizeTo_z0(p)).ToList();
            var pitches = normalizedTo_z0.Select(p => new Pitch(VectorConverter.VectorToPitch(p), p[1])).Cast<IPitch>().ToList();
            var chord = new PitchPattern { Pitches = pitches };
            return chord;
        }
        /// <summary>
        /// if Groups contains group
        /// </summary>
        public bool ContainsGroup(Group group)
        {
            return GetGroup(group) != null;
        }
        /// <summary>
        /// if Groups contains group, return the group
        /// </summary>
        public Group? GetGroup(Group group)
        {
            var m = Groups.Where(g => g.Equals(group)).ToList();
            if (m.Count == 0)
            {
                return null;
            }
            if (m.Count == 1)
            {
                return m[0];
            }
            throw new Exception();
        }
        /// <summary>
        /// returns the number of tones covered by the children
        /// </summary>
        /// <param name="children">list of children</param>
        /// <param name="count">supports short-circuit evaluation - to disable short-circuit evaluation use -1</param>
        /// <returns></returns>
        // will be removed
        public int TonesCovered(List<Child> children, int count)
        {
            var tones = new List<Group>();
            foreach (var child in children)
            {
                child.Group.ListTones(ref tones);
                if (tones.Count == count)
                {
                    break;
                }
            }
            return tones.Count;
        }

        /// <summary>
        /// return denormalized position of frameNext if it is a neighbor of frame
        /// </summary>
        /// <param name="frame">reference Frame</param>
        /// <param name="frameNext">target Frame</param>
        public Vector<int>? NeighborFrame(Group frame, Group frameNext)
        {
            Vector<int>? frameNextPos = null;
            foreach (var delta in Deltas)
            {
                foreach (var alias in VectorConverter.Aliases)
                {
                    var pos = frameNext.Position + alias;
                    if (pos[1] == frame.Position[1] + delta[1] && pos[2] == frame.Position[2] + delta[2])
                    {
                        frameNextPos = pos;
                        break;
                    }
                }
                if (frameNextPos.HasValue)
                {
                    break;
                }
            }

            return frameNextPos;
        }

        private int ThresholdHorizontalFrames(List<Group> frames)
        {
            // value TBD
            const int threshold = int.MinValue;
            if (frames.Count == 0 || frames.Any(f => f.Type != GroupType.HorizontalFrame))
            {
                return -1;
            }
            int count = -1;
            var baseline = frames[0].Distance.MaxPairwiseHorizontalFrame();
            while (count + 1 < frames.Count && (
                (double)frames[count + 1].Distance.MaxPairwiseHorizontalFrame() <= 2.5 * (double)baseline ||
                frames[count + 1].Distance.MaxPairwiseHorizontalFrame() < threshold))
            {
                count++;
            }
            return count;
        }
        public void PrintGroups()
        {
            foreach (var g in Groups)
            {
                g.DebugWrite(0, Vector<int>.Zero);
            }
        }
        public void PrintGroups(params GroupType[] types)
        {
            Debug.WriteLine("PrintGroups DISABLED"); return;
            foreach (var g in Groups.Where(g => types.Contains(g.Type)))
            {
                g.DebugWrite(0, Vector<int>.Zero);
            }
        }
        public void PrintGroups(int iChord, params GroupType[] types)
        {
            foreach (var g in Groups.Where(g => g.ChordIndex == iChord && types.Contains(g.Type)))
            {
                g.DebugWrite(0, Vector<int>.Zero);
            }
        }
        void PrintHorizFrame(int iChord, int iChordNext, Group? horizFrame)
        {
            var iFrame = Groups.Where(g => g.Type == GroupType.HorizontalFrame && g.ChordIndex == iChord).ToList().IndexOf(horizFrame);
            var iVertGroup0 = Groups.Where(g => g.Type == GroupType.Frame && g.ChordIndex == iChord).ToList().IndexOf(horizFrame.Children[0].Group);
            var iVertGroup1 = Groups.Where(g => g.Type == GroupType.Frame && g.ChordIndex == iChordNext).ToList().IndexOf(horizFrame.Children[1].Group);
            Debug.Write($"Chord {iChord} {iChordNext} : HorizFrame # {iChord} {iFrame,-2} : VertFrame # {iChord} {iVertGroup0,-2} : VertFrame # {iChordNext} {iVertGroup1,-2} : ");
            Debug.WriteLine(horizFrame.HorizFrameId);
        }
        public void PrintGroupCounts()
        {
            Debug.WriteLine($"Tone:             {Groups.Count(g => g.Type == GroupType.Tone)}");
            Debug.WriteLine($"Group:            {Groups.Count(g => g.Type == GroupType.Group)}");
            Debug.WriteLine($"Group distinct positions: {Groups.Where(g => g.Type == GroupType.Group).Select(g => g.Position.ToString()).OrderBy(p => p).Distinct().Count()}");
            Debug.WriteLine($"VerticalGroup:    {Groups.Count(g => g.Type == GroupType.VerticalGroup)}");
            Debug.WriteLine($"VerticalGroup distinct positions: {Groups.Where(g => g.Type == GroupType.VerticalGroup).Select(g => g.Children[0].Position.ToString() + g.Children[1].Position.ToString()).OrderBy(p => p).Distinct().Count()}");
            Debug.WriteLine($"Frame:            {Groups.Count(g => g.Type == GroupType.Frame)}");
            Debug.WriteLine($"HorizontalGroup:  {Groups.Count(g => g.Type == GroupType.HorizontalGroup)}");
            Debug.WriteLine($"HorizontalGroup distinct positions: {Groups.Where(g => g.Type == GroupType.HorizontalGroup).Select(g => g.Children[0].Position.ToString() + g.Children[1].Position.ToString()).OrderBy(p => p).Distinct().Count()}");
            Debug.WriteLine($"HorizontalFrame:  {Groups.Count(g => g.Type == GroupType.HorizontalFrame)}");
        }
        //private static void VerifyHorizontalGroupMap(HorizontalGroupMap_old horizontalGroupMap)
        //{
        //    var child0Tones = new List<Group>();
        //    var child1Tones = new List<Group>();
        //    var child0Pos = new List<Vector<int>>();
        //    var child1Pos = new List<Vector<int>>();
        //    foreach (var h in horizontalGroupMap.HorizontalGroups)
        //    {
        //        var list0 = new List<Group>();
        //        h.Children[0].Group.GetChildPositions(Vector<int>.Zero, ref list0, -1, GroupType.Tone);
        //        child0Tones.AddRange(list0);
        //        child0Tones = child0Tones.OrderBy(g => g.Position[1]).ThenBy(g => g.Position[2]).Distinct().ToList();
        //        child0Pos.AddRange(list0.Select(g => g.Tone.Position));
        //        child0Pos = child0Pos.OrderBy(v => v[1]).ThenBy(v => v[2]).Distinct().ToList();

        //        var list1 = new List<Group>();
        //        h.Children[0].Group.GetChildPositions(Vector<int>.Zero, ref list1, -1, GroupType.Tone);
        //        child1Tones.AddRange(list1);
        //        child1Tones = child1Tones.OrderBy(g => g.Position[1]).ThenBy(g => g.Position[2]).Distinct().ToList();
        //        child1Pos.AddRange(list1.Select(g => g.Tone.Position));
        //        child1Pos = child1Pos.OrderBy(v => v[1]).ThenBy(v => v[2]).Distinct().ToList();
        //    }
        //    if (horizontalGroupMap.IsNotNull() && (child0Pos.Count != 4 || child1Pos.Count != 4))
        //    {

        //    }
        //    horizontalGroupMap.DebugValid();
        //}

        public bool ValidateGroups()
        {
            // verify that all groups are distinct
            var valid = Groups.All(group => Groups.Count(g => group.Equals(g)) == 1);
            return valid;
        }
        public void Reset()
        {
            if (Groups.Count != 0)
            {
                Groups.Clear();
            }
            if (ChordShapes.Count != 0)
            {
                ChordShapes.Clear();
            }
        }
        public void ResetMethodOrder() => _step = 0;
        // verify that methods are called in the correct order
        public void VerifyMethodOrder(int step)
        {
            if (_step != step)
            {
                throw new InvalidOperationException();
            }
            ++_step;
        }

        #region Obsolete. Can be removed. I am still working on experimental code.
        /// <summary>
        /// convert PitchPattern to Tones
        /// </summary>
        /// <param name="chords">chord progression</param>
        public void PitchPatternToTones_old(PitchPattern[] chords)
        {
            // this is the only method for creating Tones
            VerifyMethodOrder(1);
            for (var iChord = 0; iChord < chords.Length; ++iChord)
            {
                var chord = chords[iChord];
                foreach (var pitch in chord.Pitches)
                {
                    //var vector = VectorConverter.PitchAndFifthToVector(pitch.PitchHeight, pitch.FifthHeight);
                    //--------------------------------------------------------
                    // The object model is:
                    //  Tone
                    //      Child
                    //          (none)
                    //      Tone : IPitch
                    var group = TonesToGroups(pitch, iChord);
                    Groups.Add(group);
                }
            }
        }
        /// <summary>
        /// convert Tones to Groups
        /// </summary>
        /// <param name="chords">chord progression</param>
        public void TonesToGroups_old(PitchPattern[] chords)
        {
            // When stored, the Group Positions are normalized, and the children Positions are normalized with respect to the parent Position
            VerifyMethodOrder(2);
            for (var iChord = 0; iChord < chords.Length; ++iChord)
            {
                //
                var dict = new Dictionary<string, List<Group>>();
                //
                var chord = chords[iChord];
                // distinct Positions of Tones and Groups, projected into four 'shapes' restricted to strips y = [-3..0], [-2..1], [-1..2], [0..3] and z >= 0
                var shapes = PitchConverter.ChordToShapes(chord);
                var glb = PitchConverter.GreatestLowerBound(shapes);
                var lub = PitchConverter.LeastUpperBound(shapes);
                for (var y = glb[1] - 3; y <= lub[1]; ++y)
                {
                    for (var x = glb[0]; x <= lub[0]; ++x)
                    {
                        var fund = VectorConverter.CreateVector(x, y, 0);
                        var pitchHeight = VectorConverter.VectorToPitch(fund);

                        foreach (var shape in shapes)
                        {
                            //
                            var key = string.Join(" : ", shape.Select(v => $"{v.Position[0],3}, {v.Position[1],2}, {v.Position[2],2} "));
                            //
                            var list = new List<IPitch>();
                            foreach (var p in shape)
                            {
                                var dif = p.Position - fund;
                                if (dif[0] >= 0 && dif[1] >= 0 && dif[2] >= 0)
                                {
                                    var pitch = new Pitch(p.PitchHeight, p.FifthHeight, p.Position);
                                    list.Add(pitch);
                                }
                            }
                            // find the different sized groups that all have the same fundamental
                            var sizes = new List<int>();
                            if (list.Count > 1)
                            {
                                list = list.OrderBy(p => p.PitchHeight).ToList();
                                for (var size = 1; size <= list.Count; size++)
                                {
                                    var take = list.Take(size).ToList();
                                    var fundamental = PitchConverter.GreatestLowerBound(take);
                                    if (fund == fundamental)
                                    {
                                        sizes.Add(size);
                                    }
                                }
                                var offset = VectorConverter.Normalize(fund);
                                foreach (int size in sizes)
                                {
                                    var pitches = list.Take(size).ToArray();
                                    var children = new List<Child>();
                                    foreach (var pitch in pitches)
                                    {
                                        var tone = Groups.Single(g => g.Type == GroupType.Tone && g.ChordIndex == iChord && g.Tone != null && g.Tone.PitchHeight == pitch.PitchHeight);
                                        var child = new Child(tone, pitch.Position + offset);
                                        children.Add(child);
                                    }
                                    //--------------------------------------------------------
                                    // The object model is:
                                    //  Group
                                    //      Child
                                    //          Tone x M
                                    var group = new Group
                                    {
                                        ChordIndex = iChord,
                                        Type = GroupType.Group,
                                        Position = fund + offset,
                                        Children = children,
                                        Tone = null,
                                    };
                                    if (!ContainsGroup(group))
                                    {
                                        //
                                        if (!dict.ContainsKey(key))
                                        {
                                            dict.Add(key, new List<Group>());
                                        }
                                        dict[key].Add(group);
                                        //
                                        // TODO: Group with single Tone
                                        if (group.Children.Count > 1)
                                        {
                                            Groups.Add(group);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                //
                //foreach (var key in dict.Keys)
                //{
                //    Debug.WriteLine(key);
                //    foreach (var group in dict[key])
                //    {
                //        group.DebugWrite(1, Vector<int>.Zero);
                //    }
                //}
                //
            }
        }
        /// <summary>
        /// find frames child groups that cover all chord tones
        /// </summary>
        /// <param name="chords">chord progression</param>
        /// <returns>Frames are returned to the caller so they can be further evaluated before adding them to this.Groups</returns>
        public List<Group> GroupsToFrames_old(PitchPattern[] chords)
        {
            // frames are currently not used. the thinking is that they can be generated on the fly
            VerifyMethodOrder(4);
            var frames = new List<Group>();
            for (var iChord = 0; iChord < chords.Length; ++iChord)
            {
                for (int z = 0; z < 3; z++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        var glb = VectorConverter.CreateVector(0, y, z);
                        var lub = PitchConverter.GlbToLub(glb);
                        var children = new List<Child>();
                        var tones = new List<Group>();
                        var groups = Groups.Where(g => g.ChordIndex == iChord && (g.Type == GroupType.Group /*|| g.Type == GroupType.VerticalGroup*/) && g.ToneCount() > 1);
                        foreach (var g in groups)
                        {
                            var offset = VectorConverter.Normalize(g.Position, glb);
                            var f = g.Position + offset;
                            bool inside = g.InsideFrame_new(glb, lub, offset);

                            //var within = true;
                            //foreach (var c in g.Children)
                            //{
                            //    var p = c.Group.Position + offset;
                            //    var flag = true;
                            //    for (int i = 1; i < 3; i++)
                            //    {
                            //        if (p[i] < glb[i] || p[i] > lub[i])
                            //        {
                            //            within = false;
                            //            flag = false;
                            //            break;
                            //        }
                            //    }
                            //    //
                            //    if (flag)
                            //    {
                            //        Debug.Write("within:  ");
                            //    }
                            //    else
                            //    {
                            //        Debug.Write("without: ");
                            //    }
                            //    VectorConverter.DebugWriteVector(glb);
                            //    Debug.Write(" : ");
                            //    VectorConverter.DebugWriteVector(p);
                            //    Debug.Write(" : ");
                            //    VectorConverter.DebugWriteVector(c.Group.Position);
                            //    Debug.Write(" : ");
                            //    if (c.Group.Type == GroupType.Tone)
                            //    {
                            //        Debug.Write($"{c.Group.Tone.PitchHeight} {c.Group.Tone.FifthHeight}");
                            //    }
                            //    Debug.WriteLine(null);
                            //    //
                            //}
                            if (inside)
                            {
                                var child = new Child(g, f);
                                children.Add(child);
                                g.ListTones(ref tones);
                            }
                        }
                        // does frame cover all the tones?
                        var count = chords[iChord].Pitches.Count;
                        if (tones.Count == count)
                        {
                            int x = children.Min(c => c.Position[0]);
                            var pos = VectorConverter.CreateVector(x, glb[1], glb[2]);
                            var frame = new Group
                            {
                                ChordIndex = iChord,
                                Type = GroupType.Frame,
                                Position = pos,
                                Children = children,
                                Tone = null,
                            };
                            Groups.Add(frame);
                            frames.Add(frame);
                        }
                        else
                        {

                        }
                    }
                }
            }
            return frames;
        }

        // traverse the containment hierarchy and extract chord 0 and 1 group cardinalities
        /// <summary>
        /// generate HorizontalFrameExtends and HorizontalGroupMaps
        /// </summary>
        /// <param name="chords">chord progression</param>
        //public void HorizFramesToHorizGroupMaps(PitchPattern[] chords)
        //{
        //    VerifyMethodOrder(8);
        //    for (var iChord = 0; iChord < chords.Length - 1; ++iChord)
        //    {
        //        var iChordNext = iChord + 1;
        //        var frames = new List<Group>();
        //        var maps = new List<HorizontalGroupMap_old>();
        //        //var maps = new List<HorizontalGroupMap>();
        //        const int maxCardinality = 5;
        //        foreach (var horizFrame in Groups.Where(g => g.Type == GroupType.HorizontalFrame && g.ChordIndex == iChord))
        //        {
        //            var horizFramesByCardinality = Enumerable.Range(0, maxCardinality + 1).Select(i => new Group { Type = GroupType.Null, }).ToList();
        //            //horizFrame.DebugWrite(0);

        //            // debug output
        //            //PrintHorizFrame(iChord, iChordNext, horizFrame);

        //            Vector<int> lower, upper, lowerNext, upperNext;
        //            //HorizontalFrameToBounds(horizFrame, out lower, out upper, out lowerNext, out upperNext);
        //            horizFrame.HorizontalFrameToBounds(out lower, out upper, out lowerNext, out upperNext);

        //            // filter groups and order by distance.
        //            // order by CompoundWeighted correlates exactly with the subset hierarchy order
        //            //List<Group> horizontalGroups = HorizontalFrameToHorizontalGroups(iChord, lower, upper, lowerNext, upperNext);
        //            List<Group> horizontalGroups = horizFrame.HorizontalFrameToHorizontalGroups(Groups);

        //            //foreach (var g in horizontalGroups.Take(5))
        //            //{
        //            //    g.DebugWrite(1);
        //            //}
        //            //Debug.WriteLine("--------");

        //            HorizontalGroupMap_old horizontalGroupMap = InitHorizontalGroup_old(horizFrame.HorizFrameId, iChord, iChordNext, new Vector<int>[] { lower, upper }, new Vector<int>[] { lowerNext, upperNext }, horizontalGroups);
        //            //HorizontalGroupMap horizontalGroupMap = InitHorizontalGroup(horizFrame, horizontalGroups);

        //            // debug output
        //            // horizontalGroupMap.HorizontalGroups Children 0 and 1 contain exactly 4 tone positions each
        //            VerifyHorizontalGroupMap(horizontalGroupMap);

        //            int cardinalityIndex = maxCardinality;
        //            //horizontalGroupMap.Reset();
        //            horizontalGroupMap.Next();
        //            if (horizontalGroupMap.IsNotNull())
        //            {
        //                (List<Group> groups0, List<Group> groups1) = horizontalGroupMap.GetDistinctHorizontalGroups();
        //                int cardinality = Math.Min(Math.Min(groups0.Count, groups1.Count), maxCardinality);
        //                if (horizFramesByCardinality[cardinality].Type != GroupType.Null)
        //                {
        //                    continue;
        //                }


        //                var horizGroups = new List<Group>();
        //                foreach (var chord in horizontalGroupMap.ToneToGroups)
        //                {
        //                    foreach (var tone in chord)
        //                    {
        //                        if (!horizGroups.Contains(tone.HorizontalGroup))
        //                        {
        //                            horizGroups.Add(tone.HorizontalGroup);
        //                        }
        //                    }
        //                }
        //                // this object model makes it easy to extract the nodes and edges
        //                //--------------------------------------------------------
        //                // The object model is:
        //                //  HorizontalFrameExtend
        //                //      Extends
        //                //          HorizontalFrame  
        //                //              Child
        //                //                  Frame
        //                //                      Child
        //                //                          Tone or Group
        //                //              Child
        //                //                  Frame
        //                //                      Child2
        //                //                          Tone or Group
        //                //      Child
        //                //          HorizontalGroup
        //                //      Child
        //                //          HorizontalGroup

        //                //var descending = horizGroups.OrderByDescending(g => g.Distance.Pairwise).ToList();
        //                double pairwise = horizGroups.Max(g => g.Distance.Pairwise);

        //                double average = horizGroups.Average(g => g.Distance.Weighted);

        //                var horizGroupsOrderByDescending = horizGroups.OrderByDescending(g => g.Distance.Weighted).ToArray();
        //                var count = Math.Min(DistanceAlgorithm._weights.Length, horizGroups.Count);
        //                var denominator = DistanceAlgorithm._weights[cardinality].Sum();
        //                var sum = Enumerable.Range(0, cardinality)
        //                .Select(i => DistanceAlgorithm._weights[cardinality][i] * horizGroupsOrderByDescending[i].Distance.Weighted).Sum();
        //                double weighted = (2 * sum) / (double)denominator;

        //                double compoundPairwise = horizGroups.Max(g => g.Distance.CompoundPairwise);

        //                double compoundAverage = horizGroups.Average(g => g.Distance.CompoundAverage);

        //                horizGroupsOrderByDescending = horizGroups.OrderByDescending(g => g.Distance.CompoundWeighted).ToArray();
        //                sum = Enumerable.Range(0, cardinality)
        //                .Select(i => DistanceAlgorithm._weights[cardinality][i] * horizGroupsOrderByDescending[i].Distance.CompoundWeighted).Sum();
        //                // no need to multiply by 2 again
        //                double compoundWeighted = sum /*(2 * sum)*/ / (double)denominator;

        //                var distanceStruct = new DistanceStruct
        //                {
        //                    Pairwise = pairwise,
        //                    Average = average,
        //                    Weighted = weighted,
        //                    CompoundPairwise = compoundPairwise,
        //                    CompoundAverage = compoundAverage,
        //                    CompoundWeighted = compoundWeighted,
        //                };
        //                Debug.WriteLine($"{pairwise} {average} {weighted} {compoundPairwise} {compoundAverage} {compoundWeighted}");
        //                //Debug.Assert(horizFrame.Distance.Override.Count == 0);

        //                var frame = horizFrame.Extend(false);
        //                frame.Type = GroupType.HorizontalFrameExtend;
        //                frame.Extends = horizFrame;
        //                //
        //                if (frame.Distance.Override != DistanceStruct.Null)
        //                {
        //                    Debug.WriteLine("god dammit");
        //                }
        //                //
        //                frame.Distance.Override = distanceStruct;
        //                foreach (var horizGroup in horizGroups)
        //                {
        //                    frame.Children.Add(new Child(horizGroup, horizGroup.Position));
        //                }

        //                Debug.Assert(horizFrame.Position == horizFrame.Children[0].Position);
        //                Debug.Assert(VectorConverter.IsNormalized(horizFrame.Children[0].Position));
        //                //Debug.Assert(VectorConverter.IsNormalized(horizFrame.Children[1].Position));
        //                Debug.Assert(VectorConverter.IsNormalized(horizFrame.Children[1].Group.Position));
        //                var offsetNext = frame.Children[1].Position - frame.Children[1].Group.Position;
        //                //var offsetNext = frame.Extends.Children[1].Position - frame.Extends.Children[1].Group.Position;
        //                Debug.Assert(Array.IndexOf(VectorConverter.Aliases, offsetNext) != -1);
        //                foreach (var toneOrGroup in groups0)
        //                {
        //                    // Position?
        //                    frame.Children[0].Group.Children.Add(new Child(toneOrGroup, toneOrGroup.Position));
        //                }
        //                foreach (var toneOrGroup in groups1)
        //                {
        //                    // Position + offsetNext?
        //                    frame.Children[1].Group.Children.Add(new Child(toneOrGroup, toneOrGroup.Position + offsetNext));
        //                }

        //                frames.Add(frame);
        //                maps.Add(horizontalGroupMap);
        //                horizFramesByCardinality[cardinality] = frame;
        //                cardinalityIndex--;
        //            }
        //            // ToDo move
        //        }
        //        Groups.AddRange(frames);
        //        HorizontalGroupMaps.AddRange(maps);
        //        Debug.Assert(frames.All(f => maps.Count(m => f.HorizFrameId == m.HorizFrameId) == 1));
        //        Debug.Assert(maps.All(f => frames.Count(m => f.HorizFrameId == m.HorizFrameId) == 1));
        //        Debug.Assert(frames.All(f => f.Distance.Override != null));

        //        // debug print
        //        foreach (var item in maps)
        //        {
        //            item.Reset();
        //            var cardinalities = new int[][] { Enumerable.Range(0, maxCardinality + 1).Select(i => -1).ToArray(), Enumerable.Range(0, maxCardinality + 1).Select(i => -1).ToArray() };
        //            while (item.Next())
        //            {
        //                if (item.index == 0)
        //                {
        //                    var horizFrameExtend = Groups.Single(g => g.Type == GroupType.HorizontalFrameExtend && g.HorizFrameId == item.HorizFrameId);
        //                    //var horizFrameExtend = Groups.Single(g => g.Type == GroupType.HorizontalFrameExtend && g.Extends == item.Frame);
        //                    PrintHorizFrame(iChord, iChordNext, horizFrameExtend);
        //                }
        //                if (item.IsNotNull())
        //                {
        //                    Debug.WriteLine(null);
        //                    item.DebugValid();
        //                    item.DebugWrite();
        //                    bool changed = false;
        //                    (List<Group> groups0, List<Group> groups1) = item.GetDistinctHorizontalGroups();
        //                    var count0 = groups0.Count;
        //                    var count1 = groups1.Count;
        //                    if (count0 < cardinalities[0].Length && cardinalities[0][count0] == -1)
        //                    {
        //                        cardinalities[0][count0] = item.index;
        //                        changed = true;
        //                    }
        //                    if (count1 < cardinalities[1].Length && cardinalities[1][count1] == -1)
        //                    {
        //                        cardinalities[1][count1] = item.index;
        //                        changed = true;
        //                    }
        //                    if (changed) Debug.WriteLine($"-- {item.index,2} {count0,2} {count1,2} --");
        //                }
        //                else
        //                {
        //                    Debug.WriteLine($"{item.glb[0][1],2} {item.glb[1][1],2}");
        //                    //Debug.WriteLine($"{item.Frame.Bounds[0][1],2} {item.Frame.Bounds[1][1],2}");
        //                }
        //            }
        //            Debug.WriteLine("--------------------------------");
        //        }
        //    }

        //}

        public void Experiment1(PitchPattern[] chords)
        {
            for (int iChord = 0; iChord < chords.Length - 1; ++iChord)
            {
                Debug.WriteLine($"chord {iChord} ----");
                var horizFrames = Groups.Where(g => g.Type == GroupType.HorizontalFrame && g.ChordIndex == iChord).ToList();
                foreach (var horizFrame in horizFrames)
                {
                    Debug.WriteLine($"frame {horizFrames.IndexOf(horizFrame)}----");
                    var horizGroups = horizFrame.HorizontalFrameToHorizontalGroups(Groups);
                    foreach (var horizGroup in horizGroups)
                    {
                        if (horizGroups.Any(g => horizGroup.IsSupersetHorizontalGroup(g)))
                        {
                            continue;
                        }
                        // bug to be fixed
                        if (horizGroup.Children[0].Group.Type == GroupType.Group && horizGroup.Children[0].Group.Children.Count == 1)
                        {
                            continue;
                        }
                        //
                        List<Group> list = new List<Group> { horizGroup };
                        Debug.WriteLine("----");
                        horizGroup.DebugWrite(1, Vector<int>.Zero);
                        Recurse(0, horizGroups, horizGroup, false, true, ref list);
                    }
                }
            }
        }
        public void Recurse(int depth, List<Group> horizGroups, Group horizGroup, bool left, bool right, ref List<Group> list)
        {
            foreach (var group in horizGroups.Where(g => g != horizGroup))
            {
                if (list.IndexOf(group) != -1)
                {
                    continue;
                }
                if (left && !right)
                {
                    if (group.IsSuperset(horizGroup, true, false))
                    {
                        group.DebugWrite(depth, Vector<int>.Zero);
                        list.Add(group);
                        Recurse(depth + 1, horizGroups, group, false, true, ref list);
                    }
                }
                else if (!left && right)
                {
                    if (group.IsSuperset(horizGroup, false, true))
                    {
                        group.DebugWrite(depth, Vector<int>.Zero);
                        list.Add(group);
                        Recurse(depth + 1, horizGroups, group, left, right, ref list);
                    }
                }
            }
        }

        /// <summary>
        /// THIS WAS AN EARILER UNSUCCESSFUL ATTEMPT
        /// parsimonious set of Nodes and Edges
        /// </summary>
        /// <param name="chords">chord progression</param>
        public void HorizFramesToNodeEdges(PitchPattern[] chords)
        {
            VerifyMethodOrder(11);
            // allow left and right HorizontalFrames to elect Groups to be Nodes
            var vertGroups = new List<Group>();
            // AB BC CD        (HorizontalFrames
            // traverse the left and right containment hierarchy
            for (var iChord = 0; iChord < chords.Length; ++iChord)
            {
                var iChordNext = iChord + 1;
                // first chord
                if (iChord == 0)
                {
                    // from first-chord frame AB get B
                    vertGroups = Groups.Where(g => g.Type == GroupType.HorizontalFrame && g.ChordIndex == iChord)
                        .Select(g => g.Children[0].Group).Distinct().ToList();
                }
                // middle chord
                else if (iChord < chords.Length - 1)
                {
                    // get distinct Frames in the intersection of AB and BC
                    var distinctVertFrames = new List<Group>();
                    // clearer without nested LINQ statement
                    foreach (var frame in Groups.Where(g => g.Type == GroupType.Frame && g.ChordIndex == iChord))
                    {
                        if (Groups.Count(h => h.Type == GroupType.HorizontalFrame && h.ChordIndex == iChord && h.Children[1].Group == frame) != 0 &&
                            Groups.Count(h => h.Type == GroupType.HorizontalFrame && h.ChordIndex == iChordNext && h.Children[0].Group == frame) != 0)
                        {
                            distinctVertFrames.Add(frame);
                        }
                    }
                    distinctVertFrames = distinctVertFrames.Distinct().ToList();

                    foreach (var vertFrame in distinctVertFrames)
                    {
                        // for each pair of intersecting Frames AB and BC
                        var horizFrames = Groups.Where(g => g.Type == GroupType.HorizontalFrame && g.ChordIndex == iChord && g.Children[1].Group == vertFrame)
                            .Select(g => g).ToList();
                        var horizFramesNext = Groups.Where(g => g.Type == GroupType.HorizontalFrame && g.ChordIndex == iChordNext && g.Children[0].Group == vertFrame)
                            .Select(g => g).ToList();
                        foreach (var horizFrame in horizFrames)
                        {
                            foreach (var horizFrameNext in horizFramesNext)
                            {
                                for (int cardinality = 4; cardinality > 0; cardinality--)
                                {
                                    // agglomerative hierarchical clustering
                                    Vector<int> lower, upper, lowerNext, upperNext;
                                    //HorizontalFrameToBounds(horizFrame, out lower, out upper, out lowerNext, out upperNext);
                                    horizFrame.FrameToBounds(out lower, out upper, out lowerNext, out upperNext);
                                    //List<Group> groups = HorizontalFrameToHorizontalGroups(iChord, lower, upper, lowerNext, upperNext);
                                    List<Group> groups = horizFrame.HorizontalFrameToHorizontalGroups(Groups);
                                    //HorizontalGroupMap horizontalGroupMap = InitHorizontalGroup_old(horizFrame.HorizFrameId, iChord, iChordNext, new Vector<int>[] { lower, upper }, new Vector<int>[] { lowerNext, upperNext }, groups);
                                    GroupMap/*HorizontalGroupMap*/ horizontalGroupMap = InitHorizontalGroupMap(horizFrame, groups);

                                    //HorizontalFrameToBounds(horizFrameNext, out lower, out upper, out lowerNext, out upperNext);
                                    horizFrameNext.FrameToBounds(out lower, out upper, out lowerNext, out upperNext);

                                    //List<Group> groupsNext = HorizontalFrameToHorizontalGroups(iChord, lower, upper, lowerNext, upperNext);
                                    List<Group> groupsNext = horizFrameNext.HorizontalFrameToHorizontalGroups(Groups);
                                    //HorizontalGroupMap horizontalGroupMapNext = InitHorizontalGroup_old(horizFrameNext.HorizFrameId, iChord, iChordNext, new Vector<int>[] { lower, upper }, new Vector<int>[] { lowerNext, upperNext }, groupsNext);
                                    GroupMap/*HorizontalGroupMap*/ horizontalGroupMapNext = InitHorizontalGroupMap(horizFrameNext, groupsNext);

                                    //--------------------------------------------------------
                                    // The object model is:
                                    //  HorizontalFrame
                                    //      Extends
                                    //          HorizontalFrame  
                                    //      Child
                                    //          Frame
                                    //              Child
                                    //                  Tone or Group
                                    //      Child
                                    //          Frame
                                    //              Child2
                                    //                  Tone or Group
                                    //      Child
                                    //          HorizontalGroup
                                    //      Child
                                    //          HorizontalGroup
                                    //--------------------------------------------------------

                                    var edgeSnaphot = new List<Group>();
                                    var edgeSnaphotNext = new List<Group>();
                                    var nodeSnapshot = new List<Group>();
                                    var nodeSnapshotNext = new List<Group>();

                                    (List<Group> groups0, List<Group> groups1)? distinctGroups = null;
                                    (List<Group> groups0, List<Group> groups1)? distinctGroups_ = null;
                                    while (horizontalGroupMap.Next())
                                    {
                                        if (horizontalGroupMap.IsNotNull())
                                        {
                                            distinctGroups = horizontalGroupMap.GetDistinctHorizontalGroups();
                                            if (distinctGroups.Value.groups1.Count == cardinality)
                                            {
                                                edgeSnaphot = new List<Group>(horizontalGroupMap.AvailableGroups);
                                                nodeSnapshot = new List<Group>(distinctGroups.Value.groups1);
                                                break;
                                            }
                                        }
                                    }
                                    while (horizontalGroupMapNext.Next())
                                    {
                                        if (horizontalGroupMapNext.IsNotNull())
                                        {
                                            distinctGroups_ = horizontalGroupMapNext.GetDistinctHorizontalGroups();
                                            if (distinctGroups_.Value.groups1.Count == cardinality)
                                            {
                                                edgeSnaphotNext = new List<Group>(horizontalGroupMapNext.AvailableGroups);
                                                nodeSnapshotNext = new List<Group>(distinctGroups_.Value.groups1);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                // last chord
                else if (iChord == chords.Length - 1)
                {
                    // from last-chord frame CD get C
                    vertGroups = Groups.Where(g => g.Type == GroupType.HorizontalFrame && g.ChordIndex == iChord - 1)
                        .Select(g => g.Children[1].Group).Distinct().ToList();
                }
            }
        }

        /// <summary>
        /// get VerticalGroups within arg HorizontalFrame
        /// </summary>
        private List<Group> HorizontalFrameToVerticalGroups(int iChord, Vector<int> lower, Vector<int> upper)
        {
            return Groups.Where(g => g.ChordIndex == iChord && g.IsVerticalGroupOrGroup(2) &&
                g.GroupWithinBounds(lower, upper, GroupType.VerticalGroup, GroupType.Tone, GroupType.Group)
            ).OrderBy(g => g.Distance.CompoundWeighted)
            .ThenBy(g => g.Children[0].Group.Children.Count)
            .ThenBy(g => g.Children[1].Group.Children.Count).ToList();
        }

        ///// <summary>
        ///// the preferred method for getting HorizontalGroups from HorizontalFrames
        ///// </summary>
        //private List<Group> HorizontalFrameToHorizontalGroups(int iChord, Vector<int> lower, Vector<int> upper, Vector<int> lowerNext, Vector<int> upperNext)
        //{
        //    return Groups.Where(g => g.Type == GroupType.HorizontalGroup && g.ChordIndex == iChord &&
        //        g.Children[0].Group.GroupWithinBounds(lower, upper, GroupType.Tone, GroupType.Group) &&
        //        g.Children[1].Group.GroupWithinBounds(lowerNext, upperNext, GroupType.Tone, GroupType.Group)
        //    ).OrderBy(g => g.Distance.CompoundWeighted)
        //    .ThenBy(g => g.Children[0].Group.Children.Count)
        //    .ThenBy(g => g.Children[1].Group.Children.Count).ToList();
        //}

        ///// <summary>
        ///// get HorizontalGroups with arg HorizontalFrame
        ///// </summary>
        //private List<Group> HorizontalFrameToHorizontalGroups_new(int iChord, Vector<int> lower, Vector<int> upper, Vector<int> lowerNext, Vector<int> upperNext)
        //{
        //    return Groups.Where(g => g.Type == GroupType.HorizontalGroup && g.ChordIndex == iChord &&
        //        g.Children[0].ChildWithinBounds(lower, upper, GroupType.Tone, GroupType.Group) &&
        //        g.Children[1].ChildWithinBounds(lowerNext, upperNext, GroupType.Tone, GroupType.Group)
        //    ).OrderBy(g => g.Distance.CompoundWeighted)
        //    .ThenBy(g => g.Children[0].Group.Children.Count)
        //    .ThenBy(g => g.Children[1].Group.Children.Count).ToList();
        //}

        ///// <summary>
        ///// get the bounds of the HorizontalFrame's child Frames
        ///// </summary>
        //public static void HorizontalFrameToBounds(Group? horizFrame, out Vector<int> lower, out Vector<int> upper, out Vector<int> lowerNext, out Vector<int> upperNext)
        //{
        //    if (horizFrame.Type != GroupType.HorizontalFrame && horizFrame.Type != GroupType.HorizontalFrameExtend)
        //    {
        //        throw new ArgumentException();
        //    }
        //    Debug.Assert(horizFrame.Position == horizFrame.Children[0].Position);
        //    Debug.Assert(VectorConverter.IsNormalized(horizFrame.Position));
        //    //var offset = horizFrame.Position - horizFrame.Children[0].Group.Position;
        //    lower = horizFrame.Children[0].Group.Bounds[0];
        //    upper = horizFrame.Children[0].Group.Bounds[1];
        //    var offsetNext = horizFrame.Children[1].Position - horizFrame.Children[1].Group.Position;
        //    lowerNext = horizFrame.Children[1].Group.Bounds[0] + offsetNext;
        //    upperNext = horizFrame.Children[1].Group.Bounds[1] + offsetNext;
        //}

        //public void FramesToHorizPaths_old(PitchPattern[] chords)
        //{
        //    VerifyMethodOrder(7);
        //    for (var iChord = 0; iChord < chords.Length - 1; ++iChord)
        //    {
        //        var iChordNext = iChord + 1;

        //        var list = new List<HorizontalGroupMap_old>();
        //        // for each y bounds pair
        //        for (int yMin = 0; yMin < 4; ++yMin)
        //        {
        //            for (int yMinNext = yMin - 2; yMinNext <= yMin + 2; ++yMinNext)
        //            {
        //                var lower = VectorConverter.CreateVector(int.MinValue, yMin, zMin);
        //                var upper = VectorConverter.CreateVector(int.MinValue, yMin + 3, zMax);
        //                var lowerNext = VectorConverter.CreateVector(int.MinValue, yMinNext, zMin);
        //                var upperNext = VectorConverter.CreateVector(int.MinValue, yMinNext + 3, zMax);
        //                // filter groups and order by distance.
        //                // order by CompoundWeighted correlates exactly with the subset hierarchy order
        //                var groups = Groups.Where(g => g.Type == GroupType.HorizontalGroup && g.ChordIndex == iChord &&
        //                    //g.Children[0].Group.GroupWithinBounds(yMin, zMin, yMin + 3, zMax, GroupType.Tone, GroupType.Group) &&
        //                    //g.Children[1].Group.GroupWithinBounds(yMinNext, zMin, yMinNext + 3, zMax, GroupType.Tone, GroupType.Group)
        //                    g.Children[0].Group.GroupWithinBounds(lower, upper, GroupType.Tone, GroupType.Group) &&
        //                    g.Children[1].Group.GroupWithinBounds(lowerNext, upperNext, GroupType.Tone, GroupType.Group)
        //                ).OrderBy(g => g.Distance.CompoundWeighted)
        //                .ThenBy(g => g.Children[0].Group.Children.Count)
        //                .ThenBy(g => g.Children[1].Group.Children.Count).ToList();
        //                //Debug.WriteLine($"{yMin,2} {yMinNext,2}");
        //                // debug output
        //                //foreach (var g in groups)
        //                //{
        //                //    g.DebugWriteBounds();
        //                //}

        //                HorizontalGroupMap_old horizontalGroupMap = InitHorizontalGroup_old(string.Empty, iChord, iChordNext, yMin, yMinNext, groups);
        //                horizontalGroupMap.Initialize();

        //                // debug output
        //                // horizontalGroupMap.HorizontalGroups Children 0 and 1 contain exactly 4 tone positions each
        //                VerifyHorizontalGroupMap(horizontalGroupMap);

        //                list.Add(horizontalGroupMap);
        //            }
        //        }
        //        foreach (var item in list)
        //        {
        //            if (item.IsNotNull())
        //            {
        //                // debug output
        //                Debug.WriteLine($"{item.glb[0][1],2} {item.glb[1][1],2} {item.MaxDistance(), 3} covered");
        //                item.DebugWrite();
        //                while (item.Next())
        //                {
        //                    Debug.WriteLine(null);
        //                    //Debug.WriteLine($"{item.glb[0][1],2} {item.glb[1][1],2} {item.MaxDistance(),3} covered");
        //                    //
        //                    item.DebugValid();
        //                    //
        //                    item.DebugWrite();
        //                }
        //                Debug.WriteLine("--------------------------------");
        //            }
        //            else
        //            {
        //                Debug.WriteLine($"{item.glb[0][1],2} {item.glb[1][1],2}");
        //            }
        //        }
        //    }
        //}

        public void FramesToHorizPaths_new(PitchPattern[] chords)
        {
            for (var iChord = 0; iChord < chords.Length - 1; ++iChord)
            {
                ////var iChordNext = iChord + 1;
                //var distinctPositions = Groups.Where(g => g.Type == GroupType.HorizontalGroup && g.ChordIndex == iChord)
                //    .Select(g => VectorConverter.CreateVector(0, g.Position[1], g.Position[2])).OrderBy(v => v[1]).ThenBy(v => v[2]).Distinct().ToList();
                //// positions of the horizontal groups. they are normalized
                //foreach (var distinctPosition in distinctPositions)
                //{
                //    // there are three ways to filter the groups:
                //    //   by parent position
                //    //   by child position (method used)
                //    //   by tone position
                //    var list = new List<Group>();
                //    foreach (var group in Groups.Where(g => g.Type == GroupType.HorizontalGroup && g.ChordIndex == iChord))
                //    {
                //        // group.Position is not necessarily the same as distinctPosition. so normalize it
                //        var offset = VectorConverter.Normalize(group.Position, distinctPosition);
                //        var ok = group.Children.All(child =>
                //        {
                //            var delta0 = child.Position + offset - distinctPosition;
                //            return 0 <= delta0[1] && 0 <= delta0[2] && delta0[1] < 3 && delta0[2] < 3;
                //        });
                //        if (ok)
                //        {
                //            // remember to normalize group.Position with respect to distinctPosition before using
                //            list.Add(group);
                //        }
                //    }
                //    Debug.Write($"{list.Count,3} ");
                //    VectorConverter.DebugWriteVector(distinctPosition);
                //    Debug.WriteLine(null);
                //}

                // for all horizontal groups children 0 and 1 list positions
                var triplets = new List<VectorTriplet>();
                foreach (var group in Groups.Where(g => g.Type == GroupType.HorizontalGroup && g.ChordIndex == iChord))
                {
                    var vt = new VectorTriplet(VectorConverter.CreateVector(0, group.Position[1], group.Position[2]),
                        VectorConverter.CreateVector(0, group.Children[0].Position[1], group.Children[0].Position[2]),
                        VectorConverter.CreateVector(0, group.Children[1].Position[1], group.Children[1].Position[2]));
                    if (!triplets.Contains(vt))
                    {
                        triplets.Add(vt);
                    }
                }
                triplets.Sort();
                foreach (var triplet in triplets)
                {
                    var glb = new Vector<int>[] { triplet.Child0, triplet.Child1 };
                    var lub = new Vector<int>[] { VectorConverter.CreateVector(0, triplet.Child0[1] + 3, triplet.Child0[2] + 1),
                    VectorConverter.CreateVector(0, triplet.Child1[1] + 3, triplet.Child1[2] + 1),};

                    var list = new List<Group>();
                    //foreach (var group in Groups.Where(g => g.Type == GroupType.HorizontalGroup && g.ChordIndex == iChord &&
                    //        g.Children[0].Group.GroupWithinBounds_new(glb[0][1], glb[0][2], lub[0][1], lub[0][2], GroupType.Tone, GroupType.Group) &&
                    //        g.Children[1].Group.GroupWithinBounds_new(glb[1][1], glb[1][2], lub[1][1], lub[1][2], GroupType.Tone, GroupType.Group)))
                    var inside = true;
                    foreach (var group in Groups.Where(g => g.Type == GroupType.HorizontalGroup && g.ChordIndex == iChord))
                    {
                        var offset = VectorConverter.Normalize(group.Position, triplet.Position);
                        var child0Tones = new List<Group>();
                        group.Children[0].Group.GetChildPositions(Vector<int>.Zero, ref child0Tones, GroupType.Tone);
                        foreach (var p in child0Tones.Select(g => g.Position + offset))
                        {
                            if (p[1] < glb[0][1] || p[1] > lub[0][1] || p[2] < glb[0][2] || p[2] > lub[0][2])
                            {
                                inside = false;
                                break;
                            }
                        }
                        var child1Tones = new List<Group>();
                        group.Children[0].Group.GetChildPositions(Vector<int>.Zero, ref child1Tones, GroupType.Tone);
                        foreach (var p in child1Tones.Select(g => g.Position + offset))
                        {
                            if (p[1] < glb[0][1] || p[1] > lub[0][1] || p[2] < glb[0][2] || p[2] > lub[0][2])
                            {
                                inside = false;
                                break;
                            }
                        }
                        if (inside)
                        {
                            list.Add(group);
                        }
                    }
                    Debug.WriteLine($"{list.Count,3} : {triplet.Position[1],2} {triplet.Position[2]} : {triplet.Child0[1]} {triplet.Child0[2]} : {triplet.Child1[1]} {triplet.Child1[2]} --------");
                    foreach (var group in list)
                    {
                        Debug.WriteLine($"{list.Count,3} : {group.Position[1],2} {group.Position[2]} : {group.Children[0].Position[1]} {group.Children[0].Position[2]} : {group.Children[1].Position[1]} {group.Children[1].Position[2]}");
                    }
                }
            }
        }

        //private VerticalGroupMap_old InitVerticalGroupMap(string horizFrameId, int iChord, Vector<int> glb, Vector<int> lub, List<Group> horizontalGroups)
        //{
        //    return new VerticalGroupMap_old(iChord, horizFrameId, glb, lub,
        //        Groups.Where(g => g.Type == GroupType.Tone && g.ChordIndex == iChord)
        //        .Select(g => new ToneMap { Tone = g, }).ToList(),
        //        horizontalGroups, Groups);
        //}

        private GroupMap/*HorizontalGroupMap*/ InitHorizontalGroupMap(Group frame, List<Group> horizontalGroups)
        {
            bool initialize = true;
            var horizGroupMap = new GroupMap/*HorizontalGroupMap*/(GroupMapType.HorizontalGroupMap, frame,
                //Groups.Where(g => g.Type == GroupType.Tone && g.ChordIndex == frame.ChordIndex)
                //.Select(g => new ToneMap { Tone = g, }).ToList(),
                //Groups.Where(g => g.Type == GroupType.Tone && g.ChordIndex == frame.ChordIndex + 1)
                /*.Select(g => new ToneMap { Tone = g, }).ToList(),*/ horizontalGroups, initialize);
            return horizGroupMap;
        }
        private GroupMap/*HorizontalGroupMap*/ InitHorizontalGroupMap(Group frame, List<Group> horizontalGroups, bool initialize)
        {
            var horizGroupMap = new GroupMap/*HorizontalGroupMap*/(GroupMapType.HorizontalGroupMap, frame,
                //Groups.Where(g => g.Type == GroupType.Tone && g.ChordIndex == frame.ChordIndex)
                //.Select(g => new ToneMap { Tone = g, }).ToList(),
                //Groups.Where(g => g.Type == GroupType.Tone && g.ChordIndex == frame.ChordIndex + 1)
                /*.Select(g => new ToneMap { Tone = g, }).ToList(),*/ horizontalGroups, initialize);
            return horizGroupMap;
        }
        //private VerticalGroupMap InitVerticalGroupMap(Group frame, List<Group> horizontalGroups)
        //{
        //    bool initialize = true;
        //    var vertGroupMap = new VerticalGroupMap(GroupMapType.VerticalGroupMap, frame,
        //        Groups.Where(g => g.Type == GroupType.Tone && g.ChordIndex == frame.ChordIndex)
        //        .Select(g => new ToneMap { Tone = g, }).ToList(),
        //        Groups.Where(g => g.Type == GroupType.Tone && g.ChordIndex == frame.ChordIndex + 1)
        //        .Select(g => new ToneMap { Tone = g, }).ToList(), horizontalGroups, initialize);
        //    return vertGroupMap;
        //}
        //private VerticalGroupMap InitVerticalGroupMap(Group frame, List<Group> horizontalGroups, bool initialize)
        //{
        //    var vertGroupMap = new VerticalGroupMap(GroupMapType.VerticalGroupMap, frame,
        //        Groups.Where(g => g.Type == GroupType.Tone && g.ChordIndex == frame.ChordIndex)
        //        .Select(g => new ToneMap { Tone = g, }).ToList(),
        //        Groups.Where(g => g.Type == GroupType.Tone && g.ChordIndex == frame.ChordIndex + 1)
        //        .Select(g => new ToneMap { Tone = g, }).ToList(), horizontalGroups, initialize);
        //    return vertGroupMap;
        //}

        //public int TonesCovered(List<Child> children0, List<Child> children1, int count)
        //{
        //    var tones = new List<Group>();
        //    foreach (var child in children0)
        //    {
        //        child.Group.ListTones(ref tones);
        //        if (tones.Count == count)
        //        {
        //            break;
        //        }
        //    }
        //    foreach (var child in children1)
        //    {
        //        child.Group.ListTones(ref tones);
        //        if (tones.Count == count)
        //        {
        //            break;
        //        }
        //    }
        //    return tones.Count;
        //}
        //public int TonesCovered(List<Group> groups, int count)
        //{
        //    var tones = new List<Group>();
        //    foreach (var group in groups)
        //    {
        //        group.ListTones(ref tones);
        //        if (tones.Count == count)
        //        {
        //            break;
        //        }
        //    }
        //    return tones.Count;
        //}
        //public bool DoveTail(Group group1, Group group2, out int[] order1, out int[] order2)
        //{
        //    foreach (int[] first in new int[][] { new int[] { 0, 1 }, new int[] { 1, 0 } })
        //    {
        //        foreach (int[] second in new int[][] { new int[] { 0, 1 }, new int[] { 1, 0 } })
        //        {
        //            if (group1.Children[first[1]] == group1.Children[second[0]])
        //            {
        //                order1 = first;
        //                order2 = second;
        //                return true;
        //            }
        //        }
        //    }
        //    order1 = new int[0];
        //    order2 = new int[0];
        //    return false;
        //}

        //public List<Group> SharedGroup(Group a, Group b)
        //{
        //    var shared = new List<Group>();
        //    foreach (var childA in a.Children)
        //    {
        //        foreach (var childB in b.Children)
        //        {
        //            if (childA.Group == childB.Group)
        //            {
        //                shared.Add(childA.Group);
        //            }
        //        }
        //    }
        //    return shared;
        //}
        //public List<Group> GroupsWithinFrame(Group frame)
        //{
        //    var iChord = frame.ChordIndex;
        //    var glb = Processor.CreateVector(int.MinValue, frame.Position[1], frame.Position[2]);
        //    var lub = Processor.CreateVector(int.MaxValue, frame.Position[1] + 4, frame.Position[2] + 3);
        //    var list = new List<Group>();
        //    foreach (var g in Groups.Where(g => g.ChordIndex == iChord && (g.Type == GroupType.Group || g.Type == GroupType.VerticalGroup)))
        //    {
        //        var offset = PitchConverter.Normalize(fund);
        //        if (g.ChordIndex == iChord && g.Position[0] >= glb[0] && g.Position[0] <= lub[0] && g.Position[1] >= glb[1] && g.Position[1] <= lub[1] && g.Position[2] >= glb[2] && g.Position[2] <= lub[2])
        //        {
        //            list.Add(g);
        //        }
        //    }
        //}
        /// <summary>
        /// return distinct Positions of Tones and Groups
        /// </summary>
        /// <param name="iChord">chord index</param>
        /// <summary>
        /// return the number of frames that are below threshold or are within 2.5 times the baseline
        /// </summary>
        /// <param name="frames">list of frames ordered by distance</param>
        private int ThresholdFrames(List<Group> frames)
        {
            // value TBD
            const int threshold = int.MinValue;
            if (frames.Count == 0 || frames.Any(f => f.Type != GroupType.Frame))
            {
                return -1;
            }
            int count = -1;
            var baseline = frames[0].Distance.MaxPairwiseFrame();
            while (count + 1 < frames.Count && (
                (double)frames[count + 1].Distance.MaxPairwiseFrame() <= 2.5 * (double)baseline ||
                frames[count + 1].Distance.MaxPairwiseFrame() < threshold))
            {
                count++;
            }
            return count;
        }
        #endregion
    }
}
