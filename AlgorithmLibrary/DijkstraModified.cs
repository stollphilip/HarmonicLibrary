using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonicAnalysisLib;

namespace Dijkstra_Algorithm
{
    public class Edge
    {
        public int Weight;
        public Node? Parent;
        public Node? Child;
        public Group Group;
    }
    public class Node
    {
        public string Name;
        public Node current_node;
        public List<Edge> Edges = new List<Edge>();
        public Group Group;

        public Node(string Name, Group group)
        {
            this.Name = Name;
            this.Group = group;
            current_node = this;
        }
        public Node AddEdge(Node child, int weight)
        {
            Edges.Add(new Edge()
            {
                Parent = current_node,
                Child = child,
                Weight = weight
            });
            if (!child.Edges.Exists(a => a.Parent == child && a.Child == current_node))
            {
                child.AddEdge(current_node, weight);
            }
            return current_node;
        }
    }
    class Graph
    {
        public Node? Root;
        public List<Node> AllNodes = new List<Node>();

        public Node CreateRoot(string name, Group group)
        {
            Root = CreateNode(name, group);
            return Root;
        }

        public Node CreateNode(string name, Group group)
        {
            var n = new Node(name, group);
            AllNodes.Add(n);
            return n;
        }
        public int?[,] CreateAdjMatrix()
        {
            int?[,] adj = new int?[AllNodes.Count, AllNodes.Count];
            for (int i = 0; i < AllNodes.Count; i++)
            {
                Node node1 = AllNodes[i];
                for (int j = 0; j < AllNodes.Count; j++)
                {
                    Node node2 = AllNodes[j];
                    var edge = node1.Edges.FirstOrDefault(a => a.Child == node2);
                    if (edge != null)
                    {
                        adj[i, j] = edge.Weight;
                    }
                    else
                    {
                        adj[i, j] = 0;
                    }
                }
            }
            return adj;
        }
        public int miniDist(int[] distance, bool[] tset)
        {
            int minimum = int.MaxValue;
            int index = 0;
            for (int k = 0; k < distance.Length; k++)
            {
                if (!tset[k] && distance[k] <= minimum)
                {
                    minimum = distance[k];
                    index = k;
                }
            }
            return index;
        }
        public List<int> Dijkstra(int?[,] graph, int src, int dest)
        {
            int length = graph.GetLength(0);
            int[] distance = new int[length];
            bool[] used = new bool[length];
            int[] prev = new int[length];

            for (int i = 0; i < length; i++)
            {
                distance[i] = int.MaxValue;
                used[i] = false;
                prev[i] = -1;
            }
            distance[src] = 0;

            for (int k = 0; k < length - 1; k++)
            {
                int minNode = miniDist(distance, used);
                used[minNode] = true;
                for (int i = 0; i < length; i++)
                {
                    if (graph[minNode, i] > 0)
                    {
                        int shortestToMinNode = distance[minNode];
                        int? distanceToNextNode = (int?)graph[minNode, i];
                        int? totalDistance = shortestToMinNode + distanceToNextNode;
                        if (totalDistance < distance[i])
                        {
                            distance[i] = (int)totalDistance;
                            prev[i] = minNode;
                        }
                    }
                }
            }
            if (distance[dest] == int.MaxValue)
            {
                return new List<int>();
            }
            var path = new LinkedList<int>();
            int currentNode = dest;
            while (currentNode != -1)
            {
                path.AddFirst(currentNode);
                currentNode = prev[currentNode];
            }
            return path.ToList();
        }
        public void PrintMatrix(ref int?[,] matrix, string[] labels, int count)
        {
            Console.Write("       ");
            for (int i = 0; i < count; i++)
            {
                Console.Write($" {labels[i]} ");
            }
            Console.WriteLine();

            for (int i = 0; i < count; i++)
            {
                Console.Write($" {labels[i]} | [ ");

                for (int j = 0; j < count; j++)
                {
                    if (matrix[i, j] == null)
                    {
                        Console.Write(" ,");
                    }
                    else
                    {
                        Console.Write($" {matrix[i, j]},");
                    }

                }
                Console.Write(" ]\r\n");
            }
            Console.Write("\r\n");
        }
        public void PrintPath(ref int?[,] graph, string[] labels, string src, string dest, out Path path)
        {
            int source = Array.IndexOf(labels, src);
            int destination = Array.IndexOf(labels, dest);

            Console.Write($" Shortest Path of [{src} -> {dest}] is : ");
            var paths = Dijkstra(graph, source, destination);
            List<int> lengths = [0];

            if (paths.Count > 0)
            {
                int? path_length = 0;
                for (int i = 0; i < paths.Count - 1; i++)
                {
                    int? length = (int?)graph[paths[i], paths[i + 1]];
                    lengths.Add((int)length);
                    path_length += length;
                    Console.Write($"{labels[paths[i]]} [{length}] -> ");
                }
                Console.WriteLine($"{labels[destination]} (Distance {path_length})");
                path = new Path
                {
                    Indexes = paths.ToArray(),
                    Labels = paths.Select(i => labels[i]).ToArray(),
                    Lengths = lengths.ToArray()
                };
            }
            else
            {
                Console.WriteLine("No Path");
                path = new Path();
            }
        }
    }

    class Program
    {
        // from https://medium.com/@kriangkrai.ratt/shortest-path-dijkstra-algorithm-with-rust-8fe1867d052a Shortest Path : Dijkstra Algorithm With Rust, C#, C++
        static void Main()
        {
            var graph = new Graph();
            var group = new Group();

            var a = graph.CreateRoot("A", group);
            var b = graph.CreateNode("B", group);
            var c = graph.CreateNode("C", group);
            var d = graph.CreateNode("D", group);
            var e = graph.CreateNode("E", group);
            var f = graph.CreateNode("F", group);
            var g = graph.CreateNode("G", group);
            var h = graph.CreateNode("H", group);
            var i = graph.CreateNode("I", group);
            var j = graph.CreateNode("J", group);
            var k = graph.CreateNode("K", group);
            var l = graph.CreateNode("L", group);

            a.AddEdge(d, 5)
                .AddEdge(e, 11)
                .AddEdge(f, 12);
            b.AddEdge(d, 11)
                .AddEdge(e, 6)
                .AddEdge(f, 13);
            c.AddEdge(d, 12)
                .AddEdge(e, 13)
                .AddEdge(f, 7);

            d.AddEdge(g, 5)
                .AddEdge(h, 11)
                .AddEdge(i, 12);
            e.AddEdge(g, 11)
                .AddEdge(h, 6)
                .AddEdge(i, 13);
            f.AddEdge(g, 12)
                .AddEdge(h, 13)
                .AddEdge(i, 7);

            g.AddEdge(j, 5)
                .AddEdge(k, 11)
                .AddEdge(l, 12);
            h.AddEdge(j, 11)
                .AddEdge(k, 6)
                .AddEdge(l, 13);
            i.AddEdge(j, 12)
                .AddEdge(k, 13)
                .AddEdge(l, 7);

            string[] labels = graph.AllNodes.Select(s => s.Name).ToArray();

            int?[,] adj = graph.CreateAdjMatrix();

            //List<int> paths;
            Path path;

            graph.PrintMatrix(ref adj, labels, graph.AllNodes.Count);

            //graph.PrintPath(ref adj, labels, "A", "L");

            //foreach (var src in new string[] { "A", "B", "C" })
            //{
            //    foreach (var dest in new string[] { "J", "K", "L" })
            //    {
            //        graph.PrintPath(ref adj, labels, src, dest, out paths, out path);
            //    }
            //}
            //foreach (var dest in new string[] { "J", "K", "L" })
            //{
            //    foreach (var src in new string[] { "A", "B", "C" })
            //    {
            //        graph.PrintPath(ref adj, labels, dest, src, out paths, out path);
            //    }
            //}
            var Labels = new string[][]
            {
                ["A", "B", "C"],
                ["D", "E", "F"],
                ["G", "H", "I"],
                ["J", "K", "L"]
            };

            // store[length][start][ipath]
            List<List<List<Path>>> store = new List<List<List<Path>>>();
            store.Add(new List<List<Path>>());

            // max number of edges, not nodes
            int length_max = 3;
            // the number of edges in the path
            for (int length = 1; length <= length_max; length++)
            {
                store.Add(new List<List<Path>>());
                // the starting node of the path
                for (int start = 0; start + length < Labels.Length; start++)
                {
                    store[length].Add(new List<Path>());
                    var addPaths = new List<Path>();

                    // for each source node find the shortest path to a destination node
                    foreach (var src in Labels[start])
                    {
                        var tmp = new List<Path>();
                        foreach (var dest in Labels[start + length])
                        {
                            graph.PrintPath(ref adj, labels, src, dest, out path);
                            tmp.Add(path);
                        }
                        var shortestPath = tmp.OrderBy(p => p.Length).First();
                        addPaths.Add(shortestPath);
                    }

                    // repeat but reverse the source and destination nodes
                    foreach (var src in Labels[start])
                    {
                        var tmp = new List<Path>();
                        foreach (var dest in Labels[start + length])
                        {
                            // notice that src and dest are swapped
                            graph.PrintPath(ref adj, labels, dest, src, out path);
                            tmp.Add(path);
                        }
                        var shortestPath = tmp.OrderBy(p => p.Length).First();
                        addPaths.Add(shortestPath);
                    }

                    // find mutual shortest paths
                    var removePaths = new List<Path>();
                    for (int p = 0; p < addPaths.Count - 1; p++)
                    {
                        for (int q = p + 1; q < addPaths.Count; q++)
                        {
                            if (addPaths[p].EqualsReverse(addPaths[q]))
                            {
                                addPaths[p].Mutual = true;
                                // remove one of the mutual paths
                                removePaths.Add(addPaths[q]);
                                break;
                            }
                        }
                    }
                    store[length][start].AddRange(addPaths.Where(p => !removePaths.Contains(p)));
                }
            }

            // build up the path containment hierarchy
            //   ADGJ
            //   /  \
            //  ADG DGJ
            //  / \ / \
            // AD  DG GJ
            // the difference in the number of edges in the shorter and longer paths
            for (int delta = 1; delta <= length_max; ++delta)
            {
                // the number of edges in the shorter and longer paths
                int length;
                int length2;
                // work your way from the longest paths to the shortest paths
                for (length2 = store.Count - 1, length = length2 - delta; length > 0; --length, --length2)
                {
                    for (int start = 0; start < store[length].Count; ++start)
                    {
                        for (int start2 = 0; start2 < store[length2].Count; ++start2)
                        {
                            foreach (var shorter in store[length][start])
                            {
                                foreach (var longer in store[length2][start2])
                                {
                                    Debug.WriteLine($"delta {delta} length {length} length2 {length2} start {start} start2 {start2} shorter {string.Join("", shorter.Labels)} longer {string.Join("", longer.Labels)} contains {longer.Contains(shorter)}");
                                    if (longer.Contains(shorter))
                                    {
                                        // path is contained in another path if path.Parent.Any()
                                        if (!shorter.ContainsParent(longer))
                                        {
                                            shorter.Parent.Add(longer);
                                        }
                                        else if (shorter.Parent.Any())
                                        {

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //output paths
            for (int length = store.Count - 1; length > 0; --length)
            {
                for (int start = store[length].Count - 1; start >= 0; --start)
                {
                    foreach (var p in store[length][start].Where(p => p.Parent.Count == 0))
                    {
                        Debug.Write($"Shortest Path of [{p.Labels.First()} -> {p.Labels.Last()}] is :");
                        for (int n = 0; n < p.Labels.Length; n++)
                        {
                            if (n > 0)
                            {
                                Debug.Write($" [{p.Lengths[n]}] ->");
                            }
                            Debug.Write($" {p.Labels[n]}");
                        }
                        Debug.WriteLine($" (Distance {p.Length})");
                    }
                }
            }
        }
    }
    public class Path
    {
        // indexes to labels[] and adj[,]
        public int[] Indexes;
        public string[] Labels;
        public int[] Lengths;
        public bool Mutual;
        public List<Path> Parent;
        public int Length { get { return Lengths.Sum(); } }
        public Path()
        {
            Indexes = [];
            Labels = [];
            Lengths = [];
            //Mutual = false;
            Parent = new List<Path>();
        }
        public string Join()
        {
            return string.Join("", Indexes.Select(i => i.ToString()));
        }
        public string JoinReverse()
        {
            return string.Join("", Indexes.Reverse().Select(i => i.ToString()));
        }
        public bool Contains(Path path)
        {
            return this.Join().Contains(path.Join());
        }
        public bool ContainsReverse(Path path)
        {
            return this.JoinReverse().Contains(path.Join());
        }
        public bool Equals(Path path)
        {
            return this.Join() == path.Join();
        }
        public bool EqualsReverse(Path path)
        {
            return this.JoinReverse() == path.Join();
        }
        public bool ContainsParent(Path path)
        {
            if (Parent == null)
            {
                return false;
            }
            if (Parent.Contains(path))
            {
                return true;
            }
            return Parent.Any(p => p.ContainsParent(path));
        }
    }
}
