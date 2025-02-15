using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisCommonLib;

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
    public IDistanceAlgorithmLite distance
    {
        get; set;
    }
    public int UID
    {
        get; set;
    }
}
