using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonicAnalysisCommonLib.Quarantine;

namespace ScoreLibrary.Parameter;
/// <summary>
/// Helper class containing methods to initialize and merge parameters
/// </summary>
public class InitializeParam
{
    public static Dictionary<StaffTypeEnum, ProgressionParam> Create(ProgressionParam progressionParam)
    {
        var staffParameters = new Dictionary<StaffTypeEnum, ProgressionParam>
            {
                {
                    StaffTypeEnum.PitchHeight,
                    progressionParam
                },
                {
                    StaffTypeEnum.XYZ,
                    progressionParam
                },
                {
                    StaffTypeEnum.YZ,
                    progressionParam
                },
                {
                    StaffTypeEnum.Y,
                    progressionParam
                },
            };
        return staffParameters;
    }
    public static ProgressionParam CreateProgressionParam()
    {
        return new ProgressionParam
        {
            ShowPHS = true,
            ShowXYZ = true,
            ShowYZ = true,
            ShowY = true,
            ObjectMapping = ObjectMappings.Frame_in_1_col,
            ShowDistance = true,
            ShowOctave = true,
            ShowNoteAs = ShowNoteAs.Symbol,
            ShowHull = true,
            XYZStaffLayout = XYZStaffLayout.None,
            DistanceType = DistanceType.None,
            ContentAlignment = ContentAlignment.TopLeft,
            Zoom = "100%",
        };
        // There are no snapshots yet. No need to call AttachSnapshots().
    }
    /// <summary>
    /// Merge the progression parameters into the dictionary
    /// </summary>
    /// <param name="progressionParam">merge from ProgressionParam</param>
    /// <param name="dict">merge to Dictionary</param>
    public static void Merge(ProgressionParam progressionParam, Dictionary<StaffTypeEnum, ProgressionParam> dict)
    {
        var values = dict.Values.ToList();
        for (var i = 0; i < values.Count; i++)
        {
            var pp = values[i];
            Merge(progressionParam, pp);
        }
    }
    public static void Merge(ProgressionParam progressionParam, ProgressionParam pp)
    {
        // do not copy the Progression string
        if (pp.ShowPHS != progressionParam.ShowPHS) pp.ShowPHS = progressionParam.ShowPHS;
        if (pp.ShowXYZ != progressionParam.ShowXYZ) pp.ShowXYZ = progressionParam.ShowXYZ;
        if (pp.ShowYZ != progressionParam.ShowYZ) pp.ShowYZ = progressionParam.ShowYZ;
        if (pp.ShowY != progressionParam.ShowY) pp.ShowY = progressionParam.ShowY;
        if (pp.ObjectMapping != progressionParam.ObjectMapping) pp.ObjectMapping = progressionParam.ObjectMapping;
        if (pp.ShowDistance != progressionParam.ShowDistance) pp.ShowDistance = progressionParam.ShowDistance;
        if (pp.ShowOctave != progressionParam.ShowOctave) pp.ShowOctave = progressionParam.ShowOctave;
        if (pp.ShowNoteAs != progressionParam.ShowNoteAs) pp.ShowNoteAs = progressionParam.ShowNoteAs;
        if (pp.ShowHull != progressionParam.ShowHull) pp.ShowHull = progressionParam.ShowHull;
        if (pp.XYZStaffLayout != progressionParam.XYZStaffLayout) pp.XYZStaffLayout = progressionParam.XYZStaffLayout;
        if (pp.DistanceType != progressionParam.DistanceType) pp.DistanceType = progressionParam.DistanceType;
        if (pp.ContentAlignment != progressionParam.ContentAlignment) pp.ContentAlignment = progressionParam.ContentAlignment;

        var isSnapshotSelectionsSameExceptIndex = pp.SnapshotSelections.Count == progressionParam.SnapshotSelections.Count &&
            Enumerable.Range(0, progressionParam.SnapshotSelections.Count).All(i =>
            pp.SnapshotSelections[i].EqualsExceptIndex(progressionParam.SnapshotSelections[i]));
        var isSnapshotSelectionsSame = pp.SnapshotSelections.Count == progressionParam.SnapshotSelections.Count &&
            Enumerable.Range(0, progressionParam.SnapshotSelections.Count).All(i =>
            pp.SnapshotSelections[i].Equals(progressionParam.SnapshotSelections[i]));
        if (isSnapshotSelectionsSameExceptIndex)
        {
            for (var i = 0; i < progressionParam.SnapshotSelections.Count; i++)
            {
                if (pp.SnapshotSelections[i].Index != progressionParam.SnapshotSelections[i].Index)
                {
                    pp.SnapshotSelections[i].Index = progressionParam.SnapshotSelections[i].Index;
                }
            }
            return;
        }
        else if (!isSnapshotSelectionsSame)
        {
            if (pp.SnapshotSelections.Count != 0)
            {
                pp.SnapshotSelections.Clear();
            }
            foreach (var snapshoParam in progressionParam.SnapshotSelections)
            {
                pp.SnapshotSelections.Add(snapshoParam);
            }
        }
    }
}
