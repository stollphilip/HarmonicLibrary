using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonicAnalysisCommonLib.Quarantine;

namespace HarmonicAnalysisCommonLib;
    public interface IDistanceAlgorithmLite
    {
        public double Pairwise { get; }
        public double Average { get; }
        public double Weighted { get; }
        public double CompoundPairwise { get; }
        public double CompoundAverage { get; }
        public double CompoundWeighted { get; }
        public double DistanceTypeToDouble(DistanceType type);
    }
