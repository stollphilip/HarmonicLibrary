using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreLibrary
{
    /// <summary>
    /// Location on System.Windows.Controls.Grid
    /// </summary>
    public class LocationStruct : IEquatable<LocationStruct>
    {
        public int Row
        {
            get; set;
        }
        public int Column
        {
            get; set;
        }
        public int RowSpan
        {
            get; set;
        }
        public int ColumnSpan
        {
            get; set;
        }
        public LocationStruct(LocationStruct locationStruct) : this(locationStruct.Row, locationStruct.Column, locationStruct.RowSpan, locationStruct.ColumnSpan)
        {
        }
        public LocationStruct(int row, int column, int rowSpan, int columnSpan)
        {
            Row = row;
            Column = column;
            RowSpan = rowSpan;
            ColumnSpan = columnSpan;
        }
        /// <summary>
        /// Determine if two Locations intersect
        /// </summary>
        public static bool IsIntersect(LocationStruct a, LocationStruct b)
        {
            // from https://www.geeksforgeeks.org/find-two-rectangles-overlap/ note: the code is wrong
            var l1 = (y: a.Row, x: a.Column);
            var l2 = (y: b.Row, x: b.Column);
            var r1 = (y: a.Row + a.RowSpan, x: a.Column + a.ColumnSpan);
            var r2 = (y: b.Row + b.RowSpan, x: b.Column + b.ColumnSpan);
            if (l1.x >= r2.x || l2.x >= r1.x)
                return false;

            if (l1.y >= r2.y || l2.y >= r1.y)
                return false;

            return true;
        }
        public override string ToString() => $"{Row,2} {Column,2} {RowSpan,2} {ColumnSpan,2}";

        #region IEquatable<LocationStruct>
        public bool Equals(LocationStruct other) => other.Row == Row && other.Column == Column && other.RowSpan == RowSpan && other.ColumnSpan == ColumnSpan;
        #endregion
    }
}
