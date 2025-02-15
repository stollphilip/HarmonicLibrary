using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisLib
{
    /// <summary>
    /// Contains 1 GroupMap and 2 HorizontalGroupMaps for a single chord
    /// </summary>
    public class ChordMap
    {
        public Group Frame { get; set; }
        public GroupMap VertMap { get; set; }
        public List<GroupMap/*HorizontalGroupMap*/?> HorizMaps { get; set; }
    }
}
