using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MusicXMLLib.Lite;
/// <summary>
/// SegmentLite is an exact copy of Segment but stripped down. SegmentLite adds a convenience field PositionFinal.
/// </summary>
public class SegmentLite
{
    public List<FrameStructLite> FrameStruct
    {
        get; set;
    } = new List<FrameStructLite>();
    public DistanceAlgorithmLite? Distance
    {
        get; set;
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
    public List<SnapshotLite> Snapshot
    {
        get; set;
    } = new List<SnapshotLite>();
    // do not add a Distance member here, use SnapshotLite.Distance instead
}
public class SnapshotLite
{
    public List<List<ToneMapLite>> ToneMap
    {
        get; set;
    } = new List<List<ToneMapLite>> { new List<ToneMapLite>() };
    public DistanceAlgorithmLite? Distance
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
    public DistanceAlgorithmLite? Distance
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
                break;
            case GroupType.VerticalGroup:
            case GroupType.VerticalFrame:
            case GroupType.HorizontalGroup:
            case GroupType.HorizontalFrame:
                Debug.Assert(Children.Count == 2 && Children.All(c => c.Group.Children.Count != 0));
                throw new InvalidOperationException();
            case GroupType.HorizontalPath:
            case GroupType.VerticalPath:
            case GroupType.HorizontalFrameExtend:
            case GroupType.Null:
                throw new InvalidOperationException();
        }
        return Children.Select(x => x.Group.Tone).ToList();
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
    public bool Equals(GroupLite x, GroupLite y)
    {
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
    public ToneLite(int height, Vector<int> position, Vector<int> positionFinal)
    {
        PitchHeight = height;
        Position = position;
        PositionFinal = positionFinal;
        var pitchclass = MusicXMLExtensions.ModulusOperator(PitchHeight, 12);
        this.alter = Alter.None;
        this.octave = MusicXMLExtensions.Quotient(PitchHeight, 12) + 4;
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
    #region IEquatable<ToneLite>
    public bool Equals(ToneLite other)
    {
        var pitch = this;
        var otherPitch = other;
        return pitch.PitchHeight == other.PitchHeight && pitch.step == otherPitch.step && pitch.alter == otherPitch.alter && pitch.octave == otherPitch.octave && pitch.Position.Equals(otherPitch.Position);
    }
    #endregion
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
