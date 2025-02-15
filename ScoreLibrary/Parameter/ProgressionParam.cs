using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Controls;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using HarmonicAnalysisCommonLib.Quarantine;
using ScoreLibrary;

namespace ScoreLibrary.Parameter;
// Keep this class simple. Do not add unecessary code.
// It is a POCO object. It gets sent as a JSON message. It is part of a ViewModel.
// I considered extracting an interface. That got into covariance and contravariance. Better to keep it simple.
/// <summary>
/// Represents the progression parameters
/// </summary>
public partial class ProgressionParam : ObservableObject
{
    // debug only
    [property: JsonIgnore]
    public int HandlerCount { get; set; } = 0;
    [ObservableProperty]
    private string progression = string.Empty;

    [ObservableProperty]
    [property: JsonIgnore]
    private string editProgression = string.Empty;

    #region Properties dependent on Progression
    [ObservableProperty]
    private ObservableCollection<SnapshotParam> snapshotSelections = new ObservableCollection<SnapshotParam>();
    #endregion

    #region Properties
    // pitch height staff
    [ObservableProperty]
    private bool showPHS;
    [ObservableProperty]
    private bool showXYZ;
    [ObservableProperty]
    private bool showYZ;
    [ObservableProperty]
    private bool showY;
    [ObservableProperty]
    private ObjectMappings objectMapping;
    [ObservableProperty]
    private bool showDistance;
    [ObservableProperty]
    private DistanceType distanceType;
    [ObservableProperty]
    private bool showHull;
    [ObservableProperty]
    private ContentAlignment contentAlignment;
    [ObservableProperty]
    private ShowNoteAs showNoteAs;
    [ObservableProperty]
    private bool showOctave;
    [ObservableProperty]
    private XYZStaffLayout xYZStaffLayout;
    [ObservableProperty]
    private string zoom;

    public ProgressionParam(
        string progression_,
        ObservableCollection<SnapshotParam> snapshotSelections_
        )
    {
        Progression = progression_;
        SnapshotSelections = snapshotSelections_;
        ShowNoteAs = default;
        ShowOctave = default;
        ShowHull = default;
        ObjectMapping = default;
        XYZStaffLayout = default;
        DistanceType = default;
        ContentAlignment = default;
        Zoom = "100%";
    }

    public ProgressionParam()
    {
    }


    #endregion

    #region overrides
    public override string ToString()
    {
        return $"{Progression} {ShowNoteAs} {ShowOctave} {ShowHull} {ObjectMapping} {XYZStaffLayout} {DistanceType} {ContentAlignment}";
    }
    public string FormatForHashCode()
    {
        return ToString() + " " + string.Join(" ", SnapshotSelections.Select(s => s.FormatForHashCode()));
    }
    //public override int GetHashCode() => FormatForHashCode().GetHashCode();
    //public int MyGetHashCode() => FormatForHashCode().GetHashCode();

    public ProgressionParam Clone(bool cloneShapshots = false)
    {
        return new ProgressionParam
        {
            Progression = Progression,
            ShowPHS = ShowPHS,
            ShowXYZ = ShowXYZ,
            ShowYZ = ShowYZ,
            ShowY = ShowY,
            ObjectMapping = ObjectMapping,
            ShowDistance = ShowDistance,
            ShowOctave = ShowOctave,
            ShowNoteAs = ShowNoteAs,
            ShowHull = ShowHull,
            XYZStaffLayout = XYZStaffLayout,
            DistanceType = DistanceType,
            ContentAlignment = ContentAlignment,
            Zoom = Zoom,
            SnapshotSelections = cloneShapshots ?
            new ObservableCollection<SnapshotParam>(SnapshotSelections.Select(x => x.Clone())) :
            new ObservableCollection<SnapshotParam>(),
        };
    }
    #endregion
    partial void OnEditProgressionChanged(string value)
    {
        if (value == null)
        {
            return;
        }
        var newtext = value.Replace("\r\n", ", ");
        if (Progression == newtext)
        {
            return;
        }
        this.Progression = newtext;
    }
    partial void OnProgressionChanged(string value)
    {
        if (value == null)
        {
            return;
        }
        var newtext = value.Replace(",  ", ", ").Replace(", ", ",").Replace(",", "\r\n");
        if (EditProgression == newtext)
        {
            return;
        }
        this.EditProgression = newtext;
    }
    #region NotifyPropChangedEvent
    /// <summary>
    /// Property names that trigger Send()
    /// </summary>
    public static readonly List<string> propertyNames = new List<string> {
        "ShowPHS",
        "ShowXYZ",
        "ShowYZ",
        "ShowY",
        "ObjectMapping",
        "ShowDistance",
        "ShowOctave",
        "ShowNoteAs",
        "ShowHull",
        "XYZStaffLayout",
        "DistanceType",
        "ContentAlignment",
        "Zoom",
        "Index", // SnapshotParam.Index
        //"Snapshot",
    };

    /// <summary>
    /// Send progression parameters to Score for redrawing when certain properties change
    /// </summary>
    private void PropertyChanged_handler(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is SnapshotParam snapshot)
        {
            if (snapshot.HandlerCount == 0)
            {
                return;
            }
        }
        var propertyName = e.PropertyName;
        if (propertyNames.Contains(propertyName))
        {
            OnPropertyChanged(propertyName);
        }
    }
    public void DetachSnapshots()
    {
        foreach (var snapshot in SnapshotSelections)
        {
            snapshot.HandlerCount--;
            snapshot.PropertyChanged -= PropertyChanged_handler;
        }
    }
    public void AttachSnapshots()
    {
        foreach (var snapshot in SnapshotSelections)
        {
            snapshot.HandlerCount++;
            snapshot.PropertyChanged += PropertyChanged_handler;
        }
    }
    #endregion
}
