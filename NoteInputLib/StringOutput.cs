//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using HarmonicAnalysisCommonLib;

//namespace NoteInputLib;
//public class StringOutput
//{
//    protected static readonly List<(Alter alter, string accidental)> AlterToAccidental = new List<(Alter alter, string accidental)> {
//            (Alter.None, string.Empty),
//            (Alter.DoubleFlat, "𝄫"),
//            (Alter.Flat, "♭"),
//            (Alter.Natural, "♮"),
//            (Alter.Sharp, "♯"),
//            (Alter.DoubleSharp, "𝄪"),
//        };
//    public static string ToneLiteToString(ToneLite toneLite, InputFormat outputFormat)
//    {
//        switch (outputFormat)
//        {
//            case InputFormat.NumberBase10:
//                return toneLite.PitchHeight.ToString();
//            case InputFormat.NumberBase12:
//                return toneLite.PitchHeight.Base10ToOctavePitchClassBase12();
//            case InputFormat.Scientific:
//                var accidental = AlterToAccidental.First(a => a.alter == toneLite.alter).accidental;
//                return $"{toneLite.step}{toneLite.octave}{accidental}";
//            default:
//                throw new NotImplementedException();
//        }
//    }
//}
