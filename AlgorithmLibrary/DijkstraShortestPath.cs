using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DijkstraModified
{
    // from https://medium.com/@kriangkrai.ratt/shortest-path-dijkstra-algorithm-with-rust-8fe1867d052a
    public class Edge
    {
        public int Weight;
        public Node? Parent;
        public Node? Child;
    }
    public class Node
    {
        public string Name;
        public Node current_node;
        public List<Edge> Edges = new List<Edge>();

        public Node(string Name)
        {
            this.Name = Name;
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

        public Node CreateRoot(string name)
        {
            Root = CreateNode(name);
            return Root;
        }

        public Node CreateNode(string name)
        {
            var n = new Node(name);
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
        public List<int> Dijkstar(int?[,] graph, int src, int dest)
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
        public void PrintPath(ref int?[,] graph, string[] labels, string src, string dest)
        {
            int source = Array.IndexOf(labels, src);
            int destination = Array.IndexOf(labels, dest);

            Console.Write($" Shortest Path of [{src} -> {dest}] is : ");
            var paths = Dijkstar(graph, source, destination);

            if (paths.Count > 0)
            {
                int? path_length = 0;
                for (int i = 0; i < paths.Count - 1; i++)
                {
                    int? length = (int?)graph[paths[i], paths[i + 1]];
                    path_length += length;
                    Console.Write($"{labels[paths[i]]} [{length}] -> ");
                }
                Console.WriteLine($"{labels[destination]} (Distance {path_length})");
            }
            else
            {
                Console.WriteLine("No Path");
            }
        }
    }

    class Program
    {
        // from https://medium.com/@kriangkrai.ratt/shortest-path-dijkstra-algorithm-with-rust-8fe1867d052a Shortest Path : Dijkstra Algorithm With Rust, C#, C++
        static void Main_()
        {
            var graph = new Graph();

            var a = graph.CreateRoot("A");
            var b = graph.CreateNode("B");
            var c = graph.CreateNode("C");
            var d = graph.CreateNode("D");
            var e = graph.CreateNode("E");
            var f = graph.CreateNode("F");
            var g = graph.CreateNode("G");
            var h = graph.CreateNode("H");
            var i = graph.CreateNode("I");
            var j = graph.CreateNode("J");
            var k = graph.CreateNode("K");
            var l = graph.CreateNode("L");

            a.AddEdge(b, 3)
             .AddEdge(c, 2);

            b.AddEdge(c, 5)
             .AddEdge(d, 2)
             .AddEdge(g, 7);

            c.AddEdge(e, 2)
             .AddEdge(f, 9);

            d.AddEdge(e, 8)
             .AddEdge(f, 1);

            e.AddEdge(g, 3);

            f.AddEdge(g, 6)
             .AddEdge(h, 7)
             .AddEdge(k, 8);

            g.AddEdge(i, 6)
             .AddEdge(j, 9);

            h.AddEdge(i, 7)
             .AddEdge(j, 2);

            i.AddEdge(k, 4);

            j.AddEdge(k, 6)
             .AddEdge(l, 4);

            k.AddEdge(l, 5);

            string[] labels = graph.AllNodes.Select(s => s.Name).ToArray();

            int?[,] adj = graph.CreateAdjMatrix();

            graph.PrintMatrix(ref adj, labels, graph.AllNodes.Count);

            graph.PrintPath(ref adj, labels, "A", "L");
        }
    }
}
