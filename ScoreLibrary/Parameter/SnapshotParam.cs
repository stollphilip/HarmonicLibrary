using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ScoreLibrary.Parameter;
/// <summary>
/// Represents the selected snapshot
/// </summary>
public partial class SnapshotParam : ObservableObject
{
    // debug only
    [property: JsonIgnore]
    public int HandlerCount { get; set; } = 0;
    public SnapshotParam(string chord_, int segment_, int frame_, int index_, int count_, HarmonicAnalysisCommonLib.Quarantine.GroupType frameType_)
    {
        chord = chord_;
        segment = segment_;
        frame = frame_;
        index = index_;
        count = count_;
        frameType = frameType_;
        FormatStrings();
    }
    public SnapshotParam()
    {
    }

    [ObservableProperty]
    private string chord = string.Empty;

    [ObservableProperty]
    [property: JsonIgnore]
    private string path = string.Empty;

    [ObservableProperty]
    private int segment;

    [ObservableProperty]
    private int frame;

    [ObservableProperty]
    private HarmonicAnalysisCommonLib.Quarantine.GroupType frameType;

    [ObservableProperty]
    private string frameTypeString;

    // Index is 0-based. Index minimum is 0 and maximum is Count - 1 or 0, whichever is greater.
    [ObservableProperty]
    private int index;

    [ObservableProperty]
    private int count;

    [ObservableProperty]
    [property: JsonIgnore]
    private int maxIndex;

    // Snapshot is 1-based. 
    [ObservableProperty]
    [property: JsonIgnore]
    private string snapshot;

    private void FormatStrings()
    {
        Path = $"S[{this.Segment}] F[{this.Frame}]";
        MaxIndex = Math.Max(0, Count - 1);
        Snapshot = Count == 0 ? $"{0} of {0}" : $"{Index + 1} of {Count}";
        switch (FrameType)
        {
            case HarmonicAnalysisCommonLib.Quarantine.GroupType.Frame:
                FrameTypeString = "Frame";
                break;
            case HarmonicAnalysisCommonLib.Quarantine.GroupType.VerticalFrame:
                FrameTypeString = "VertFrame";
                break;
            case HarmonicAnalysisCommonLib.Quarantine.GroupType.HorizontalFrame:
                FrameTypeString = "HorizFrame";
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    partial void OnIndexChanged(int value)
    {
        FormatStrings();
    }

    partial void OnCountChanged(int value)
    {
        FormatStrings();
    }
    #region overrides
    public override string ToString()
    {
        return $"{Path} {Index} of {Count}";
    }
    public string FormatForHashCode() => ToString();
    //public override int GetHashCode()
    //{
    //    return ToString().GetHashCode();
    //}
    public override bool Equals(object obj)
    {
        if (obj is SnapshotParam other)
        {
            return Chord == other.Chord &&
                Segment == other.Segment &&
                Frame == other.Frame &&
                Index == other.Index &&
                Count == other.Count;
        }
        return false;
    }
    public bool EqualsExceptIndex(SnapshotParam other)
    {
        return Chord == other.Chord &&
            Segment == other.Segment &&
            Frame == other.Frame &&
            //Index == other.Index &&
            Count == other.Count;
    }
    public SnapshotParam Clone()
    {
        return new SnapshotParam
        {
            Chord = Chord,
            Segment = Segment,
            Frame = Frame,
            FrameType = FrameType,
            Index = Index,
            Count = Count,
        };
    }
    #endregion
}
