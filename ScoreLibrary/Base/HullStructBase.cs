using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ScoreLibrary.Algorithm;

namespace ScoreLibrary.Base
{
    public class LineSegment
    {
        /// <summary>
        /// Field used for collision avoidance
        /// </summary>
        /// <remarks>Start.X => Column Start.Y => Row</remarks>
        public Point Start
        {
            get; set;
        }
        /// <summary>
        /// Field used for collision avoidance
        /// </summary>
        /// <remarks>End.X => Column End.Y => Row</remarks>
        public Point End
        {
            get; set;
        }
        /// <summary>
        /// Field used for Create PathFigure
        /// </summary>
        public Point Vertex => End;
        /// <summary>
        /// Field used for Create PathFigure
        /// </summary>
        public SweepDirection Sweep
        {
            get; set;
        }
        public LineSegment(Point start, Point end)
        {
            Start = start;
            End = end;
        }
        public LineSegment(LineSegment lineSegment, System.Windows.Vector vector)
        {
            Start = lineSegment.Start + vector;
            End = lineSegment.End + vector;
        }
        public LineSegment()
        {
        }

        public bool IsHorizontal => Start.Y == End.Y;
        public bool IsVertical => Start.X == End.X;
        public bool Leftward => IsHorizontal && Start.X > End.X;
        public bool Rightward => IsHorizontal && Start.X < End.X;
        public bool Upward => IsVertical && Start.Y > End.Y;
        public bool Downward => IsVertical && Start.Y < End.Y;
        public string Direction()
        {
            if (Leftward) return "Leftward";
            if (Rightward) return "Rightward";
            if (Upward) return "Upward";
            if (Downward) return "Downward";
            throw new Exception("Direction not found");
        }
        public static bool LinesIntersect(LineSegment a, LineSegment b)
        {
            // lines are horizontal and co-linear
            if (a.IsHorizontal && b.IsHorizontal && a.Start.Y == b.Start.Y)
            {
                bool disjoint = Math.Max(a.Start.X, a.End.X) < Math.Min(b.Start.X, b.End.X) || Math.Max(b.Start.X, b.End.X) < Math.Min(a.Start.X, a.End.X);
                return !disjoint;
            }
            // lines are vertical and co-linear
            if (a.IsVertical && b.IsVertical && a.Start.X == b.Start.X)
            {
                bool disjoint = Math.Max(a.Start.Y, a.End.Y) < Math.Min(b.Start.Y, b.End.Y) || Math.Max(b.Start.Y, b.End.Y) < Math.Min(a.Start.Y, a.End.Y);
                return !disjoint;
            }
            return false;
        }
        public override string ToString()
        {
            return $"{Start} {End} {Sweep}";
        }
    }

    /// <summary>
    /// Rectilinear Convex Hull, also known as Orthogonal Convex Hull
    /// </summary>
    public class HullStructBase
    {
        public List<LineSegment> Segments
        {
            get; set;
        } = new List<LineSegment>();
        public List<Point> Hull
        {
            get; set;
        }
        public List<Point> UpperHull
        {
            get; set;
        }
        public List<Point> LowerHull
        {
            get; set;
        }
        /// <summary>
        /// Hull before collision avoidance
        /// </summary>
        public List<Point> OriginalHull
        {
            get; set;
        }
        public HullStructBase(List<Point> input, double radius = 5.0)
        {
            var points = new List<Point>(input);
            RectilinearConvexHull.ExpandPoints(ref points, radius);
            var (hull, upper_perimeter, lower_perimeter) = RectilinearConvexHull.GetHull(points, true);

            Hull = hull;
            UpperHull = upper_perimeter;
            LowerHull = lower_perimeter;
            OriginalHull = hull;

            for (int i = 0; i < hull.Count; i++)
            {
                var lineSegment = new LineSegment(hull[i], hull[(i + 1) % hull.Count]);
                Segments.Add(lineSegment);
            }
        }
        public HullStructBase()
        {
        }
        // NextAvailableLine makes the hull larger. It never makes it smaller.
        // TODO: given two intersecting hull, pick the "right" one to make larger
        /// <summary>
        /// Move the hull to avoid collision with other hulls
        /// </summary>
        public static bool NextAvailableLine(ElementStructBase element, List<ElementStructBase> elementStructs)
        {
            // this is the only place in the code where Segments from different elements are compared
            // we have to take into account the element Locations
            var segments = new List<LineSegment>();
            foreach (var elementStruct in elementStructs.Where(e => e != element))
            {
                var hullLocation = elementStruct.HullLocation();
                segments.AddRange(elementStruct.Hull.Segments.Select(s => new LineSegment(s, hullLocation)));
            }
            const int limit = 2;
            const double margin = 3.5/*margin*/;
            var moved = false;
            for (var i = 0; i < element.Hull.Segments.Count; i++)
            {
                var hullLocation = element.HullLocation();
                var segment = new LineSegment(element.Hull.Segments[i], hullLocation);
                var count = 0;
                while (segments.Where(s => s != segment && s.IsHorizontal).Any(l => LineSegment.LinesIntersect(l, segment)) && count++ < limit)
                {
                    if (segment.Leftward)
                    {
                        //Debug.Write($"MoveDown {segment,10} -> ");
                        element.Hull.MoveDown(i, margin);
                        //Debug.WriteLine($"{segment,10}");
                    }
                    else
                    {
                        //Debug.Write($"MoveDown {segment,10} -> ");
                        element.Hull.MoveDown(i, -margin);
                        //Debug.WriteLine($"{segment,10}");
                        Debug.Assert(segment.Rightward);
                    }
                    moved = true;
                }
                count = 0;
                while (segments.Where(s => s != segment && s.IsVertical).Any(l => LineSegment.LinesIntersect(l, segment)) && count++ < limit)
                {
                    if (segment.Upward)
                    {
                        //Debug.Write($"MoveRight {segment,10} -> ");
                        element.Hull.MoveRight(i, -margin);
                        //Debug.WriteLine($"{segment,10}");
                    }
                    else
                    {
                        //Debug.Write($"MoveRight {segment,10} -> ");
                        element.Hull.MoveRight(i, margin);
                        Debug.Assert(segment.Downward);
                        //Debug.WriteLine($"{segment,10}");
                    }
                    moved = true;
                }
            }
            return moved;
        }
        private void MoveDown(int index, double i)
        {
            int count = Segments.Count;
            var A = Segments[index];
            var B = Segments[(index + 1) % count];
            var C = Segments[(index + count - 1) % count];
            Debug.Assert(A.Start == C.End);
            Debug.Assert(A.End == B.Start);
            A.Start = new Point(A.Start.X, A.Start.Y + i);
            A.End = new Point(A.End.X, A.End.Y + i);
            C.End = A.Start;
            B.Start = A.End;
            Debug.Assert(A.Start == C.End);
            Debug.Assert(A.End == B.Start);
        }
        private void MoveRight(int index, double i)
        {
            int count = Segments.Count;
            var A = Segments[index];
            var B = Segments[(index + 1) % count];
            var C = Segments[(index + count - 1) % count];
            Debug.Assert(A.Start == C.End);
            Debug.Assert(A.End == B.Start);
            A.Start = new Point(A.Start.X + i, A.Start.Y);
            A.End = new Point(A.End.X + i, A.End.Y);
            C.End = A.Start;
            B.Start = A.End;
            Debug.Assert(A.Start == C.End);
            Debug.Assert(A.End == B.Start);
        }
        /// <summary>
        /// Return the sides of the Location after collision avoidance that are outside the original Location
        /// </summary>
        /// <remarks>The sides are needed to SetGrid</remarks>
        public List<Side> GetSides()
        {
            // the strategy is to compare the original Location with the collision avoidance updated Location 
            (double left, double top, double right, double bottom) original = (
                OriginalHull.Min(h => h.X),
                OriginalHull.Min(h => h.Y),
                OriginalHull.Max(h => h.X),
                OriginalHull.Max(h => h.Y));
            (double left, double top, double right, double bottom) hull = (
                Segments.Min(s => Math.Min(s.Start.X, s.End.X)),
                Segments.Min(s => Math.Min(s.Start.Y, s.End.Y)),
                Segments.Max(s => Math.Max(s.Start.X, s.End.X)),
                Segments.Max(s => Math.Max(s.Start.Y, s.End.Y)));
            var sides = new List<Side>();
            if (hull.left < original.left)
            {
                sides.Add(Side.Left);
            }
            if (hull.top < original.top)
            {
                sides.Add(Side.Top);
            }
            if (hull.right > original.right)
            {
                sides.Add(Side.Right);
            }
            if (hull.bottom > original.bottom)
            {
                sides.Add(Side.Bottom);
            }
            return sides;
        }
    }
}
