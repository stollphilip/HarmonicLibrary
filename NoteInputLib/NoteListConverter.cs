using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonicAnalysisLib;

namespace NoteInputLib;
public class NoteListConverter
{
    /// <summary>
    /// Convert List&lt;NoteList&gt; to PitchPattern[][]
    /// </summary>
    /// <param name="noteLists"></param>
    /// <returns></returns>
    public static PitchPattern[][] Convert(List<NoteList> noteLists)
    {
        var pitchPatternListList = new List<List<PitchPattern>>();
        foreach (var noteList in noteLists)
        {
            var pitchPatternList = new List<PitchPattern>();
            pitchPatternListList.Add(pitchPatternList);
            foreach (var chordInput in noteList.Chords)
            {
                var pitchPattern = new PitchPattern
                {
                    Pitches = chordInput.Notes.Select(note => new Pitch(note.Height, note.FifthHeight(), note.Accidental)).Cast<IPitch>().ToList()
                };
                pitchPatternList.Add(pitchPattern);
            }
        }
        return pitchPatternListList.Select(list => list.ToArray()).ToArray();
    }
}
