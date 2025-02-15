using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace ScoreLibrary
{
    /// <summary>
    /// Pitch position
    /// </summary>
    public class PositionStruct
    {
        /// <summary>
        /// Diatonic position on PitchHeightStaff
        /// </summary>
        public StepStruct StepStruct { get; set; }
        /// <summary>
        /// Harmonic position
        /// </summary>
        public Vector<int> Position { get; set; } = new Vector<int>();
        public override string ToString() => $"{Position[0],2} {Position[1],2} {Position[2],2}";
    }
}
