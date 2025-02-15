using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonicAnalysisCommonLib.Quarantine;

namespace NoteInputLib;
public class StringInput
{
    /// <summary>
    /// Read List&lt;NoteList&gt; from string
    /// </summary>
    /// <param name="str">comma delimited string the format of which is defined in NoteInputLib</param>
    /// <param name="delimiter">delimiter character</param>
    public static List<NoteList> ReadStringInput(string str, char delimiter = ',')
    {
        var separator = new char[] { delimiter, };
        var lines = str.Split(separator, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).ToArray();
        ShowNoteAs inputFormat = ParseInput.GetInputFormat(lines, ShowNoteAs.NumberBase10);
        var noteList = ParseInput.Parse(lines, inputFormat);
        return noteList;
    }
    /// <summary>
    /// Read List&lt;NoteList&gt; from string
    /// </summary>
    /// <param name="str">NewLine delimited string the format of which is defined in NoteInputLib</param>
    /// <param name="delimiter">delimiter string - use Environment.NewLine</param>
    /// <returns></returns>
    public static List<NoteList> ReadStringInput(string str, string delimiter)
    {
        var separator = new string[] { delimiter, };
        var lines = str.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        ShowNoteAs inputFormat = ParseInput.GetInputFormat(lines, ShowNoteAs.NumberBase10);
        var noteList = ParseInput.Parse(lines, inputFormat);
        return noteList;
    }
}
