using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonicAnalysisCommonLib.Quarantine;

namespace NoteInputLib
{
    public class NoteInput
    {
        public int Height
        {
            get; set;
        }
        // use enum instead of int to distinguish Natural from None
        public Accidental Accidental
        {
            get; set;
        }
        public override bool Equals(object obj)
        {
            var other = (NoteInput)obj;
            return Height == other.Height && Accidental == other.Accidental;
        }
        public override string ToString()
        {
            var str = VectorConverter.Base10To12(Height);
            switch (Accidental)
            {
                case Accidental.None:
                    break;
                case Accidental.Natural:
                    str += "n";//"♮"
                    break;
                case Accidental.Sharp:
                    str += "s";//"♯"
                    break;
                case Accidental.Flat:
                    str += "f";//"♭"
                    break;
                case Accidental.DoubleSharp:
                    str += "ss";// 𝄪
                    break;
                case Accidental.DoubleFlat:
                    str += "ff";// 𝄫
                    break;
            }
            return str;
        }

        #region MusicXML convenience methods
        public Step GetStep()
        {
            int height;
            if (Accidental == Accidental.None)
            {
                height = Height;
            }
            else
            {
                height = Height - this.Accidental.AccidentalToAlter();
            }
            switch (ExtensionMethods.ModulusOperator(height, 12))
            {
                case 0:
                    return Step.C;
                case 2:
                    return Step.D;
                case 4:
                    return Step.E;
                case 5:
                    return Step.F;
                case 7:
                    return Step.G;
                case 9:
                    return Step.A;
                case 11:
                    return Step.B;
                default:
                    return Step.Invalid;
            }
        }

        public int GetOctave()
        {
            int height;
            if (Accidental != Accidental.None)
            {
                height = Height;
            }
            else
            {
                height = Height - this.Accidental.AccidentalToAlter();
            }
            return ExtensionMethods.Quotient(height, 12);
        }
        #endregion

        #region Input methods

        /// <summary>
        /// Get the default Accidental for when it is not specified or the wrong one is specified
        /// </summary>
        public Accidental DefaultAccidental()
        {
            switch (ExtensionMethods.ModulusOperator(Height, 12))
            {
                case 0:
                    return Accidental.None; ;
                case 1:
                    return Accidental.Sharp;
                case 2:
                    return Accidental.None; ;
                case 3:
                    return Accidental.Flat;
                case 4:
                case 5:
                    return Accidental.None; ;
                case 6:
                    return Accidental.Sharp;
                case 7:
                    return Accidental.None; ;
                case 8:
                    return Accidental.Flat;
                case 9:
                    return Accidental.None; ;
                case 10:
                    return Accidental.Flat;
                case 11:
                    return Accidental.None;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// convenience method for HarmonicAnalysisLib which requires the fifthHeight
        /// </summary>
        public int FifthHeight()
        {
            // from TryModulo.csproj
            // this is kludgy, but it works
            var fifthHeight = Enumerable.Range(0, 12)
                .Single(x => ExtensionMethods.ModulusOperator(19 * x, 12) == ExtensionMethods.ModulusOperator(Height, 12));

            var octave = 0;
            switch (Accidental)
            {
                case Accidental.None:
                case Accidental.Natural:
                    if (fifthHeight == 11)
                        octave = -1;
                    break;
                case Accidental.DoubleFlat:
                    if (fifthHeight <= 3)
                        octave = -1;
                    else if (fifthHeight >= 9)
                        octave = -2;
                    break;
                case Accidental.Flat:
                    octave = -1;
                    break;
                case Accidental.Sharp:
                    if (fifthHeight == 0)
                        octave = 1;
                    break;
                case Accidental.DoubleSharp:
                    octave = 2;
                    break;
            }
            return fifthHeight + 12 * octave;
        }
        #endregion
    }

    public class ChordInput
    {
        public MajorKey MajorKey { get; set; } = MajorKey.None;
        public List<NoteInput> Notes { get; set; } = new List<NoteInput>();

        public override string ToString()
        {
            return string.Join(" ", Notes.Select(n => n/*.Height*/.ToString()));
        }

        public bool IsEqual(ChordInput other)
        {
            return MajorKey == other.MajorKey && Notes.SequenceEqual(other.Notes);
        }

        public ChordInput Clone(int displacement)
        {
            return new ChordInput
            {
                MajorKey = MajorKey,
                Notes = Notes.Select(n => new NoteInput { Accidental = n.Accidental, Height = n.Height + displacement }).ToList()
            };
        }
        public List<double> ToList() => Notes.Select(n => (double)n.Height).ToList();
    }

    public class NoteList
    {
        public List<ChordInput> Chords { get; set; } = new List<ChordInput>();
    }

    public enum MajorKey
    {
        None = -1,
        C = 0,
        G = 1,
        D = 2,
        A = 3,
        E = 4,
        B = 5,
        Fsharp = 6,
        Dflat = 7,
        Aflat = 8,
        Eflat = 9,
        Bflat = 10,
        F = 11,
    }
    public enum Step
    {
        Invalid = -1,
        C = 0,
        D = 2,
        E = 4,
        F = 5,
        G = 7,
        A = 9,
        B = 11,
    }
}
