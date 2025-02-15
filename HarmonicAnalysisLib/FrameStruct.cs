using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisLib
{
    /// <summary>
    /// Stores the array indexes of the FrameStruct. Wraps a GroupMap containing a Frame.
    /// <br></br><br></br>
    /// [ChordIndex][VerticalOrHorizontal][DeltaIndex][z][y]
    /// </summary>
    /// <remarks>FrameStruct array provides a simple, unified navigation of chord and chord pair shapes.</remarks>
    public class FrameStruct
    {
        // The int properties are used to index into the FrameStructs array.
        // Do not change the order of the int properties.
        public int ChordIndex { get; set; }
        /// <summary>
        /// 0 = vertical, 1 = horizontal
        /// </summary>
        public int VerticalOrHorizontal { get; set; }
        /// <summary>
        /// Index into FrameStructLib.Deltas. The group is a Frame when DeltaIndex is 0. The group is a VerticalFrame or HorizontalFrame when DeltaIndex is not 0.
        /// </summary>
        public int DeltaIndex { get; set; }
        public int z { get; set; }
        public int y { get; set; }
        /// <summary>
        /// nullable GroupMap, is non-null for viable FrameStructs
        /// </summary>
        public GroupMap? GroupMap { get; set; } = new GroupMap();
        public override string ToString()
        {
            string positions = string.Empty;
            string distance = string.Empty;
            string type = string.Empty;
            if (GroupMap != null)
            {
                var frame = GroupMap.Frame;
                if (frame.Type == GroupType.Frame)
                {
                    positions = $"{VectorConverter.DebugFormatVector(frame.Position)} : {VectorConverter.DebugFormatVector(frame.Position)}";
                }
                else
                {
                    positions = string.Join(" : ", frame.Children.Select(child => VectorConverter.DebugFormatVector(child.Position)));
                }
                distance = $"{frame.Distance.CompoundWeighted:0.}";
                switch (GroupMap.Type)
                {
                    case GroupMapType.GroupMap:
                        type = "Group";
                        break;
                    case GroupMapType.VerticalGroupMap:
                        type = "Vertical";
                        break;
                    case GroupMapType.HorizontalGroupMap:
                        type = "Horizontal";
                        break;
                }
                return $"chord {ChordIndex,1}  vert {VerticalOrHorizontal,1}  delta {DeltaIndex,1}  z {z,1}  y {y,1} {type,-10} : {positions} : {distance}";
            }
            return $"chord {ChordIndex,1}  vert {VerticalOrHorizontal,1}  delta {DeltaIndex,1}  z {z,1}  y {y,1}";
        }
    }
}
