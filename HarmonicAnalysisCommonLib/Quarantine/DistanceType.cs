using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisCommonLib.Quarantine;
public enum DistanceType
{
    // TODO: HarmonicAnalysisCommonLib.Quarantine.DistanceType has None. HarmonicAnalysisLib.DistanceType does not.
    None,
    Pairwise,
    Average,
    Weighted,
    CompoundPairwise,
    CompoundAverage,
    CompoundWeighted,
}