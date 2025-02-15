using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using HarmonicAnalysisCommonLib.Quarantine;

namespace NoteInputLib
{
    public static class ExtensionMethods
    {
        public static int[] StringToInts(this string str)
        {
            return str.Split().Select(s => int.Parse(s)).ToArray();
        }

        /////<summary> Returns an integer pitch height </summary>
        //public static int ToHeight(this Point3D point) => ((int)point.X) * 12 + ((int)point.Y) * 19 + ((int)point.Z) * 28;

        /////<summary> Returns a fifth height between 0 and 11 </summary>
        //public static int ToFifthHeight(this Point3D point) => ((int)point.Y) + ((int)point.Z) * 4;

        /////<summary> Returns a fifth height between 0 and 11 </summary>
        //public static int ToFifthHeightModulo(this Point3D point) => ModulusOperator(((int)point.Y) + ((int)point.Z) * 4, 12);

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

        public static int Base12To10(this string base12)
        {
            int sign;
            if (base12.StartsWith("-"))
            {
                base12 = base12.Substring(1);
                sign = -1;
            }
            else
            {
                sign = 1;
            }
            int base16 = int.Parse(base12, System.Globalization.NumberStyles.HexNumber);
            return sign * (base16 % 16 + 12 * (base16 / 16));
        }

        public static string Base10To12(int value)
        {
            // input  -13 -12 -11 -10 -9 -8 -7 -6 -5 -4 -3 -2 -1 0 1 2 3 4 5 6 7 8 9 10 11 12 13
            // output -11 -10  -B  -A -9 -8 -7 -6 -5 -4 -3 -2 -1 0 1 2 3 4 5 6 7 8 9  A  B 10 11
            return (value < 0 ? "-" : string.Empty) +
                (Math.Abs(value % 12) + 16 * Math.Abs(value / 12)).ToString("X1");
        }

        //public static Point3D FifthHeightToPoint3D(this int fifthHeight, Point3D minPoint)
        //{
        //    for (int y = (int)minPoint.Y; y < (int)minPoint.Y + 4; y++)
        //    {
        //        for (int z = (int)minPoint.Z; z < (int)minPoint.Z + 3; z++)
        //        {
        //            if (ModulusOperator(y + z * 4, 12) == ModulusOperator(fifthHeight, 12))
        //            {
        //                return new Point3D(0, y, z);
        //            }
        //        }
        //    }
        //    throw new Exception("Programming error.");
        //}
        //public static int ArrayToMask(int[] chordFHs, int fundamentalFH)
        //{
        //    int mask = 0;
        //    for (int fh = 0; fh < 8; fh++)
        //    {
        //        if (chordFHs.Count(c => c == fh + fundamentalFH) != 0)
        //        {
        //            mask |= (1 << fh);
        //        }
        //    }
        //    return mask;
        //}
        //public static int[] MaskToArray(int mask, int fundamentalFH)
        //{
        //    var ints = new List<int>();
        //    for (int bit = 0; bit < 8; bit++)
        //    {
        //        if ((mask & (1 << bit)) != 0)
        //        {
        //            ints.Add(fundamentalFH + bit);
        //        }
        //    }
        //    return ints.ToArray();
        //}
        //public static int TonesToMask(Tone[] tones, int fundamentalFH)
        //{
        //    int mask = 0;
        //    for (int fh = 0; fh < 8; fh++)
        //    {
        //        if (tones.Count(c => (int)c.Vect.Y == fh + fundamentalFH) != 0)
        //        {
        //            mask |= (1 << fh);
        //        }
        //    }
        //    return mask;
        //}
        //public static Tone[] MaskToTones(Tone[] tones, int mask, int fundamentalFH)
        //{
        //    var list = new List<Tone>();
        //    for (int bit = 0; bit < 8; bit++)
        //    {
        //        if ((mask & (1 << bit)) != 0)
        //        {
        //            list.AddRange(tones.Where(t => (int)t.Vect.Y == fundamentalFH + bit));
        //        }
        //    }
        //    return list.ToArray();
        //}
        public static int CountBits(int value)
        {
            int count = 0;
            while (value != 0)
            {
                if ((value & 1) != 0)
                    count++;
                value >>= 1;
            }
            return count;
        }
        ///<summary> Returns an integer pitch height </summary>
        public static int ToHeight(this Vector3 point) => ((int)point.X) * 12 + ((int)point.Y) * 19 + ((int)point.Z) * 28;
        //public static Vector3 XYtoXYZ(this Vector3 vector)
        //{
        //    Debug.Assert(vector.Z == 0);
        //    int z = Quotient((int)vector.Y, 4);
        //    var xyz = new Vector3(4 * z + vector.X, -4 * z + vector.Y, z);

        //    return xyz;
        //}
        public static Vector3 XYZtoXY(this Vector3 vector)
        {
            return new Vector3(4 * vector.Z + vector.X, -4 * vector.Z + vector.Y, 0);
        }
        public static Vector3 Fundamental(this Vector3[] tones)
        {
            if (tones.Length == 0) throw new Exception();
            return new Vector3((int)tones.Min(t => t.X), (int)tones.Min(t => t.Y), (int)tones.Min(t => t.Z));
        }
        public static string ToZYX(this Vector3 vector3D)
        {
            return $"{vector3D.Z}{vector3D.Y}{vector3D.X}";
        }
        /// <summary>
        /// Convert pitch height to 1/3 octave band and modulus
        /// </summary>
        /// <param name="height">pitch height</param>
        /// <param name="bandReferenceHeight">center pitch height of band number 0</param>
        /// <param name="band">1/3 octave band</param>
        /// <param name="modulus">modulus</param>
        /// <param name="octaveOffset">add octave offet to pitch height to normalize the pitch height to band number 0, 1, 2, 3</param>
        public static void HeightToBand(int height, int bandReferenceHeight, out int band, out int modulus, out int octaveOffset)
        {
            band = Quotient(height - bandReferenceHeight + 1, 3);
            modulus = ModulusOperator(height - bandReferenceHeight + 1, 3);
            octaveOffset = Quotient(band, 4);
        }
        /// <summary>
        /// Convert 1/3 octave band and modulusto pitch height
        /// </summary>
        /// <param name="band">1/3 octave band</param>
        /// <param name="bandReferenceHeight">center pitch height of band number 0</param>
        /// <param name="modulus">modulus</param>
        /// <returns>pitch height</returns>
        public static int BandToHeight(int band, int bandReferenceHeight, int modulus)
        {
            return 3 * band + modulus + bandReferenceHeight - 1;
        }
        /// <summary>
        /// Convert int height to octave pitch-class format.
        /// Format height as a signed octave digit followed by a base-12 digit.
        /// Omit octave digit when octave is 0.
        /// </summary>
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

        /// <summary>
        /// Convert a base-12 number to a base-10 number.
        /// </summary>
        /// <remarks>The sign is applied to octave digit only. The sign is ignored if octave digit is not present.</remarks>
        public static int Base12To10_new(this string base12)
        {
            // TODO: OctavePitchClassBase12To10 does the same thing
            int sign;
            if (base12.StartsWith("-"))
            {
                base12 = base12.Substring(1);
                sign = -1;
            }
            else
            {
                sign = 1;
            }
            int octave;
            if (base12.Length > 1)
            {
                octave = int.Parse(base12.Substring(0, 1));
                base12 = base12.Substring(1);
            }
            else
            {
                octave = 0;
            }
            int base16 = int.Parse(base12, System.Globalization.NumberStyles.HexNumber);
            return sign * octave * 12 + (base16 % 16 + 12 * (base16 / 16));
        }

        /// <summary>
        /// Convert octave pitch-class format to int height
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static int OctavePitchClassBase12To10(this string height)
        {
            // input   -2B -10 -11 -12 -13 -14 -15 -16 -17 -18 -19 -1A -1B 0 1 2 3 4 5 6 7 8 9  A  B 10 11
            // output  -13 -12 -11 -10  -9  -8  -7  -6  -5  -4  -3  -2  -1 0 1 2 3 4 5 6 7 8 9 10 11 12 13
            int digits = height.Count(c => char.IsDigit(c) || char.IsAsciiLetter(c));
            int signs = height.Count(c => c == '-');
            if (digits == 0 || digits == 1 && signs != 0)
            {
                throw new ArgumentException("signed expressions must have an octave digit", nameof(height));
            }
            int octave;
            int pitchClass;
            if (digits == 1)
            {
                octave = 0;
                pitchClass = height.Base12To10();
            }
            else
            {
                octave = int.Parse(height.Substring(0, height.Length - 1));
                pitchClass = height.Substring(height.Length - 1).Base12To10();
            }
            return 12 * octave + pitchClass;
        }
        public static string Base10To12Signed(int value)
        {
            // input  -13 -12 -11 -10  -9  -8  -7  -6  -5  -4  -3  -2 -1 0 1 2 3 4 5 6 7 8 9 10 11 12 13
            // output -2B -10 -11 -12 -13 -14 -15 -16 -17 -18 -19 -1A  B 0 1 2 3 4 5 6 7 8 9  A  B 10 11
            return (value < 0 ? "-" : string.Empty) +
                (Math.Abs(value % 12) + 16 * Math.Abs(value / 12)).ToString("X1");
        }
        public static string FifthHeightToAccidental(int fifthHeight)
        {
            string accidental;
            if (fifthHeight > 12)
                accidental = "ss"; // 𝄪
            else if (fifthHeight > 5)
                accidental = "s"; // ♯
            else if (fifthHeight > -2)
                accidental = string.Empty;
            else if (fifthHeight > -9)
                accidental = "f"; // ♭
            else if (fifthHeight > -16)
                accidental = "ff"; // 𝄫
            else throw new InvalidOperationException();
            return accidental;
        }
        public static int AccidentalToAlter(this Accidental accidental)
        {
            int val = 0;
            switch (accidental)
            {
                case Accidental.DoubleFlat:
                    val = -2; break;
                case Accidental.Flat:
                    val = -1; break;
                case Accidental.Natural:
                case Accidental.None:
                    val = 0; break;
                case Accidental.Sharp:
                    val = 1; break;
                case Accidental.DoubleSharp:
                    val = 2; break;
                default:
                    break;
            }
            return val;
        }
        public static Step NoteToStep(this string note)
        {
            switch (note.ToLower())
            {
                case "c":
                    return Step.C;
                case "d":
                    return Step.D;
                case "e":
                    return Step.E;
                case "f":
                    return Step.F;
                case "g":
                    return Step.G;
                case "a":
                    return Step.A;
                case "b":
                    return Step.B;
                default:
                    return Step.Invalid;
            }
        }

    }
}
