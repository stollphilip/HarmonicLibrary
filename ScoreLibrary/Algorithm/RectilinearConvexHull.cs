﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ScoreLibrary.Algorithm
{
    // from OrthogonalConvexHull.sln
    /// <summary>
    /// Library for calculating the rectilinear convex hull of a set of points
    /// </summary>
    public class RectilinearConvexHull
    {
        /// <summary>
        /// 
        /// </summary>
        public static void ExpandPoints(ref List<Point> points, double radius = 1)
        {
            Vector[] deltas = new Vector[] { new Vector(0, 0), new Vector(0, radius), new Vector(radius, 0), new Vector(radius, radius) };

            var list = new List<Point>();
            foreach (var point in points)
            {
                foreach (var delta in deltas)
                {
                    var p = point + delta;
                    if (list.Count(i => i.Equals(p)) == 0)
                    {
                        list.Add(p);
                    }
                }
            }
            points = list;
        }

        // from Search Labs | AI Overview "Rectilinear convex hull python"
        /// <summary>
        /// calculates the rectilinear convex hull of a set of points
        /// </summary>
        /// <remarks>Returns the hull, and the lower and upper halves of the hull.</remarks>
        public static (List<Point> hull, List<Point> upper_perimeter, List<Point> lower_perimeter) GetHull(List<Point> points, bool removeCollinearPoints = false)
        {
            if (points.Count < 2)
                return (points, null, null);
            // Sort points by x-coordinate, then y-coordinate
            points = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();

            // Compute upper hull
            var upper = new List<Point>();
            foreach (var p in points)
            {
                // modified code
                while (upper.Count >= 2 && upper.Last().Y >/*>=*/ p.Y)
                {
                    for (int j = 0; j < upper.Count; j++)
                    {
                        var remove = j == upper.Count - 1 ? "remove" : string.Empty;
                    }
                    upper.RemoveAt(upper.Count - 1);
                }
                upper.Add(p);
            }

            // Compute lower hull
            var lower = new List<Point>();
            points.Reverse();
            foreach (var p in points)
            {
                // modified code
                while (lower.Count >= 2 && lower.Last().Y </*<=*/ p.Y)
                {
                    for (int j = 0; j < lower.Count; j++)
                    {
                        var remove = j == lower.Count - 1 ? "remove" : string.Empty;
                    }
                    lower.RemoveAt(lower.Count - 1);
                }
                lower.Add(p);
            }

            if (removeCollinearPoints)
            {
                RemoveCollinearPoints(ref upper);
                RemoveCollinearPoints(ref lower);
            }

            // Remove duplicate points
            var hull = new List<Point>(upper);
            for (int i = 1; i < lower.Count - 1; i++)
            {
                hull.Add(lower[i]);
            }

            (List<Point> upper_perimeter, List<Point> lower_perimeter) = GetRectilinearPerimeterAroundHull(upper, lower);

            // modified code
            if (removeCollinearPoints)
            {
                RemoveCollinearPoints(ref upper_perimeter);
                RemoveCollinearPoints(ref lower_perimeter);

                // Remove duplicate points
                hull = new List<Point>(upper_perimeter);
                for (int i = 1; i < lower_perimeter.Count - 1; i++)
                {
                    hull.Add(lower_perimeter[i]);
                }
            }

            return (hull, upper_perimeter, lower_perimeter);
        }
        // change >= to > and <= to < in the while loops to get a better result
        /// <summary>
        /// Original code generated by AI for reference
        /// </summary>
        public static (List<Point> hull, List<Point> upper_perimeter, List<Point> lower_perimeter) GetHull_original(List<Point> points, bool removeCollinearPoints = false)
        {
            if (points.Count < 2)
                return (points, null, null);
            // Sort points by x-coordinate, then y-coordinate
            points = points.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();

            // Compute upper hull
            Debug.WriteLine("upper");
            var upper = new List<Point>();
            foreach (var p in points)
            {
                Debug.WriteLine(p.ToString().Replace("Point", "p    "));
                while (upper.Count >= 2 && upper.Last().Y >= p.Y)
                {
                    for (int j = 0; j < upper.Count; j++)
                    {
                        var remove = j == upper.Count - 1 ? "remove" : string.Empty;
                        Debug.WriteLine($"{upper[j]} {remove}");
                    }
                    upper.RemoveAt(upper.Count - 1);
                    Debug.WriteLine(null);
                }
                upper.Add(p);
            }
            Debug.WriteLine("upper");
            for (int j = 0; j < upper.Count; j++)
            {
                Debug.WriteLine($"{upper[j]}");
            }
            Debug.WriteLine(null);

            // Compute lower hull
            Debug.WriteLine("lower");
            var lower = new List<Point>();
            points.Reverse();
            foreach (var p in points)
            {
                Debug.WriteLine(p.ToString().Replace("Point", "p    "));
                while (lower.Count >= 2 && lower.Last().Y <= p.Y)
                {
                    for (int j = 0; j < lower.Count; j++)
                    {
                        var remove = j == lower.Count - 1 ? "remove" : string.Empty;
                        Debug.WriteLine($"{lower[j]} {remove}");
                    }
                    lower.RemoveAt(lower.Count - 1);
                    Debug.WriteLine(null);
                }
                lower.Add(p);
            }
            Debug.WriteLine("lower");
            for (int j = 0; j < lower.Count; j++)
            {
                Debug.WriteLine($"{lower[j]}");
            }
            Debug.WriteLine(null);

            if (removeCollinearPoints)
            {
                RemoveCollinearPoints(ref upper);
                RemoveCollinearPoints(ref lower);
            }

            // Remove duplicate points
            var hull = new List<Point>(upper);
            for (int i = 1; i < lower.Count - 1; i++)
            {
                hull.Add(lower[i]);
            }

            (List<Point> upper_perimeter, List<Point> lower_perimeter) = GetRectilinearPerimeterAroundHull(upper, lower);

            return (hull, upper_perimeter, lower_perimeter);
        }

        /// <summary>
        /// adds points to the hull to make a rectilinear perimeter around the hull
        /// </summary>
        /// <remarks>to get the perimeter as a single list, remove duplicate points exactly as in GetHull</remarks>
        public static (List<Point> upper_perimeter, List<Point> lower_perimeter) GetRectilinearPerimeterAroundHull(List<Point> upper, List<Point> lower)
        {
            var upper_perimeter = new List<Point>();
            for (int i = 0; i < upper.Count; i++)
            {
                var curr = upper[i];
                upper_perimeter.Add(curr);
                if (i == upper.Count - 1)
                    break;
                var next = upper[(i + 1) % upper.Count];
                Point point;
                if (next.Y < curr.Y)
                {
                    point = new Point(next.X, curr.Y);
                }
                else
                {
                    point = new Point(curr.X, next.Y);
                }
                if (point != curr && point != next)
                {
                    upper_perimeter.Add(point);
                }
            }
            var lower_perimeter = new List<Point>();
            for (int i = 0; i < lower.Count; i++)
            {
                var curr = lower[i];
                lower_perimeter.Add(curr);
                if (i == lower.Count - 1)
                    break;
                var next = lower[(i + 1) % lower.Count];
                var pt = new Point(Math.Min(curr.X, next.X), Math.Min(curr.Y, next.Y));
                Point point;
                if (next.Y > curr.Y)
                {
                    point = new Point(next.X, curr.Y);
                }
                else
                {
                    point = new Point(curr.X, next.Y);
                }
                if (!point.Equals(curr) && !point.Equals(next))
                {
                    lower_perimeter.Add(point);
                }
            }

            return (upper_perimeter, lower_perimeter);
        }

        /// <summary>
        /// removes collinear points from a list of points
        /// </summary>
        /// <remarks>whenever three points pass through the same line, remove the middle point</remarks>
        public static void RemoveCollinearPoints(ref List<Point> points)
        {
            var remove = new List<Point>();
            for (int i = 0; i < points.Count - 2; i++)
            {
                if (points[i].X == points[i + 1].X && points[i + 1].X == points[i + 2].X ||
                    points[i].Y == points[i + 1].Y && points[i + 1].Y == points[i + 2].Y)
                {
                    remove.Add(points[i + 1]);
                }
            }
            points = points.Where(p => !remove.Contains(p)).ToList();
        }
    }
}
