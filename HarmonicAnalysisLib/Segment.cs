using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisLib
{
    /// <summary>
    /// A sequence of FrameStructs that alternates between Frame and VerticalFrame|HoriztonalFrame groups. Starts and ends with a Frame.
    /// </summary>
    public class Segment
    {
        // The basic unit of a progression is the Segement. A Segment represents a series of chord shapes and shape transformations.
        public List<FrameStruct> FrameStructs { get; set; } = new List<FrameStruct>();
        public SegmentDistanceAlgorithm Distance { get; set; }
        public Segment()
        {
            Distance = new SegmentDistanceAlgorithm(this);
        }
        public override string ToString()
        {
            return string.Join(", ", FrameStructs.Select(f =>
            {
                string chordIndexes = f.GroupMap.Frame.Type == GroupType.Frame ? $"{f.GroupMap.Frame.ChordIndex}" : string.Join(" ", f.GroupMap.Frame.Children.Select(child => child.Group.ChordIndex));
                string position = f.GroupMap.Frame.Type == GroupType.Frame ? $"({f.GroupMap.Frame.Position[1],-2} {f.GroupMap.Frame.Position[1],-2})" : $"({f.GroupMap.Frame.Children[0].Position[1],-2} {f.GroupMap.Frame.Children[0].Position[1],-2})({f.GroupMap.Frame.Children[1].Position[1],-2} {f.GroupMap.Frame.Children[1].Position[1],-2})";
                return $"{f.GroupMap.Type,-18} {chordIndexes,3} {position,14}";
            }));
        }
    }
}
