using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicXMLLib.Lite;

namespace MusicXMLLib;

/// <summary>
/// provides all the information to create a Border
/// </summary>
/// <remarks>A Border consists of a set notes and a distance</remarks>
public class BorderData
{
    public List<ToneLite> notes
    {
        get; set;
    } = new List<ToneLite>();
    public DistanceAlgorithmLite distance
    {
        get; set;
    }
    public int UID
    {
        get; set;
    }
}
