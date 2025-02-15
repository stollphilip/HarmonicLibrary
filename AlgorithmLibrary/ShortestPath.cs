using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AlgorithmLibrary
{
    public class Graph
    {
        // from https://www.w3schools.com/dsa/dsa_algo_graphs_dijkstra.php
        double[][] adj_matrix = new double[0][];
        public int size = 0;
        public string[] vertex_data = new string[0];
        public Graph(int size)
        {
            adj_matrix = Enumerable.Range(0, size).Select(i => Enumerable.Range(0, size).Select(j => 0d).ToArray()).ToArray();
            this.size = size;
            vertex_data = Enumerable.Range(0, size).Select(i => string.Empty).ToArray();

        }
        public void add_edge(int u, int v, double weight)
        {
            if (0 <= u && u < size && 0 <= v && v < size)
            {
                adj_matrix[u][v] = weight;
                adj_matrix[v][u] = weight;  // For undirected graph
            }
        }
        public void add_vertex_data(int vertex, string data)
        {
            if (0 <= vertex && vertex < size)
            {
                vertex_data[vertex] = data;
            }
        }
        public (double[] distance,  int[] predecessor) dijkstra(string start_vertex_data)
        {
            int start_vertex = vertex_data.ToList().IndexOf(start_vertex_data);
            var distances = Enumerable.Range(0, size).Select(i => double.MaxValue).ToArray();
            //distances = [float('inf')] * size
            var predecessors = Enumerable.Range(0, size).Select(i => int.MaxValue).ToArray();
            distances[start_vertex] = 0;
            //visited = [False] * size
            var visited = Enumerable.Range(0, size).Select(i => false).ToArray();

            for (int h = 0; h < size; h++)
            //for _ in range(size):
            {
                var min_distance= double.MaxValue;
                //min_distance = float('inf')
            //u = None
                int u = int.MaxValue;
                for (int i = 0; i < size; ++i)
                //for i in range(size):
                {
                    if (!visited[i] && distances[i] < min_distance)
                    {
                        min_distance = distances[i];
                        u = i;
                    }
                }

                if (u == int.MaxValue)
                    break;

                visited[u] = true;

                for (int v = 0; v < size; ++v)
                //for v in range(size):
                {
                    if (adj_matrix[u][v] != 0 && !visited[v])
                    {
                        double alt = distances[u] + adj_matrix[u][v];
                    if (alt < distances[v])
                        {
                            distances[v] = alt;
                            predecessors[v] = u;
                        }
                    }
                }

            }
            return (distances, predecessors);
        }
        public string get_path(int[] predecessors, string start_vertex, string end_vertex)
        {
            var path = new List<string>();
            var current = vertex_data.ToList().IndexOf(end_vertex);
            while (current != int.MaxValue)
            {
                path.Insert(0, vertex_data[current]);
                current = predecessors[current];
                if (current == vertex_data.ToList().IndexOf(start_vertex))
                {
                    path.Insert(0, start_vertex);
                    break;
                }
            }
            return string.Join(" -> ", path.ToArray());
        }
        public static void TextGraph()
        {
            var g = new Graph(7);

            g.add_vertex_data(0, "A");
            g.add_vertex_data(1, "B");
            g.add_vertex_data(2, "C");
            g.add_vertex_data(3, "D");
            g.add_vertex_data(4, "E");
            g.add_vertex_data(5, "F");
            g.add_vertex_data(6, "G");

            g.add_edge(3, 0, 4); // D - A, weight 5
            g.add_edge(3, 4, 2); // D - E, weight 2
            g.add_edge(0, 2, 3); // A - C, weight 3
            g.add_edge(0, 4, 4); // A - E, weight 4
            g.add_edge(4, 2, 4); // E - C, weight 4
            g.add_edge(4, 6, 5); // E - G, weight 5
            g.add_edge(2, 5, 5); // C - F, weight 5
            g.add_edge(2, 1, 2); // C - B, weight 2
            g.add_edge(1, 5, 2); // B - F, weight 2
            g.add_edge(6, 5, 5); // G - F, weight 5

            // Dijkstra's algorithm from D to all vertices
            Debug.WriteLine("Dijkstra's Algorithm starting from vertex D:");
            (double[] distances, int[] predecessors) a = g.dijkstra("D");
            int i = 0;
            foreach (var d in a.distances)
            {
                Debug.WriteLine($"Distance from D to {g.vertex_data[i]}: {d}");
                i++;
            }
            i = 0;
            foreach (var d in a.distances)
            {
                var path = g.get_path(a.predecessors, "D", g.vertex_data[i]);
                Debug.WriteLine($"{path}, Distance: {d}");
                i++;
            }
        }
    }
}
