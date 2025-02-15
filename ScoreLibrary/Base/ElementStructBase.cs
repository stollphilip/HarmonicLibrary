using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace ScoreLibrary.Base
{
    // An element on a staff
    /// <summary>
    /// represents a FrameworkElement and its location
    /// </summary>
    /// <remarks>responsible for setting the element's location on the Grid</remarks>
    public class ElementStructBase
    {

        public FrameworkElement Element { get; set; }
        /// <summary>
        /// The location before collision avoidance of the Element on the Grid with respect to its parent Chord
        /// </summary>
        public LocationStruct Original { get; }
        /// <summary>
        /// The location after collision avoidance of the Element on the Grid with respect to its parent Chord
        /// </summary>
        public LocationStruct Location { get; set; }
        // The pitch position
        public PositionStruct Position
        {
            get;
        }
        public StepStruct StepToRow
        {
            get => Position.StepStruct;
        }
        #region fields used by PitchHeightStaff
        // staff is calculated from the diatonic degree (StepStruct.row)
        // currently, HarmonicAnalysis leaves the staff at default in the MusicXML score, so don't rely on it.
        /// <summary>
        /// 0 - upper staff, 1 - lower staff
        /// </summary>
        /// <remarks>used only by PitchHeightStaff</remarks>
        public int Staff
        {
            get; set;
        }
        #endregion

        #region fields used by Hull
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>used only by Hull</remarks>
        public HullStructBase Hull
        {
            get; set;
        }
        /// <summary>
        /// Return the Hull Location, which is also the element Location
        /// </summary>
        /// <remarks>The only time this is needed is when Segments from different elements are compared.</remarks>
        public System.Windows.Vector HullLocation()
        {
            return new Vector(Location.Column * Zoom.GridRowHeightColumnSize, Location.Row * Zoom.GridRowHeightColumnSize);
        }
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <remarks>used only by Hull</remarks>
        //public BorderData BorderData
        //{
        //    get; set;
        //}
        #endregion

        public Visibility Visibility
        {
            get => Element.Visibility;
        }
        public bool IsLaidOut { get; set; }
        // TODO: use enum instead
        // "note", "alter", "clef", "line", "border", etc...
        public string Tag { get; set; }

        public ElementStructBase(FrameworkElement element, LocationStruct originalLocation, PositionStruct positionStruct, string tag, int staff = 0, bool hiddden = false)
        {
            Element = element;
            Original = new LocationStruct(originalLocation);
            Location = new LocationStruct(originalLocation);
            Position = positionStruct;
            Staff = staff;
            Tag = tag;
            // Tag: ChordWithoutNotes - do not remove this comment
            if (hiddden)
            {
                Element.Visibility = Visibility.Hidden;
            }
        }
        #region public interface
        public virtual void SetGrid(int row, int column, int rowSpan, int columnSpan)
        {
            var location = new LocationStruct(Location.Row + row, Location.Column + column, Location.RowSpan, Location.ColumnSpan);
            if (Grid.GetRow(Element) != location.Row)
                Grid.SetRow(Element, location.Row);
            if (Grid.GetColumn(Element) != location.Column)
                Grid.SetColumn(Element, location.Column);
            if (Grid.GetRowSpan(Element) != location.RowSpan)
                Grid.SetRowSpan(Element, location.RowSpan);
            if (Grid.GetColumnSpan(Element) != location.ColumnSpan)
                Grid.SetColumnSpan(Element, location.ColumnSpan);
        }
        #endregion
        public override string ToString()
        {
            return $"{Element.Tag} {Location.Row} {Location.Column} {Location.RowSpan} {Location.ColumnSpan}";
        }

        public void Reset()
        {
            IsLaidOut = false;
            Location = new LocationStruct(Original.Row, Original.Column, Original.RowSpan, Original.ColumnSpan);
        }
        public void DebugWrite()
        {
            Debug.WriteLine($"Eleme {Location}");
        }
    }
}
