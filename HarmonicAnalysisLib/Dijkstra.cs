using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisLib;

public class AdjListNode : IComparable<AdjListNode>
{
    private int vertex;
    double weight;
    List<Segment> segments;
    public AdjListNode(int v, double w)
    {
        vertex = v;
        weight = w;
    }
    public AdjListNode(int v, List<Segment> segments)
    {
        vertex = v;
        weight = segments.Sum(s => s.Distance.Weighted);
        this.segments = segments;
    }
    public int getVertex()
    {
        return vertex;
    }
    public double getWeight()
    {
        return weight;
    }
    public int CompareTo(AdjListNode other)
    {
        return (int)(1000 * (weight - other.weight));
    }
}

public class Dijkstra
{
    // dijkstra is the original code and is included here for reference
    // dijkstra_mod is a modified version

    // Function to find the shortest distance of all the
    // vertices from the source vertex S.
    public static double[] dijkstra(
        int V, List<List<AdjListNode>> graph, int src)
    {
        double[] distance = new double[V];
        for (int i = 0; i < V; i++)
            distance[i] = Int32.MaxValue;
        distance[src] = 0;

        SortedSet<AdjListNode> pq
        = new SortedSet<AdjListNode>();
        pq.Add(new AdjListNode(src, 0));

        while (pq.Count > 0)
        {
            AdjListNode current = pq.First();
            pq.Remove(current);

            foreach (
                AdjListNode n in graph[current.getVertex()])
            {
                if (distance[current.getVertex()]
                    + n.getWeight()
                    < distance[n.getVertex()])
                {
                    distance[n.getVertex()]
                        = n.getWeight()
                        + distance[current.getVertex()];
                    // TODO: check that ctor doesn't need List<Segment> arg
                    pq.Add(new AdjListNode(
                        n.getVertex(),
                        distance[n.getVertex()]));
                }
            }
        }
        // If you want to calculate distance from source to
        // a particular target, you can return
        // distance[target]
        return distance;
    }

    public static (double[] distance, int[] prev) dijkstra_mod(
        int V, List<List<AdjListNode>> graph, int src)
    {
        double[] distance = new double[V];
        var prev = new int[V];
        for (int i = 0; i < V; i++)
        {
            distance[i] = Int32.MaxValue;
            prev[i] = -1;
        }
        distance[src] = 0;

        SortedSet<AdjListNode> pq
        = new SortedSet<AdjListNode>();
        pq.Add(new AdjListNode(src, 0));

        while (pq.Count > 0)
        {
            AdjListNode current = pq.First();
            pq.Remove(current);

            foreach (
                AdjListNode n in graph[current.getVertex()])
            {
                if (distance[current.getVertex()]
                    + n.getWeight()
                    < distance[n.getVertex()])
                {
                    distance[n.getVertex()]
                        = n.getWeight()
                        + distance[current.getVertex()];
                    prev[n.getVertex()] = current.getVertex();
                    // TODO: check that ctor doesn't need List<Segment> arg
                    pq.Add(new AdjListNode(
                        n.getVertex(),
                        distance[n.getVertex()]));
                }
            }
        }
        // If you want to calculate distance from source to
        // a particular target, you can return
        // distance[target]
        return (distance, prev);
    }
    public static List<int> shortest_path(int[] prev, int source, int dest)
    {
        var path = new LinkedList<int>();
        int u = dest;
        while (u != -1)
        {
            path.AddFirst(u);
            u = prev[u];
        }
        return path.ToList();
    }

    static void Main(string[] args)
    {
        int V = 9;
        List<List<AdjListNode>> graph
        = new List<List<AdjListNode>>();
        for (int i = 0; i < V; i++)
        {
            graph.Add(new List<AdjListNode>());
        }
        int source = 0;
        graph[0].Add(new AdjListNode(1, 4));
        graph[0].Add(new AdjListNode(7, 8));
        graph[1].Add(new AdjListNode(2, 8));
        graph[1].Add(new AdjListNode(7, 11));
        graph[1].Add(new AdjListNode(0, 7));
        graph[2].Add(new AdjListNode(1, 8));
        graph[2].Add(new AdjListNode(3, 7));
        graph[2].Add(new AdjListNode(8, 2));
        graph[2].Add(new AdjListNode(5, 4));
        graph[3].Add(new AdjListNode(2, 7));
        graph[3].Add(new AdjListNode(4, 9));
        graph[3].Add(new AdjListNode(5, 14));
        graph[4].Add(new AdjListNode(3, 9));
        graph[4].Add(new AdjListNode(5, 10));
        graph[5].Add(new AdjListNode(4, 10));
        graph[5].Add(new AdjListNode(6, 2));
        graph[6].Add(new AdjListNode(5, 2));
        graph[6].Add(new AdjListNode(7, 1));
        graph[6].Add(new AdjListNode(8, 6));
        graph[7].Add(new AdjListNode(0, 8));
        graph[7].Add(new AdjListNode(1, 11));
        graph[7].Add(new AdjListNode(6, 1));
        graph[7].Add(new AdjListNode(8, 7));
        graph[8].Add(new AdjListNode(2, 2));
        graph[8].Add(new AdjListNode(6, 6));
        graph[8].Add(new AdjListNode(7, 1));

        double[] distance = dijkstra(V, graph, source);
        // Printing the Output
        Console.WriteLine("Vertex "
                        + " Distance from Source");
        for (int i = 0; i < V; i++)
        {
            Console.WriteLine(
                "{0}			 {1}", i,
                distance[i]);
        }
    }
}

// This code is contributed by cavi4762.


