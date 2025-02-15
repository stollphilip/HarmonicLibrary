using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ScoreLibrary.Base;

namespace ScoreLibrary.YZStaff
{
    internal class ChordStruct : ChordStructBase
    {
        public override LocationStruct Translate(Vector<int> position)
        {
            var x = position[0];
            var y = position[1];
            var z = position[2];
            return new LocationStruct(-z, y, 2, 2);
        }
        public override LocationStruct Translate(Vector<int> position, int rowSpan, int columnSpan)
        {
            var x = position[0];
            var y = position[1];
            var z = position[2];
            return new LocationStruct(-z, y, rowSpan, columnSpan);
        }
    }
}
