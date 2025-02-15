using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisLib
{
    /// <summary>
    /// mapping from Tone to Group (to VerticalGroup or HorizontalGroup)
    /// </summary>
    public class ToneMap
    {
        // this class maps tones to HorizontalGroups
        // to optimize between chord connection each tone is mapped to the smallest distance HorizontalGroups
        public Group Tone { get; set; }
        public Group? Group { get; set; }
        // Tone and Group are also stored in Child TonePosition and GroupPosition, and can be removed
        public Child TonePosition { get; set; }
        // currently not used
        public Child? GroupPosition { get; set; }
        //public ToneMap? Alias { get; set; }
        // indexes of the child Frames that contain the Tone Position
        public List<int> ChildFrameIndexes { get; set; }

        /// <summary>
        /// Find which child Frames contains the Tone Position
        /// </summary>
        /// <remarks>This is how Aliases are detected.<br></br>
        /// For VerticalFrame, if a Tone is in one child Frame, but not in the other, it has an Alias.<br></br>
        /// For HorizontalFrame if a Tone is in one child Frame, but not in the other, it has something like an Alias.</remarks>
        /// <param name="frame">Frame, VerticalFrame or HorizontalFrame</param>
        public List<int> GetChildFrames(Group frame)
        {
            // this method has been verified
            var indexes = new List<int>();
            Vector<int> lower, upper, lowerNext, upperNext;
            switch (frame.Type)
            {
                case GroupType.Frame:
                    break;
                case GroupType.VerticalFrame:
                case GroupType.HorizontalFrame:
                    frame.FrameToBounds(out lower, out upper, out lowerNext, out upperNext);
                    for (var i = 0; i < frame.Children.Count; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                if (TonePosition.ChildWithinBounds(lower, upper, Vector<int>.Zero, GroupType.Tone))
                                {
                                    indexes.Add(i);
                                }
                                break;
                            case 1:
                                if (TonePosition.ChildWithinBounds(lowerNext, upperNext, Vector<int>.Zero, GroupType.Tone))
                                {
                                    indexes.Add(i);
                                }
                                break;
                            default: throw new InvalidOperationException();
                        }
                    }
                    break;
                default:
                    return indexes;
            }
            return indexes;
        }
    }
}
