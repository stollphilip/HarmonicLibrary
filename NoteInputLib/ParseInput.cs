using System.Diagnostics;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using HarmonicAnalysisCommonLib.Quarantine;
using NoteInputLib;

namespace NoteInputLib;

// copied from TryGetInputFormat.csproj
public class ParseInput
{
    public static readonly string[] LineDelimiters = new string[] { "\r\n", "\n", ";" };
    public const string Comment = @"//";
    private const string patternBase10 = @"([\-]?)([0-9]?[0-9])[\-]?(df|doubleflat|flat|natural|sharp|ds|doublesharp|f|n|s|#|b|𝄫|♭|♮|♯|𝄪)?";
    // Note: patternBase12 is missing |b|
    private const string patternBase12 = @"([\-]?)([\(-)]?)([0-9]?[0-9ab])([\)]?)[\-]?(df|doubleflat|flat|natural|sharp|ds|doublesharp|f|n|s|#|𝄫|♭|♮|♯|𝄪)?";
    private const string patternScientific = @"([a-g])([\-]?)([0-9])[\-]?(df|doubleflat|flat|natural|sharp|ds|doublesharp|f|n|s|#|b|𝄫|♭|♮|♯|𝄪)?";
    static (NoteInput note, bool success) ParseBase10(string input)
    {
        input = input.ToLower();
        // TODO: use GeneratedRegexAttribute
        Match match = Regex.Match(input, patternBase10);
        if (match.Success)
        {
            string signstring = match.Groups.Count > 1 ? match.Groups[1].Value : string.Empty;
            string heightstring = match.Groups.Count > 2 ? match.Groups[2].Value : string.Empty;
            string accidentalstring = match.Groups.Count > 3 ? match.Groups[3].Value : string.Empty;
            int sign = signstring.Length == 0 ? 1 : -1;
            int height = sign * int.Parse(heightstring);
            Accidental accidental = GetAccidental(accidentalstring);
            var note = new NoteInput
            {
                Height = height,
                Accidental = accidental,
            };
            if (note.GetStep() == Step.Invalid)
            {
                // Step is invalid. User omitted the step or entered the wrong step.
                note.Accidental = note.DefaultAccidental();
            }
            Debug.WriteLine($"input {input,8} sign {sign,2} height {height,3} accidental {accidental}");
            return (note, true);
        }
        else
        {
            Debug.WriteLine($"Could not Parse {input}");
            return (new NoteInput(), false);
        }
    }

    static (NoteInput note, bool success) ParseBase12(string input)
    {
        input = input.ToLower();
        Match match = Regex.Match(input, patternBase12);
        if (match.Success)
        {
            string signstring = match.Groups.Count > 1 ? match.Groups[1].Value : string.Empty;
            string lparstring = match.Groups.Count > 2 ? match.Groups[2].Value : string.Empty;
            string heightstring = match.Groups.Count > 3 ? match.Groups[3].Value : string.Empty;
            string rparstring = match.Groups.Count > 4 ? match.Groups[4].Value : string.Empty;
            string accidentalstring = match.Groups.Count > 5 ? match.Groups[5].Value : string.Empty;
            int sign = signstring.Length == 0 ? 1 : -1;
            int height;
            if (lparstring.Length != 0 || rparstring.Length != 0)
            {
                // -(1A) or (1A)
                height = -heightstring.Base12To10_new();
            }
            else
            {
                // -1A
                height = (signstring + heightstring).Base12To10_new();
            }
            Accidental accidental = GetAccidental(accidentalstring);
            var note = new NoteInput
            {
                Height = height,
                Accidental = accidental,
            };
            if (note.GetStep() == Step.Invalid)
            {
                // Step is invalid. User omitted the step or entered the wrong step.
                note.Accidental = note.DefaultAccidental();
            }
            //Debug.WriteLine($"input {input,8} sign {sign,2} height {height,3} accidental {accidental}");
            return (note, true);
        }
        else
        {
            Debug.WriteLine($"Could not Parse {input}");
            return (new NoteInput(), false);
        }
    }
    static (NoteInput note, bool success) ParseScientific(string input)
    {
        input = input.ToLower();
        Match match = Regex.Match(input, patternScientific);
        if (match.Success)
        {
            string stepstring = match.Groups.Count > 1 ? match.Groups[1].Value : string.Empty;
            string signstring = match.Groups.Count > 2 ? match.Groups[2].Value : string.Empty;
            string octavestring = match.Groups.Count > 3 ? match.Groups[3].Value : string.Empty;
            string accidentalstring = match.Groups.Count > 4 ? match.Groups[4].Value : string.Empty;
            int step = (int)stepstring.NoteToStep();
            int sign = signstring.Length == 0 ? 1 : -1;
            int octave = int.Parse(octavestring);
            Accidental accidental = GetAccidental(accidentalstring);
            int alter = GetAccidental(accidentalstring).AccidentalToAlter();
            int height = step + sign * 12 * octave + alter;
            var note = new NoteInput
            {
                Height = height,
                Accidental = accidental,
            };
            if (note.GetStep() == Step.Invalid)
            {
                // Step is invalid. User omitted the step or entered the wrong step.
                note.Accidental = note.DefaultAccidental();
            }
            Debug.WriteLine($"input {input,8} sign {sign,2} height {height,3} accidental {accidental}");
            return (note, true);
        }
        else
        {
            Debug.WriteLine($"Could not Parse {input}");
            return (new NoteInput(), false);
        }
    }
    public static List<NoteList> Parse(IEnumerable<string> lines, ShowNoteAs inputFormat)
    {
        var list = new List<NoteList>();
        var noteList = new NoteList();
        list.Add(noteList);

        foreach (var line in lines)
        {
            var substrings = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).ToArray();
            if (substrings.Length == 0)
            {
                continue;
            }
            // comment line "//" starts a new NoteList
            if (line.Trim().Equals(Comment))
            {
                if (noteList.Chords.Any())
                {
                    list.Add(noteList);
                }
                noteList = new NoteList();
                continue;
            }
            // comment lines beginning with "//" are ignored
            if (line.Trim().StartsWith(Comment))
                continue;

            var chordInput = new ChordInput();
            MajorKey majorKey;
            if (GetMajorKey(substrings, out majorKey))
            {
                chordInput.MajorKey = majorKey;
                continue;
            }
            foreach (var substring in substrings)
            {
                (NoteInput note, bool success) = Parse(substring, inputFormat);
                if (success)
                {
                    chordInput.Notes.Add(note);
                }
                else
                {
                    Debug.WriteLine($"Could not Parse {substring}");
                }
            }
            noteList.Chords.Add(chordInput);
        }
        return list;
    }

    static (NoteInput note, bool success) Parse(string input, ShowNoteAs inputFormat)
    {
        switch (inputFormat)
        {
            case ShowNoteAs.NumberBase10:
                return ParseBase10(input);
            case ShowNoteAs.NumberBase12:
                return ParseBase12(input);
            case ShowNoteAs.Scientific:
                return ParseScientific(input);
            default:
                return (new NoteInput(), false);
        }
    }

    private static bool GetMajorKey(string[] substrings, out MajorKey majorKey)
    {
        majorKey = MajorKey.None;
        return substrings.Length > 1 && substrings[1].ToLower() == "major" && Enum.TryParse<MajorKey>(substrings[0], out majorKey);
    }
    // to specify the format, include one of the following lines in the input file or string:
    // // Scientific
    // // NumberBase10
    // // NumberBase12
    public static ShowNoteAs GetInputFormat(IEnumerable<string> lines, ShowNoteAs inputFormatDefault)
    {
        var scientific = lines.Count(l => l.StartsWith(Comment) && l.ToLower().Contains("Scientific".ToLower()));
        var numberBase10 = lines.Count(l => l.StartsWith(Comment) && l.ToLower().Contains("NumberBase10".ToLower()));
        var numberBase12 = lines.Count(l => l.StartsWith(Comment) && l.ToLower().Contains("NumberBase12".ToLower()));
        if (scientific + numberBase10 + numberBase12 == 1)
        {
            if (scientific != 0)
            {
                return ShowNoteAs.Scientific;
            }
            if (numberBase10 != 0)
            {
                return ShowNoteAs.NumberBase10;
            }
            if (numberBase12 != 0)
            {
                return ShowNoteAs.NumberBase12;
            }
        }

        //var tokens = new List<string>();
        //foreach (var line in lines.Select(l => l.Trim()).Where(l => l.Length != 0 && l.StartsWith(Comment)))
        //{
        //    var split = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        //    tokens.AddRange(split);
        //}
        return inputFormatDefault;
    }

    public static Accidental GetAccidental(string s)
    {
        Accidental accidental = Accidental.None;
        switch (s)
        {
            case "":
            case "none":
                accidental = Accidental.None;
                break;
            case "flat":
            case "f":
            case "♭":
            case "b":
                accidental = Accidental.Flat;
                break;

            case "natural":
            case "n":
            case "♮":
                accidental = Accidental.Natural;
                break;
            case "sharp":
            case "s":
            case "#":
            case "♯":
                accidental = Accidental.Sharp;
                break;
            case "doubleflat":
            case "df":
            case "𝄫":
                accidental = Accidental.DoubleFlat;
                break;
            case "doublesharp":
            case "ds":
            case "𝄪":
                accidental = Accidental.DoubleSharp;
                break;
            default:
                throw new ArgumentException();
        }
        return accidental;
    }
}
