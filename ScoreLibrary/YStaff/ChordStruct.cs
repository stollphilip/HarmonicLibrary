using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ScoreLibrary.Base;

namespace ScoreLibrary.YStaff
{
    internal class ChordStruct : ChordStructBase
    {
        public override LocationStruct Translate(Vector<int> position)
        {
            var x = position[0];
            var y = position[1];
            var z = position[2];
            return new LocationStruct(-(y + 4 * z), 0, 2, 2);
        }
        public virtual LocationStruct Translate(Vector<int> position, int rowSpan, int columnSpan)
        {
            var x = position[0];
            var y = position[1];
            var z = position[2];
            return new LocationStruct(-(y + 4 * z), 0, rowSpan, columnSpan);
        }
        public override void Scale(ref LocationStruct location)
        {
            const int rowScaleFactor = 1;
            const int columnScaleFactor = 2;
            location.Row *= rowScaleFactor;
            location.Column *= columnScaleFactor;
        }
    }
}
