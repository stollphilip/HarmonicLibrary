using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisCommonLib.Quarantine;
// TODO: YAGNI
public enum DistanceFormat
{
    Decimal,
    // DistanceType.Pairwise must be set for Base12 to have any effect
    Base12,
    // DistanceType.Pairwise must be set for ZYX to have any effect
    ZYX,
}
