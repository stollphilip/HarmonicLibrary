using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisCommonLib;

/*
 * ChordData
 *     |
 *     -> ColumnData
 *            |
 *            -> BorderData
 * */

/// <summary>
/// provides all the information to create a Chord
/// </summary>
/// <remarks>A Chord consists of one or more Columns</remarks>
public class ChordData
{
    public List<ColumnData> ColumnData
    {
        get; set;
    } = new List<ColumnData>();
}
