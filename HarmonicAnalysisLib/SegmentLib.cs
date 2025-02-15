using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using HarmonicAnalysisCommonLib;

namespace HarmonicAnalysisLib
{
    /// <summary>
    /// SegmentLib handles
    /// </summary>
    public class SegmentLib
    {
        const bool ShouldDebugPrintSegments = false;
        const bool DebugOutput = false;
        // shortest path logic was worked out in TryFramePairs.sln

        // A path is a series of Segments.
        // Segments in a series are always dovetailed, so that the last FrameStruct of Segment N
        // is identical to the first FrameStruct of Segment N + 1.
        // The path from chord M to M + 1 consists of a series of 1 or 2 Segments, depending on which series has the smaller distance.
        // Therefore, Neighbor() may return a series of 2 Segments where the first and last FrameStruct of Segment 1 have the same chord index.
        // You can filter out such paths, if desired.
        // Segments in a series are always dovetailed.

        /// <summary>
        /// Find the shortest path, and process the list of edges and weights
        /// </summary>
        /// <param name="chords">FrameStruct navigation struct</param>
        /// <param name="maxWidth">max recursion width, AKA max paths</param>
        /// <returns>The analysis as a MusicXML scoreparwise object</returns>
        public static List<SegmentLite>? CallShortestPath_new(List<List<List<List<List<FrameStruct>>>>> chords, int maxWidth)
        {
            List<Segment> bestEdgePath;
            List<double> bestWeight;
            var success = ShortestPath_new(chords, maxWidth, out bestEdgePath, out bestWeight);
            if (success)
            {
                List<SegmentLite> segmentLites = MusicXML.Analysis.Parse(bestEdgePath);
                return segmentLites;
            }
            return null;
        }

        // as the length of the progression increases, the number of paths increases exponentially.
        // to prevent this from happening, paths are stored in a sorted list, and only a fixed number of paths are considered at each depth,
        /// <param name="maxWidth">suggest using a large number to begin with</param>
        /// <param name="s">debug only</param>
        public static bool ShortestPath_new(List<List<List<List<List<FrameStruct>>>>> chords, int maxWidth, out List<Segment> newEdgePath, out List<double> newWeight)
        {
            SortedList<double, SegmentsStruct> currentList = new SortedList<double, SegmentsStruct>();
            SortedList<double, SegmentsStruct> oldList;
            int maxDepth = chords.Count;
            for (int depth = 0; depth < maxDepth; depth++)
            {
                if (depth == 0)
                {
                    foreach (var firstFrameStruct in FrameStructLib.FrameStructIterator(chords).Where(f => f.ChordIndex == 0 && f.VerticalOrHorizontal == 0 && f.DeltaIndex == 0 && f.GroupMap != null))
                    {
                        var neighborSegments /* [iNeighbor][iSegment] */ = Neighbor(chords, firstFrameStruct);
                        // all Distances are defined
                        var isDefinedSegments = neighborSegments.Where(path => path.All(seg => seg.Distance.IsDefined)).ToList();
                        foreach (var segments in isDefinedSegments)
                        {
                            foreach (var segment in segments)
                            {
                                Debug.Assert(segment.FrameStructs.First() == firstFrameStruct);
                                // TODO: There is a bug in Neighbor(). Unfortunately, I didn't bother to document the preconditions. So, fixing the bug will have to wait.
                                if (segment.FrameStructs.Last().ChordIndex != 1)
                                {
                                    continue;
                                }
                                Debug.Assert(segment.FrameStructs.First().ChordIndex == 0 && segment.FrameStructs.Last().ChordIndex == 1);
                                var newSegmentsStruct = new SegmentsStruct(segment);
                                Debug.Assert(newSegmentsStruct.List.Count == depth + 1);
                                if (currentList.Values.Any(ss => SegmentsStruct.AreEqual(ss, newSegmentsStruct)))
                                {

                                }
                                else
                                {
                                    currentList.Add(GenKey(currentList, newSegmentsStruct), newSegmentsStruct);
                                }
                            }
                        }
                    }
                }
                if (depth < maxDepth - 2)
                {
                    // this code isn't reached when maxDepth is 1
                    oldList = new SortedList<double, SegmentsStruct>(currentList);
                    currentList.Clear();
                    // limit recursion width
                    var max = Math.Min(maxWidth, oldList.Count);
                    for (var index = 0; index < max; index++)
                    {
                        SegmentsStruct segmentsStruct = oldList.GetValueAtIndex(index);
                        Segment startSegment = segmentsStruct.List.Last();
                        FrameStruct lastFrameStruct = startSegment.FrameStructs.Last();

                        var neighborSegments /* [iNeighbor][iSegment] */ = Neighbor(chords, lastFrameStruct);
                        // all Distances are defined
                        var isDefinedSegments = neighborSegments.Where(path => path.All(seg => seg.Distance.IsDefined)).ToList();
                        foreach (var segments in isDefinedSegments)
                        {
                            foreach (var segment in segments)
                            {
                                // TODO: There is a bug in Neighbor().
                                if (segment.FrameStructs.Last().ChordIndex != depth + 2)
                                {
                                    continue;
                                }
                                Debug.Assert(segment.FrameStructs.First().ChordIndex == depth + 1 && segment.FrameStructs.Last().ChordIndex == depth + 2);
                                var list = segmentsStruct.List.ToList();
                                list.Add(segment);
                                var newSegmentsStruct = new SegmentsStruct(list);
                                Debug.Assert(newSegmentsStruct.List.Count == depth + 2);
                                if (currentList.Values.Any(ss => SegmentsStruct.AreEqual(ss, newSegmentsStruct)))
                                {

                                }
                                else
                                {
                                    currentList.Add(GenKey(currentList, newSegmentsStruct), newSegmentsStruct);
                                }
                            }
                        }
                    }
                    Debug.WriteLine($"depth {depth} oldCount {oldList.Count,3} currentCount {currentList.Count,3} max {max}");
                }
            }

            // output
            /*
            for (int i = 0; i < currentList.Count; i++)
            {
                var segStruct = currentList.GetValueAtIndex(i);
                var distance = segStruct.Sum();
                var distString = string.Join(" ", segStruct.List.Select(s => $"{Math.Round(s.Distance.Weighted, 0)}"));
                var pathString = string.Join(" -> ", currentList.GetValueAtIndex(i).List.Select(seg => seg.ToString()));
                Debug.WriteLine($"{i,-2} {Math.Round(distance, 0),3} {distString,18} {pathString}");
            }
            */
            if (currentList.Count != 0)
            {
                newEdgePath = new List<Segment>(currentList.GetValueAtIndex(0).List);
                newWeight = new List<double>(currentList.GetValueAtIndex(0).List.Select(s => s.Distance.Weighted));
                return true;
            }
            else
            {
                newEdgePath = new List<Segment>();
                newWeight = new List<double>();
                return false;
            }
        }
        protected static double GenKey(SortedList<double, SegmentsStruct> sortedList, SegmentsStruct segmentsStruct)
        {
            var sum = segmentsStruct.Sum();
            while (sortedList.ContainsKey(sum))
            {
                if (sum == sum + 0.000000000001d)
                {
                    throw new InvalidOperationException();
                }
                sum += 0.000000000001d;
            }
            return sum;
        }

        /// <summary>
        /// Find the shortest path, and process the list of edges and weights
        /// </summary>
        /// <param name="chords"></param>
        /// <returns>The analysis as a MusicXML scoreparwise object</returns>
        public static List<SegmentLite>? CallShortestPath(List<List<List<List<List<FrameStruct>>>>> chords)
        {
            List<Segment> bestEdgePath;
            List<double> bestWeight;
            var success = ShortestPath(chords, out bestEdgePath, out bestWeight);
            if (success)
            {
                //PrintSegments(fs, bestEdgePath, fs.ChordIndex, fs.z, fs.y);
                //PrintSegments(bestEdgePath, chords.Count);
                //var scorepartwise_mod = MusicXML.Analysis.Process(bestEdgePath);
                List<SegmentLite> segmentLites = MusicXML.Analysis.Parse(bestEdgePath);
                return segmentLites;
            }
            return null;
        }
        /// <summary>
        /// Try each starting segment and find the one that leads to the shortest path
        /// </summary>
        /// <returns>the lists of edges and weights</returns>
        static bool ShortestPath(List<List<List<List<List<FrameStruct>>>>> chords, out List<Segment> bestEdgePath, out List<double> bestWeight)
        {
            List<Segment> newEdgePath;
            List<double> newWeight;
            bestEdgePath = new List<Segment>();
            bestWeight = new List<double> { double.MaxValue };
            foreach (var firstFrameStruct in FrameStructLib.FrameStructIterator(chords).Where(f => f.ChordIndex == 0 && f.VerticalOrHorizontal == 0 && f.DeltaIndex == 0 && f.GroupMap != null))
            {
                Debug.Assert(firstFrameStruct.GroupMap.Type == GroupMapType.GroupMap);
                var edgePath = new List<Segment>();
                var weight = new List<double>();
                bool success = Find(0, firstFrameStruct, chords, edgePath, weight, out newEdgePath, out newWeight);
                if (success && newWeight.Sum() < bestWeight.Sum())
                {
                    bestWeight = newWeight;
                    bestEdgePath = newEdgePath;
                    DebugPrintSegments(bestEdgePath, 0, "success");
                }
                if (success)
                {
                    for (int i = 0; i < newEdgePath.Count - 1; i++)
                    {
                        Debug.Assert(newEdgePath[i].FrameStructs.Last() == newEdgePath[i + 1].FrameStructs.First());
                    }
                }
                //else
                //{
                //    Debug.WriteLine("no HorizontalFramePath found.");
                //}
            }
            return bestEdgePath.Count != 0;
        }

        // from TryRecursivePath.csproj
        /// <summary>
        /// Find shortest acyclic neighbor segment and append it to the lists of edges and weights
        /// </summary>
        /// <param name="depth">used for debug output only</param>
        /// <param name="frameStruct">last FrameStruct on Path so far</param>
        /// <param name="chords">FrameStruct navigation struct</param>
        /// <param name="oldEdgePath">Edge Path so far</param>
        /// <param name="oldWeight">Edge Weights so far</param>
        /// <param name="newEdgePath">new Edge Path</param>
        /// <param name="newWeight">new Edge Weights</param>
        /// <returns>new Edge Path and Weights</returns>
        static bool Find(int depth, FrameStruct frameStruct, List<List<List<List<List<FrameStruct>>>>> chords, List<Segment> oldEdgePath, List<double> oldWeight, out List<Segment> newEdgePath, out List<double> newWeight)
        {
            // assert only so I understand the code
            Debug.Assert(oldEdgePath.Count == 0 || oldEdgePath.Last().FrameStructs.Last() == frameStruct);
            // Each recurisve call of Find takes you to the next chord, or the same chord.
            // The second case finds segments consisting of one or more vertical jogs.
            (int start, int end) = frameStruct.GroupMap.GetChordIndexes();
            Debug.WriteLineIf(DebugOutput, $"{new string(' ', depth)} depth {depth} count {oldEdgePath.Count} end {end}");
            if (end == chords.Count - 1)
            {
                newWeight = oldWeight;
                newEdgePath = oldEdgePath;
                return true;
            }
            List<double> bestWeight = new List<double> { double.MaxValue };
            List<Segment> bestEdgePath = new List<Segment>();
            var neighborSegments /* [iNeighbor][iSegment] */ = Neighbor(chords, frameStruct);
            foreach (var segment in neighborSegments.Where(n => n.Any(s => s.Distance.IsDefined)))
            {
                // detect series of Segments that form a cycle
                var acyclicSegments = segment.Where(s => !IsCycle(oldEdgePath, s)).ToList();
                if (acyclicSegments.Count == 0)
                {
                    continue;
                }
                var shortestSegment = acyclicSegments.OrderBy(e => e.Distance.Weighted).First();
                if (ShouldDebugPrintSegments)
                {
                    int width = Math.Min(depth, 5);
                    var depthString = $"{new string(' ', width)}depth {depth}{new string(' ', 5 - width)}";
                    Debug.WriteLine("segments");
                    foreach (var seg in acyclicSegments)
                    {
                        var asterisk = acyclicSegments.Count > 1 && seg == shortestSegment ? "*" : string.Empty;
                        Debug.WriteLine($"{depthString} {seg} {seg.Distance.Weighted:0.0} {asterisk}");
                    }
                }
                // ToList() to make a copy
                var weight = oldWeight.ToList().Append(shortestSegment.Distance.Weighted).ToList();
                var edgePath = oldEdgePath.ToList().Append(shortestSegment).ToList();
                FrameStruct lastFrameStruct = shortestSegment.FrameStructs.Last();
                bool success = Find(depth + 1, lastFrameStruct, chords, edgePath, weight, out newEdgePath, out newWeight);

                /*
                var segStruct = new SegmentsStruct(newEdgePath);
                var distance = segStruct.Sum();
                var distString = string.Join(" ", segStruct.List.Select(s => $"{Math.Round(s.Distance.Weighted, 0)}"));
                var pathString_ = string.Join(" -> ", newEdgePath.Select(seg => seg.ToString()));
                Debug.WriteLine($"{0,-2} {Math.Round(distance, 0),3} {distString,18} {pathString_}");
                */

                if (success && newWeight.Sum() < bestWeight.Sum())
                {
                    bestWeight = newWeight;
                    bestEdgePath = newEdgePath;
                    DebugPrintSegments(bestEdgePath, depth, "best");
                }
            }
            if (bestWeight.Sum() != double.MaxValue)
            {
                newWeight = bestWeight;
                newEdgePath = bestEdgePath;
                DebugPrintSegments(newEdgePath, depth, "true");
                return true;
            }
            newWeight = oldWeight;
            newEdgePath = oldEdgePath;
            DebugPrintSegments(newEdgePath, depth, "false");
            return false;
        }
        /// <summary>
        /// convenience method for Neighbor
        /// </summary>
        /// <returns>a list where each item in the list is a path between adjoining chords, consisting of a series of 1 or 2 dovetailed Segments</returns>
        /// <remarks>always use the item in the list with the smallest distance</remarks>
        public static List<List<Segment>> /* [iNeighbor][iSegment] */ Neighbor(List<List<List<List<List<FrameStruct>>>>> chords, FrameStruct f)
        {
            var neighbors = new List<List<Segment>>();
            foreach (var delta in FrameStructLib.Deltas)
            {
                List<Segment> segments = Neighbor(chords, f, f.ChordIndex + 1, delta[2], delta[1]);
                if (segments.Count != 0)
                {
                    // remove duplicates
                    if (!neighbors.Any(segs => SegmentsStruct.AreEqual(segs, segments)))
                    {
                        neighbors.Add(segments);
                    }
                }
            }
            return neighbors;
        }

        /// <summary>
        /// returns all segments or paths from FrameStruct in current chord to FrameStruct in next chord
        /// </summary>
        private static List<Segment> Neighbor(List<List<List<List<List<FrameStruct>>>>> chords,
            FrameStruct f, int chordIndex, int z, int y)
        {
            if (chordIndex < 0 || chordIndex > chords.Count - 1) throw new InvalidOperationException();
            if (Math.Abs(y) + Math.Abs(z) > 1) throw new InvalidOperationException();

            int deltaIndex = DeltaIndex(z, y);

            var list = new List<List<FrameStruct>>();
            if (chordIndex == f.ChordIndex)
            {
                list.Add(Move(chords, f, z, y));
            }
            else if (chordIndex == f.ChordIndex + 1)
            {
                if (y == 0 && z == 0)
                {
                    list.Add(Into(chords, f));
                }
                else
                {
                    list.Add(Diagonal(chords, f, z, y));
                    list.Add(MoveThenInto(chords, f, z, y));
                    list.Add(IntoThenMove(chords, f, z, y));
                }
            }
            // remove empty lists
            list = list.Where(l => l.Count != 0).ToList();
            // check that all FrameStructs are references
            Debug.Assert(list.All(l => l.All(fs => FrameStructLib.FrameStructIterator(chords).Count(frameStruct => frameStruct == fs) == 1)));
            return list.Select(l => new Segment { FrameStructs = l }).ToList();
        }

        /// <summary>
        /// Detect cycles. A cycle occurs when the first FrameStructs of two Segements are the same.
        /// </summary>
        /// <param name="segments">list of segments</param>
        //private static bool IsCycle(List<Segment> segments)
        //{
        //    return segments.Select(ep => ep.FrameStructs[0]).ToArray().Length !=
        //        segments.Select(ep => ep.FrameStructs[0]).Distinct().ToArray().Length;
        //}
        private static bool IsCycle(List<Segment> segments, Segment segment)
        {
            bool isCycle = segments.Count(s => s.FrameStructs[0] == segment.FrameStructs[0]) != 0;
            return isCycle;
        }

        private static List<FrameStruct> Into(List<List<List<List<List<FrameStruct>>>>> chords, FrameStruct f)
        {
            // create temporary FrameStruct to index chords
            var list = new List<FrameStruct>();
            // starting frame
            list.Add(f);
            // transitional frame
            list.Add(new FrameStruct
            {
                ChordIndex = f.ChordIndex,
                VerticalOrHorizontal = f.VerticalOrHorizontal + 1,
                DeltaIndex = f.DeltaIndex,
                z = f.z,
                y = f.y,
            });
            // ending frame
            list.Add(new FrameStruct
            {
                ChordIndex = f.ChordIndex + 1,
                VerticalOrHorizontal = f.VerticalOrHorizontal,
                DeltaIndex = f.DeltaIndex,
                z = f.z,
                y = f.y,
            });
            return TranslateFrameStruct(chords, list);
        }
        private static List<FrameStruct> Move(List<List<List<List<List<FrameStruct>>>>> chords, FrameStruct f, int z, int y)
        {
            // create temporary FrameStruct to index chords
            var position = f.GroupMap.Frame.Position;
            Debug.Assert(f.DeltaIndex == 0, "FrameStruct must be a Frame.");
            Debug.Assert(VectorConverter.IsNormalized(position));

            var list = new List<FrameStruct>();
            int deltaIndex = DeltaIndex(z, y);
            if (y == 0 && z == 0)
            {
                list.Add(f);
                return list;
            }

            // starting frame
            list.Add(f);
            // transitional frame
            var pos = VectorConverter.CreateVector(0,
                Math.Min(position[1], position[1] + y),
                Math.Min(position[2], position[2] + z));
            pos = pos + VectorConverter.Normalize(pos);
            list.Add(new FrameStruct
            {
                ChordIndex = f.ChordIndex,
                VerticalOrHorizontal = f.VerticalOrHorizontal,
                DeltaIndex = f.DeltaIndex + deltaIndex,
                z = position[2],
                y = position[1],
            });
            // ending frame
            pos = VectorConverter.CreateVector(0,
                position[1] + y,
                position[2] + z);
            pos = pos + VectorConverter.Normalize(pos);
            list.Add(new FrameStruct
            {
                ChordIndex = f.ChordIndex,
                VerticalOrHorizontal = f.VerticalOrHorizontal,
                DeltaIndex = f.DeltaIndex,
                z = pos[2],
                y = pos[1],
            });
            return TranslateFrameStruct(chords, list);
        }
        private static List<FrameStruct> Diagonal(List<List<List<List<List<FrameStruct>>>>> chords, FrameStruct f, int z, int y)
        {
            // create temporary FrameStruct to index chords
            var position = f.GroupMap.Frame.Position;
            Debug.Assert(f.DeltaIndex == 0, "FrameStruct must be a Frame.");
            Debug.Assert(VectorConverter.IsNormalized(position));

            var list = new List<FrameStruct>();
            int deltaIndex = DeltaIndex(z, y);
            // starting frame
            list.Add(f);
            // transitional frame
            var pos = VectorConverter.CreateVector(0,
                Math.Min(position[1], position[1] + y),
                Math.Min(position[2], position[2] + z));
            pos = pos + VectorConverter.Normalize(pos);
            list.Add(new FrameStruct
            {
                ChordIndex = f.ChordIndex,
                VerticalOrHorizontal = f.VerticalOrHorizontal + 1,
                DeltaIndex = f.DeltaIndex + deltaIndex,
                z = position[2],
                y = position[1],
            });
            // ending frame
            pos = VectorConverter.CreateVector(0,
                position[1] + y,
                position[2] + z);
            pos = pos + VectorConverter.Normalize(pos);
            list.Add(new FrameStruct
            {
                ChordIndex = f.ChordIndex + 1,
                VerticalOrHorizontal = f.VerticalOrHorizontal,
                DeltaIndex = f.DeltaIndex,
                z = pos[2],
                y = pos[1],
            });
            return TranslateFrameStruct(chords, list);
        }
        private static List<FrameStruct> MoveThenInto(List<List<List<List<List<FrameStruct>>>>> chords, FrameStruct f, int z, int y)
        {
            // create temporary FrameStruct to index chords
            //var move = Move(chords, f, z, y);
            //var frame = move.Last();
            //var into = Into(chords, frame);
            //move.AddRange(into.Skip(1));

            var move = Move(chords, f, z, y);
            if (move.Count != 0)
            {
                var frame = move.Last();
                var into = Into(chords, frame);
                if (into.Count != 0)
                {
                    move.AddRange(into.Skip(1));
                }
                else
                {

                }
            }
            //return move;
            return TranslateFrameStruct(chords, move);
        }
        private static List<FrameStruct> IntoThenMove(List<List<List<List<List<FrameStruct>>>>> chords, FrameStruct f, int z, int y)
        {
            // create temporary FrameStruct to index chords
            //var into = Into(chords, f);
            //var frame = into.Last();
            //var move = Move(chords, frame, z, y);
            //into.AddRange(move.Skip(1));
            var into = Into(chords, f);
            if (into.Count != 0)
            {
                var frame = into.Last();
                var move = Move(chords, frame, z, y);
                if (move.Count != 0)
                {
                    into.AddRange(move.Skip(1));
                }
                else
                {

                }
            }
            //return into;
            return TranslateFrameStruct(chords, into);
        }

        /// <summary>
        /// Converts z and y to an index into the Deltas array.
        /// </summary>
        private static int DeltaIndex(int z, int y)
        {
            var Deltas = FrameStructLib.Deltas;
            return Enumerable.Range(0, Deltas.Length).Single(i => Deltas[i][1] == y && Deltas[i][2] == z);
        }

        /// <summary>
        /// Convert indexes to references to the master FrameStruct list
        /// </summary>
        /// <param name="chords">master FrameStruct list</param>
        /// <param name="list">temporary FrameStruct list</param>
        static List<FrameStruct> TranslateFrameStruct(List<List<List<List<List<FrameStruct>>>>> chords, List<FrameStruct> list)
        {
            var ret = list.Select(f => chords[f.ChordIndex][f.VerticalOrHorizontal][f.DeltaIndex][f.z][f.y]).ToList();
            // this is the only place in the program where GroupMap is checked for null
            if (ret.All(f => f.GroupMap != null))
            {
                return ret;
            }
            return new List<FrameStruct>();
        }

        public static void PrintSegments(FrameStruct frame, List<Segment> segments, int chordIndex, int z, int y)
        {
            Debug.WriteLine($"> {frame}  : chord {chordIndex}  z {z}  y {y}");
            foreach (var path in segments)
            {
                foreach (var f in path.FrameStructs)
                {
                    Debug.WriteLine($"  {f}");
                }
                Debug.WriteLine(null);
            }
            //
            foreach (var segment in segments)
            {
                foreach (var f in segment.FrameStructs)
                {
                    switch (f.GroupMap.Type)
                    {
                        case GroupMapType.GroupMap:
                            Debug.WriteLine("GroupMap:");
                            break;
                        case GroupMapType.VerticalGroupMap:
                            Debug.WriteLine("VerticalGroupMap:");
                            break;
                        case GroupMapType.HorizontalGroupMap:
                            Debug.WriteLine("HorizontalGroupMap:");
                            break;
                    }
                    if (f.GroupMap.Snapshots.Count > 0)
                    {
                        f.GroupMap.Snapshots[f.GroupMap.SnapshotIndex].DebugWrite();
                    }
                }
                Debug.WriteLine(null);
            }
        }

        /// <summary>
        /// experiment to explore the possiblity of merging groups
        /// </summary>
        /// <param name="segments"></param>
        /// <param name="chordCount"></param>
        public static void PrintSegments(List<Segment> segments, int chordCount)
        {
            // output: chord segment GroupMapType fundamental set-members 
            // list chord indexes
            var chordIndexes = segments.SelectMany(s => s.FrameStructs).Select(f => f.GroupMap.ToneToGroups).SelectMany(t => t).SelectMany(t => t)
                .Where(t => t.Group != null).Select(t => t.Group.ChordIndex).Distinct().OrderBy(i => i).ToList();
            for (int chordIndex = 0; chordIndex < chordCount; ++chordIndex)
            {
                var GroupMaps = new List<(Group group, Vector<int> position)>();
                var VerticalGroupMaps = new List<(Group group, Vector<int> position)>();
                var HorizontalGroupMaps = new List<(Group group, Vector<int> position)>();
                var all = new List<(Group group, Vector<int> position, string groupMapType, int iSegment, int iFrame)>();
                // go through all the segments because multiple segments touch individual chords
                foreach (var segment in segments)
                {
                    int segmentIndex = segments.IndexOf(segment);
                    foreach (var framestruct in segment.FrameStructs)
                    {
                        int framestructIndex = segment.FrameStructs.IndexOf(framestruct);
                        string groupMapType = framestruct.GroupMap.Type == GroupMapType.GroupMap ? "GroupMap" : framestruct.GroupMap.Type == GroupMapType.VerticalGroupMap ? "VerticalGroupMap" : "HorizontalGroupMap";
                        all.AddRange(
                            framestruct.GroupMap.GetGroups(GroupType.Tone, GroupType.Group)
                            .Where(g => g.group.ChordIndex == chordIndex)
                            .Select(h => (h.group, h.position, groupMapType, segmentIndex, framestructIndex))
                            );
                    }
                }
                // order by position class, then by x, then by tone count
                foreach (var item in all
                    .OrderBy(a => 19 * a.position[1] + 28 * a.position[2])
                    .ThenBy(a => a.position[0])
                    .ThenBy(a => a.group.Children.Count))
                {
                    string tones =
                        item.group.Type == GroupType.Tone ?
                        $"{item.group.Tone.PitchHeight}" :
                        string.Join(" ", item.group.Children.Select(c => $"{c.Group.Tone.PitchHeight,2}"));
                    Debug.WriteLine($"{item.groupMapType,-18} chord {chordIndex} frame {item.iFrame} {VectorConverter.DebugFormatVector(item.position)} : {tones}");
                }
                var orderA = all
                    .GroupBy(h => new { h.position, h.group.Children.Count, h.group })
                    .Select(h => new { h.Key.position, h.Key.Count, h.Key.group }).ToList();
            }
        }

        static void DebugPrintSegments(List<Segment> segments, int depth, string msg)
        {
            if (!ShouldDebugPrintSegments) return;
            Debug.WriteLine(null);
            Debug.WriteLine(msg);
            int width = Math.Min(depth, 5);
            depth = Math.Min(depth, 5);
            var depthString = $"{new string(' ', width)}depth {depth}{new string(' ', 5 - width)}";
            foreach (var seg in segments)
            {
                Debug.WriteLine($"{depthString} {seg} {seg.Distance.Weighted:0.0}");
            }
        }
    }
}
