using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HarmonicAnalysisCommonLib;
using HarmonicAnalysisCommonLib.Quarantine;
using ScoreLibrary.Base;
using ScoreLibrary.Parameter;

namespace ScoreLibrary
{
    /// <summary>
    /// represents the progression on multiple Staffs
    /// </summary>
    /// <remarks>responsible for adding Staffs, updating layout and location of Staffs. responsible for setting locations on Grid</remarks>
    public class Progression
    {
        //protected scorepartwise scorepartwise;
        public Dictionary<StaffTypeEnum, ProgressionParam> staffParameters { get; } = new Dictionary<StaffTypeEnum, ProgressionParam>();
        /// <summary>
        /// multiple Staffs added to the Progression 
        /// </summary>
        /// <remarks>Can be accessed by DictionaryKey or as a list.</remarks>
        private Dictionary<StaffTypeEnum, StaffProgressionStructBase> staffProgressionStructs = new Dictionary<StaffTypeEnum, StaffProgressionStructBase>();
        private List<StaffProgressionStructBase> StaffProgressions = new List<StaffProgressionStructBase>();
        private static GroupLite GroupLiteComparer = new GroupLite();
        /// <summary>
        /// Location of the Progression with respect to the master Grid
        /// </summary>
        public LocationStruct Location
        {
            get; private set;
        }
        //protected List<ChordData> chordData
        //{
        //    get; set;
        //} = new List<ChordData>();

        public Progression(/*scorepartwise scorepartwise,*/ Dictionary<StaffTypeEnum, ProgressionParam> dict)
        {
            //this.scorepartwise = scorepartwise;
            staffParameters = dict;
        }

        #region public interface

        /// <summary>
        /// Add multiple staffs to the progression and initialize, according to the staff parameters.
        /// </summary>
        public void InitStaffs()
        {
            foreach (var key in staffParameters.Keys)
            {
                Add(key, staffParameters[key]);
            }
        }

        /// <summary>
        /// Get the selected snapshot from the ProgressionParam
        /// </summary>
        /// <param name="segmentIndex">Index of the Segment</param>
        /// <param name="frameIndex">Index of FrameStruct in the Segment</param>
        /// <param name="count">Number of FrameStructs in the Segment</param>
        /// <returns>Snapshot index</returns>
        public int GetSelectedSnapshot(int segmentIndex, int frameIndex, int count)
        {
            var snapshotIndex = 0;
            var snapshotParams = staffParameters[StaffTypeEnum.PitchHeight].SnapshotSelections.Where(snap => snap.Segment == segmentIndex && snap.Frame == frameIndex).ToList(); ;
            if (snapshotParams.Count == 0)
            {
                // the snapshots have not yet been initialized
                snapshotIndex = count - 1;
            }
            else if (snapshotParams.Count == 1)
            {
                snapshotIndex = snapshotParams[0].Index;
                Debug.Assert(snapshotParams[0].Count == count);
            }
            else
            {
                throw new InvalidOperationException();
            }
            return snapshotIndex;
        }
        public GroupMapSnapshotLite LookupSnapshot(int segmentIndex, int frameIndex, List<SegmentLite> segments, ObservableCollection<SnapshotParam> SnapshotSelections)
        {
            var snapshotParam = SnapshotSelections.SingleOrDefault(x => x.Segment == segmentIndex && x.Frame == frameIndex);
            int snapshotIndex;
            if (snapshotParam != null)
            {
                snapshotIndex = snapshotParam.Index;
            }
            else
            {
                snapshotIndex = Math.Max(0, segments[segmentIndex].FrameStructs[frameIndex].GroupMap.Snapshots.Count - 1);
            }
            return segments[segmentIndex].FrameStructs[frameIndex].GroupMap.Snapshots[snapshotIndex];
        }
        // TODO: select snapshot
        /// <summary>
        /// Add chords to multiple staffs
        /// </summary>
        public void InitChords(List<SegmentLite> segments)
        {
            var list = new List<string>();
            for (var s = 0; s < segments.Count; s++)
            {
                var segment = segments[s];
                var segmentDist = segment.Distance;
                list.Add($"/Segment[{s}]/Distance");
                for (var f = 0; f < segment.FrameStructs.Count; f++)
                {
                    var frameStruct = segment.FrameStructs[f];
                    var groupMap = frameStruct.GroupMap;
                    var groupMapDist = groupMap.Distance;
                    list.Add($"/Segment[{s}]/FrameStruct[{f}]/{groupMap.Type}/Distance");

                    var groupMapFrame = groupMap.Frame;
                    var groupMapFrameDist = groupMapFrame.Distance;
                    list.Add($"/Segment[{s}]/FrameStruct[{f}]/{groupMap.Type}/{groupMapFrame.Type}/Distance");
                    for (var c = 0; c < groupMapFrame.Children.Count; c++)
                    {
                        var child = groupMapFrame.Children[c];
                        var childGroup = child.Group;
                        if (childGroup.Children.Count == 0)
                        {
                            list.Add($"/Segment[{s}]/FrameStruct[{f}]/{groupMap.Type}/{groupMapFrame.Type}/Children[{c}]/{childGroup.Type}");
                            break;
                        }
                        else
                        {
                            list.Add($"/Segment[{s}]/FrameStruct[{f}]/{groupMap.Type}/{groupMapFrame.Type}/Children[{c}]/{childGroup.Type}/Distance");
                        }
                    }
                    for (var snap = groupMap.Snapshots.Count / 2/*0*/; snap < groupMap.Snapshots.Count; snap++)
                    {
                        var snapshot = groupMap.Snapshots[snap];
                        var snapshotDistance = snapshot.Distance;
                        list.Add($"/Segment[{s}]/FrameStruct[{f}]/{groupMap.Type}/Snapshot[{snap}]/Distance");
                        for (var t = 0; t < snapshot.ToneMaps.Count; t++)
                        {
                            var toneMap = snapshot.ToneMaps[t];
                            for (var u = 0; u < toneMap.Count; u++)
                            {
                                var tone = toneMap[u];
                                var toneMapGroup = tone.Group;
                                var toneMapGroupDist = toneMapGroup.Distance;
                                if (toneMapGroup.Children.Count == 0)
                                {
                                    var tone_ = toneMapGroup.Tone;
                                    list.Add($"/Segment[{s}]/FrameStruct[{f}]/{groupMap.Type}/Snapshot[{snap}]/ToneMap[{t}][{u}]/{toneMapGroup.Type}");
                                }
                                else
                                {
                                    list.Add($"/Segment[{s}]/FrameStruct[{f}]/{groupMap.Type}/Snapshot[{snap}]/ToneMap[{t}][{u}]/{toneMapGroup.Type}/Distance");
                                    for (var c = 0; c < toneMapGroup.Children.Count; c++)
                                    {
                                        var child = toneMapGroup.Children[c];
                                        var childGroup = child.Group;
                                        if (childGroup.Children.Count == 0)
                                        {
                                            list.Add($"/Segment[{s}]/FrameStruct[{f}]/{groupMap.Type}/Snapshot[{snap}]/ToneMap[{t}][{u}]/{toneMapGroup.Type}/Children[{c}]/{childGroup.Type}");
                                            break;
                                        }
                                        else
                                        {
                                            list.Add($"/Segment[{s}]/FrameStruct[{f}]/{groupMap.Type}/Snapshot[{snap}]/ToneMap[{t}][{u}]/{toneMapGroup.Type}/Children[{c}]/{childGroup.Type}/Distance");
                                        }
                                    }
                                }
                            }
                        }
                        break;
                    }
                }
            }
            //foreach (var s in list)
            //{
            //    Debug.WriteLine(s);
            //}
            ChordData chordData;
            ColumnData columnData;
            BorderData borderData;
            GroupLite group;
            int numberOfColumns;
            int chordIndex = 0;

            var chordDataList = new List<ChordData>();
            for (var segmentIndex = 0; segmentIndex < segments.Count; segmentIndex++)
            {
                var segment = segments[segmentIndex];
                // new ChordData
                chordData = new ChordData();
                chordDataList.Add(chordData);

                var objectMapping = staffParameters[StaffTypeEnum.PitchHeight].ObjectMapping;
                switch (objectMapping)
                {
                    case ObjectMappings.None:
                        break;
                    case ObjectMappings.Group_2_Child_in_1_col:
                        for (var frameStructIndex = 0; frameStructIndex < segment.FrameStructs.Count; frameStructIndex++)
                        {
                            var frameStruct = segment.FrameStructs[frameStructIndex];
                            if (frameStruct.GroupMap.Type != GroupMapType.GroupMap)
                            {
                                continue;
                            }
                            var snapshot = LookupSnapshot(segmentIndex, frameStructIndex, segments, staffParameters[StaffTypeEnum.PitchHeight].SnapshotSelections);

                            var groups = snapshot.ToneMaps.SelectMany(A => A.Select(B => B.Group)).Distinct().ToList();
                            groups = groups.Distinct(GroupLiteComparer).ToList();
                            for (var first = 0; first < groups.Count; first++)
                            {
                                // determine the number of columns required
                                // new ColumnData
                                columnData = new ColumnData();
                                chordData.ColumnData.Add(columnData);

                                columnData.AddBorder(groups[first].GetBorderData());
                                // init ColumnData
                                columnData.ChordIndex = chordIndex++;
                                // set columnData.notes to the union of borderData.notes
                                columnData.notes = columnData.ListNotes();
                            }
                        }
                        break;
                    case ObjectMappings.Group_2_Child_in_2_col:
                        for (var frameStructIndex = 0; frameStructIndex < segment.FrameStructs.Count; frameStructIndex++)
                        {
                            var frameStruct = segment.FrameStructs[frameStructIndex];
                            if (frameStruct.GroupMap.Type != GroupMapType.GroupMap)
                            {
                                continue;
                            }
                            var snapshot = LookupSnapshot(segmentIndex, frameStructIndex, segments, staffParameters[StaffTypeEnum.PitchHeight].SnapshotSelections);

                            var groups = snapshot.ToneMaps.SelectMany(A => A.Select(B => B.Group)).Distinct().ToList();
                            groups = groups.Distinct(GroupLiteComparer).ToList();
                            for (var first = 0; first < groups.Count; first++)
                            {
                                if (groups[first].Children.All(c => c.Group.Children.Count == 0))
                                {
                                    // determine the number of columns required
                                    // new ColumnData
                                    columnData = new ColumnData();
                                    chordData.ColumnData.Add(columnData);

                                    // add borders to the appropriate columns
                                    // new BorderData
                                    borderData = groups[first].GetBorderData();
                                    columnData.AddBorder(borderData);
                                    // init ColumnData
                                    columnData.ChordIndex = chordIndex++;
                                    // set columnData.notes to the union of borderData.notes
                                    columnData.notes = columnData.ListNotes();
                                    continue;
                                }
                                foreach (var childGroup in groups[first].Children.Select(c => c.Group))
                                {
                                    // determine the number of columns required
                                    // new ColumnData
                                    columnData = new ColumnData();
                                    chordData.ColumnData.Add(columnData);

                                    // add borders to the appropriate columns
                                    // new BorderData
                                    borderData = childGroup.GetBorderData(/*snapshot.Distance*/);
                                    columnData.AddBorder(borderData);
                                    // init ColumnData
                                    columnData.ChordIndex = chordIndex++;
                                    // set columnData.notes to the union of borderData.notes
                                    columnData.notes = columnData.ListNotes();
                                }
                            }
                        }
                        break;
                    case ObjectMappings.HorizGroup_2_Child_in_1_col:
                        for (var frameStructIndex = 0; frameStructIndex < segment.FrameStructs.Count; frameStructIndex++)
                        {
                            var frameStruct = segment.FrameStructs[frameStructIndex];
                            if (frameStruct.GroupMap.Type != GroupMapType.VerticalGroupMap && frameStruct.GroupMap.Type != GroupMapType.HorizontalGroupMap)
                            {
                                continue;
                            }
                            var snapshot = LookupSnapshot(segmentIndex, frameStructIndex, segments, staffParameters[StaffTypeEnum.PitchHeight].SnapshotSelections);

                            var groups = snapshot.ToneMaps.SelectMany(A => A.Select(B => B.Group)).Distinct().ToList();
                            groups = groups.Distinct(GroupLiteComparer).ToList();
                            for (var first = 0; first < groups.Count; first++)
                            {
                                // determine the number of columns required
                                // new ColumnData
                                columnData = new ColumnData();
                                chordData.ColumnData.Add(columnData);
                                foreach (var childGroup in groups[first].Children.Select(c => c.Group))
                                {
                                    // add borders to the appropriate columns
                                    // new BorderData
                                    borderData = childGroup.GetBorderData();
                                    columnData.AddBorder(borderData);
                                    // init ColumnData
                                    columnData.ChordIndex = chordIndex++;
                                    // set columnData.notes to the union of borderData.notes
                                    columnData.notes = columnData.ListNotes();
                                }
                            }
                        }
                        break;
                    case ObjectMappings.HorizGroup_2_Child_in_2_col:
                        for (var frameStructIndex = 0; frameStructIndex < segment.FrameStructs.Count; frameStructIndex++)
                        {
                            var frameStruct = segment.FrameStructs[frameStructIndex];
                            if (frameStruct.GroupMap.Type != GroupMapType.VerticalGroupMap && frameStruct.GroupMap.Type != GroupMapType.HorizontalGroupMap)
                            {
                                continue;
                            }
                            var snapshot = LookupSnapshot(segmentIndex, frameStructIndex, segments, staffParameters[StaffTypeEnum.PitchHeight].SnapshotSelections);

                            var groups = snapshot.ToneMaps.SelectMany(A => A.Select(B => B.Group)).Distinct().ToList();
                            groups = groups.Distinct(GroupLiteComparer).ToList();
                            for (var first = 0; first < groups.Count; first++)
                            {
                                foreach (var childGroup in groups[first].Children.Select(c => c.Group))
                                {
                                    // determine the number of columns required
                                    // new ColumnData
                                    columnData = new ColumnData();
                                    chordData.ColumnData.Add(columnData);

                                    // add borders to the appropriate columns
                                    // new BorderData
                                    borderData = childGroup.GetBorderData();
                                    columnData.AddBorder(borderData);
                                    // init ColumnData
                                    columnData.ChordIndex = chordIndex++;
                                    // set columnData.notes to the union of borderData.notes
                                    columnData.notes = columnData.ListNotes();
                                }
                            }
                        }
                        break;
                    case ObjectMappings.Frame_in_1_col:
                        // get the first frame of each segment and the last frame of the last segment
                        var group_distance = new List<(GroupLite group, IDistanceAlgorithmLite distance)>();
                        for (var frameStructIndex = 0; frameStructIndex < segment.FrameStructs.Count; frameStructIndex++)
                        {
                            if (frameStructIndex == 0 || segmentIndex == segments.Count - 1 && frameStructIndex == segment.FrameStructs.Count - 1)
                            {
                                var snapshot = LookupSnapshot(segmentIndex, frameStructIndex, segments, staffParameters[StaffTypeEnum.PitchHeight].SnapshotSelections);
                                group_distance.Add((segment.FrameStructs[frameStructIndex].GroupMap.Frame, snapshot.Distance));
                            }
                        }
                        foreach (var g in group_distance)
                        {
                            group = g.group;
                            // determine the number of columns required
                            // new ColumnData
                            columnData = new ColumnData();
                            chordData.ColumnData.Add(columnData);

                            // add borders to the appropriate columns
                            // new BorderData
                            borderData = group.GetBorderData(/*g.distance*/);
                            columnData.AddBorder(borderData);

                            // init ColumnData
                            columnData.ChordIndex = chordIndex++;
                            // set columnData.notes to the union of borderData.notes
                            columnData.notes = columnData.ListNotes();
                        }
                        /*
                        foreach (var frameStruct in segment.FrameStruct.Where(fs => fs.GroupMap.Type == GroupMapType.GroupMap))
                        {
                            var frame = frameStruct.GroupMap.Frame;
                            group = segment.FrameStruct[0].GroupMap.Frame;
                            Debug.Assert(group.Type == GroupType.Frame);

                            // determine the number of columns required
                            // new ColumnData
                            columnData = new ColumnData();
                            chordData.ColumnData.Add(columnData);

                            // add borders to the appropriate columns
                            // new BorderData
                            borderData = group.GetBorderData();
                            columnData.AddBorder(borderData);

                            // init ColumnData
                            columnData.ChordIndex = chordIndex++;
                            // set columnData.notes to the union of borderData.notes
                            columnData.notes = columnData.ListNotes();
                        }
                        */
                        break;
                    case ObjectMappings.HorizFrame_in_1_col:
                        // useless
                        foreach (var frameStruct in segment.FrameStructs.Where(fs => fs.GroupMap.Type == GroupMapType.HorizontalGroupMap))
                        {
                            var frame = frameStruct.GroupMap.Frame;
                            Debug.Assert(frame.Type == GroupType.HorizontalFrame);

                            // determine the number of columns required
                            // new ColumnData
                            columnData = new ColumnData();
                            chordData.ColumnData.Add(columnData);
                            foreach (var g in frame.Children.Select(c => c.Group))
                            {
                                // add borders to the appropriate columns
                                // new BorderData
                                borderData = g.GetBorderData();
                                columnData.AddBorder(borderData);
                                // init ColumnData
                                columnData.ChordIndex = chordIndex++;
                                // set columnData.notes to the union of borderData.notes
                                columnData.notes = columnData.ListNotes();
                            }
                        }
                        break;
                    case ObjectMappings.HorizFrame_2_Frames_in_1_col:
                        for (var frameStructIndex = 0; frameStructIndex < segment.FrameStructs.Count; frameStructIndex++)
                        {
                            var frameStruct = segment.FrameStructs[frameStructIndex];
                            if (frameStruct.GroupMap.Type != GroupMapType.VerticalGroupMap && frameStruct.GroupMap.Type != GroupMapType.HorizontalGroupMap)
                            {
                                continue;
                            }
                            var snapshot = LookupSnapshot(segmentIndex, frameStructIndex, segments, staffParameters[StaffTypeEnum.PitchHeight].SnapshotSelections);

                            var frame = frameStruct.GroupMap.Frame;

                            // determine the number of columns required
                            // new ColumnData
                            columnData = new ColumnData();
                            chordData.ColumnData.Add(columnData);
                            foreach (var g in frame.Children.Select(c => c.Group))
                            {
                                // add borders to the appropriate columns
                                // new BorderData
                                borderData = g.GetBorderData(/*snapshot.Distance*/);
                                columnData.AddBorder(borderData);
                                // init ColumnData
                                columnData.ChordIndex = chordIndex++;
                                // set columnData.notes to the union of borderData.notes
                                columnData.notes = columnData.ListNotes();
                            }
                        }
                        break;
                    case ObjectMappings.HorizFrame_2_Frames_in_2_cols:
                        for (var frameStructIndex = 0; frameStructIndex < segment.FrameStructs.Count; frameStructIndex++)
                        {
                            var frameStruct = segment.FrameStructs[frameStructIndex];
                            if (frameStruct.GroupMap.Type != GroupMapType.VerticalGroupMap && frameStruct.GroupMap.Type != GroupMapType.HorizontalGroupMap)
                            {
                                continue;
                            }
                            var snapshot = LookupSnapshot(segmentIndex, frameStructIndex, segments, staffParameters[StaffTypeEnum.PitchHeight].SnapshotSelections);

                            var frame = frameStruct.GroupMap.Frame;

                            chordIndex = segmentIndex;
                            foreach (var g in frame.Children.Select(c => c.Group))
                            {
                                // determine the number of columns required
                                // new ColumnData
                                columnData = new ColumnData();
                                chordData.ColumnData.Add(columnData);

                                // add borders to the appropriate columns
                                // new BorderData
                                borderData = g.GetBorderData(/*snapshot.Distance*/);
                                columnData.AddBorder(borderData);
                                // init ColumnData
                                columnData.ChordIndex = chordIndex++;
                                // set columnData.notes to the union of borderData.notes
                                columnData.notes = columnData.ListNotes();
                            }
                        }
                        break;
                    case ObjectMappings.HorizFrame_N_DualGroups_in_2_Cols:
                        for (var frameIndex = 0; frameIndex < segment.FrameStructs.Count; frameIndex++)
                        {
                            var frameStruct = segment.FrameStructs[frameIndex];
                            if (segment.FrameStructs[frameIndex].GroupMap.Type != GroupMapType.HorizontalGroupMap)
                            {
                                continue;
                            }
                        }
                        foreach (var frameStruct in segment.FrameStructs.Where(fs => fs.GroupMap.Type == GroupMapType.HorizontalGroupMap))
                        {
                            // determine the snapshot index
                            var count = frameStruct.GroupMap.Snapshots.Count;
                            var frameIndex = segment.FrameStructs.IndexOf(frameStruct);
                            var snapshotIndex = GetSelectedSnapshot(segmentIndex, frameIndex, count);
                            //Debug.WriteLine($"segment {segmentIndex} snapshotIndex: {snapshotIndex}");
                            var groups = frameStruct.GroupMap.Snapshots[snapshotIndex].ToneMaps.SelectMany(A => A.Select(B => B.Group)).Distinct().ToList();
                            groups = groups.Distinct(GroupLiteComparer).ToList();
                            var initialGroups = groups.Select(g => g.Children[0].Group).ToList();
                            var finalGroups = groups.Select(g => g.Children[0].Group).ToList();
                            // determine the number of columns required
                            // new ColumnData
                            foreach (var i in Enumerable.Range(0, groups.Count))
                            {
                                chordData.ColumnData.Add(new ColumnData());
                            }
                            for (var first = 0; first < groups.Count; first++)
                            {
                                var second = first + groups.Count;
                                // add borders to the appropriate columns
                                // new BorderData
                                chordData.ColumnData[first].AddBorder(groups[first].Children[0].Group.GetBorderData());
                                chordData.ColumnData[first].AddBorder(groups[first].Children[1].Group.GetBorderData());
                                // init ColumnData
                                chordData.ColumnData[first].ChordIndex = chordIndex++;
                                // set columnData.notes to the union of borderData.notes
                                chordData.ColumnData[first].notes = chordData.ColumnData[first].ListNotes();
                            }
                        }
                        break;
                    case ObjectMappings.HorizFrame_N_DualGroups_in_2N_Cols:
                        foreach (var frameStruct in segment.FrameStructs.Where(fs => fs.GroupMap.Type == GroupMapType.HorizontalGroupMap))
                        {
                            var count = frameStruct.GroupMap.Snapshots.Count;
                            // determine the snapshot index
                            var frameIndex = segment.FrameStructs.IndexOf(frameStruct);
                            var snapshotIndex = GetSelectedSnapshot(segmentIndex, frameIndex, count);
                            //Debug.WriteLine($"segment {segmentIndex} snapshotIndex: {snapshotIndex}");
                            var groups = frameStruct.GroupMap.Snapshots[snapshotIndex].ToneMaps.SelectMany(A => A.Select(B => B.Group)).Distinct().ToList();
                            // determine the number of columns required
                            // new ColumnData
                            foreach (var i in Enumerable.Range(0, groups.Count * 2))
                            {
                                chordData.ColumnData.Add(new ColumnData());
                            }
                            for (var first = 0; first < groups.Count; first++)
                            {
                                var second = first + groups.Count;
                                // add borders to the appropriate columns
                                // new BorderData
                                chordData.ColumnData[first].AddBorder(groups[first].Children[0].Group.GetBorderData());
                                chordData.ColumnData[second].AddBorder(groups[first].Children[1].Group.GetBorderData());
                                // init ColumnData
                                chordData.ColumnData[first].ChordIndex = chordIndex++;
                                chordData.ColumnData[second].ChordIndex = chordIndex++;
                                // set columnData.notes to the union of borderData.notes
                                chordData.ColumnData[first].notes = chordData.ColumnData[first].ListNotes();
                                chordData.ColumnData[second].notes = chordData.ColumnData[second].ListNotes();
                            }
                        }
                        break;
                    case ObjectMappings.HorizFrame_N_DualGroups_in_2N_Cols_alternating:
                        foreach (var frameStruct in segment.FrameStructs.Where(fs => fs.GroupMap.Type == GroupMapType.HorizontalGroupMap))
                        {
                            // for now, take the last snapshot, it's more interesting
                            var count = frameStruct.GroupMap.Snapshots.Count;
                            // determine the snapshot index
                            var frameIndex = segment.FrameStructs.IndexOf(frameStruct);
                            var snapshotIndex = GetSelectedSnapshot(segmentIndex, frameIndex, count);
                            //Debug.WriteLine($"segment {segmentIndex} snapshotIndex: {snapshotIndex}");
                            var groups = frameStruct.GroupMap.Snapshots[snapshotIndex].ToneMaps.SelectMany(A => A.Select(B => B.Group)).Distinct().ToList();
                            // determine the number of columns required
                            // new ColumnData
                            foreach (var i in Enumerable.Range(0, groups.Count * 2))
                            {
                                chordData.ColumnData.Add(new ColumnData());
                            }
                            for (var first = 0; first < groups.Count; first++)
                            {
                                // add borders to the appropriate columns
                                // new BorderData
                                chordData.ColumnData[first * 2].AddBorder(groups[first].Children[0].Group.GetBorderData());
                                chordData.ColumnData[first * 2 + 1].AddBorder(groups[first].Children[1].Group.GetBorderData());
                                // init ColumnData
                                chordData.ColumnData[first * 2].ChordIndex = chordIndex++;
                                chordData.ColumnData[first * 2 + 1].ChordIndex = chordIndex++;
                                // set columnData.notes to the union of borderData.notes
                                chordData.ColumnData[first * 2].notes = chordData.ColumnData[first * 2].ListNotes();
                                chordData.ColumnData[first * 2 + 1].notes = chordData.ColumnData[first * 2 + 1].ListNotes();
                            }
                        }
                        break;
                    case ObjectMappings.VertFrame_in_1_col:
                        // useless
                        foreach (var frameStruct in segment.FrameStructs.Where(fs => fs.GroupMap.Type == GroupMapType.VerticalGroupMap))
                        {
                            var frame = frameStruct.GroupMap.Frame;
                            Debug.Assert(frame.Type == GroupType.VerticalFrame);

                            // determine the number of columns required
                            // new ColumnData
                            columnData = new ColumnData();
                            chordData.ColumnData.Add(columnData);
                            foreach (var g in frame.Children.Select(c => c.Group))
                            {
                                // add borders to the appropriate columns
                                // new BorderData
                                borderData = g.GetBorderData();
                                columnData.AddBorder(borderData);
                                // init ColumnData
                                columnData.ChordIndex = chordIndex++;
                                // set columnData.notes to the union of borderData.notes
                                columnData.notes = columnData.ListNotes();
                            }
                        }
                        break;
                    case ObjectMappings.VertFrame_2_Frames_in_1_col:
                        foreach (var frameStruct in segment.FrameStructs.Where(fs => fs.GroupMap.Type == GroupMapType.VerticalGroupMap))
                        {
                            var frame = frameStruct.GroupMap.Frame;
                            group = segment.FrameStructs[0].GroupMap.Frame;
                            Debug.Assert(group.Type == GroupType.Frame);

                            // determine the number of columns required
                            // new ColumnData
                            columnData = new ColumnData();
                            chordData.ColumnData.Add(columnData);
                            foreach (var g in frame.Children.Select(c => c.Group))
                            {
                                // add borders to the appropriate columns
                                // new BorderData
                                borderData = g.GetBorderData();
                                columnData.AddBorder(borderData);
                                // init ColumnData
                                columnData.ChordIndex = chordIndex++;
                                // set columnData.notes to the union of borderData.notes
                                columnData.notes = columnData.ListNotes();
                            }
                        }
                        break;
                    case ObjectMappings.VertFrame_2_Frames_in_2_cols:
                        foreach (var frameStruct in segment.FrameStructs.Where(fs => fs.GroupMap.Type == GroupMapType.VerticalGroupMap))
                        {
                            var frame = frameStruct.GroupMap.Frame;

                            chordIndex = segmentIndex;
                            foreach (var g in frame.Children.Select(c => c.Group))
                            {
                                // determine the number of columns required
                                // new ColumnData
                                columnData = new ColumnData();
                                chordData.ColumnData.Add(columnData);

                                // add borders to the appropriate columns
                                // new BorderData
                                borderData = g.GetBorderData();
                                columnData.AddBorder(borderData);
                                // init ColumnData
                                columnData.ChordIndex = chordIndex++;
                                // set columnData.notes to the union of borderData.notes
                                columnData.notes = columnData.ListNotes();
                            }
                        }
                        break;
                    case ObjectMappings.VertFrame_N_DualGroups_in_2_Cols:
                        foreach (var frameStruct in segment.FrameStructs.Where(fs => fs.GroupMap.Type == GroupMapType.VerticalGroupMap))
                        {
                            var count = frameStruct.GroupMap.Snapshots.Count;
                            // determine the snapshot index
                            var frameIndex = segment.FrameStructs.IndexOf(frameStruct);
                            var snapshotIndex = GetSelectedSnapshot(segmentIndex, frameIndex, count);
                            var groups = frameStruct.GroupMap.Snapshots[snapshotIndex].ToneMaps.SelectMany(A => A.Select(B => B.Group)).Distinct().ToList();
                            groups = groups.Distinct(GroupLiteComparer).ToList();
                            var initialGroups = groups.Select(g => g.Children[0].Group).ToList();
                            var finalGroups = groups.Select(g => g.Children[0].Group).ToList();
                            // determine the number of columns required
                            // new ColumnData
                            foreach (var i in Enumerable.Range(0, groups.Count))
                            {
                                chordData.ColumnData.Add(new ColumnData());
                            }
                            for (var first = 0; first < groups.Count; first++)
                            {
                                var second = first + groups.Count;
                                // add borders to the appropriate columns
                                // new BorderData
                                chordData.ColumnData[first].AddBorder(groups[first].Children[0].Group.GetBorderData());
                                chordData.ColumnData[first].AddBorder(groups[first].Children[1].Group.GetBorderData());
                                // init ColumnData
                                chordData.ColumnData[first].ChordIndex = chordIndex++;
                                // set columnData.notes to the union of borderData.notes
                                chordData.ColumnData[first].notes = chordData.ColumnData[first].ListNotes();
                            }
                        }
                        break;
                    case ObjectMappings.VertFrame_N_DualGroups_in_2N_Cols:
                        foreach (var frameStruct in segment.FrameStructs.Where(fs => fs.GroupMap.Type == GroupMapType.VerticalGroupMap))
                        {
                            var count = frameStruct.GroupMap.Snapshots.Count;
                            // determine the snapshot index
                            var frameIndex = segment.FrameStructs.IndexOf(frameStruct);
                            var snapshotIndex = GetSelectedSnapshot(segmentIndex, frameIndex, count);
                            var groups = frameStruct.GroupMap.Snapshots[snapshotIndex].ToneMaps.SelectMany(A => A.Select(B => B.Group)).Distinct().ToList();
                            // determine the number of columns required
                            // new ColumnData
                            foreach (var i in Enumerable.Range(0, groups.Count * 2))
                            {
                                chordData.ColumnData.Add(new ColumnData());
                            }
                            for (var first = 0; first < groups.Count; first++)
                            {
                                var second = first + groups.Count;
                                // add borders to the appropriate columns
                                // new BorderData
                                chordData.ColumnData[first].AddBorder(groups[first].Children[0].Group.GetBorderData());
                                chordData.ColumnData[second].AddBorder(groups[first].Children[1].Group.GetBorderData());
                                // init ColumnData
                                chordData.ColumnData[first].ChordIndex = chordIndex++;
                                chordData.ColumnData[second].ChordIndex = chordIndex++;
                                // set columnData.notes to the union of borderData.notes
                                chordData.ColumnData[first].notes = chordData.ColumnData[first].ListNotes();
                                chordData.ColumnData[second].notes = chordData.ColumnData[second].ListNotes();
                            }
                        }
                        break;
                    case ObjectMappings.VertFrame_N_DualGroups_in_2N_Cols_alternating:
                        foreach (var frameStruct in segment.FrameStructs.Where(fs => fs.GroupMap.Type == GroupMapType.VerticalGroupMap))
                        {
                            // for now, take the last snapshot, it's more interesting
                            var count = frameStruct.GroupMap.Snapshots.Count;
                            // determine the snapshot index
                            var frameIndex = segment.FrameStructs.IndexOf(frameStruct);
                            var snapshotIndex = GetSelectedSnapshot(segmentIndex, frameIndex, count);
                            var groups = frameStruct.GroupMap.Snapshots[snapshotIndex].ToneMaps.SelectMany(A => A.Select(B => B.Group)).Distinct().ToList();
                            // determine the number of columns required
                            // new ColumnData
                            foreach (var i in Enumerable.Range(0, groups.Count * 2))
                            {
                                chordData.ColumnData.Add(new ColumnData());
                            }
                            for (var first = 0; first < groups.Count; first++)
                            {
                                // add borders to the appropriate columns
                                // new BorderData
                                chordData.ColumnData[first * 2].AddBorder(groups[first].Children[0].Group.GetBorderData());
                                chordData.ColumnData[first * 2 + 1].AddBorder(groups[first].Children[1].Group.GetBorderData());
                                // init ColumnData
                                chordData.ColumnData[first * 2].ChordIndex = chordIndex++;
                                chordData.ColumnData[first * 2 + 1].ChordIndex = chordIndex++;
                                // set columnData.notes to the union of borderData.notes
                                chordData.ColumnData[first * 2].notes = chordData.ColumnData[first * 2].ListNotes();
                                chordData.ColumnData[first * 2 + 1].notes = chordData.ColumnData[first * 2 + 1].ListNotes();
                            }
                        }
                        break;
                    case ObjectMappings.Segment_in_1_col:
                        var frameStructs = new List<FrameStructLite> { segment.FrameStructs.First() };
                        Debug.Assert(frameStructs[0].GroupMap.Type == GroupMapType.GroupMap);
                        if (segmentIndex == segments.Count - 1)
                        {
                            frameStructs.Add(segment.FrameStructs.Last());
                            Debug.Assert(frameStructs[segments.Count - 1].GroupMap.Type == GroupMapType.GroupMap);
                        }
                        for (var i = 0; i < frameStructs.Count; i++)
                        {
                            var frameStruct = frameStructs[i];
                            var frame = frameStruct.GroupMap.Frame;
                            Debug.Assert(frame.Type == GroupType.Frame);

                            // determine the number of columns required
                            // new ColumnData
                            columnData = new ColumnData();
                            chordData.ColumnData.Add(columnData);
                            // add borders to the appropriate columns
                            // new BorderData
                            borderData = frame.GetBorderData(segment.Distance);
                            columnData.AddBorder(borderData);
                            // init ColumnData
                            columnData.ChordIndex = chordIndex++;
                            // set columnData.notes to the union of borderData.notes
                            columnData.notes = columnData.ListNotes();
                            /*
                            foreach (var g in frame.Children.Select(c => c.Group))
                            {
                                // add borders to the appropriate columns
                                // new BorderData
                                borderData = g.GetBorderData(segment.Distance);
                                columnData.AddBorder(borderData);
                                // init ColumnData
                                columnData.ChordIndex = chordIndex++;
                                // set columnData.notes to the union of borderData.notes
                                columnData.notes = columnData.ListNotes();
                            }
                            */
                        }
                        break;
                    default:
                        break;
                }
                // there are no chords, so display and empty column
                if (chordData.ColumnData.Count == 0)
                {
                    //chordData.ColumnData.Add(new ColumnData { notes = new List<note>()});
                    foreach (var frameStruct in segment.FrameStructs.Where(fs => fs.GroupMap.Type == GroupMapType.GroupMap))
                    {
                        var frame = frameStruct.GroupMap.Frame;
                        group = segment.FrameStructs[0].GroupMap.Frame;
                        Debug.Assert(group.Type == GroupType.Frame);

                        // determine the number of columns required
                        // new ColumnData
                        columnData = new ColumnData();
                        chordData.ColumnData.Add(columnData);

                        // add borders to the appropriate columns
                        // new BorderData
                        borderData = group.GetBorderData();
                        borderData.notes = new List<ToneLite> { borderData.notes[0] };
                        columnData.AddBorder(borderData);

                        // init ColumnData
                        columnData.ChordIndex = chordIndex++;
                        // set columnData.notes to the union of borderData.notes
                        columnData.notes = columnData.ListNotes();
                        // add a hidden note
                        // Tag: ChordWithoutNotes - do not remove this comment
                        var hiddenNote = new ToneLite(columnData.notes[0].PitchHeight,  columnData.notes[0].Position, columnData.notes[0].PositionFinal, Accidental.None, hidden: true);
                        columnData.notes = new List<ToneLite> { hiddenNote };
                        borderData.notes = new List<ToneLite> { hiddenNote };
                    }
                }
            }
            foreach (var chord in chordDataList)
            {
                foreach (var column in chord.ColumnData)
                {
                    foreach (var staffProgression in StaffProgressions)
                    {
                        staffProgression.AddChord(column);
                    }
                }
            }
        }

        /// <summary>
        /// Chord locations surrounding all the Element locations
        /// </summary>
        public void UpdateChordLocations1()
        {
            var chordCount = ChordCount();
            foreach (var staffProgression in StaffProgressions)
            {
                for (var indexChord = 0; indexChord < chordCount; indexChord++)
                {
                    staffProgression.ChordStructs[indexChord].UpdateLocation();
                }
            }
        }

        /// <summary>
        /// horizontally align Chords in all Staffs
        /// </summary>
        public void UpdateChordLocations2()
        {
            const int margin = 1;
            var chordCount = ChordCount();
            var column = margin;
            for (var indexChord = 0; indexChord < chordCount; indexChord++)
            {
                var maxColumnSpan = int.MinValue;
                foreach (var staffProgression in StaffProgressions)
                {
                    var columnSpan = staffProgression.ChordStructs[indexChord].Location.ColumnSpan;
                    // If you want horizontal location of Chords to be the same regardless of ShowStaff, then comment out /*&& staffProgression.ShowStaff*/
                    if (columnSpan > maxColumnSpan && staffProgression.ShowStaff)
                    {
                        maxColumnSpan = columnSpan;
                    }
                    staffProgression.ChordStructs[indexChord].Location.Column = column;
                }
                column += maxColumnSpan + margin;
            }
        }

        /// <summary>
        /// vertically align Chords in each Staffs
        /// </summary>
        public void UpdateChordLocations3()
        {
            var chordCount = ChordCount();
            //
            if (chordCount == 0)
            {
                if (StaffProgressions.All(s => s.ChordStructs.Count == 0))
                {
                    return;
                }
            }
            //
            for (var indexStaff = 0; indexStaff < StaffProgressions.Count; indexStaff++)
            {
                var staffProgression = StaffProgressions[indexStaff];
                var margin = indexStaff == 0 ? 1 : 3;
                //margin = 0;
                var minRow = staffProgression.ChordStructs
                    .Where(c => c.Visibility == Visibility.Visible)
                    .Min(c => c.Location.Row);
                // for PitchHeightStaff, row 0 (minRow) is constant
                if (staffProgression.StaffType == StaffTypeEnum.PitchHeight)
                {
                    // TODO: figure out why 1 and not 0
                    minRow = 1;
                }

                for (var indexChord = 0; indexChord < chordCount; indexChord++)
                {
                    var chord = staffProgression.ChordStructs[indexChord];
                    foreach (var element in chord.elementStructs.Where(e => e.Visibility != Visibility.Collapsed))
                    {
                        element.Location.Row += -minRow + margin;
                        //Debug.Assert(element.Location.Row >= margin);
                    }
                    chord.Location.Row += -minRow + margin;
                    //Debug.Assert(chord.Location.Row >= margin);
                    //Debug.Assert(chord.elementStructs.All(e => chord.Location.Row + e.Location.Row >= margin));
                    if (indexChord == 0 && (staffProgression.StaffType == StaffTypeEnum.PitchHeight || staffProgression.StaffType == StaffTypeEnum.Y))
                    {
                        //Debug.Assert(chord.DeltaRow == 0);
                        chord.DeltaRow = -minRow + margin;
                    }
                    //Debug.WriteLine($"chord {indexChord}: ");
                    //foreach (var element in chord.elementStructs.Where(e => e.Tag == "note"))
                    //{
                    //    Debug.Write($"{element.Location} <{element.Position}>");
                    //}
                    //Debug.WriteLine(null);
                }
            }
        }

        /// <summary>
        /// update locations of Staffs
        /// </summary>
        /// <remarks>update each Staff's location, and update each Chord's location so Chords are horizontally aligned</remarks>
        public void UpdateStaffLocations()
        {
            if (ChordCount() == 0)
            {
                if (StaffProgressions.All(s => s.ChordStructs.Count == 0))
                {
                    return;
                }
            }
            var columnSpan = StaffProgressions
                .Where(staff => staff.ShowStaff)
                .Max(staff => staff.ChordStructs.Max(chord => chord.Location.Column + chord.Location.ColumnSpan));
            var row = 0;

            // allocate Rows to each Staff
            foreach (var staffProgression in StaffProgressions/*.Where(s => s.ShowStaff)*/)
            {
                if (staffProgression.ShowStaff)
                {
                    int max_row;
                    if (staffProgression.StaffType == StaffTypeEnum.PitchHeight)
                    {
                        // the height of PitchHeightStaff is constant
                        max_row = StepStruct.StepToRow.Single(x => x.step == "C2").row;
                    }
                    else
                    {
                        // the height of all other staffs is the maximum row of all Chord heights
                        max_row = staffProgression.ChordStructs.Max(chord => chord.Location.Row + chord.Location.RowSpan);
                    }
                    staffProgression.Location = new LocationStruct(row, 0, max_row, columnSpan);
                    row += max_row;
                }
                else
                {
                    // there are two lines of code responsible for hiding Staff:
                    // staffProgression.Location = new LocationStruct(row, 0, 0, columnSpan);
                    // if (columnSpan > maxColumnSpan && staffProgression.ShowStaff)

                    // Hide the staff.
                    // TODO: should rowSpan be 0?
                    staffProgression.Location = new LocationStruct(row, 0, 0, columnSpan);
                }
            }
        }

        /// <summary>
        /// upldate location of Progression
        /// </summary>
        public void UpdateProgressionLocation()
        {
            //
            if (StaffProgressions.All(staff => staff.Location == null))
            {
                return;
            }
            //
            Location = new LocationStruct(0, 0,
                StaffProgressions.Sum(staff => staff.Location.RowSpan),
                StaffProgressions.Max(staff => staff.Location.ColumnSpan));
        }

        /// <summary>
        /// Add Staff Lines to YStaff
        /// </summary>
        public void AddStaffLines()
        {
            foreach (var staffProgression in StaffProgressions.Where(s => s.ShowStaff))
            {
                staffProgression.AddStaffLines();
            }
        }
        /// <summary>
        /// Set the Grid Row, Column, RowSpan, ColumnSpan recursively for the Progression
        /// </summary>
        public virtual void SetGrid(int row, int column, int rowSpan, int columnSpan)
        {
            if (Location == null)
            {
                return;
            }
            foreach (var staff in StaffProgressions)
            {
                staff.SetGrid(Location.Row, Location.Column, Location.RowSpan, Location.ColumnSpan);
            }
        }

        /// <summary>
        /// Add the FrameworkElements to the Grid
        /// </summary>
        /// <param name="clearGrid">Action to clear child elements from the grid</param>
        /// <param name="addToGrid">Action to add child elements to the grid</param>
        public void AddToGrid(Action clearGrid, Action<List<FrameworkElement>> addToGrid = null)
        {
            clearGrid();
            foreach (var staffProgression in StaffProgressions)
            {
                if (staffProgression.ShowStaff)
                {
                    foreach (var chordStruct in staffProgression.ChordStructIterator())
                    {
                        if (chordStruct.Visibility == Visibility.Visible)
                        {
                            //var loc = chordStruct.LocationRelativeStaff;
                            var list = new List<FrameworkElement>();
                            foreach (var elementStruct in chordStruct.ElementStructIterator())
                            {
                                // Tag: ChordWithoutNotes - do not remove this comment
                                if (elementStruct.Visibility == Visibility.Hidden)
                                {
                                    continue;
                                }
                                //elementStruct.SetElement(chordStruct);
                                list.Add(elementStruct.Element);
                            }
                            addToGrid(list);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// add a Staff
        /// </summary>
        public void Add(StaffTypeEnum staffType, ProgressionParam staffParameter)
        {
            if (staffProgressionStructs.Keys.Contains(staffType))
            {
                return;
            }
            switch (staffType)
            {
                case StaffTypeEnum.None:
                    break;
                case StaffTypeEnum.PitchHeight:
                    staffProgressionStructs.Add(StaffTypeEnum.PitchHeight, new PitchHeightStaff.StaffProgressionStruct(staffParameter));
                    break;
                case StaffTypeEnum.XYZ:
                    staffProgressionStructs.Add(StaffTypeEnum.XYZ, new XYZStaff.StaffProgressionStruct(staffParameter));
                    break;
                case StaffTypeEnum.YZ:
                    staffProgressionStructs.Add(StaffTypeEnum.YZ, new YZStaff.StaffProgressionStruct(staffParameter));
                    break;
                case StaffTypeEnum.Y:
                    staffProgressionStructs.Add(StaffTypeEnum.Y, new YStaff.StaffProgressionStruct(staffParameter));
                    break;
            }
            StaffProgressions = staffProgressionStructs.Values.ToList();
        }
        #endregion

        private int ChordCount()
        {
            foreach (var staff in StaffProgressions)
            {
                // the chord count is the same for all staffs
                return staff.ChordCount();
            }
            return 0;
        }
        public void DebugWrite()
        {
            Debug.WriteLine($"Progression {Location}");
            foreach (var staff in StaffProgressions)
                staff.DebugWrite();
        }
    }
}
