using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using HarmonicAnalysisCommonLib.Quarantine;

namespace HarmonicAnalysisLib
{
    public class Pitch : IPitch
    {
        public int PitchHeight { get; set; }
        public int FifthHeight { get; set; }
        public Vector<int> Position { get; set; }
        public Accidental Accidental { get; set; }
        public Pitch(int pitchHeight, int fifthHeight, Accidental accidental = Accidental.None)
        {
            PitchHeight = pitchHeight;
            FifthHeight = fifthHeight;
            Position = VectorConverter.HeightToNormalPosition(pitchHeight);
            Accidental = accidental;
        }
        public Pitch(int pitchHeight, int fifthHeight, Vector<int> position, Accidental accidental = Accidental.None)
        {
            PitchHeight = pitchHeight;
            FifthHeight = fifthHeight;
            Position = position;
            Accidental = accidental;
        }
        //public Pitch Offset(Vector<int> offset)
        //{
        //    return new Pitch(PitchHeight, FifthHeight, Position + offset);
        //}
    }
}
