using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using HarmonicAnalysisCommonLib;
using System.Windows.Media;

namespace ScoreLibrary.Base
{
    // data for drawing one chord in one column
    /// <summary>
    /// represents a chord in one column on a Staff
    /// </summary>
    /// <remarks>responsible for adding Elements, updating layout and location of Elements</remarks>
    public class ChordStructBase
    {
        // list of elementStructs
        public List<ElementStructBase> elementStructs = new List<ElementStructBase>();
        public Visibility Visibility
        {
            get; private set;
        } = Visibility.Visible;

        //// overlay another chord over this chord
        //public ChordStructBase Overlay
        //{
        //    get; private set;
        //}
        ///// <summary>
        ///// original Location of the Staff on the Grid with respect to its parent Progression
        ///// </summary>
        //public LocationStruct Original
        //{
        //    get; private set;
        //}
        /// <summary>
        /// The location after (Element) collision avoidance of the Staff on the Grid with respect to its parent Progression
        /// </summary>
        public LocationStruct Location
        {
            get; set;
        }

        // YStaff has staff lines, and is the only class that implements this
        /// <summary>
        ///  the difference between Location and Original
        /// </summary>
        public int DeltaRow
        {
            get; set;
        }
        public IEnumerable<ElementStructBase> ElementStructIterator()
        {
            foreach (var elementStruct in elementStructs)
            {
                yield return elementStruct;
            }
        }

        #region public interface
        /// <summary>
        ///  add Element
        /// </summary>
        public virtual void Add(ElementStructBase elementStruct)
        {
            Debug.Assert(!elementStructs.Contains(elementStruct));
            elementStructs.Add(elementStruct);
        }
        /// <summary>
        /// Update Location
        /// </summary>
        public LocationStruct UpdateLocation()
        {
            //if (elementStructs.Count == 0)
            //{
            //    return new LocationStruct(0, 0, 0, 0);
            //}
            var rowMin = elementStructs.Where(e => e.Visibility != Visibility.Collapsed).Min(e => e.Location.Row);
            var columnMin = elementStructs.Where(e => e.Visibility != Visibility.Collapsed).Min(e => e.Location.Column);
            var rowMax = elementStructs.Where(e => e.Visibility != Visibility.Collapsed).Max(e => e.Location.Row + e.Location.RowSpan);
            var columnMax = elementStructs.Where(e => e.Visibility != Visibility.Collapsed).Max(e => e.Location.Column + e.Location.ColumnSpan);
            Location = new LocationStruct(rowMin, columnMin, rowMax - rowMin, columnMax - columnMin);
            return Location;
        }
        /// <summary>
        /// Return next available ElementStruct location after performing collision avoidance
        /// </summary>
        public virtual LocationStruct NextAvailableLocation(LocationStruct location)
        {
            return location;
        }
        public virtual void SetGrid(int row, int column, int rowSpan, int columnSpan)
        {
            foreach (var element in elementStructs)
            {
                // not "Location.Row + row"
                element.SetGrid(row, Location.Column + column, Location.RowSpan, Location.ColumnSpan);
            }
        }
        #endregion

        public virtual LocationStruct Translate(Vector<int> position)
        {
            return new LocationStruct(0, 0, 0, 0);
        }
        public virtual LocationStruct Translate(Vector<int> position, int rowSpan, int columnSpan)
        {
            return new LocationStruct(0, 0, 0, 0);
        }
        public virtual LocationStruct Translate(Vector<int> position, XYZStaffLayout xYZStaffLayout)
        {
            return new LocationStruct(0, 0, 0, 0);
        }
        public virtual LocationStruct Translate(Vector<int> position, XYZStaffLayout xYZStaffLayout, int rowSpan, int columnSpan)
        {
            return new LocationStruct(0, 0, 0, 0);
        }
        public virtual LocationStruct Translate(ToneLite pitch)
        {
            return new LocationStruct(0, 0, 0, 0);
        }
        public virtual LocationStruct Translate(ToneLite pitch, int rowSpan, int columnSpan)
        {
            return new LocationStruct(0, 0, rowSpan, columnSpan);
        }
        /// <summary>
        /// Scale LocationStruct Row and Column by a scaling factor
        /// </summary>
        /// <remarks>a note occupies 2 Grid Rows and Columns on most staffs</remarks>
        public virtual void Scale(ref LocationStruct location)
        {
            const int scaleFactor = 2;
            location.Row *= scaleFactor;
            location.Column *= scaleFactor;
        }
        public virtual Brush MapColor(Vector<int> position, XYZStaffLayout xYZStaffLayout)
        {
            return Brushes.Black;
        }
        /// <summary>
        /// Normalize so the minimum Column of the elements is 0
        /// </summary>
        public virtual void Normalize()
        {
            if (elementStructs.Count == 0)
            {
                return;
            }
            var minColumn = elementStructs.Min(e => e.Location.Column);
            foreach (var element in elementStructs)
            {
                element.Location.Column -= minColumn;
            }
            Debug.Assert(elementStructs.Min(e => e.Location.Column) == 0);
        }
        /// <summary>
        /// Get the next available Grid location for an element, implementing collision avoidance
        /// </summary>
        /// <param name="elem">the element whose location is being requested</param>
        /// <param name="stemDirection">stem direction</param>
        public virtual LocationStruct NextAvailableLocation(ElementStructBase elem, StemDirection stemDirection = StemDirection.Up)
        {
            var tag = elem.Tag as string;
            var delta = 0;
            switch (tag)
            {
                case "note":
                case "clef":
                    while (elementStructs.Where(x => x.Tag == tag && x.IsLaidOut).Any(x => LocationStruct.IsIntersect(GetLocation(elem, delta), x.Location)))
                    {
                        if (stemDirection == StemDirection.Up)
                        {
                            ++delta;
                        }
                        else
                        {
                            --delta;
                        }
                    }
                    return GetLocation(elem, delta);
                case "dot":
                    //TODO: implement
                    return GetLocation(elem, delta);
                case "alter":
                    while (elementStructs.Where(x => (x.Tag == "note" || x.Tag == "alter") && x.IsLaidOut).Any(x => LocationStruct.IsIntersect(GetLocation(elem, delta), x.Location)))
                    {
                        --delta;
                    }
                    return GetLocation(elem, delta);
                default:
                    throw new InvalidOperationException();
            }
        }
        /// <summary>
        /// Get Location plus an optional delta
        /// </summary>
        private LocationStruct GetLocation(ElementStructBase t, int delta = 0)
        {
            return new LocationStruct(t.Location.Row, t.Location.Column + delta, t.Location.RowSpan, t.Location.ColumnSpan);
        }
        public virtual void LayoutNotes()
        {
            LayoutNotes(0, StemDirection.Up);
        }
        /// <summary>
        /// implement collision avoidance
        /// </summary>
        /// <param name="staff">used only by PitchHeightStaff</param>
        /// <param name="stemDirection">used only by PitchHeightStaff</param>
        public void LayoutNotes(int staff = 0, StemDirection stemDirection = StemDirection.Up)
        {
            List<ElementStructBase> list;
            if (stemDirection == StemDirection.Up)
            {
                // TODO: implement OrderBy
                list = elementStructs.Where(e => e.Staff == staff && e.Tag == "note")/*.OrderByDescending(e => e.StepToRow.row)*/.ToList();
            }
            else
            {
                list = elementStructs.Where(e => e.Staff == staff && e.Tag == "note")/*.OrderBy(e => e.StepToRow.row)*/.ToList();
            }
            List<ElementStructBase> list2 = new List<ElementStructBase>();
            foreach (var element in list)
            {
                LocationStruct p = NextAvailableLocation(element, stemDirection);
                element.Location = new LocationStruct(p.Row, p.Column, p.RowSpan, p.ColumnSpan);
                Debug.Assert(!element.IsLaidOut);
                element.IsLaidOut = true;
                // uncomment to debug (very useful)
                //if (element.Position.Position == System.Numerics.Vector<int>.Zero)
                //{
                //    AddBorder(element.Location);
                //}
            }
        }
        /// <summary>
        /// implement Hull collision avoidance
        /// </summary>
        /// <remarks>Layout the Segments and Locations of the Hulls</remarks>
        public void LayoutHulls()
        {
            //return;
            List<ElementStructBase> elements = elementStructs.Where(e => e.Tag == "border").ToList();
            foreach (var element in elements)
            {
                // don't want to reset Location if we don't have to
                element.IsLaidOut = false;
            }
            List<HullStructBase> list = elementStructs.Where(e => e.Tag == "border").Select(e => e.Hull).ToList();
            for (var i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                var changed = HullStructBase.NextAvailableLine(element, elements);
                if (changed)
                {
                    var vector = UpdateBorderElementLocation(element);
                    RefreshLayoutHulls(element, vector);
                    element.IsLaidOut = true;
                }
            }
        }

        /// <summary>
        /// Update "border" Element Location
        /// </summary>
        /// <param name="element">"border" Element</param>
        /// <returns></returns>
        private System.Windows.Vector UpdateBorderElementLocation(ElementStructBase element)
        {
            // The "border" element Location is a Grid Location
            // In case collision avoidance causes the Hull to spill over to adjoining Grid locations
            // update the Location of the "border" Element, and update the Segments accordingly so that the Segments don't move.
            // 
            // The strategy is to get the Sides that oveflowed and update the Location accordingly.

            // get the sides of the Location after collision avoidance that are outside the original Location
            var sides = element.Hull.GetSides();
            var vector = new System.Windows.Vector();
            var scale = Zoom.GridRowHeightColumnSize;
            // update the Location of the "border" Element
            if (sides.Contains(Side.Left))
            {
                element.Location.Column--;
                element.Location.ColumnSpan++;
                vector.X += scale;
            }
            if (sides.Contains(Side.Top))
            {
                element.Location.Row--;
                element.Location.RowSpan++;
                vector.Y += scale;
            }
            if (sides.Contains(Side.Right))
            {
                element.Location.ColumnSpan++;
            }
            if (sides.Contains(Side.Bottom))
            {
                element.Location.RowSpan++;
            }
            // the caller is responsible to update the Segments accordingly so that the Segments don't move
            return vector;
        }

        // TODO: this bit of code is from TryCollisionAvoidance.csproj
        /// <summary>
        /// refresh Hull after collision avoidance
        /// </summary>
        /// <param name="element"></param>
        /// <param name="vector">update Segments after collision avoidance</param>
        private static void RefreshLayoutHulls(ElementStructBase element, System.Windows.Vector vector)
        {
            var scale = Zoom.GridRowHeightColumnSize;
            var pts = element.Hull.Segments.Select(x => x.End + vector).ToList();
            var newPath = Algorithm.RoundedCorners.CreatePath(pts, scale);
            var path = element.Element as System.Windows.Shapes.Path;
            path.Data = newPath.Data;
        }

        /// <summary>
        /// Find Element by Location
        /// </summary>
        /// <returns>The Element with the specified Location</returns>
        public ElementStructBase FindElement(LocationStruct original)
        {
            return elementStructs.FirstOrDefault(e => e.Tag == "note" && e.Original.Equals(original));
        }
        public void DebugWrite()
        {
            Debug.WriteLine($"Chord {Location}");
            foreach (var elem in elementStructs)
                elem.DebugWrite();
        }
        // to debug, uncomment AddBorder in LayoutNotes (very useful)
        public void AddBorder(LocationStruct p)
        {
            var border = new Border
            {
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(.5),
            };
            Grid.SetRow(border, p.Row);
            Grid.SetColumn(border, p.Column);
            Grid.SetRowSpan(border, p.RowSpan);
            Grid.SetColumnSpan(border, p.ColumnSpan);
            elementStructs.Add(new ElementStructBase(border, p, new PositionStruct(), "border"));
        }
    }
}
