using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonicAnalysisCommonLib;

namespace ScoreLibrary
{
    /// <summary>
    /// Diatonic pitch position on PitchHeightStaff
    /// </summary>
    public class StepStruct
    {
        /// <summary>
        /// C4
        /// </summary>
        public string step { get; set; }
        /// <summary>
        /// diatonic degree
        /// </summary>
        public int row { get; set; }
        /// <summary>
        /// -1 = flat 0 = natural 1 = sharp
        /// </summary>
        public int alter { get; set; }

        public StepStruct(ToneLite pitch)
        {
            var s = $"{pitch.step}{pitch.octave}";
            var ss = StepToRow.SingleOrDefault(st => st.step == s);
            step = ss.step;
            row = ss.row;
            alter = (int)pitch.alter;
        }
        public StepStruct(string step, int row, int alter = 0)
        {
            this.step = step;
            this.row = row;
            this.alter = alter;
        }
        public StepStruct()
        {

        }
        public static List<StepStruct> StepToRow = new List<StepStruct>
        {
        new StepStruct("C0", 46),
        new StepStruct("D0", 45),
        new StepStruct("E0", 44),
        new StepStruct("F0", 43),
        new StepStruct("G0", 42),
        new StepStruct("A0", 41),
        new StepStruct("B0", 40),

        new StepStruct("C1", 39),
        new StepStruct("D1", 38),
        new StepStruct("E1", 37),
        new StepStruct("F1", 36),
        new StepStruct("G1", 35),
        new StepStruct("A1", 34),
        new StepStruct("B1", 33),

        new StepStruct("C2", 32),
        new StepStruct("D2", 31),
        new StepStruct("E2", 30),
        new StepStruct("F2", 29),
        new StepStruct("G2", 28),
        new StepStruct("A2", 27),
        new StepStruct("B2", 26),

        new StepStruct("C3", 25),
        new StepStruct("D3", 24),
        new StepStruct("E3", 23),
        new StepStruct("F3", 22),
        new StepStruct("G3", 21),
        new StepStruct("A3", 20),
        new StepStruct("B3", 19),

        new StepStruct("C4", 18),
        new StepStruct("D4", 17),
        new StepStruct("E4", 16),
        new StepStruct("F4", 15),
        new StepStruct("G4", 14),
        new StepStruct("A4", 13),
        new StepStruct("B4", 12),

        new StepStruct("C5", 11),
        new StepStruct("D5", 10),
        new StepStruct("E5", 9),
        new StepStruct("F5", 8),
        new StepStruct("G5", 7),
        new StepStruct("A5", 6),
        new StepStruct("B5", 5),

        new StepStruct("C6", 4),
        new StepStruct("D6", 3),
        new StepStruct("E6", 2),
        new StepStruct("F6", 1),
        new StepStruct("G6", 0),
        };
        public static readonly int TurningPoint = 18;
    }
}
