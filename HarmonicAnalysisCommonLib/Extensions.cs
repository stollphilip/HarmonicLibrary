using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisCommonLib;
public static class Extensions
{
    ///<summary> ModulusOperator always returns a number between 0 and modulus-1 </summary>
    public static int ModulusOperator(int arg, int modulus)
    {
        return (arg < 0) ? ((arg % modulus) + modulus) % modulus : arg % modulus;
    }

    ///<summary> Quotient(*, 2) maps the series {-4, -3, -2, -1, 0, 1, 2, 3, 4} to {-2, -2, -1, -1, 0, 0, 1, 1, 2} </summary>
    public static int Quotient(int arg, int modulus)
    {
        return (arg < 0) ? (arg - modulus + 1) / modulus : arg / modulus;
    }
    public static string Base10To12(int value)
    {
        // input  -13 -12 -11 -10 -9 -8 -7 -6 -5 -4 -3 -2 -1 0 1 2 3 4 5 6 7 8 9 10 11 12 13
        // output -11 -10  -B  -A -9 -8 -7 -6 -5 -4 -3 -2 -1 0 1 2 3 4 5 6 7 8 9  A  B 10 11
        return (value < 0 ? "-" : string.Empty) +
            (Math.Abs(value % 12) + 16 * Math.Abs(value / 12)).ToString("X1");
    }
    public static string Base10ToOctavePitchClassBase12(this int height)
    {
        // input   -13 -12 -11 -10  -9  -8  -7  -6  -5  -4  -3  -2  -1 0 1 2 3 4 5 6 7 8 9 10 11 12 13
        // output  -2B -10 -11 -12 -13 -14 -15 -16 -17 -18 -19 -1A -1B 0 1 2 3 4 5 6 7 8 9  A  B 10 11
        int pitchClass = ModulusOperator(height, 12);
        int octave = (height - pitchClass) / 12;
        return octave == 0 ?
            $"{Base10To12(pitchClass)}" :
            $"{octave}{Base10To12(pitchClass)}";
    }
}
