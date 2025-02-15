using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using HarmonicAnalysisCommonLib.Quarantine;

namespace HarmonicAnalysisCommonLib;
/// <summary>
/// SegmentLite is an exact copy of Segment but stripped down. SegmentLite adds a convenience field PositionFinal.
/// </summary>
public class SegmentLite
{
    public List<FrameStructLite> FrameStructs
    {
        get; set;
    } = new List<FrameStructLite>();
    public IDistanceAlgorithmLite? Distance
    {
        get; set;
    }
    public SegmentLite()
    {
        Distance = new SegmentDistanceAlgorithmLite(this);
    }
}
public class FrameStructLite
{
    public GroupMapLite GroupMap
    {
        get; set;
    } = new GroupMapLite();
}
public class GroupMapLite
{
    public GroupMapType Type
    {
        get; set;
    } = GroupMapType.GroupMap;
    public GroupLite Frame
    {
        get; set;
    } = new GroupLite();
    public GroupMapDistanceAlgorithmLite? Distance
    {
        get; set;
    }
    public List<GroupMapSnapshotLite> Snapshots
    {
        get; set;
    } = new List<GroupMapSnapshotLite>();
    public int SnapshotIndex
    {
        get; set;
    } = -1;
    // do not add a Distance member here, use SnapshotLite.Distance instead
    public GroupMapLite(GroupMapType type, GroupLite frame, List<GroupMapSnapshotLite> snapshots, int snapshotIndex)
    {
        Type = type;
        Frame = frame;
        Snapshots = snapshots;
        SnapshotIndex = snapshotIndex;
        Distance = new GroupMapDistanceAlgorithmLite(this);
    }
    public GroupMapLite()
    {
        Distance = new GroupMapDistanceAlgorithmLite(this);
    }

    public List<GroupLite> GetDistinctGroups(params GroupType[] types)
    {
        if (Snapshots.Count == 0)
        {
            return new List<GroupLite>();
        }
        return Snapshots[SnapshotIndex].ToneMaps.SelectMany(t => t)
            .Where(t => t.Group != null && types.Contains(t.Group.Type))
            .Select(t => t.Group).Cast<GroupLite>().Distinct().ToList();
    }

}
public class GroupMapSnapshotLite
{
    // needed?
    //public GroupMapType Type { get; set; }
    public List<List<ToneMapLite>> ToneMaps
    {
        get; set;
    } = new List<List<ToneMapLite>> { new List<ToneMapLite>() };
    public IDistanceAlgorithmLite? Distance
    {
        get; set;
    }
}
public class ToneMapLite
{
    public GroupLite Group
    {
        get; set;
    } = new GroupLite();
    // TODO: is GroupPosition needed?
    //public Vector<int> GroupPosition
    //{
    //    get; set;
    //}
}
// GroupLite objects can be assigned to at most one ColumnBorder
// GroupLite provides all the information to create a Hull
// for bipartite Groups, call ListNotes on the two children
//  caution: the second Child is sometime empty
// for bipartite Groups, access distance on the two children
public class GroupLite : IEqualityComparer<GroupLite>
{
    public int ChordIndex
    {
        get; set;
    }
    public GroupType Type
    {
        get; set;
    } = GroupType.Group;
    public Vector<int> Position
    {
        get; set;
    }
    /// <summary>
    /// Position with respect to the root, as opposed to the parent.
    /// </summary>
    /// <remarks>Equal to Position plus Child offset.</remarks>
    public Vector<int> PositionFinal
    {
        get; set;
    }
    public ToneLite? Tone
    {
        get; set;
    } = null;
    public List<ChildLite> Children
    {
        get; set;
    } = new List<ChildLite>();
    public IDistanceAlgorithmLite? Distance
    {
        get; set;
    }
    public BorderData GetBorderData()
    {
        return new BorderData
        {
            notes = ListNotes(),
            distance = Distance,
        };
    }
    public BorderData GetBorderData(IDistanceAlgorithmLite distanceAlgorithmLite)
    {
        return new BorderData
        {
            notes = ListNotes(),
            distance = distanceAlgorithmLite,
        };
    }
    public List<ToneLite> ListNotes()
    {
        switch (Type)
        {
            case GroupType.Tone:
                Debug.Assert(Children.Count == 0);
                return new List<ToneLite> { Tone };
            case GroupType.Group:
            case GroupType.Frame:
                Debug.Assert(Children.Count != 0 && Children.All(c => c.Group.Children.Count == 0));
                return Children.Select(x => x.Group.Tone).Cast<ToneLite>().ToList();
            case GroupType.VerticalGroup:
            case GroupType.VerticalFrame:
            case GroupType.HorizontalGroup:
            case GroupType.HorizontalFrame:
                var list = new List<ToneLite>();
                foreach (var child in Children)
                {
                    if (child.Group.Children.Count == 0)
                    {
                        Debug.Assert(child.Group.Tone != null);
                        if (!list.Any(t => t == child.Group.Tone))
                        {
                            list.Add(child.Group.Tone);
                        }
                    }
                    else if (child.Group.Children.Count != 0)
                    {
                        foreach(var c in child.Group.Children)
                        {
                            Debug.Assert(c.Group.Tone != null);
                            if (!list.Any(t => t == c.Group.Tone))
                            {
                                list.Add(c.Group.Tone);
                            }
                        }
                    }
                }
                return list;
            case GroupType.HorizontalPath:
            case GroupType.VerticalPath:
            case GroupType.HorizontalFrameExtend:
            case GroupType.Null:
            default:
                throw new InvalidOperationException();
        }
    }

    #region override
    public override string ToString()
    {
        switch (this.Type)
        {
            case GroupType.Tone:
                if (Tone is not null)
                {
                    return $"{Type,-15} {ChordIndex,2} : {Position[0],3} {Position[1],2} {Position[2],2} : {Tone.PitchHeight,3} {Tone.FifthHeight,2} ";
                }
                return string.Empty;
            case GroupType.Group:
            case GroupType.VerticalGroup:
            case GroupType.HorizontalGroup:
                var sb = new StringBuilder();
                sb.Append($"{Type,-15} {ChordIndex,2} : ");
                foreach (var child in Children)
                {
                    sb.Append($"  {child.Group.Type,-15} ");
                    var p = child.Position;
                    sb.Append($"{p[0],3} {p[1],2} {p[2],2} : ");
                }
                return sb.ToString();
            case GroupType.Frame:
            case GroupType.VerticalPath:
            case GroupType.HorizontalPath:
            case GroupType.VerticalFrame:
            case GroupType.HorizontalFrame:
            case GroupType.HorizontalFrameExtend:
                Debug.Assert(Position == Children[0].Position);
                sb = new StringBuilder();
                sb.Append($"{Type,-15} {ChordIndex,2} : {Position[0],3} {Position[1],2} {Position[2],2} : ");
                foreach (var child in Children)
                {
                    sb.Append($"  {child.Group.Type,-15} ");
                    var p = child.Position;
                    sb.Append($"{p[0],3} {p[1],2} {p[2],2} : ");
                }
                return sb.ToString();
            default:
                throw new NotImplementedException();
        }
    }
    #endregion
    #region IEqualityComparer<GroupLite>

    // IEqualityComparer<GroupLite> is used by LINQ Distinct
    public bool Equals(GroupLite? x, GroupLite? y)
    {
        if (x == null || y == null)
        {
            return false;
        }
        if (x.Type != y.Type)
        {
            return false;
        }
        if (x.Type == GroupType.Tone)
        {
            return x.Tone.Equals(y.Tone);
        }
        if (x.Children.Count != y.Children.Count)
        {
            return false;
        }
        for (var i = 0; i < x.Children.Count; i++)
        {
            if (!Equals(x.Children[i].Group, y.Children[i].Group))
            {
                return false;
            }
        }
        return true;
    }

    public int GetHashCode(GroupLite obj)
    {
        return obj.FormatHashCode().GetHashCode();
    }

    public string FormatHashCode()
    {
        string s = (Tone != null) ? $"{Tone} " : s = $"{Type} ";
        s += string.Join(" ", Children.Select(c => c.Group.FormatHashCode()));
        return s;
    }
    #endregion
}
public class ChildLite
{
    public GroupLite Group
    {
        get; set;
    } = new GroupLite();
    // TODO: possibly not needed
    public Vector<int> Position
    {
        get; set;
    }
    public Vector<int> Offset()
    {
        return Position - Group.Position;
    }
}
public class ToneLite : IEquatable<ToneLite>
{
    public ToneLite(int height, Vector<int> position, Vector<int> positionFinal, Accidental accidental, bool hidden = false)
    {
        PitchHeight = height;
        Position = position;
        PositionFinal = positionFinal;
        Hidden = hidden;
        var pitchclass = Extensions.ModulusOperator(PitchHeight, 12);
        this.alter = Alter.None;
        this.octave = Extensions.Quotient(PitchHeight, 12) + 4;
        switch (pitchclass)
        {
            case 0:
                step = step.C;
                break;
            case 1:
                step = step.C;
                alter = Alter.Sharp;
                break;
            case 2:
                step = step.D;
                break;
            case 3:
                step = step.E;
                alter = Alter.Flat;
                break;
            case 4:
                step = step.E;
                break;
            case 5:
                step = step.F;
                break;
            case 6:
                step = step.F;
                alter = Alter.Sharp;
                break;
            case 7:
                step = step.G;
                break;
            case 8:
                step = step.A;
                alter = Alter.Flat;
                break;
            case 9:
                step = step.A;
                break;
            case 10:
                step = step.B;
                alter = Alter.Flat;
                break;
            case 11:
                step = step.B;
                break;
        }

        switch (accidental)
        {
            case Accidental.None:
                break;
            case Accidental.DoubleFlat:
                switch (pitchclass)
                {
                    case 0:
                        step = step.D;
                        alter = Alter.DoubleFlat;
                        break;
                    case 1:
                        break;
                    case 2:
                        step = step.E;
                        alter = Alter.DoubleFlat;
                        break;
                    case 3:
                        step = step.F;
                        alter = Alter.DoubleFlat;
                        break;
                    case 4:
                        break;
                    case 5:
                        step = step.G;
                        alter = Alter.DoubleFlat;
                        break;
                    case 6:
                        break;
                    case 7:
                        step = step.A;
                        alter = Alter.DoubleFlat;
                        break;
                    case 8:
                        break;
                    case 9:
                        step = step.B;
                        alter = Alter.DoubleFlat;
                        break;
                    case 10:
                        step = step.C;
                        alter = Alter.DoubleFlat;
                        break;
                    case 11:
                        break;
                }
                break;
            case Accidental.Flat:
                switch (pitchclass)
                {
                    case 0:
                        break;
                    case 1:
                        step = step.D;
                        alter = Alter.Flat;
                        break;
                    case 2:
                        break;
                    case 3:
                        step = step.E;
                        alter = Alter.Flat;
                        break;
                    case 4:
                        step = step.F;
                        alter = Alter.Flat;
                        break;
                    case 5:
                        break;
                    case 6:
                        step = step.G;
                        alter = Alter.Flat;
                        break;
                    case 7:
                        break;
                    case 8:
                        step = step.A;
                        alter = Alter.Flat;
                        break;
                    case 9:
                        break;
                    case 10:
                        step = step.B;
                        alter = Alter.Flat;
                        break;
                    case 11:
                        step = step.C;
                        alter = Alter.Flat;
                        break;
                }
                break;
            case Accidental.Natural:
                switch (pitchclass)
                {
                    case 0:
                        step = step.C;
                        alter = Alter.Natural;
                        break;
                    case 1:
                        break;
                    case 2:
                        step = step.D;
                        alter = Alter.Natural;
                        break;
                    case 3:
                        break;
                    case 4:
                        step = step.E;
                        alter = Alter.Natural;
                        break;
                    case 5:
                        step = step.F;
                        alter = Alter.Natural;
                        break;
                    case 6:
                        break;
                    case 7:
                        step = step.G;
                        alter = Alter.Natural;
                        break;
                    case 8:
                        break;
                    case 9:
                        step = step.A;
                        alter = Alter.Natural;
                        break;
                    case 10:
                        break;
                    case 11:
                        step = step.B;
                        alter = Alter.Natural;
                        break;
                }
                break;
            case Accidental.Sharp:
                switch (pitchclass)
                {
                    case 0:
                        step = step.B;
                        alter = Alter.Sharp;
                        break;
                    case 1:
                        step = step.C;
                        alter = Alter.Sharp;
                        break;
                    case 2:
                        break;
                    case 3:
                        step = step.D;
                        alter = Alter.Sharp;
                        break;
                    case 4:
                        break;
                    case 5:
                        step = step.E;
                        alter = Alter.Sharp;
                        break;
                    case 6:
                        step = step.F;
                        alter = Alter.Sharp;
                        break;
                    case 7:
                        break;
                    case 8:
                        step = step.G;
                        alter = Alter.Sharp;
                        break;
                    case 9:
                        break;
                    case 10:
                        step = step.A;
                        alter = Alter.Sharp;
                        break;
                    case 11:
                        break;
                }
                break;
            case Accidental.DoubleSharp:
                switch (pitchclass)
                {
                    case 0:
                        break;
                    case 1:
                        step = step.B;
                        alter = Alter.DoubleSharp;
                        break;
                    case 2:
                        step = step.C;
                        alter = Alter.DoubleSharp;
                        break;
                    case 3:
                        break;
                    case 4:
                        step = step.D;
                        alter = Alter.DoubleSharp;
                        break;
                    case 5:
                        break;
                    case 6:
                        step = step.E;
                        alter = Alter.DoubleSharp;
                        break;
                    case 7:
                        step = step.F;
                        alter = Alter.DoubleSharp;
                        break;
                    case 8:
                        break;
                    case 9:
                        step = step.G;
                        alter = Alter.DoubleSharp;
                        break;
                    case 10:
                        break;
                    case 11:
                        step = step.A;
                        alter = Alter.DoubleSharp;
                        break;
                }
                break;
        }
    }

    public ToneLite()
    {
    }

    public int PitchHeight
    {
        get; set;
    }
    // TODO: remove
    public int FifthHeight
    {
        get { return (int)(Position[1] + 4 * Position[2]); }
    }
    public step step
    {
        get; set;
    }
    public Alter alter
    {
        get; set;
    } = Alter.None;
    public int octave
    {
        get; set;
    }
    public Vector<int> Position
    {
        get; set;
    }
    public Vector<int> PositionFinal
    {
        get; set;
    }
    // when a chord has no notes, you have two choices:
    // mark the staff origin with a visible note text, or layout the staff with a hidden note.
    // search for "ChordWithoutNotes" to find code that handles this
    // Tag: ChordWithoutNotes - do not remove this comment
    public bool Hidden
    {
        get; protected set;
    }
    #region IEquatable<ToneLite>
    public bool Equals(ToneLite? other)
    {
        var pitch = this;
        return other != null && pitch.PitchHeight == other.PitchHeight && pitch.step == other.step && pitch.alter == other.alter && pitch.octave == other.octave && pitch.Position.Equals(other.Position);
    }
    #endregion
    protected readonly List<(Alter alter, string accidental)> AlterToAccidental = new List<(Alter alter, string accidental)> {
            (Alter.None, string.Empty),
            (Alter.DoubleFlat, "𝄫"),
            (Alter.Flat, "♭"),
            (Alter.Natural, "♮"),
            (Alter.Sharp, "♯"),
            (Alter.DoubleSharp, "𝄪"),
        };
    public string ToneLiteToString(ShowNoteAs outputFormat)
    {
        switch (outputFormat)
        {
            case ShowNoteAs.NumberBase10:
                return PitchHeight.ToString();
            case ShowNoteAs.NumberBase12:
                return PitchHeight.Base10ToOctavePitchClassBase12();
            case ShowNoteAs.Scientific:
                var accidental = AlterToAccidental.First(a => a.alter == alter).accidental;
                return $"{step}{octave}{accidental}";
            default:
                throw new NotImplementedException();
        }
    }

}
public enum Alter
{
    // use ToInt() extension method to convert to int
    None = 3,
    DoubleFlat = -2,
    Flat = -1,
    Natural = 0,
    Sharp = 1,
    DoubleSharp = 2,
}
