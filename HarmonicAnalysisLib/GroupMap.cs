using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;   

namespace HarmonicAnalysisLib
{
    /// <summary>
    /// set of mappings from Tone to Group. Implements an agglomerative hierarchical clustering.
    /// </summary>
    public class GroupMap
    {
        public GroupMapType Type { get; set; }
        public Group Frame { get; private set; }
        // ToneToGroups[0] tones are in Frame.Children[0]
        // ToneToGroups[1] tones are in Frame.Children[1]
        // ToneToGroups gets updated every time Next() is called, and depending, a Snapshot gets taken
        public List<List<ToneMap>> ToneToGroups /* [iChordPair][iTone] */ { get; }
        public List<Group> AvailableGroups { get; private set; }
        public int index { get; protected set; } = -1;
        public List<GroupMapSnapshot> Snapshots /* [iSnapshot] */ { get; set; } = new List<GroupMapSnapshot>();
        public int SnapshotIndex { get; set; } = -1;
        public GroupMapDistanceAlgorithm Distance;
        // Tones that are in one Frame but not in the other
        // For VerticalGroups, these are aliases. For HorizontalGroups, these are or are not aliases.
        public List<List<ToneMap>> Aliases /* [iChord][iTone] */ { get; private set; }

        public GroupMap() { }
        public GroupMap(GroupMapType type, Group frame, List<Group> availableGroups, bool initialize)
        {
            //--------------------------------------------------------
            // The object model is:
            //  GroupMap
            //      Frame
            //      ToneMap ToneToGroups
            //          Tone
            //          Child TonePosition
            //              Tone
            //          Group
            //          Child GroupPosition
            //              Group
            //      GroupMapSnapshot
            //          ToneMap ToneToGroups
            //              Tone
            //              Child TonePositions
            //                  Tone
            //              Group
            //              Child GroupPosition
            //                  Group

            List<ToneMap> chord1;
            List<ToneMap> chord2;
            switch (type)
            {
                case GroupMapType.GroupMap:
                    Debug.Assert(frame.Children.All(child => child.Group.Type == GroupType.Tone));
                    chord1 = frame.Children.Select(child => new ToneMap { Tone = child.Group }).ToList();
                    chord2 = new List<ToneMap>();
                    break;
                case GroupMapType.VerticalGroupMap:
                case GroupMapType.HorizontalGroupMap:
                    Debug.Assert(frame.Children.All(child => child.Group.Type == GroupType.Frame));
                    chord1 = frame.Children[0].Group.Children.Select(child => new ToneMap { Tone = child.Group }).ToList();
                    chord2 = frame.Children[1].Group.Children.Select(child => new ToneMap { Tone = child.Group }).ToList();
                    break;
                default:
                    throw new ArgumentException("type must be GroupMap, VerticalGroupMap, or HorizontalGroupMap", nameof(type));
            }
            Type = type;
            Frame = frame;
            ToneToGroups = new List<List<ToneMap>>() { chord1, chord2, };
            AvailableGroups = availableGroups;
            Distance = new GroupMapDistanceAlgorithm(this);
            Aliases = GetAliases();
            for (int iChordPair = 0; iChordPair < ToneToGroups.Count; ++iChordPair)
            {
                for (int iTone = 0; iTone < ToneToGroups[iChordPair].Count; ++iTone)
                {
                    ToneToGroups[iChordPair][iTone].TonePosition = GetTonePosition(iChordPair, iTone);
                }
            }
            foreach (var chord in ToneToGroups)
            {
                foreach (var toneMap in chord)
                {
                    toneMap.ChildFrameIndexes = toneMap.GetChildFrames(Frame);
                }
            }
            Debug.Assert(ToneToGroups.All(chord => chord.All(toneMap => toneMap.TonePosition != null)));
            Debug.Assert(Aliases.SelectMany(a => a).All(alias => alias.ChildFrameIndexes.Count == 1));
            switch (Type)
            {
                case GroupMapType.GroupMap:
                    if (frame.Type != GroupType.Frame)
                    {
                        throw new ArgumentException("frame must be a Frame", nameof(frame));
                    }
                    break;
                case GroupMapType.VerticalGroupMap:
                    if (frame.Type != GroupType.VerticalFrame)
                    {
                        throw new ArgumentException("frame must be a VerticalFrame", nameof(frame));
                    }
                    break;
                case GroupMapType.HorizontalGroupMap:
                    if (frame.Type != GroupType.HorizontalFrame)
                    {
                        throw new ArgumentException("frame must be a HorizontalFrame", nameof(frame));
                    }
                    break;
            }
            if (initialize)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            if (index != -1)
            {
                throw new Exception("call Reset() before calling Initialize()");
            }
            while (Next())
            {

            }
            if (Type == GroupMapType.VerticalGroupMap && Snapshots.Count == 0)
            {
                // debug breakpoint
            }
        }

        public bool Next()
        {
            if (index == -1)
            {
                Snapshots.Clear();
                foreach (var chord in ToneToGroups)
                {
                    foreach (var toneMap in chord)
                    {
                        toneMap.Group = AvailableGroups.FirstOrDefault(g => g.ContainsTone(toneMap.Tone));
                        toneMap.GroupPosition = GetGroupPosition(ToneToGroups.IndexOf(chord), chord.IndexOf(toneMap));
                    }
                }
                if (IsNotNull())
                {
                    index = ToneToGroups.SelectMany(t => t).Min(t => AvailableGroups.IndexOf(t.Group));
                    TakeSnapshot();
                    return true;
                }
                return false;
            }
            while (index < AvailableGroups.Count - 1)
            {
                var group = AvailableGroups[++index];
                bool found = false;
                switch (Type)
                {
                    case GroupMapType.GroupMap:
                        /*
                        foreach (var toneMap in ToneToGroups.SelectMany(t => t))
                        {
                            if (group.IsSupersetVerticalGroup(toneMap.HorizontalGroup))
                            {
                                Debug.Assert(AvailableGroups.IndexOf(group) > AvailableGroups.IndexOf(toneMap.HorizontalGroup));
                                found = true;
                                toneMap.HorizontalGroup = group;
                            }
                        }
                        */
                        foreach (var chord in ToneToGroups)
                        {
                            foreach (var toneMap in chord)
                            {
                                if (group.IsSupersetVerticalGroup(toneMap.Group, false))
                                {
                                    Debug.Assert(AvailableGroups.IndexOf(group) > AvailableGroups.IndexOf(toneMap.Group));
                                    found = true;
                                    toneMap.Group = group;
                                    toneMap.GroupPosition = GetGroupPosition(ToneToGroups.IndexOf(chord), chord.IndexOf(toneMap));
                                }
                            }
                        }
                        if (found && IsNotNull())
                        {
                            TakeSnapshot();
                            //break;
                        }
                        break;
                    case GroupMapType.VerticalGroupMap:
                        /*
                        foreach (var toneMap in ToneToGroups.SelectMany(t => t))
                        {
                            if (group.IsSupersetVerticalGroup(toneMap.HorizontalGroup))
                            {
                                Debug.Assert(AvailableGroups.IndexOf(group) > AvailableGroups.IndexOf(toneMap.HorizontalGroup));
                                found = true;
                                toneMap.HorizontalGroup = group;
                            }
                            //Debug.Assert(group.IsSupersetVerticalGroup(toneMap.HorizontalGroup) == group.IsSuperset(toneMap.HorizontalGroup, GroupType.Tone));
                        }
                        */
                        foreach (var chord in ToneToGroups)
                        {
                            foreach (var toneMap in chord)
                            {
                                if (group.IsSupersetVerticalGroup(toneMap.Group, true))
                                {
                                    if (AvailableGroups.IndexOf(group) > AvailableGroups.IndexOf(toneMap.Group))
                                    {
                                        Debug.Assert(true);
                                    }
                                    else
                                    {
                                        //TODO: ordering VerticalGroups is more complicated
                                        //Debug.Assert(false);
                                    }
                                    found = true;
                                    toneMap.Group = group;
                                    toneMap.GroupPosition = GetGroupPosition(ToneToGroups.IndexOf(chord), chord.IndexOf(toneMap));
                                }
                                //Debug.Assert(group.IsSupersetVerticalGroup(toneMap.HorizontalGroup) == group.IsSuperset(toneMap.HorizontalGroup, GroupType.Tone));
                            }
                        }
                        if (found && !IsNotNull())
                        {
                            // debug breakpoint
                        }
                        if (found && IsNotNull())
                        {
                            TakeSnapshot();
                            //break;
                        }
                        break;
                    case GroupMapType.HorizontalGroupMap:
                        /*
                        foreach (var toneMap in ToneToGroups.SelectMany(t => t))
                        {
                            if (group.IsSupersetHorizontalGroup(toneMap.HorizontalGroup))
                            {
                                Debug.Assert(AvailableGroups.IndexOf(group) > AvailableGroups.IndexOf(toneMap.HorizontalGroup));
                                found = true;
                                toneMap.HorizontalGroup = group;
                            }
                        }
                        */
                        foreach (var chord in ToneToGroups)
                        {
                            foreach (var toneMap in chord)
                            {
                                if (group.IsSupersetHorizontalGroup(toneMap.Group))
                                {
                                    Debug.Assert(AvailableGroups.IndexOf(group) > AvailableGroups.IndexOf(toneMap.Group));
                                    found = true;
                                    toneMap.Group = group;
                                    toneMap.GroupPosition = GetGroupPosition(ToneToGroups.IndexOf(chord), chord.IndexOf(toneMap));
                                }
                            }
                        }
                        if (found /* && IsNotNull() */)
                        {
                            TakeSnapshot();
                            //break;
                        }
                        break;
                }
            }
            return index < AvailableGroups.Count - 1;
        }

        public void Reset()
        {
            index = -1;
        }

        public bool IsNotNull()
        {
            return ToneToGroups.SelectMany(t => t).All(t => t.Group != null);
        }

        /// <summary>
        /// Tests whether graph is a single component
        /// </summary>
        public bool IsSingleComponent()
        {
            int IsSingleComponent = 0;
            var processed = new List<Group>();
            var groups = ToneToGroups[0].Select(t => t.Group).Distinct().ToList();
            var set = new HashSet<Group>();
            while (processed.Count < groups.Count)
            {
                bool found = false;
                foreach (var group in groups.Where(g => processed.IndexOf(g) == -1))
                {
                    var tones = new HashSet<Group>(group.ListTones());
                    if (set.Count == 0 || set.Intersect(tones).Count() > 0)
                    {
                        found = true;
                        set.UnionWith(tones);
                        processed.Add(group);
                    }
                }
                if (!found)
                {
                    break;
                }
            }
            if (processed.Count == groups.Count)
            {
                ++IsSingleComponent;
            }
            return IsSingleComponent != 0;
        }

        /// <summary>
        /// Get ToneMaps that are not inside both Frames. For VerticalFrame these are aliases. For HorizontalFrame these are alias-like Tones.
        /// </summary>
        private List<List<ToneMap>> GetAliases()
        {
            var aliases = new List<List<ToneMap>> { new List<ToneMap>(), new List<ToneMap>() };
            if ((Frame.Type == GroupType.VerticalFrame || Frame.Type == GroupType.HorizontalFrame) && Frame.Children[0] != Frame.Children[1])
            {
                Debug.Assert(Frame.Children.All(c => c.Group.Type == GroupType.Frame));
                Debug.Assert(Frame.Children.Count == ToneToGroups.Count);
                Vector<int> lower, upper, lowerNext, upperNext;
                switch (Frame.Type)
                {
                    case GroupType.VerticalFrame:
                        Frame.FrameToBounds(out lower, out upper, out lowerNext, out upperNext);
                        break;
                    case GroupType.HorizontalFrame:
                        Frame.FrameToBounds(out lower, out upper, out lowerNext, out upperNext);
                        break;
                    default: throw new InvalidOperationException();
                }
                if (VectorConverter.PositionClassEqual(lower, lowerNext))
                {
                    return aliases;
                }

                for (int i = 0; i < ToneToGroups.Count; ++i)
                {
                    foreach (var child in Frame.Children[i].Group.Children)
                    {
                        var offset = Frame.Children[i].Offset();
                        if (offset != Vector<int>.Zero)
                        {
                            // for setting breakpoint
                        }
                        if (child.ChildWithinBounds(lower, upper, offset, GroupType.Tone) &&
                            !child.ChildWithinBounds(lowerNext, upperNext, offset, GroupType.Tone))
                        {
                            var toneMap = ToneToGroups[i].Single(chord => chord.Tone == child.Group);
                            aliases[i].Add(toneMap);
                        }
                        if (!child.ChildWithinBounds(lower, upper, offset, GroupType.Tone) &&
                            child.ChildWithinBounds(lowerNext, upperNext, offset, GroupType.Tone))
                        {
                            var toneMap = ToneToGroups[i].Single(chord => chord.Tone == child.Group);
                            aliases[i].Add(toneMap);
                        }
                    }
                }
                if (Type == GroupMapType.VerticalGroupMap)
                {
                    Debug.Assert(aliases[0].Count == aliases[1].Count);
                }
            }
            return aliases;
        }

        /// <summary>
        /// return ToneMap.Tone and Position with respect to the Frame using same index as ToneToGroups
        /// </summary>
        /// <remarks>
        /// Tone and Position depend on the Frame, so it can be initialized in the ctor
        /// </remarks>
        private Child GetTonePosition(int iChord, int iTone)
        {
            if (Frame.Type == GroupType.Frame && iChord > 0)
            {
                throw new ArgumentException("iChord must be 0 for GroupType.Frame", nameof(iChord));
            }
            Vector<int> position;
            Group tone;
            switch (Frame.Type)
            {
                case GroupType.Frame:
                    Debug.Assert(Frame.Children[iTone].Group.Type == GroupType.Tone);
                    tone = Frame.Children[iTone].Group;
                    position = Frame.Children[iTone].Position;
                    break;
                case GroupType.VerticalFrame:
                case GroupType.HorizontalFrame:
                    Debug.Assert(Frame.Children[iChord].Group.Children[iTone].Group.Type == GroupType.Tone);
                    Debug.Assert(Frame.Children[iChord].Group.Type == GroupType.Frame);
                    tone = Frame.Children[iChord].Group.Children[iTone].Group;
                    position = tone.Position + VectorConverter.Normalize(tone.Position, Frame.Children[iChord].Position);
                    // you can do vector arithmitic instead as below, but Normalize is more readable
                    //position = Frame.Children[iChord].Position - Frame.Children[iChord].Group.Position +
                    //    Frame.Children[iChord].Group.Children[iTone].Position;
                    break;
                default: throw new InvalidOperationException();
            }
            Debug.Assert(tone == ToneToGroups[iChord][iTone].Tone);
            Debug.Assert(tone.Type == GroupType.Tone);
            return new Child(tone, position);
        }

        /// <summary>
        /// return ToneMap.Group and Position with respect to the Frame using same index as ToneToGroups
        /// </summary>
        /// <remarks>
        /// Group and Position depend on the Frame and the snapshot
        /// </remarks>
        private Child? GetGroupPosition(int iChord, int iTone)
        {
            if (Frame.Type == GroupType.Frame && iChord > 0)
            {
                throw new ArgumentException("iChord must be 0 for GroupType.Frame", nameof(iChord));
            }
            Vector<int> position;
            var group = ToneToGroups[iChord][iTone].Group;
            if (group == null)
            {
                return null;
            }
            switch (Frame.Type)
            {
                case GroupType.Frame:
                    position = group.Position + VectorConverter.Normalize(group.Position, Frame.Position);
                    break;
                case GroupType.VerticalFrame:
                case GroupType.HorizontalFrame:
                    position = Frame.Children[iChord].Offset() +
                        group.Position + VectorConverter.Normalize(group.Position, Frame.Children[iChord].Group.Position);
                    break;
                default: throw new InvalidOperationException();
            }
            return new Child(group, position);
        }

        /// <summary>
        /// recursively list Groups and Child Groups of the given type or types
        /// </summary>
        /// <param name="types">usually Tone and Group</param>
        public List<(Group group, Vector<int> position)> GetGroups(params GroupType[] types)
        {
            var groups = new List<(Group group, Vector<int> position)>();
            foreach (var g in Snapshots.SelectMany(s => s.ToneMaps).SelectMany(t => t).Where(t => t.Group != null).Select(t => t.Group).Cast<Group>())
            {
                foreach (var group in g.ListGroup(types))
                {
                    if (!groups.Contains(group))
                    {
                        groups.Add(group);
                    }
                }
            }
            return groups;
        }

        public List<Group> GetDistinctGroups(params GroupType[] types)
        {
            if (Snapshots.Count == 0)
            {
                return new List<Group>();
            }
            //TODO: first Snapshot is not ideal
            return Snapshots[SnapshotIndex].ToneMaps.SelectMany(t => t)
                .Where(t => t.Group != null && types.Contains(t.Group.Type))
                .Select(t => t.Group).Cast<Group>().Distinct().ToList();
        }

        /// <summary>
        /// return the distinct vertical groups in the HorizontalGroups
        /// </summary>
        //  will be removed
        public (List<Group> groups0, List<Group> groups1) GetDistinctHorizontalGroups()
        {
            var groups0 = ToneToGroups.SelectMany(chord => chord
            .Where(toneMap => toneMap.Group != null)
            .Select(toneMap => toneMap.Group.Children[0].Group))
                .Distinct().ToList();
            var groups1 = ToneToGroups.SelectMany(chord => chord
            .Where(toneMap => toneMap.Group != null)
            .Select(toneMap => toneMap.Group.Children[1].Group))
                .Distinct().ToList();
            return (groups0, groups1);
        }

        public double AliasDistance(DistanceType type)
        {
            if (Aliases.Count == 2 && Aliases.All(chord => chord.Count == 1))
            {
                var aliases = Aliases.Select(chord => chord[0]).ToList();
                var snapshot = Snapshots.Any(s => s.IsSingleComponent) ?
                    Snapshots.First(s => s.IsSingleComponent) :
                    Snapshots[SnapshotIndex];
                var groups = new List<Group>();
                foreach (var alais in aliases)
                {
                    foreach (var chordPar in snapshot.ToneMaps)
                    {
                        foreach (var toneMap in chordPar)
                        {
                            if (toneMap.TonePosition.Position == alais.TonePosition.Position)
                            {
                                groups.Add(toneMap.Group);
                            }
                        }
                    }
                }
                var a = new HashSet<Vector<int>>(groups[0].Children.Select(c => c.Position));
                var b = new HashSet<Vector<int>>(groups[1].Children.Select(c => c.Position));
                var intersect = a.Intersect(b).ToList();
                Debug.Assert(groups.Count == 2);
                switch (type)
                {
                    case DistanceType.Pairwise:
                        return groups[0].Distance.Pairwise + groups[1].Distance.Pairwise;
                    case DistanceType.Average:
                        return groups[0].Distance.Average + groups[1].Distance.Average;
                    case DistanceType.Weighted:
                        return groups[0].Distance.Weighted + groups[1].Distance.Weighted;
                    case DistanceType.CompoundPairwise:
                        return groups[0].Distance.CompoundPairwise + groups[1].Distance.CompoundPairwise;
                    case DistanceType.CompoundAverage:
                        return groups[0].Distance.CompoundAverage + groups[1].Distance.CompoundAverage;
                    case DistanceType.CompoundWeighted:
                        return groups[0].Distance.CompoundWeighted + groups[1].Distance.CompoundWeighted;
                }
            }
            return -1;
        }
        /*
        /// <summary>
        /// Frame does not require alias handling
        /// </summary>
        // will be removed
        public bool IsSimpleFrame(ToneMap toneMap)
        {
            return Frame.Type == GroupType.Frame || (Frame.Type == GroupType.VerticalFrame && toneMap.ChildFrameIndexes.Count == 2) ||
                            Frame.Type == GroupType.HorizontalFrame;
        }
        /// <summary>
        /// Frame does require alias handling
        /// </summary>
        // will be removed
        public bool IsComplexFrame(ToneMap toneMap)
        {
            return Frame.Type == GroupType.VerticalFrame && toneMap.ChildFrameIndexes.Count == 1;
        }
        */

        /// <summary>
        /// Get the Frame's start and end chord indexes
        /// </summary>
        public (int start, int end) GetChordIndexes()
        {
            int start = Frame.ChordIndex;
            int end = Frame.Children.Count == 2 ? Frame.Children[1].Group.ChordIndex : Frame.ChordIndex;
            return (start, end);
        }

        public void Validate()
        {
            switch (Type)
            {
                case GroupMapType.GroupMap:
                    Debug.Assert(Frame.Type == GroupType.Frame);
                    break;
                case GroupMapType.VerticalGroupMap:
                    Debug.Assert(Frame.Type == GroupType.VerticalFrame);
                    break;
                case GroupMapType.HorizontalGroupMap:
                    Debug.Assert(Frame.Type == GroupType.HorizontalFrame);
                    break;
            }
            var tones = new List<Group>();
            Frame.ListTones(ref tones);
            // each Tone corresponds to a ToneMap, and vice versa
            int expectedCount = Type == GroupMapType.VerticalGroupMap ? 2 : 1;
            foreach (var tone in tones)
            {
                Debug.Assert(ToneToGroups.SelectMany(t => t).Count(t => t.Tone == tone) == expectedCount);
            }
            foreach (var toneMap in ToneToGroups.SelectMany(t => t).Select(t => t))
            {
                Debug.Assert(tones.Count(t => t == toneMap.Tone) == 1);
            }
            foreach (var toneMap in ToneToGroups.SelectMany(t => t).Select(t => t))
            {
                // Tone and TonePosition agree
                Debug.Assert(toneMap.Tone.Type == GroupType.Tone);
                Debug.Assert(toneMap.Tone == toneMap.TonePosition.Group);

                // Group and GroupPosition agree
                Debug.Assert((toneMap.Group == null && toneMap.GroupPosition == null) || (toneMap.Group != null && toneMap.GroupPosition != null));
                if (toneMap.GroupPosition != null)
                {
                    Debug.Assert(toneMap.Group.Type == toneMap.GroupPosition.Group.Type);
                }

            }
            Debug.Assert(Aliases.Count == 2);
            if (Aliases.All(a => a.Count == 0))
            {
                foreach (var toneMap in ToneToGroups.SelectMany(t => t).Select(t => t))
                {
                    //TODO uncomment
                    //Debug.Assert(Frame.Type == GroupType.Frame && toneMap.ChildFrameIndexes.Count == 0 ||
                    //    Frame.Type == GroupType.VerticalFrame && toneMap.ChildFrameIndexes.Count == 2 ||
                    //    Frame.Type == GroupType.HorizontalFrame && toneMap.ChildFrameIndexes.Count == 2);

                    //Debug.Assert(toneMap.ChildFrameIndexes.Count == 0 || 
                    //    toneMap.ChildFrameIndexes.Count == 2 && toneMap.ChildFrameIndexes[0] == 0 && toneMap.ChildFrameIndexes[1] == 1);
                }
            }
            else
            {
                foreach (var toneMap in ToneToGroups.SelectMany(t => t).Select(t => t))
                {
                    var a = Aliases.SelectMany(b => b).SingleOrDefault(b => b == toneMap);
                    if (a != null)
                    {
                        //TODO uncomment
                        //Debug.Assert(a.ChildFrameIndexes.Count == 1);
                    }
                    else
                    {
                        //TODO uncomment
                        //Debug.Assert(Frame.Type == GroupType.Frame && toneMap.ChildFrameIndexes.Count == 0 ||
                        //    (Frame.Type == GroupType.VerticalFrame || Frame.Type == GroupType.HorizontalFrame) &&
                        //    toneMap.ChildFrameIndexes.Count == 2 && toneMap.ChildFrameIndexes[0] == 0 && toneMap.ChildFrameIndexes[1] == 1);
                    }
                }
            }
        }
        public override string ToString()
        {
            return $"{Type} Frame: {Frame} ToneToGroups: {ToneToGroups[0].Count} {ToneToGroups[1].Count} Snapshots {Snapshots.Count}";
        }
        public static void PrintVerticalGroup(Group group, string arrow)
        {
            group.ValidateGroupType(GroupType.Group, GroupType.VerticalGroup, GroupType.HorizontalGroup);

            List<Group> groups;
            if (group.Children.Count == 2)
            {
                groups = group.Children[0].Group.ListTones().ToList();
                for (int i = 0; i < 4; ++i)
                {
                    if (i < groups.Count)
                    {
                        Debug.Write($"{groups[i].Tone.PitchHeight,3} ");
                    }
                    else
                    {
                        Debug.Write($"{string.Empty,3} ");
                    }
                }
                Debug.Write($"{group.Distance.Pairwise,3} {arrow} ");
                groups = group.Children[1].Group.ListTones().ToList();
                for (int i = 0; i < 4; ++i)
                {
                    if (i < groups.Count)
                    {
                        Debug.Write($"{groups[i].Tone.PitchHeight,3} ");
                    }
                    else
                    {
                        Debug.Write($"{string.Empty,3} ");
                    }
                }
                Debug.Write($"{group.Children[0].Group.Distance.Pairwise,3} {group.Distance.Pairwise,3} {group.Children[1].Group.Distance.Pairwise,3} {group.Distance.CompoundPairwise,3} {group.Children[0].Group.Distance.Weighted,5:F1} {group.Distance.Weighted,5:F1} {group.Children[1].Group.Distance.Weighted,5:F1} {group.Distance.CompoundWeighted,5:F1}");
                Debug.Write(" : ");
                VectorConverter.DebugWriteVector(group.Children[0].Position);
                Debug.Write(" : ");
                VectorConverter.DebugWriteVector(group.Children[1].Position);
                Debug.WriteLine(null);
            }
            else if (group.Children.Count > 2)
            {
                groups = group.ListTones().ToList();
                for (int i = 0; i < 4; ++i)
                {
                    if (i < groups.Count)
                    {
                        Debug.Write($"{groups[i].Tone.PitchHeight,3} ");
                    }
                    else
                    {
                        Debug.Write($"{string.Empty,3} ");
                    }
                }
                Debug.Write($"{group.Distance.Pairwise,3} {arrow} ");
                groups = group.ListTones().ToList();
                for (int i = 0; i < 4; ++i)
                {
                    if (i < groups.Count)
                    {
                        Debug.Write($"{groups[i].Tone.PitchHeight,3} ");
                    }
                    else
                    {
                        Debug.Write($"{string.Empty,3} ");
                    }
                }
                Debug.Write($"{group.Distance.Pairwise,3} {0,3} {group.Distance.Pairwise,3} {group.Distance.CompoundPairwise,3} {group.Distance.Weighted,5:F1} {0,5:F1} {group.Distance.Weighted,5:F1} {group.Distance.CompoundWeighted,5:F1}");
                Debug.Write(" : ");
                VectorConverter.DebugWriteVector(group.Position);
            }
            else
            {
                throw new InvalidOperationException("group Children count must be 2 or greater.");
            }
        }
        public static void PrintHorizontalGroup(Group group, string arrow)
        {
            group.ValidateGroupType(GroupType.HorizontalGroup);

            var groups = group.Children[0].Group.ListTones().ToList();
            for (int i = 0; i < 4; ++i)
            {
                if (i < groups.Count)
                {
                    Debug.Write($"{groups[i].Tone.PitchHeight,3} ");
                }
                else
                {
                    Debug.Write($"{string.Empty,3} ");
                }
            }
            Debug.Write($"{group.Distance.Pairwise,3} {arrow} ");
            groups = group.Children[1].Group.ListTones().ToList();
            for (int i = 0; i < 4; ++i)
            {
                if (i < groups.Count)
                {
                    Debug.Write($"{groups[i].Tone.PitchHeight,3} ");
                }
                else
                {
                    Debug.Write($"{string.Empty,3} ");
                }
            }
            Debug.Write($"{group.Children[0].Group.Distance.Pairwise,3} {group.Distance.Pairwise,3} {group.Children[1].Group.Distance.Pairwise,3} {group.Distance.CompoundPairwise,3} {group.Children[0].Group.Distance.Weighted,5:F1} {group.Distance.Weighted,5:F1} {group.Children[1].Group.Distance.Weighted,5:F1} {group.Distance.CompoundWeighted,5:F1}");
            Debug.Write(" : ");
            VectorConverter.DebugWriteVector(group.Children[0].Position);
            Debug.Write(" : ");
            VectorConverter.DebugWriteVector(group.Children[1].Position);
            Debug.WriteLine(null);
        }

        public void DebugWrite()
        {
            var distinct = ToneToGroups[0].Select(t => t.Group).Distinct().OrderBy(t => t!.Distance.Pairwise).ToList();
            var distinct_j = ToneToGroups[1].Select(t => t.Group).Distinct().OrderBy(t => t!.Distance.Pairwise).ToList();
            foreach (var group in distinct.Intersect(distinct_j))
            {
                PrintHorizontalGroup(group, "<->");
            }
            foreach (var group in distinct.Except(distinct_j))
            {
                PrintHorizontalGroup(group, " ->");
            }
            foreach (var group in distinct_j.Except(distinct))
            {
                PrintHorizontalGroup(group, "<- ");
            }
        }

        public bool DebugValid()
        {
            if (!IsNotNull())
            {
                return true;
            }
            var valid = true;
            //for (int i = 0; i < glb.Length; i++)
            //{
            //    var min = ToneToGroups[i].Min(t => t.HorizontalGroup.Children[i].Position[1]);
            //    if (glb[i][1] != min)
            //    {
            //        valid = false;
            //    }
            //}
            return valid;
        }

        public List<List<ToneMap>> ToneToGroupsCopy()
        {
            return ToneToGroups.Select(x => x.Select(y => new ToneMap { Tone = y.Tone, TonePosition = y.TonePosition, Group = y.Group, GroupPosition = y.GroupPosition }).ToList()).ToList();
        }
        private void TakeSnapshot()
        {
            Snapshots.Add(new GroupMapSnapshot 
            {
                Type = Type,
                // make copy of list, since ToneMaps will be modified
                ToneMaps = ToneToGroupsCopy(),
                Distance = new GroupMapDistanceAlgorithm(this),
                IsCover = IsNotNull(),
                IsSingleComponent = IsSingleComponent()
            });
            // TODO: kludge simulates auto-selecting snapshot
            SnapshotIndex = Math.Max(0, Snapshots.Count / 2);
        }
    }
}
