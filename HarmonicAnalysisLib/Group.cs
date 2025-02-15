using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace HarmonicAnalysisLib
{
    /// <summary>
    /// Group represents different types of pitch objects
    /// </summary>
    public class Group : IEquatable<Group>
    {

        public int ChordIndex { get; set; }
        public GroupType Type { get; set; }
        public Vector<int> Position { get; set; } = new Vector<int>();
        public List<Child> Children { get; set; } = new List<Child>();
        // Tone only
        public IPitch? Tone { get; set; }

        // Frame & HorizontalFrame only
        public Vector<int>[] Bounds { get; set; } = new Vector<int>[0];
        public DistanceAlgorithm Distance { get; }
        // Dijkstra's algorithm only
        public int Index { get; set; }

        public Group()
        {
            Distance = new DistanceAlgorithm(this);
        }

        /// <summary>
        /// Validate this.Type. Usually this is called in the first line of a method.
        /// </summary>
        /// <param name="types"></param>
        /// <exception cref="ArgumentException"></exception>
        public void ValidateGroupType(params GroupType[] types)
        {
            if (!types.Contains(this.Type))
            {
                var join = string.Join(" or ", types);
                throw new ArgumentException($"group must be a {join}");
            }
        }
        /// <summary>
        /// Whether this Group is suitable to to use in GroupMapType GroupMap to represent a chord.
        /// </summary>
        /// <remarks>VerticalGroup, Group with 2 Tones, Group with N Tones where N is number of tones in Frame are all suitable</remarks>
        /// <param name="counts">expected number(s) of child Tones.</param>
        public bool IsVerticalGroupOrGroup(params int[] counts)
        {
            return Type == GroupType.VerticalGroup ||
                Type == GroupType.Group && counts.Any(count => count == Children.Count) && Children.All(c => c.Group.Type == GroupType.Tone);
        }
        /// <summary>
        /// get the bounds of the Frame
        /// </summary>
        public void FrameToBounds(out Vector<int> lower, out Vector<int> upper)
        {
            ValidateGroupType(GroupType.Frame);
            lower = Bounds[0];
            upper = Bounds[1];
        }

        /// <summary>
        /// get the bounds of the Frame
        /// </summary>
        public void FrameToBounds(out Vector<int> lower, out Vector<int> upper, out Vector<int> lowerNext, out Vector<int> upperNext)
        {
            ValidateGroupType(GroupType.VerticalFrame, GroupType.HorizontalFrame);

            lower = Children[0].Group.Bounds[0];
            upper = Children[0].Group.Bounds[1];
            lowerNext = Children[1].Group.Bounds[0] + Children[1].Offset();
            upperNext = Children[1].Group.Bounds[1] + Children[1].Offset();
        }

        /// <summary>
        /// Tests whether the child positions are within bounds
        /// </summary>
        /// <param name="glb">greatest lower bound</param>
        /// <param name="lub">least upper bound</param>
        /// <param name="types">Tests whether the specified GroupTypes are within bounds. Usually VerticalGroup Group and Tone.</param>
        /// <returns></returns>
        public bool GroupWithinBounds(Vector<int> glb, Vector<int> lub, params GroupType[] types)
        {
            ValidateGroupType(GroupType.Group, GroupType.VerticalGroup);

            var inside = true;
            var typeFound = false;
            if (types.Contains(this.Type))
            {
                typeFound = true;
                var p = this.Position;
                if (p[1] < glb[1] || lub[1] < p[1] || p[2] < glb[2] || lub[2] < p[2])
                {
                    inside = false;
                }
            }
            foreach (var c in this.Children)
            {
                if (types.Contains(c.Group.Type))
                {
                    typeFound = true;
                    var p = c.Position;
                    if (p[1] < glb[1] || lub[1] < p[1] || p[2] < glb[2] || lub[2] < p[2])
                    {
                        inside = false;
                        break;
                    }
                }
            }
            return inside && typeFound;
        }
        /// <summary>
        /// Tests whether the child positions are within range
        /// </summary>
        public bool GroupWithinRange(int maxY, int maxZ, params GroupType[] types)
        {
            var tones = ListTonePositions(Vector<int>.Zero);
            var y_max = tones.Max(t => t.Position[1]) - tones.Min(t => t.Position[1]);
            var z_max = tones.Max(t => t.Position[2]) - tones.Min(t => t.Position[2]);
            return (y_max <= maxY && z_max <= maxZ);
        }
        // will be removed
        public bool InsideFrame(Group frame, int iFrame)
        {
            Vector<int> offset;
            Vector<int> lower, upper, lowerNext, upperNext;

            switch (frame.Type)
            {
                case GroupType.Frame:
                    offset = VectorConverter.Normalize(this.Position, frame.Position);
                    frame.FrameToBounds(out lower, out upper);
                    return InsideFrame(lower, upper, offset);
                case GroupType.VerticalFrame:
                case GroupType.HorizontalFrame:
                    offset = VectorConverter.Normalize(this.Position, frame.Children[iFrame].Position);
                    frame.FrameToBounds(out lower, out upper, out lowerNext, out upperNext);
                    switch (iFrame)
                    {
                        case 0:
                            return InsideFrame(lower, upper, offset);
                        case 1:
                            return InsideFrame(lowerNext, upperNext, offset);
                        default: throw new InvalidOperationException();
                    }
                default: throw new InvalidOperationException();
            }
        }
        /// <summary>
        /// are the group's children inside frame
        /// </summary>
        /// <param name="glb">greatest lower bound</param>
        /// <param name="lub">least upper bound</param>
        /// <param name="offset">offset supports normalizing the Group's Position</param>
        // will be removed
        public bool InsideFrame(Vector<int> glb, Vector<int> lub, Vector<int> offset)
        {
            // for Group, checks that all children (Tones) are inside the frame
            // for VerticalGroup, checks that both children (Groups) are inside the frame
            // but does not check that all children of children (Tones) are inside the frame
            var inside = true;
            foreach (var c in this.Children)
            {
                var p = c/*.Group*/.Position + offset;
                for (int i = 1; i < 3; i++)
                {
                    if (p[i] < glb[i] || p[i] > lub[i])
                    {
                        inside = false;
                        break;
                    }
                }
            }
            return inside;
        }
        // will be removed
        public bool InsideFrame_new(Vector<int> glb, Vector<int> lub, Vector<int> offset)
        {
            var tones = new List<Group>();
            GetChildPositions(offset, ref tones, GroupType.Tone);
            foreach (var tone in tones)
            {
                var p = tone.Position + offset;
                for (int i = 1; i < 3; i++)
                {
                    if (p[i] < glb[i] || p[i] > lub[i])
                    {
                        return false;
                    }
                }
            }
            return tones.Count != 0; ;
        }
        /// <summary>
        /// return Groups within Frame
        /// </summary>
        /// <param name="groups">Manager.Groups</param>
        public List<Group> FrameToVerticalGroups(List<Group> groups)
        {
            ValidateGroupType(GroupType.Frame);

            Vector<int> lower, upper;
            FrameToBounds(out lower, out upper);
            var toneCount = this.ToneCount();

            return groups.Where(g => g.ChordIndex == ChordIndex && g.IsVerticalGroupOrGroup(2, toneCount) &&
                g.GroupWithinBounds(lower, upper, GroupType.VerticalGroup, GroupType.Tone, GroupType.Group)
            ).OrderBy(g => g.Distance.CompoundWeighted)
            .ThenBy(g => g.Children[0].Group.Children.Count)
            .ThenBy(g => g.Children[1].Group.Children.Count).ToList();
        }
        /// <summary>
        /// return VerticalGroups within VerticalFrame
        /// </summary>
        /// <remarks>the preferred method for getting VerticalGroups from VerticalFrames</remarks>
        public List<Group> VerticalFrameToVerticalGroups(List<Group> groups)
        {
            ValidateGroupType(GroupType.VerticalFrame);

            Vector<int> lower, upper, lowerNext, upperNext;
            FrameToBounds(out lower, out upper, out lowerNext, out upperNext);
            var list = new List<Group>();

            // include Groups that cover the entire chord and are contained in either child Frame
            // groups with > 2 children
            int toneCount = this.ToneCount();
            foreach (var g in groups.Where(G => G.ChordIndex == ChordIndex && G.Type == GroupType.Group && G.Children.Count == toneCount))
            {
                Debug.Assert(g.Children.All(c => c.Group.Type == GroupType.Tone));
                if (g.GroupWithinBounds(lower, upper, GroupType.VerticalGroup, GroupType.Tone, GroupType.Group) ||
                    g.GroupWithinBounds(lowerNext, upperNext, GroupType.VerticalGroup, GroupType.Tone, GroupType.Group))
                {
                    Debug.Assert(!list.Contains(g));
                    list.Add(g);
                }
            }
            // include VerticalGroups or Groups that have 2 children where the children are contained in the respective child Frames or in the reversed child Frames
            // groups with 2 children
            foreach (var g in groups.Where(G => G.ChordIndex == ChordIndex && G.IsVerticalGroupOrGroup(2/*, toneCount*/)))
            {
                Debug.Assert(g.Children.Count == 2);
                Debug.Assert(VectorConverter.IsNormalized(g.Position));
                var offset = VectorConverter.Normalize(g.Position, Position);
                if (offset != Vector<int>.Zero)
                {
                    // debug breakpoint
                }
                if (g.Children[0].ChildWithinBounds(lower, upper, offset, GroupType.Tone, GroupType.Group) &&
                g.Children[1].ChildWithinBounds(lowerNext, upperNext, offset, GroupType.Tone, GroupType.Group))
                {
                    Debug.Assert(!list.Contains(g));
                    list.Add(g);
                }
                else if (g.Children[1].ChildWithinBounds(lower, upper, offset, GroupType.Tone, GroupType.Group) &&
                g.Children[0].ChildWithinBounds(lowerNext, upperNext, offset, GroupType.Tone, GroupType.Group))
                {
                    Debug.Assert(!list.Contains(g));
                    list.Add(g);
                }
            }
            return list.OrderBy(g => g.Distance.CompoundWeighted)
            .ThenBy(g => g.Children[0].Group.Children.Count)
            .ThenBy(g => g.Children[1].Group.Children.Count).ToList();
        }

        /// <summary>
        /// return HorizontalGroups within HorizontalFrame
        /// </summary>
        /// <remarks>the preferred method for getting HorizontalGroups from HorizontalFrames</remarks>
        public List<Group> HorizontalFrameToHorizontalGroups(List<Group> groups)
        {
            if (Type != GroupType.HorizontalFrame)
            {
                throw new ArgumentException();
            }
            Vector<int> lower, upper, lowerNext, upperNext;
            FrameToBounds(out lower, out upper, out lowerNext, out upperNext);

            return groups.Where(g => g.Type == GroupType.HorizontalGroup && g.ChordIndex == ChordIndex &&
                g.Children[0].ChildWithinBounds(lower, upper, Vector<int>.Zero, GroupType.Tone, GroupType.Group) &&
                g.Children[1].ChildWithinBounds(lowerNext, upperNext, Vector<int>.Zero, GroupType.Tone, GroupType.Group)
            ).OrderBy(g => g.Distance.CompoundWeighted)
            .ThenBy(g => g.Children[0].Group.Children.Count)
            .ThenBy(g => g.Children[1].Group.Children.Count).ToList();
        }
        /// <summary>
        /// recursively list Groups and Child Groups of the given type or types
        /// </summary>
        /// <param name="types">usually Tone and Group</param>
        /// <remarks>don't change this signature to return chordIndex since it is already available from Group</remarks>
        public List<(Group group, Vector<int> position)> ListGroup(params GroupType[] types)
        {
            var groups = new List<(Group group, Vector<int> position)>();
            if (types.Contains(Type))
            {
                groups.Add((this, Position));
            }
            foreach (var child in Children)
            {
                foreach (var c in child.Group.ListGroup(types))
                {
                    if (!groups.Contains(c))
                    {
                        groups.Add((c.group, c.position + child.Offset()));
                    }
                }
            }
            return groups;
        }

        /// <summary>
        /// List the tones in the group recursively
        /// </summary>
        /// <param name="tones">returns the list of tones</param>
        public void ListTones(ref List<Group> tones)
        {
            if (this.Type == GroupType.Tone)
            {
                if (!tones.Contains(this))
                {
                    tones.Add(this);
                }
            }
            else if (this.Type == GroupType.Group || this.Type == GroupType.VerticalGroup || this.Type == GroupType.HorizontalGroup || this.Type == GroupType.Frame || this.Type == GroupType.VerticalFrame || this.Type == GroupType.HorizontalFrame)
            {
                foreach (var child in this.Children)
                {
                    child.Group.ListTones(ref tones);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// List the tones in the group recursively
        /// </summary>
        public List<Group> ListTones()
        {
            var tones = new List<Group>();
            ListTones(ref tones);
            return tones;
        }
        /// <summary>
        /// Count the Tones in the Group
        /// </summary>
        public int ToneCount()
        {
            switch (this.Type)
            {
                case GroupType.Tone:
                    return 1;
                case GroupType.Group:
                    return this.Children.Count;
                case GroupType.Frame:
                    return this.Children.Count;
                case GroupType.VerticalGroup:
                    // the Children of VerticalGroups can overlap
                    return ListTones().Count;
                case GroupType.HorizontalGroup:
                    return this.Children[0].Group.ToneCount();
                case GroupType.VerticalFrame:
                    return this.Children[0].Group.ToneCount();
                case GroupType.HorizontalFrame:
                    return this.Children[0].Group.ToneCount();
                default:
                    throw new NotImplementedException();
            }
        }
        /// <summary>
        /// return list of (iChord, iChordPair, Tone, Position) tuples
        /// </summary>
        /// <remarks>this method is very useful and could replace other methods</remarks>
        /// <param name="offset"></param>
        /// <param name="iChordPair">Optional parameter. Always omit.</param>
        public List<(int iChord, int iChordPair, Group Tone, Vector<int> Position)> ListTonePositions(Vector<int> offset, int iChordPair = 0)
        {
            // when a Tone in a Group has an alias, iChordPair indicated which Frame the Tone is in, to be able to distinguish the Tone from its alias
            List<(int iChord, int iChordPair, Group Tone, Vector<int> Position)> list = new List<(int iChord, int iChordPair, Group Tone, Vector<int> Position)>();
            if (Type == GroupType.Tone)
            {
                var item = (ChordIndex, iChordPair, this, Position + offset);
                list.Add(item);
            }
            else if (Type == GroupType.Group || Type == GroupType.VerticalGroup || Type == GroupType.HorizontalGroup)
            {
                foreach (var child in Children)
                {
                    if (Type == GroupType.VerticalGroup || Type == GroupType.HorizontalGroup)
                    {
                        iChordPair = Children.IndexOf(child);
                    }
                    var items = child.Group.ListTonePositions(child.Offset() + offset, iChordPair);
                    list.AddRange(items);
                }
            }
            else if (Type == GroupType.Frame || Type == GroupType.VerticalFrame || Type == GroupType.HorizontalFrame)
            {
                foreach (var child in Children)
                {
                    if (Type == GroupType.VerticalFrame || Type == GroupType.HorizontalFrame)
                    {
                        iChordPair = Children.IndexOf(child);
                    }
                    var items = child.Group.ListTonePositions(child.Offset() + offset, iChordPair);
                    list.AddRange(items);
                }
            }
            else throw new InvalidOperationException();
            return list;
        }
        /// <summary>
        /// get the child positions in the parent's coordinate system
        /// </summary>
        // will be removed
        public void GetChildPositions(Vector<int> delta, ref List<Group> groups, params GroupType[] types)
        {
            GetChildPositions(delta, ref groups, -1, types);
        }
        // will be removed
        public void GetChildPositions(Vector<int> delta, ref List<Group> groups, int iChild, params GroupType[] types)
        {
            if (delta != Vector<int>.Zero)
            {

            }
            int count = 0;
            foreach (var child in Children)
            {
                if (iChild == -1 || iChild == count)
                {
                    if (types.Contains(child.Group.Type))
                    {
                        var group = new Group
                        {
                            ChordIndex = child.Group.ChordIndex,
                            Position = child.Position + delta,
                            Type = child.Group.Type,
                            Tone = child.Group.Tone,
                        };
                        groups.Add(group);
                    }
                    else
                    {
                        var delta_ = child.Offset();
                        child.Group.GetChildPositions(delta + delta_, ref groups, types);
                    }
                }
                count++;
            }
        }
        /// <summary>
        /// Tests whether Group (this) contains Tone (argument). Only tests whether references are the same.
        /// </summary>
        /// <param name="tone"></param>
        /// <returns></returns>
        public bool ContainsTone(Group tone)
        {
            return ListTones().Contains(tone);
        }
        // will be removed
        public bool ContainsTone(Group Tone, Vector<int> Position, Vector<int> groupOffset)
        {
            // ListTonePositions checks this.Type
            var tonePositions = ListTonePositions(groupOffset);
            return tonePositions.Count(t => t.Tone == Tone && t.Position == Position) == 1;
        }
        /// <summary>
        /// is this VerticalGroup a superset of the arg VerticalGroup
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        // will be removed
        public bool IsSupersetVerticalGroup(Group group)
        {
            bool vertical = (this.Type == GroupType.VerticalGroup || this.Type == GroupType.Group) && (group.Type == GroupType.VerticalGroup || group.Type == GroupType.Group);
            if (!vertical)
            {
                throw new ArgumentException("group must be a VerticalGroup or Group", nameof(group));
            }
            bool allChordTones = this.Children.Count > 2 || group.Children.Count > 2;
            if (allChordTones)
            {
                var a = this.ListTones();
                var b = group.ListTones();
                bool same = a.All(t => b.Contains(t)) && b.All(t => a.Contains(t));
                bool superset = b.All(t => a.Contains(t)) && !a.All(t => b.Contains(t));
                return !same && superset;
            }
            else
            {
                var A = this.ListTones();
                var B = group.ListTones();
                var BsupersetA = A.All(t => B.Contains(t)) && !B.All(t => A.Contains(t));
                var AsupersetB = B.All(t => A.Contains(t)) && !A.All(t => B.Contains(t));
                return BsupersetA || AsupersetB;
            }
        }
        /// <summary>
        /// is this VerticalGroup a superset of the arg VerticalGroup
        /// </summary>
        /// <param name="useTonePositions">use false for GroupMap, or true for VerticalGroupMap</param>
        public bool IsSupersetVerticalGroup(Group group, bool useTonePositions)
        {
            ValidateGroupType(GroupType.VerticalGroup, GroupType.Group);

            // handle Group or VerticalGroup with 2 children
            // handle Group with > 2 children, i.e., that covers the chord
            // handle the 2 Children in order, and in reverse order
            bool allChordTones_this = this.Type == GroupType.Group && this.Children.Count > 2;
            bool allChordTones_arg = group.Type == GroupType.Group && group.Children.Count > 2;
            if (!useTonePositions)
            {
                // for GroupMap perform set operations on Tones, not on Positions, since there are no aliases in GroupMap
                if (allChordTones_this || allChordTones_arg)
                {
                    // handle Group with > 2 children, i.e., that covers the chord
                    var a = this.ListTones();
                    var b = group.ListTones();
                    bool same = a.All(t => b.Contains(t)) && b.All(t => a.Contains(t));
                    bool superset = b.All(t => a.Contains(t)) && !a.All(t => b.Contains(t));
                    if (!same && superset)
                    {
                        return true;
                    }
                }
                else
                {
                    // handle Group or VerticalGroup with 2 children
                    // handle the 2 Children in order, and in reverse order
                    var groupChildrenOrdered = new List<List<Child>> { new List<Child> { group.Children[0], group.Children[1] }, new List<Child> { group.Children[1], group.Children[0] } };
                    foreach (var groupChildren in groupChildrenOrdered)
                    {
                        var A = this.Children[0].Group.ListTones();
                        var B = groupChildren[0].Group.ListTones();
                        var C = this.Children[1].Group.ListTones();
                        var D = groupChildren[1].Group.ListTones();
                        var AsupersetB = B.All(t => A.Contains(t));
                        var AsameB = B.All(t => A.Contains(t)) && A.All(t => B.Contains(t));
                        var CsupersetD = D.All(t => C.Contains(t));
                        var CsameD = D.All(t => C.Contains(t)) && C.All(t => D.Contains(t));
                        var same = AsameB && CsameD;
                        var superset = AsupersetB && CsupersetD;
                        if (!same && superset)
                        {
                            return true;
                        }
                    }
                }
            }
            else if (useTonePositions)
            {
                // for VerticalGroupMap perform set operations on Positions, not on Tones, since there may be aliases in VerticalGroupMap
                if (allChordTones_this || allChordTones_arg)
                {
                    // handle Group with > 2 children, i.e., that covers the chord
                    var a = this.ListTonePositions(Vector<int>.Zero);
                    var b = group.ListTonePositions(Vector<int>.Zero);
                    bool same = a.All(t => b.Contains(t)) && b.All(t => a.Contains(t));
                    bool superset = b.All(t => a.Contains(t)) && !a.All(t => b.Contains(t));
                    if (!same && superset)
                    {
                        return true;
                    }
                }
                else
                {
                    // handle Group or VerticalGroup with 2 children
                    // handle the 2 Children in order, and in reverse order
                    var groupChildrenOrdered = new List<List<Child>> { new List<Child> { group.Children[0], group.Children[1] }, new List<Child> { group.Children[1], group.Children[0] } };
                    foreach (var groupChildren in groupChildrenOrdered)
                    {
                        var A = this.Children[0].Group.ListTonePositions(this.Children[0].Offset());
                        var B = groupChildren[0].Group.ListTonePositions(groupChildren[0].Offset());
                        var C = this.Children[1].Group.ListTonePositions(this.Children[1].Offset());
                        var D = groupChildren[1].Group.ListTonePositions(groupChildren[1].Offset());
                        var AsupersetB = B.All(t => A.Contains(t));
                        var AsameB = B.All(t => A.Contains(t)) && A.All(t => B.Contains(t));
                        var CsupersetD = D.All(t => C.Contains(t));
                        var CsameD = D.All(t => C.Contains(t)) && C.All(t => D.Contains(t));
                        var same = AsameB && CsameD;
                        var superset = AsupersetB && CsupersetD;
                        if (!same && superset)
                        {
                            return true;
                        }
                    }
                }
            }
            else throw new InvalidOperationException();
            return false;
        }

        /// <summary>
        /// is this HorizontalGroup a superset of the arg HorizontalGroup
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool IsSupersetHorizontalGroup(Group group)
        {
            bool vertical = (this.Type == GroupType.VerticalGroup || this.Type == GroupType.Group) && (group.Type == GroupType.VerticalGroup || group.Type == GroupType.Group);
            bool horizontal = this.Type == GroupType.HorizontalGroup && group.Type == GroupType.HorizontalGroup;
            if (vertical && (this.Children.Count > 2 || group.Children.Count > 2))
            {
                var a = this.ListTones();
                var b = group.ListTones();
                bool same = a.All(t => b.Contains(t)) && b.All(t => a.Contains(t));
                bool superset = b.All(t => a.Contains(t)) && !a.All(t => b.Contains(t));
                return !same && superset;
            }
            else if (vertical || horizontal)
            {
                if (this.Type == GroupType.HorizontalGroup && this.Type != group.Type)
                {
                    throw new ArgumentException("group must be a HorizontalGroup", nameof(group));
                }
                var tonesChild0 = this.Children[0].Group.ListTones();
                var tonesChild1 = this.Children[1].Group.ListTones();
                var groupTonesChild0 = group.Children[0].Group.ListTones();
                var groupTonesChild1 = group.Children[1].Group.ListTones();
                bool same0 = groupTonesChild0.All(t => tonesChild0.Contains(t)) && tonesChild0.All(t => groupTonesChild0.Contains(t));
                bool same1 = groupTonesChild1.All(t => tonesChild1.Contains(t)) && tonesChild1.All(t => groupTonesChild1.Contains(t));
                bool superset0 = groupTonesChild0.All(t => tonesChild0.Contains(t)) && !tonesChild0.All(t => groupTonesChild0.Contains(t));
                bool superset1 = groupTonesChild1.All(t => tonesChild1.Contains(t)) && !tonesChild1.All(t => groupTonesChild1.Contains(t));
                if (same0 && superset1)
                {
                    return true;
                }
                if (superset0 && same1)
                {
                    return true;
                }
                if (superset0 && superset1)
                {
                    return true;
                }
                return false;
            }
            return false;
        }
        /// <summary>
        /// Tests whether group1 is a superset of group2
        /// </summary>
        // will be removed
        public bool IsSuperset(Vector<int> offset1, Group group2, Vector<int> offset2)
        {
            Group group1 = this;
            var list1 = group1.ListTonePositions(offset1);
            var list2 = group2.ListTonePositions(offset2);
            bool isSuperset = list2.All(l2 => list1.Count(l1 => /*l1.iChord == l2.iChord && l1.iChordPair == l2.iChordPair &&*/ l1.Tone == l2.Tone && l1.Position == l2.Position) == 1);
            bool isSubset = list1.All(l1 => list2.Count(l2 => /*l2.iChord == l1.iChord && l2.iChordPair == l1.iChordPair &&*/ l2.Tone == l1.Tone && l2.Position == l1.Position) == 1);
            return isSuperset && !isSubset;
        }

        // will be removed
        public bool IsSuperset(Group group, bool checkChild0, bool checkChild1)
        {
            if (this.Type == GroupType.VerticalGroup || this.Type == GroupType.HorizontalGroup || this.Type == GroupType.Group)
            {

            }
            else
            {
                return false;
            }
            if (VectorConverter.PositionClassEqual(this.Children[0].Position, group.Children[0].Position) &&
                VectorConverter.PositionClassEqual(this.Children[1].Position, group.Children[1].Position))
            {

            }
            else
            {
                return false;
            }
            var tonesChild0 = this.Children[0].Group.ListTones();
            var tonesChild1 = this.Children[1].Group.ListTones();
            var groupTonesChild0 = group.Children[0].Group.ListTones();
            var groupTonesChild1 = group.Children[1].Group.ListTones();
            bool same0 = groupTonesChild0.All(t => tonesChild0.Contains(t)) && tonesChild0.All(t => groupTonesChild0.Contains(t));
            bool same1 = groupTonesChild1.All(t => tonesChild1.Contains(t)) && tonesChild1.All(t => groupTonesChild1.Contains(t));
            bool superset0 = groupTonesChild0.All(t => tonesChild0.Contains(t)) && !tonesChild0.All(t => groupTonesChild0.Contains(t));
            bool superset1 = groupTonesChild1.All(t => tonesChild1.Contains(t)) && !tonesChild1.All(t => groupTonesChild1.Contains(t));
            if (same0 && superset1 && !checkChild0 && checkChild1)
            {
                return true;
            }
            if (superset0 && same1 && checkChild0 && !checkChild1)
            {
                return true;
            }
            if (superset0 && superset1 && checkChild0 && checkChild1)
            {
                return true;
            }
            return false;
        }

        // class invariants go here
        public bool Validate()
        {
            var valid = true;
            switch (this.Type)
            {
                case GroupType.Tone:
                    valid = valid && this.Tone != null;
                    valid = valid && this.Tone.Position == this.Position;
                    break;
                default:
                    valid = valid && this.Tone == null;
                    break;
            }
            switch (this.Type)
            {
                case GroupType.Frame:
                    valid = valid && this.Bounds.Length == 2;
                    valid = valid && Bounds[1][0] == Children.Max(child => child.Position[0]);
                    valid = valid && Bounds[1][1] == Bounds[0][1] + 3;
                    valid = valid && Bounds[1][2] == Bounds[0][2] + 2;
                    break;
                default:
                    valid = valid && this.Bounds.Length == 0;
                    break;
            }
            switch (this.Type)
            {
                case GroupType.Tone:
                    valid = valid && this.Children.Count == 0;
                    break;
                case GroupType.Group:
                case GroupType.VerticalGroup:
                    valid = valid && this.Children.Count != 0;
                    break;
                case GroupType.Frame:
                    //valid = valid && this.Children.Count == 0;
                    break;
                case GroupType.HorizontalGroup:
                    valid = valid && this.Children.Count == 2;
                    break;
                case GroupType.VerticalFrame:
                case GroupType.HorizontalFrame:
                    valid = valid && this.Children.Count == 2;
                    break;
            }
            switch (this.Type)
            {
                case GroupType.Tone:
                    valid = valid && VectorConverter.IsNormalized(this.Position);
                    break;
                case GroupType.Group:
                case GroupType.VerticalGroup:
                case GroupType.Frame:
                case GroupType.HorizontalGroup:
                case GroupType.VerticalFrame:
                case GroupType.HorizontalFrame:
                    valid = valid && VectorConverter.IsNormalized(this.Position);
                    break;
            }
            switch (this.Type)
            {
                case GroupType.Tone:
                case GroupType.Group:
                case GroupType.VerticalGroup:
                case GroupType.Frame:
                case GroupType.HorizontalGroup:
                case GroupType.VerticalFrame:
                case GroupType.HorizontalFrame:
                    valid = valid && ValidateChordIndex(this.ChordIndex);
                    break;
            }
            switch (this.Type)
            {
                case GroupType.Tone:
                    break;
                case GroupType.Group:
                case GroupType.VerticalGroup:
                    var a = PitchConverter.GreatestLowerBound(this.Children.Select(c => c.Position).ToList());
                    valid = valid && this.Position == PitchConverter.GreatestLowerBound(this.Children.Select(c => c.Position).ToList());
                    break;
                case GroupType.Frame:
                    break;
                case GroupType.HorizontalGroup:
                    if (this.Position != PitchConverter.GreatestLowerBound(this.Children.Select(c => c.Position).ToList()))
                        break;
                    valid = valid && this.Position == PitchConverter.GreatestLowerBound(this.Children.Select(c => c.Position).ToList());
                    break;
                case GroupType.VerticalFrame:
                case GroupType.HorizontalFrame:
                    break;
            }
            switch (this.Type)
            {
                case GroupType.VerticalGroup:
                    valid = valid && this.Children[0].Position != this.Children[1].Position;
                    break;
            }
            valid = valid && this.Children.All(c => c.Validate());
            valid = valid && this.Children.All(c => c.Group.Validate());
            return valid;
        }
        public bool ValidateChordIndex(int chordIndex)
        {
            var valid = true;
            valid = valid && this.ChordIndex == chordIndex;
            // TODO: validate children
            return valid;
        }
        private void ValidateGroupType(Group left, Group right)
        {
            if ((left.Type == GroupType.Tone || left.Type == GroupType.Group) && (right.Type == GroupType.Tone || right.Type == GroupType.Group))
            {

            }
            else
            {
                throw new ArgumentException("group must be a Tone or Group", nameof(left));
            }
        }
        public void DebugWrite(int depth, Vector<int> offset)
        {
            Debug.Write(new string(' ', 2 * depth));
            string offsetString = offset == Vector<int>.Zero ? "       " : "offset ";
            Vector<int> childOffset;
            var pos = Position + offset;
            switch (this.Type)
            {
                case GroupType.Tone:
                    if (Tone is not null)
                    {
                        Debug.WriteLine($"{Type,-15} {ChordIndex,2} : {pos[0],3} {pos[1],2} {pos[2],2} : {Tone.PitchHeight,3} {Tone.FifthHeight,2} {offsetString}");
                    }
                    break;
                case GroupType.Group:
                    Debug.WriteLine($"{Type,-15} {ChordIndex,2} : {pos[0],3} {pos[1],2} {pos[2],2} : {offsetString}");
                    foreach (var child in Children)
                    {
                        child.Group.DebugWrite(depth + 1, child.Offset());
                    }
                    break;
                case GroupType.VerticalGroup:
                case GroupType.HorizontalGroup:
                    Debug.WriteLine($"{Type,-15} {ChordIndex,2} : {pos[0],3} {pos[1],2} {pos[2],2} : {offsetString}");
                    foreach (var child in Children)
                    {
                        child.Group.DebugWrite(depth + 1, child.Offset());
                    }
                    //Debug.WriteLine(null);
                    break;
                case GroupType.Frame:
                    Debug.Assert(Position == Bounds[0]);
                    Debug.WriteLine($"{Type,-15} {ChordIndex,2} : {VectorConverter.DebugFormatVector(Bounds[0] + offset)} : {VectorConverter.DebugFormatVector(Bounds[1] + offset)} : {offsetString}");
                    foreach (var child in Children)
                    {
                        child.Group.DebugWrite(depth + 1, child.Offset() + offset);
                    }
                    //Debug.WriteLine(null);
                    break;
                case GroupType.VerticalFrame:
                case GroupType.HorizontalFrame:
                    Debug.Assert(Position == Children[0].Group.Bounds[0]);
                    childOffset = Children[1].Offset();
                    Debug.Write($"{Type,-15} {ChordIndex,2} : ");

                    foreach (var bounds in Children[0].Group.Bounds)
                    {
                        Debug.Write($"{VectorConverter.DebugFormatVector(bounds)} : ");
                    }
                    foreach (var bounds in Children[1].Group.Bounds)
                    {
                        Debug.Write($"{VectorConverter.DebugFormatVector(bounds + childOffset)} : ");
                    }
                    if (Children[1].Position != Children[1].Group.Position)
                    {
                        Debug.Write($" offset {VectorConverter.DebugFormatVector(childOffset)} : ");
                    }
                    Debug.WriteLine(null);
                    if (Children[1].Position != Children[1].Group.Position)
                    {

                    }
                    Children[0].Group.DebugWrite(depth + 1, Vector<int>.Zero);
                    Children[1].Group.DebugWrite(depth + 1, Children[1].Offset());
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

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
                    //sb = new StringBuilder();
                    //Debug.Assert(Position == Children[0].Group.Bounds[0]);
                    //sb.Append($"{Type,-15} {ChordIndex,2} : {Position[0],3} {Position[1],2} {Position[2],2} : ");
                    //foreach (var child in Children)
                    //{
                    //    sb.Append($"  {child.Group.Type,-15} ");
                    //    var p = child.Position;
                    //    sb.Append($"{p[0],3} {p[1],2} {p[2],2} : ");
                    //}
                    //return sb.ToString();
                case GroupType.VerticalFrame:
                case GroupType.HorizontalFrame:
                    //Debug.Assert(Position == Children[0].Position);
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

        /// <summary>
        /// unique identifier for HorizontalFrame
        /// </summary>
        public string HorizFrameId
        {
            get
            {
                switch (Type)
                {
                    case GroupType.VerticalFrame:
                    case GroupType.HorizontalFrame:
                        var pos = Position;
                        var b = Children[0].Position;
                        var child = Children[1].Position;
                        var group = Children[1].Group.Position;
                        if (pos != b)
                        {
                            throw new Exception();
                        }
                        if (child != group)
                        {
                            var delta = child - group;
                            return $"{ChordIndex,2} : {pos[0],3}, {pos[1],2} {pos[2],2} : {child[0],3} {child[1],2} {child[2],2} : {group[0],3} {group[1],2} {group[2],2} : {delta[0],3} {delta[1],2} {delta[2],2}";
                        }
                        return $"{ChordIndex,2} : {pos[0],3} {pos[1],2} {pos[2],2} : {child[0],3} {child[1],2} {child[2],2}";
                    case GroupType.Frame:
                        pos = Position;
                        return $"{ChordIndex,2} : {pos[0],3} {pos[1],2} {pos[2],2}";
                    default:
                        return string.Empty;
                }
            }
        }

        #region IEquatable<Group>
        public bool Equals(Group? other)
        {
            if (this == other)
            {
                return true;
            }
            if (this.ChordIndex != other?.ChordIndex ||
                this.Type != other?.Type ||
                this.Position != other?.Position ||
                this.Children.Count != other?.Children.Count)
            {
                return false;
            }
            switch (Type)
            {
                case GroupType.Tone:
                    return this.Tone?.PitchHeight == other?.Tone?.PitchHeight &&
                        this.Tone?.FifthHeight == other?.Tone?.FifthHeight &&
                        this.Tone?.Accidental == other?.Tone?.Accidental;
                case GroupType.Group:
                case GroupType.VerticalGroup:
                case GroupType.HorizontalGroup:
                    var same = true;
                    for (int i = 0; i < this.Children.Count; i++)
                    {
                        same = same && this.Children[i].Equals(other?.Children[i]);
                    }
                    return same;
                case GroupType.Frame:
                case GroupType.VerticalFrame:
                case GroupType.HorizontalFrame:
                    same = true;
                    for (int i = 0; i < this.Children.Count; i++)
                    {
                        same = same && this.Children[i].Equals(other?.Children[i]);
                    }
                    return same;
                default:
                    throw new NotImplementedException();
            }
        }
        #endregion

        #region Unused code
        /// <summary>
        /// Tests whether the two groups are in the same equivalence class
        /// </summary>
        /// <param name="other">the group to compare right to</param>
        /// <param name="super">the group that is the superset</param>
        /// <returns>the group that is the superset</returns>
        public bool IsEquivalenceClass_old(Group other, out Group super)
        {
            if (this.Type != GroupType.Tone && this.Type != GroupType.Group || other.Type != GroupType.Tone && other.Type != GroupType.Group)
            {
                throw new ArgumentException("Invalid GroupType.");
            }

            super = this;
            Vector<int> delta = this.Position - other.Position;
            bool same = Math.Abs(delta[0]) <= 1 && delta[1] == 0 && delta[2] != 0;
            if (!same)
            {
                return false;
            }
            var thisSet = new HashSet<Group>(this.ListTones());
            var otherSet = new HashSet<Group>(other.ListTones());
            if (thisSet.IsSubsetOf(otherSet))
            {
                super = other;
                return true;
            }
            if (otherSet.IsSubsetOf(thisSet))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// is one of the children of one group a subset of one of the children of the other group
        /// </summary>
        public bool IsChildEquivalenceClass(Group left, Group right)
        {
            foreach (var group in left.Children.Select(c => c.Group))
            {
                foreach (var group1 in right.Children.Select(c => c.Group))
                {
                    if (IsEquivalenceClass(group, group1))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Tests whether left Group is a proper subset of right Group
        /// </summary>
        public bool IsEquivalenceClass_LeftSubsetOfRight(Group left, Group right)
        {
            ValidateGroupType(left, right);

            Vector<int> delta = left.Position - right.Position;
            bool same = Math.Abs(delta[0]) <= 1 && delta[1] == 0 && delta[2] != 0;
            if (!same)
            {
                return false;
            }
            var leftSet = new HashSet<Group>(left.ListTones());
            var rightSet = new HashSet<Group>(right.ListTones());
            return leftSet.IsSubsetOf(rightSet) && !rightSet.IsSubsetOf(leftSet);
        }
        /// <summary>
        /// Tests whether right Group is a proper subset of left Group
        /// </summary>
        public bool IsEquivalenceClass_RightSubsetOfLeft(Group left, Group right)
        {
            ValidateGroupType(left, right);

            Vector<int> delta = left.Position - right.Position;
            bool same = Math.Abs(delta[0]) <= 1 && delta[1] == 0 && delta[2] != 0;
            if (!same)
            {
                return false;
            }
            var leftSet = new HashSet<Group>(left.ListTones());
            var rightSet = new HashSet<Group>(right.ListTones());
            return rightSet.IsSubsetOf(leftSet) && !leftSet.IsSubsetOf(rightSet);
        }
        public bool IsEquivalenceClass(Group left, Group right)
        {
            ValidateGroupType(left, right);

            Vector<int> delta = left.Position - right.Position;
            bool same = Math.Abs(delta[0]) <= 1 && delta[1] == 0 && delta[2] == 0;
            if (!same)
            {
                return false;
            }
            var leftSet = new HashSet<Group>(left.ListTones());
            var rightSet = new HashSet<Group>(right.ListTones());
            if (leftSet.IsSubsetOf(rightSet) || rightSet.IsSubsetOf(leftSet))
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}
