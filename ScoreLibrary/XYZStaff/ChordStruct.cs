using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ScoreLibrary.Base;

namespace ScoreLibrary.XYZStaff
{
    internal class ChordStruct : ChordStructBase
    {
        public override LocationStruct Translate(Vector<int> position)
        {
            return Translate(position, XYZStaffLayout.Planar);
        }
        public override LocationStruct Translate(Vector<int> position, XYZStaffLayout xYZStaffLayout)
        {
            var x = position[0];
            var y = position[1];
            var z = position[2];
            if (xYZStaffLayout == XYZStaffLayout.Shifted)
            {
                return new LocationStruct(-(y + 4 * z), x, 2, 2);
            }
            else
            {
                return new LocationStruct(-y, x, 2, 2);
            }
        }
        public override LocationStruct Translate(Vector<int> position, XYZStaffLayout xYZStaffLayout, int rowSpan, int columnSpan)
        {
            var x = position[0];
            var y = position[1];
            var z = position[2];
            if (xYZStaffLayout == XYZStaffLayout.Shifted)
            {
                return new LocationStruct(-(y + 4 * z), x, rowSpan, columnSpan);
            }
            else
            {
                return new LocationStruct(-y, x, rowSpan, columnSpan);
            }
        }
        private readonly Brush[] FillColors = new Brush[]
        {
            Brushes.Purple,
            Brushes.Red,
            Brushes.Black,
            Brushes.Blue,
            Brushes.Green,
        };
        public override Brush MapColor(Vector<int> position, XYZStaffLayout xYZStaffLayout)
        {
            return xYZStaffLayout == XYZStaffLayout.Planar ?
                FillColors[(int)position[2] + 2] :
                Brushes.Black;
        }
    }
}
