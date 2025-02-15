using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisLib;
/// <summary>
/// SegmentLib handles Segments for calling Dijkstra's algorithm
/// </summary>
public class SegmentLib2
{
    const bool DebugOutput = true;
    public static void CallDijkstra(List<List<List<List<List<FrameStruct>>>>> chordShapes /* [ChordIndex][VerticalHorizontal][DeltaIndex][z][y] */, PitchPattern[] chords)
    {
        var distinct = new List<Group>();
        foreach (var frameStruct in FrameStructLib.FrameStructIterator(chordShapes)
            .Where(fs => fs.GroupMap != null))
        {
            foreach (var frame in FrameStruct_to_Frames(frameStruct).Where(f => !distinct.Contains(f)))
            {
                distinct.Add(frame);
            }
        }
        distinct = distinct.OrderBy(f => f.ChordIndex).ToList();
        // assign Index
        for (int index = 0; index < distinct.Count; index++)
        {
            distinct[index].Index = index;
        }
        var sources = distinct.Where(f => f.ChordIndex == 0).Select(f => f.Index).ToArray();
        var destinations = distinct.Where(f => f.ChordIndex == chords.Length - 1).Select(f => f.Index).ToArray();
        // init graph
        var graph = new List<List<AdjListNode>>();
        for (int i = 0; i < distinct.Count; i++)
        {
            graph.Add(new List<AdjListNode>());
        }
        foreach (var frame in FrameStructLib.FrameStructIterator(chordShapes)
            .Where(fs => fs.ChordIndex < chords.Length - 1 && fs.VerticalOrHorizontal == 0 && fs.DeltaIndex == 0 && fs.GroupMap != null))
        {
            // TODO: detect cycles
            List<List<Segment>> neighbors = SegmentLib.Neighbor(chordShapes, frame);
            foreach (var neighbor in neighbors)
            {
                var fl = FirstAndLastGroups(neighbor);
                int u = fl[0].Index;
                int v = fl[1].Index;
                Debug.Assert(u != v);
                Debug.Assert(distinct.Contains(fl[0]));
                Debug.Assert(distinct.Contains(fl[1]));
                Debug.Assert(fl[0] != fl[1]);
                graph[u].Add(new AdjListNode(v, neighbor));
                // Kludge: reverse the AdjListNode
                graph[v].Add(new AdjListNode(u, neighbor));
            }
        }
        // find shortest path source and end vertexes
        var V = distinct.Count;
        (double dist, int src, int dest) shortest = (0, -1, -1);
        foreach (int source in sources)
        {
            (double[] distance, int[] prev) = Dijkstra.dijkstra_mod(V, graph, source);
            double minDistance = destinations.Min(d => distance[d]);
            int indexDest = distance.ToList().IndexOf(minDistance);
            if (shortest.src == -1 || minDistance < shortest.dist)
            {
                shortest = (minDistance, source, indexDest);
            }
        }
        if (shortest.src != -1)
        {
            (double[] distance, int[] prev) = Dijkstra.dijkstra_mod(V, graph, shortest.src);
            var list = Dijkstra.shortest_path(prev, shortest.src, shortest.dest);
            var s = string.Join(" -> ", list);
            Debug.WriteLineIf(DebugOutput, s);
        }
    }
    public static List<Group> FrameStruct_to_Frames(FrameStruct frameStruct)
    {
        if (frameStruct.GroupMap.Frame.Type == GroupType.Frame)
        {
            return new List<Group> { frameStruct.GroupMap.Frame };
        }
        else
        {
            Debug.Assert(frameStruct.GroupMap.Frame.Type == GroupType.VerticalFrame || frameStruct.GroupMap.Frame.Type == GroupType.HorizontalFrame);
            return frameStruct.GroupMap.Frame.Children.Select(child => child.Group).ToList();
        }
    }
    public static Group FirstGroup(Segment segment)
    {
        return FrameStruct_to_Frames(segment.FrameStructs.First()).First();
    }
    public static Group LastGroup(Segment segment)
    {
        return FrameStruct_to_Frames(segment.FrameStructs.Last()).Last();
    }
    public static Group FirstGroup(List<Segment> segments)
    {
        return FirstGroup(segments.First());
    }
    public static Group LastGroup(List<Segment> segments)
    {
        return LastGroup(segments.Last());
    }
    public static List<Group> FirstAndLastGroups(List<Segment> segments)
    {
        return new List<Group> { FirstGroup(segments.First()), LastGroup(segments.Last()) };
    }
}
