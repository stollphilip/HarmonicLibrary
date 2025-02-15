using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisCommonLib.Quarantine;
// To avoid adding System.Drawing.Common NuGet package, I copied ContentAlignment enum here
// I also added None value to the enum
public enum ContentAlignment
{
    //None = 0x000,
    TopLeft = 0x001,
    TopCenter = 0x002,
    TopRight = 0x004,
    MiddleLeft = 0x010,
    MiddleCenter = 0x020,
    MiddleRight = 0x040,
    BottomLeft = 0x100,
    BottomCenter = 0x200,
    BottomRight = 0x400,
}
