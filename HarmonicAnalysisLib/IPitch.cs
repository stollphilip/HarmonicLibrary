using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using HarmonicAnalysisCommonLib.Quarantine;

namespace HarmonicAnalysisLib
{
    public interface IPitch
    {
        public int PitchHeight { get; set; }
        public int FifthHeight { get; set; }
        public Vector<int> Position { get; set; }
        //public Pitch Offset(Vector<int> offset);
        public Accidental Accidental { get; set; }
    }
}
