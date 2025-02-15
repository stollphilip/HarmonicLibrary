using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicXMLLib.Quarantine;

namespace MusicXMLLib.Lite;
public class DistanceAlgorithmLite
{
    public double Pairwise
    {
        get;
    }
    public double Average
    {
        get;
    }
    public double Weighted
    {
        get;
    }
    public double CompoundPairwise
    {
        get;
    }
    public double CompoundAverage
    {
        get;
    }
    public double CompoundWeighted
    {
        get;
    }
    public DistanceAlgorithmLite(double pairwise, double average, double weighted, double compoundPairwise, double compoundAverage, double compoundWeighted)
    {
        Pairwise = pairwise;
        Average = average;
        Weighted = weighted;
        CompoundPairwise = compoundPairwise;
        CompoundAverage = compoundAverage;
        CompoundWeighted = compoundWeighted;
    }

    public double DistanceTypeToDouble(DistanceType type)
    {
        switch (type)
        {
            case DistanceType.Pairwise:
                return Pairwise;
            case DistanceType.Average:
                return Average;
            case DistanceType.Weighted:
                return Weighted;
            case DistanceType.CompoundPairwise:
                return CompoundPairwise;
            case DistanceType.CompoundAverage:
                return CompoundAverage;
            case DistanceType.CompoundWeighted:
                return CompoundWeighted;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
    }

}
