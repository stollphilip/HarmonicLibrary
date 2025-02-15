using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using HarmonicAnalysisCommonLib;

namespace HarmonicAnalysisLib.MusicXML;
public class Analysis
{
    public static List<SegmentLite> Parse(List<Segment> segments)
    {
        List<SegmentLite> segmentLites = new();
        foreach (Segment segment in segments)
        {
            SegmentLite segmentLite = new SegmentLite();
            segmentLites.Add(segmentLite);

            foreach (FrameStruct frameStruct in segment.FrameStructs)
            {
                GroupMap groupMap = frameStruct.GroupMap;

                List<GroupMapSnapshotLite> snapshotLites = new();
                foreach (GroupMapSnapshot snapshot in groupMap.Snapshots)
                {
                    // TODO: check offset expression is correct
                    List<List<ToneMapLite>> toneMapLites = snapshot.ToneMaps
                        .Select(toneMap => toneMap.Select(tone => new ToneMapLite { Group = Convert(tone.Group, Vector<int>.Zero) }).ToList()).ToList();
                    GroupMapSnapshotLite snapshotLite = new GroupMapSnapshotLite
                    {
                        ToneMaps = toneMapLites,
                    };
                    snapshotLites.Add(snapshotLite);
                }

                GroupMapLite groupMapLite = new GroupMapLite(
                    type : Convert(groupMap.Type),
                    frame : Convert(groupMap.Frame, Vector<int>.Zero),
                    snapshots : snapshotLites,
                    snapshotIndex : groupMap.SnapshotIndex
                    );

                FrameStructLite frameStructLite = new FrameStructLite
                {
                    GroupMap = groupMapLite,
                };
                segmentLite.FrameStructs.Add(frameStructLite);
            }
        }
        return segmentLites;
    }

    public static DistanceAlgorithmLite Convert(IDistanceAlgorithm d)
    {
        return new(d.Pairwise, d.Average, d.Weighted, d.CompoundPairwise, d.CompoundAverage, d.CompoundWeighted);
    }

    public static HarmonicAnalysisCommonLib.Quarantine.GroupMapType Convert(GroupMapType t)
    {
        return (HarmonicAnalysisCommonLib.Quarantine.GroupMapType)Enum.Parse(typeof(HarmonicAnalysisCommonLib.Quarantine.GroupMapType), t.ToString());
    }

    public static HarmonicAnalysisCommonLib.Quarantine.GroupType Convert(GroupType t)
    {
        return (HarmonicAnalysisCommonLib.Quarantine.GroupType)Enum.Parse(typeof(HarmonicAnalysisCommonLib.Quarantine.GroupType), t.ToString());
    }

    public static GroupLite Convert(Group g, Vector<int> offset)
    {
        if (offset[0] != 0 || offset[1] != 0 || offset[2] != 0)
        {
        }
        if (g.Children.Any(c => c.Offset()[0] != 0 || c.Offset()[1] != 0 || c.Offset()[2] != 0))
        {
        }
        List<ChildLite> children;
        switch (g.Type)
        {
            case GroupType.Tone:
                Debug.Assert(g.Children.Count == 0);
                ToneLite toneLite = new ToneLite(g.Tone.PitchHeight, g.Position, g.Position + offset, g.Tone.Accidental);
                return new GroupLite
                {
                    ChordIndex = g.ChordIndex,
                    Type = Convert(g.Type),
                    Position = g.Position,
                    PositionFinal = g.Position + offset,
                    Tone = toneLite,
                    Distance = Convert(g.Distance),
                };
            case GroupType.Group:
            case GroupType.Frame:
                Debug.Assert(g.Children.Count != 0);
                Debug.Assert(g.Children.All(c => c.Group.Children.Count == 0));
                // TODO: check offset expression is correct
                if (!offset.Equals(Vector<int>.Zero))
                {
                
                }
                if (g.Children.Any(c => !c.Offset().Equals(Vector<int>.Zero)))
                {

                }
                children = g.Children.Select(c => new ChildLite { Group = Convert(c.Group, c.Offset() + offset), Position = c.Position, }).ToList();
                return new GroupLite
                {
                    ChordIndex = g.ChordIndex,
                    Type = Convert(g.Type),
                    Position = g.Position,
                    PositionFinal = g.Position + offset,
                    Children = children,
                    Tone = null,
                    Distance = Convert(g.Distance),
                };
            case GroupType.VerticalGroup:
            case GroupType.VerticalFrame:
            case GroupType.HorizontalGroup:
            case GroupType.HorizontalFrame:
                Debug.Assert(g.Children.Count == 2);
                // TODO: check offset expression is correct
                Debug.Assert(offset == Vector<int>.Zero);
                children = g.Children.Select(c => new ChildLite { Group = Convert(c.Group, c.Offset() + offset), Position = c.Position, }).ToList();
                return new GroupLite
                {
                    ChordIndex = g.ChordIndex,
                    Type = Convert(g.Type),
                    Position = g.Position + offset,
                    PositionFinal = g.Position,
                    Children = children,
                    Tone = null,
                    Distance = Convert(g.Distance),
                };
            case GroupType.Null:
            default:
                throw new NotImplementedException();
        }
    }
}
