using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisLib;
public class SegmentsStruct
{
    public List<Segment> List { get; set; } = new List<Segment>();
    public double Sum() =>
        List.Count != 0 ?
        List.Sum(s => s.Distance.Weighted) :
        0;
    public SegmentsStruct(List<Segment> list)
    {
        List = list;
    }
    public SegmentsStruct(Segment segment)
    {
        List = new List<Segment> { segment };
    }
    public SegmentsStruct()
    {
    }
    public static bool AreEqual(SegmentsStruct a, SegmentsStruct b)
    {
        if (a.List.Count != b.List.Count)
        {
            return false;
        }
        for (var i = 0; i < a.List.Count; i++)
        {
            if (a.List[i].FrameStructs.Count != b.List[i].FrameStructs.Count)
            {
                return false;
            }
            for (var j = 0; j < a.List[i].FrameStructs.Count; j++)
            {
                if (a.List[i].FrameStructs[j] != (b.List[i].FrameStructs[j]))
                {
                    return false;
                }
            }
        }
        return true;
    }
    public static bool AreEqual(List<Segment> a, List<Segment> b)
    {
        if (a.Count != b.Count)
        {
            return false;
        }
        for (var i = 0; i < a.Count; i++)
        {
            if (a[i].FrameStructs.Count != b[i].FrameStructs.Count)
            {
                return false;
            }
            for (var j = 0; j < a[i].FrameStructs.Count; j++)
            {
                if (a[i].FrameStructs[j] != (b[i].FrameStructs[j]))
                {
                    return false;
                }
            }

        }
        return true;
    }
}