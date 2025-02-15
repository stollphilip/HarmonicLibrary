using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisLib
{
    /// <summary>
    /// a snapshot of a GroupMap
    /// </summary>
    public class GroupMapSnapshot
    {
        public GroupMapType Type { get; set; }
        public List<List<ToneMap>> ToneMaps /* [iChordPair][iTone] */{ get; set; }
        public GroupMapDistanceAlgorithm Distance;
        public bool IsCover { get; set; }
        public bool IsSingleComponent { get; set; }

        public List<Group> GetDistinctGroups()
        {
            var list = new List<Group>();
            foreach (var toneMap in ToneMaps.SelectMany(t => t).Where(t => t.Group != null))
            {
                var group = toneMap.Group;
                if (group.Type == GroupType.VerticalGroup)
                {
                    foreach (var child in group.Children)
                    {
                        list.Add(child.Group);
                    }
                }
                else if (group.Type == GroupType.Group)
                {
                    foreach (var child in group.Children)
                    {
                        list.Add(child.Group);
                    }
                }
                else
                {
                    throw new InvalidOperationException("group must be VerticalGroup or Group");
                }
            }
            return list.Distinct().ToList();
        }
        public List<Group> GetDistinctGroups(int iChord)
        {
            var list = new List<Group>();
            foreach (var toneMap in ToneMaps.SelectMany(t => t).Where(t => t.Group != null))
            {
                var group = toneMap.Group;
                foreach (var child in group.Children.Where(c => c.Group.ChordIndex == iChord))
                {
                    Debug.Assert(child.Group.Type == GroupType.Group || child.Group.Type == GroupType.Tone);
                    list.Add(child.Group);
                }
            }
            return list.Distinct().ToList();
        }
        private List<Group> GetGroups()
        {
            var list = new List<Group>();
            foreach (var toneMap in ToneMaps.SelectMany(t => t).Where(t => t.Group != null))
            {
                var group = toneMap.Group;
                if (group.Type == GroupType.VerticalGroup)
                {
                    foreach (var child in group.Children)
                    {
                        list.Add(child.Group);
                    }
                }
                else if (group.Type == GroupType.Group)
                {
                    list.Add(group);
                }
                else
                {
                    throw new InvalidOperationException("group must be VerticalGroup or Group");
                }
            }
            return list.Distinct().ToList();
        }
        /// <summary>
        /// Return distinct position classes of the groups in the snapshot.
        /// </summary>
        /// <param name="frame">use VerticalGroupMap.Frame</param>
        public List<Vector<int>> DistinctPositionClass(Group frame)
        {
            if (frame.Type != GroupType.Frame)
            {
                throw new ArgumentException("frame must be Frame.", nameof(frame));
            }
            // Positions are normalized with respect to the frame.
            var positionClasses = new List<Vector<int>>();
            foreach (var group in GetGroups())
            {
                var position = group.Position + VectorConverter.Normalize(group.Position, frame.Position);
                var pos = VectorConverter.CreateVector(0, position[1], position[2]);
                positionClasses.Add(pos);
            }
            return positionClasses.Distinct().OrderBy(p => p[2]).ThenBy(p => p[1]).ToList();
        }
        /// <summary>
        /// Return distinct position classes of the groups in the snapshot.
        /// </summary>
        /// <param name="frame">use HorizontalGroupMap.Frame</param>
        public List<Vector<int>> DistinctPositionClass(Group frame, int iChord)
        {
            if (frame.Type != GroupType.Frame)
            {
                throw new ArgumentException("frame must be Frame.", nameof(frame));
            }
            // Positions are normalized with respect to the frame.
            var positionClasses = new List<Vector<int>>();
            foreach (var group in GetDistinctGroups(iChord))
            {
                var position = group.Position + VectorConverter.Normalize(group.Position, frame.Position);
                var pos = VectorConverter.CreateVector(0, position[1], position[2]);
                positionClasses.Add(pos);
            }
            return positionClasses.Distinct().OrderBy(p => p[2]).ThenBy(p => p[1]).ToList();
        }
        public void DebugWrite()
        {
            switch (Type)
            {
                case GroupMapType.GroupMap:
                case GroupMapType.VerticalGroupMap:
                    //var distinct = ToneMaps[0].Select(t => t.Group).Distinct().OrderBy(t => t!.Distance.Pairwise).ToList();
                    var distinct = ToneMaps.SelectMany(toneMap => toneMap).Select(t => t.Group).Distinct().OrderBy(t => t!.Distance.Pairwise).ToList();
                    foreach (var group in distinct)
                    {
                        int count = ToneMaps[0].Count(t => t.Group == group);
                        switch (count)
                        {
                            case 0:
                                break;
                            case 1:
                                GroupMap.PrintVerticalGroup(group, " ->");
                                break;
                            default:
                                GroupMap.PrintVerticalGroup(group, "<->");
                                break;
                        }
                    }
                    Debug.WriteLine(null);
                    break;
                case GroupMapType.HorizontalGroupMap:
                    distinct = ToneMaps[0].Select(t => t.Group).Distinct().OrderBy(t => t!.Distance.Pairwise).ToList();
                    var distinct_j = ToneMaps[1].Select(t => t.Group).Distinct().OrderBy(t => t!.Distance.Pairwise).ToList();
                    foreach (var group in distinct.Intersect(distinct_j))
                    {
                        GroupMap.PrintVerticalGroup(group, "<->");
                    }
                    foreach (var group in distinct.Except(distinct_j))
                    {
                        GroupMap.PrintVerticalGroup(group, " ->");
                    }
                    foreach (var group in distinct_j.Except(distinct))
                    {
                        GroupMap.PrintVerticalGroup(group, "<- ");
                    }
                    Debug.WriteLine(null);
                    break;
            }
        }
    }
}
