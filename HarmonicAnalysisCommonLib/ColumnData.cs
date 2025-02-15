using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisCommonLib;

/// <summary>
/// provides all the information to create a Column
/// </summary>
/// <remarks>A Column contains of zero or more Borders</remarks>
public class ColumnData
{
    /// <summary>
    /// The union of all notes in the Column
    /// </summary>
    public List<ToneLite> notes
    {
        get; set;
    }
    /// <summary>
    /// Borders in a Column for a particular Chord
    /// </summary>
    public List<BorderData> BorderData
    {
        get; set;
    } = new List<BorderData>();
    // allows click event to get the Chord
    public int ChordIndex
    {
        get; set;
    }
    public void AddBorder(BorderData border)
    {
        BorderData.Add(border);
        border.UID = _uid++;
    }
    public List<ToneLite> ListNotes()
    {
        return BorderData.SelectMany(b => b.notes).Distinct().ToList();
    }
    private static int _uid = 0;
}
