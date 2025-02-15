using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonicAnalysisCommonLib.Quarantine;

namespace NoteInputLib;
public class FileInput
{
    /// <summary>
    /// Read List&lt;NoteList&gt; from file
    /// </summary>
    /// <param name="path">Note input file the format of which is defined in NoteInputLib</param>
    public static List<NoteList> ReadFileInput(string path)
    {
        var noteList = new List<NoteList>();
        if (!File.Exists(path))
        {
            return noteList;
        }
        try
        {
            var lines = File.ReadAllLines(path);
            ShowNoteAs inputFormat = ParseInput.GetInputFormat(lines, ShowNoteAs.NumberBase12);
            noteList = ParseInput.Parse(lines, inputFormat);
        }
        catch (Exception ex)
        {
            throw;
        }
        return noteList;
    }
}
