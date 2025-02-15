using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisCommonLib.Quarantine;
public enum Accidental
{
    // use ToInt() extension method to convert to int
    None = 3,
    DoubleFlat = -2,
    Flat = -1,
    Natural = 0,
    Sharp = 1,
    DoubleSharp = 2,
}
