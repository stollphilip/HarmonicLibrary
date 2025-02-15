using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisLib
{
    public enum GroupType
    {
        Tone,
        Group,
        Frame,
        VerticalGroup,
        /// <summary>
        /// two child Frames point to the same chord and whose positions are different by 1
        /// </summary>
        VerticalFrame,
        HorizontalGroup,
        /// <summary>
        /// two child Frames point to different chords and whose positions are different by 1 or 0
        /// </summary>
        HorizontalFrame,
        Null,
    }
}
