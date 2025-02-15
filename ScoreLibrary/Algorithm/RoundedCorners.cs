using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ScoreLibrary.Algorithm
{
    // from OrthogonalConvexHull.sln
    /// <summary>
    /// Library for drawing rounded corners as <see cref="System.Windows.Shapes.Path"/>
    /// </summary>
    public class RoundedCorners
    {
        public const double Delta = 5/*25*/;
        public static PathGeometry CreatePathGeometry(List<Point> vertices)
        {
            var points = Expand(vertices);
            var figure = CreatePathFigure(points);
            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);
            return geometry;
        }
        public static Path CreatePath(List<Point> vertices, double scale)
        {
            var geometry = CreatePathGeometry(vertices);

            var path = new Path { Stroke = Brushes.Black,
                StrokeThickness = 1,
                // so that notes are centered on the staff line
                Margin = new Thickness(0, -scale * 0.5, 0, 0),
            };
            path.Data = geometry;

            return path;
        }
        public static Path CreatePath(List<Point> vertices, Canvas canvas)
        {
            var points = Expand(vertices);
            var figure = CreatePathFigure(points);
            var geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            var path = new Path { Stroke = Brushes.Black, StrokeThickness = 1 };
            path.Data = geometry;

            //var paths = canvas.Children.OfType<Path>().ToList();
            //foreach (var item in paths)
            //{
            //    canvas.Children.Remove(item);
            //}
            canvas.Children.Add(path);
            return path;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        public static PathFigure CreatePathFigure(List<(Point vertex, Point A, Point B, SweepDirection sweep)> points, double delta = Delta)
        {
            delta = delta * Zoom.ZoomFactor;
            var figure = new PathFigure { IsClosed = true, StartPoint = points[0].A };
            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i];
                var q = points[(i + 1) % points.Count];
                figure.Segments.Add(new LineSegment() { Point = p.B });
                figure.Segments.Add(new ArcSegment() { Size = new Size(delta, delta), IsLargeArc = false, SweepDirection = p.sweep, Point = q.A });
            }
            return figure;
        }

        /// <summary>
        /// Generate the Points and SweepDirections to create a PathFigure
        /// </summary>
        /// <param name="vertices">a set of points forming a close, rectilinear, counterclockwise perimeter</param>
        /// <param name="delta">should half or less the shortest between points/param>
        public static List<(Point vertex, Point A, Point B, SweepDirection sweep)> Expand(List<Point> vertices, double delta = Delta)
        {
            delta = delta * Zoom.ZoomFactor;
            var list = new List<(Point vertex, Point A, Point B, SweepDirection sweep)>();
            for (int i = 0; i < vertices.Count; i++)
            {
                SweepDirection sweep;
                var A = vertices[i];
                var B = vertices[(i + 1) % vertices.Count];
                var C = vertices[(i + 2) % vertices.Count];

                // validate arguments
                if (A.Equals(B))
                {
                    throw new InvalidOperationException("Consecutive Points must not be equal.");
                }
                if (IsHorizontal(A, B) == IsVertical(A, B))
                {
                    throw new InvalidOperationException("Consecutive Points must form horizontal and vertical lines, and must alternate horizontal and vertical.");
                }
                if (IsHorizontal(A, B) && Math.Abs(A.X - B.X) < 2 * delta ||
                    IsVertical(A, B) && Math.Abs(A.Y - B.Y) < 2 * delta)
                {
                    // TODO: "Consecutive Points must be no closer than 2*delta."
                    //throw new ArgumentException("Consecutive Points must be no closer than 2*delta.");
                }

                if (
                    IsDownward(A, B) && IsRightward(B, C) ||
                    IsRightward(A, B) && IsUpward(B, C) ||
                    IsUpward(A, B) && IsLeftward(B, C) ||
                    IsLeftward(A, B) && IsDownward(B, C)
                    )
                {
                    sweep = SweepDirection.Counterclockwise;
                }
                else
                {
                    sweep = SweepDirection.Clockwise;
                }

                if (IsDownward(A, B))
                {
                    list.Add((B, new Point(A.X, A.Y + delta), new Point(B.X, B.Y - delta), sweep));
                }
                else if (IsUpward(A, B))
                {
                    list.Add((B, new Point(A.X, A.Y - delta), new Point(B.X, B.Y + delta), sweep));
                }
                else if (IsRightward(A, B))
                {
                    list.Add((B, new Point(A.X + delta, A.Y), new Point(B.X - delta, B.Y), sweep));
                }
                else if (IsLeftward(A, B))
                {
                    list.Add((B, new Point(A.X - delta, A.Y), new Point(B.X + delta, B.Y), sweep));
                }
                else
                {
                    throw new InvalidOperationException($"Points must be rectilinear.");
                }
            }
            return list;
        }
        public static bool IsVertical(Point A, Point B) => A.X == B.X;
        public static bool IsHorizontal(Point A, Point B) => A.Y == B.Y;
        public static bool IsDownward(Point A, Point B) => A.Y < B.Y && IsVertical(A, B);
        public static bool IsUpward(Point A, Point B) => A.Y > B.Y && IsVertical(A, B);
        public static bool IsRightward(Point A, Point B) => A.X < B.X && IsHorizontal(A, B);
        public static bool IsLeftward(Point A, Point B) => A.X > B.X && IsHorizontal(A, B);

    }
}
