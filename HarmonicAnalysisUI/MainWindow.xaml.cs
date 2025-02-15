using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using HarmonicAnalysisLib;
using HarmonicAnalysisUI.SecondWindows;
using NoteInputLib;
using ScoreLibrary;
using ScoreLibrary.Parameter;
using HarmonicAnalysisUI.Utilities;

namespace HarmonicAnalysisUI;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        secondaryWindow = new SecondWindow();
        secondaryWindow.NotifyEvent += NotifyEventCallback;
        secondaryWindow.CommandEvent += SecondaryWindowCallback;
        this.RespondEvent += secondaryWindow.RespondEventCallback;
        InitializeComponent();
        secondaryWindow.Show();
        manager = new Manager();
    }
    private SecondWindow secondaryWindow { get; }

    private Manager manager { get; }

    // the layout of staffs on MainWindow
    private Progression? progression { get; set; }

    // the user editable settings on SecondWindow
    private ProgressionsParam? progressionsParam { get; set; }

    public ObservableCollection<ZoomStruct> ZoomFactors => Zoom.zoomFactor3s;
    private ZoomStruct _SZoom = Zoom.DefaultZoomStruct;
    public ZoomStruct SZoom
    {
        get
        {
            return _SZoom;
        }
        set
        {
            _SZoom = value;
            Zoom.UpdateZoom(grid, value.Value);
        }
    }

    #region MainWindow SecondWindow communication
    private void NotifyEventCallback(string json)
    {
        //MessageBox.Show(json);
        ProcessMessage(json);
    }

    private void SecondaryWindowCallback(string command)
    {
        switch (command)
        {
            case MyViewModel.Print:
                PrintSupport.Print(this);
                break;
        }
    }

    public event Action<string>? RespondEvent;
    #endregion

    private void ProcessMessage(string json)
    {
        ProgressionsParam? progressionsParam_ =
            JsonSerializer.Deserialize<ProgressionsParam>(json);

        if (progressionsParam_.Progressions.Count == 0)
        {
            return;
        }
        if (progressionsParam_.SelectedIndex == -1)
        {
            // this is an programming error
            return;
        }
        var index = progressionsParam_.SelectedIndex;

        var isProgressionSame = IsProgressionSame(progressionsParam_, index);
        var isSelectedIndexSame = IsSelectedIndexSame(progressionsParam_, index);
        var isZoomSame = IsZoomSame(progressionsParam_, index);
        // not needed
        var isSnapshotInitialized = IsSnapshotSame(progressionsParam_, index);
        var snapshotChanges = GetSnapshotChanges(progressionsParam_, index);

        if (!isProgressionSame || !isSelectedIndexSame || !isZoomSame)
        {
            progressionsParam = progressionsParam_;
            // create default staff parameters
            var staffParameters = InitializeParam.Create(progressionsParam.Progressions[index]);

            // handle the notification by running the analysis, and send a response containing the snapshots
            var progressionString = progressionsParam_.Progressions[index].Progression;
            var split = progressionString.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
            var noteList = StringInput.ReadStringInput(progressionString, ',');
            PitchPattern[][] progressions = NoteListConverter.Convert(noteList);
            manager.Process(progressions[0/*index*/]);
            ProcessScorepartwise(staffParameters);

            // initialize ProgressionsParam SnapshotSelections
            progressionsParam_.Progressions[index].SnapshotSelections = new ObservableCollection<SnapshotParam>();
            for (var s = 0; s < manager.SegmentLites.Count; s++)
            {
                var segmentLite = manager.SegmentLites[s];
                for (var f = 0; f < segmentLite.FrameStructs.Count; f++)
                {
                    var frameStructLite = segmentLite.FrameStructs[f];
                    var groupMapLite = frameStructLite.GroupMap;
                    var groupType = groupMapLite.Frame.Type;
                    var snapshotCount = frameStructLite.GroupMap.Snapshots.Count;
                    var snapshotIndex = frameStructLite.GroupMap.SnapshotIndex/*Math.Max(0, snapshotCount - 1)*/;
                    var snapshotParam = new SnapshotParam(split[s], s, f, snapshotIndex, snapshotCount, groupType);
                    progressionsParam_.Progressions[index].SnapshotSelections.Add(snapshotParam);
                }
            }

            //StaffParam.Merge(progression.staffParameters[StaffTypeEnum.PitchHeight], progressionsParam_.Progressions[index]);
            var json_new = JsonSerializer.Serialize(progressionsParam_);
            RespondEvent?.Invoke(json_new);
        }
        else
        {
            // merge changes to all four of the Staffs
            InitializeParam.Merge(progressionsParam_.Progressions[index], progression.staffParameters);
            ProcessScorepartwise(progression.staffParameters);
            // no RespondEvent needed
        }
    }

    /// <summary>
    /// Check whether the progression string changed
    /// </summary>
    /// <returns>true if the progression string changed</returns>
    private bool IsProgressionSame(ProgressionsParam? ProgressionsModel_, int selectedProgressionIndex)
    {
        return (progressionsParam != null &&
            progressionsParam.Progressions.Count == ProgressionsModel_.Progressions.Count &&
            progressionsParam.Progressions[selectedProgressionIndex].Progression == ProgressionsModel_.Progressions[selectedProgressionIndex].Progression);
    }
    /// <summary>
    /// Check whether the zoom is the same
    /// </summary>
    /// <returns>true if the selected zoom is the same</returns>
    private bool IsZoomSame(ProgressionsParam? ProgressionsModel_, int selectedProgressionIndex)
    {
        if (IsProgressionSame(ProgressionsModel_, selectedProgressionIndex) &&
            IsSelectedIndexSame(ProgressionsModel_, selectedProgressionIndex))
        {
            var isZoomSame = progressionsParam.Progressions[selectedProgressionIndex].Zoom == ProgressionsModel_.Progressions[selectedProgressionIndex].Zoom;
            if (isZoomSame)
            {
                return true;
            }
            {
                SZoom = ZoomFactors.Single(z => z.Name == ProgressionsModel_.Progressions[selectedProgressionIndex].Zoom);
            }
        }
        return false;
    }
    /// <summary>
    /// Check whether the selected index (selected snapshot) is the same
    /// </summary>
    /// <returns>true if the selected index is the same</returns>
    private bool IsSelectedIndexSame(ProgressionsParam? ProgressionsModel_, int selectedProgressionIndex)
    {
        return IsProgressionSame(ProgressionsModel_, selectedProgressionIndex) &&
            progressionsParam?.SelectedIndex == ProgressionsModel_.SelectedIndex;
    }
    /// <summary>
    /// Check whether the snapshot is same, except for the Index
    /// </summary>
    /// <remarks>true if the snapshot is same, except for the Index</remarks>
    private bool IsSnapshotSame(ProgressionsParam? ProgressionsModel_, int selectedProgressionIndex)
    {
        if (!IsProgressionSame(ProgressionsModel_, selectedProgressionIndex))
        {
            return false;
        }
        var a = progressionsParam.Progressions[selectedProgressionIndex];
        var b = ProgressionsModel_.Progressions[selectedProgressionIndex];
        if (a.SnapshotSelections.Count != b.SnapshotSelections.Count)
        {
            return false;
        }
        for (int i = 0; i < a.SnapshotSelections.Count; i++)
        {
            if (a.SnapshotSelections[i].Segment != b.SnapshotSelections[i].Segment ||
                a.SnapshotSelections[i].Frame != b.SnapshotSelections[i].Frame ||
                a.SnapshotSelections[i].Count != b.SnapshotSelections[i].Count)
                return false;
        }
        return true;
    }

    /// <summary>
    /// if everything in the snapshots is the same except for the Index, 
    /// return the list of snapshot changes,
    /// otherwise return an empty list
    /// </summary>
    /// <returns>the list of snapshot changes</returns>
    private List<(int segment, int frame, int originalIndex, int newIndex)> GetSnapshotChanges(ProgressionsParam? ProgressionsModel_, int selectedProgressionIndex)
    {
        var list = new List<(int segment, int frame, int originalIndex, int newIndex)>();
        if (!IsSnapshotSame(ProgressionsModel_, selectedProgressionIndex))
        {
            return list;
        }
        var a = progressionsParam.Progressions[selectedProgressionIndex];
        var b = ProgressionsModel_.Progressions[selectedProgressionIndex];
        for (int i = 0; i < a.SnapshotSelections.Count; i++)
        {
            Debug.Assert(a.SnapshotSelections[i].Segment == b.SnapshotSelections[i].Segment);
            Debug.Assert(a.SnapshotSelections[i].Frame == b.SnapshotSelections[i].Frame);
            Debug.Assert(a.SnapshotSelections[i].Count == b.SnapshotSelections[i].Count);
            if (a.SnapshotSelections[i].Index != b.SnapshotSelections[i].Index)
            {
                list.Add((a.SnapshotSelections[i].Segment, a.SnapshotSelections[i].Frame, a.SnapshotSelections[i].Index, b.SnapshotSelections[i].Index));
            }
        }
        return list;
    }

    // copied from TryAddFormattedText.csproj
    private void ProcessScorepartwise(Dictionary<StaffTypeEnum, ProgressionParam> staffParameters)
    {
        progression = new Progression(staffParameters);
        progression.InitStaffs();
        var segments = manager.SegmentLites;
        progression.InitChords(segments);
        // initialize Chord locations
        progression.UpdateChordLocations1();
        // horizontally align Chords in all Staffs
        progression.UpdateChordLocations2();
        // vertically align Chords in each Staffs
        progression.UpdateChordLocations3();
        progression.UpdateStaffLocations();
        progression.UpdateProgressionLocation();
        progression.AddStaffLines();
        //progression.UpdateForPitchHeight();
        ResizeGrid();
        progression.SetGrid(0, 0, 0, 0);
        progression.AddToGrid(clearGrid, addToGrid);
    }
    private void ResizeGrid()
    {
        if (progression.Location == null)
        {
            return;
        }
        var loc = progression.Location;
        var firstRow = grid.RowDefinitions[0];
        var firstColumn = grid.ColumnDefinitions[0];
        grid.RowDefinitions.Clear();
        grid.ColumnDefinitions.Clear();
        for (int row = 0; row < 2 * loc.RowSpan; row++)
        {
            grid.RowDefinitions.Add(new RowDefinition
            {
                Height = new System.Windows.GridLength(firstRow.Height.Value, firstRow.Height.GridUnitType)
            });
        }
        for (int column = 0; column < loc.ColumnSpan; column++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new System.Windows.GridLength(firstColumn.Width.Value, firstColumn.Width.GridUnitType)
            });
        }
    }
    private void clearGrid()
    {
        if (grid.Children.Count != 0)
        {
            grid.Children.Clear();
        }
    }
    private void addToGrid(List<FrameworkElement> frameworkElementsAdded)
    {
        foreach (var f in frameworkElementsAdded)
        {
            Debug.Assert(!grid.Children.Contains(f));
            grid.Children.Add(f);
        }
    }
}