using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisLib
{
    public class DistanceAlgorithm : IDistanceAlgorithm
    {
        // distances are 2 times the average so that pairwise distances are integers
        public DistanceStruct Override = DistanceStruct.Null;
        private Group _group;
        /// <summary>
        /// distance between the two tones that are most distant from the fundamental
        /// </summary>
        public double Pairwise
        {
            get
            {
                return CalculateAverage(2);
            }
        }
        /// <summary>
        /// 2 times the average distance from the fundamental
        /// </summary>
        public double Average
        {
            get
            {
                return CalculateAverage(_group.Children.Count);
            }
        }
        /// <summary>
        /// 2 times the weighted average distance from the fundamental
        /// </summary>
        public double Weighted
        {
            get
            {
                return CalculateWeightedAverage();
            }
        }
        /// <summary>
        /// Sum of the parent and child pairwise distances
        /// </summary>
        /// <remarks>CompoundPairwise is the same as Pairwise when the Children are Tones</remarks>
        public double CompoundPairwise
        {
            get
            {
                return Pairwise + _group.Children.Sum(g => g.Group.Distance.Pairwise);
            }
        }
        /// <summary>
        /// Sum of the parent and child average distances
        /// </summary>
        /// <remarks>CompoundAverage is the same as Average when the Children are Tones</remarks>
        public double CompoundAverage
        {
            get
            {
                return Average + _group.Children.Sum(g => g.Group.Distance.Average);
            }
        }
        /// <summary>
        /// Sum of the parent and child weighted distances
        /// </summary>
        /// <remarks>CompoundWeighted is the same as Weighted when the Children are Tones</remarks>
        public double CompoundWeighted
        {
            get
            {
                return Weighted + _group.Children.Sum(g => g.Group.Distance.Weighted);
            }
        }
        public double DistanceTypeToDouble(DistanceType type)
        {
            switch (type)
            {
                case DistanceType.Pairwise:
                    return this.Pairwise;
                case DistanceType.Average:
                    return this.Average;
                case DistanceType.Weighted:
                    return this.Weighted;
                case DistanceType.CompoundPairwise:
                    return this.CompoundPairwise;
                case DistanceType.CompoundAverage:
                    return this.CompoundAverage;
                case DistanceType.CompoundWeighted:
                    return this.CompoundWeighted;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        public DistanceAlgorithm(Group group)
        {
            _group = group;
        }

        public Vector<int>[] GetChildPositions()
        {
            return _group.Children
                .OrderBy(c => VectorConverter.VectorToPitch(c.Position))
                .Select(c => c.Position).ToArray();
        }
        /// <summary>
        /// 2 times the average distance from the fundamental
        /// </summary>
        /// <param name="N">average the last N elements</param>
        private double CalculateAverage(int N)
        {
            var count = _group.Children.Count;
            if (count <= 1)
            {
                return 0;
            }
            else if (count == 2)
            {
                // avoid double rounding errors
                return GetChildPositions().Sum(p => VectorConverter.VectorToDistance(p - _group.Position));
            }
            else
            {
                var childPositions = GetChildPositions();
                var sum = childPositions
                    // take the last N elements
                    .Skip(Math.Max(0, count - N))
                    // sum the distances from the fundamental
                    .Sum(p => VectorConverter.VectorToDistance(p - _group.Position));
                if (N == 2)
                {
                    // avoid double rounding errors
                    return sum;
                }
                double average = (2 * sum) / (double)N;
                return average;
            }
        }
        /// <summary>
        /// 2 times the weighted average distance from the fundamental
        /// </summary>
        private double CalculateWeightedAverage()
        {
            var count = _group.Children.Count;
            if (count <= 1)
            {
                return 0;
            }
            else
            {
                var denominator = _weights[count].Sum();
                var childPositions = GetChildPositions();
                var sum = Enumerable.Range(0, count)
                    .Select(i => _weights[count][i] * VectorConverter.VectorToDistance(childPositions[i] - _group.Position)).Sum();
                double average = (2 * sum) / (double)denominator;
                return average;
            }
        }

        #region Frame Methods
        public int MaxPairwiseFrame()
        {
            // return the maximum of the pairwise distances. this reflects the overall size of the tones within the frame
            if (IsFrame())
            {
                int max = int.MinValue;
                for (int i = 0; i < _group.Children.Count - 1; ++i)
                {
                    for (int j = i + 1; j < _group.Children.Count; j++)
                    {
                        int distance = VectorConverter.VectorToDistance(_group.Children[i].Position - _group.Children[j].Position);
                        if (distance > max)
                        {
                            max = distance;
                        }
                    }
                }
                return max;
            }
            else
            {
                return int.MaxValue;
            }
        }
        public int MaxPairwiseHorizontalFrame()
        {
            // for each tone in chord 0 find the closest tone in chord 1, and vice versa. return the maximum of those distances
            if (IsHorizontalFrame())
            {
                // get chord 0 tone positions
                var list0 = _group.Children[0].Group.ListTonePositions(_group.Children[0].Offset());

                // get chord 1 tone positions
                var list1 = _group.Children[1].Group.ListTonePositions(_group.Children[1].Offset());

                var distances = new List<int>();
                foreach (var tone0 in list0)
                {
                    // Min
                    int distance = list1.Min(tone1 => VectorConverter.VectorToDistance(tone1.Position - tone0.Position));
                    distances.Add(distance);
                }
                foreach (var tone1 in list1)
                {
                    // Min
                    int distance = list0.Min(tone0 => VectorConverter.VectorToDistance(tone0.Position - tone0.Position));
                    distances.Add(distance);
                }
                // Max
                return distances.Max();
            }
            else
            {
                return int.MaxValue;
            }
        }
        //public int MaxPairwiseHorizontalFrame_old()
        //{
        //    // for each tone in chord 0 find the closest tone in chord 1, and vice versa. return the maximum of those distances
        //    if (IsHorizontalFrame())
        //    {
        //        // get chord 0 tone positions
        //        var list0 = new List<Group>();
        //        _group.Children[0].Group.GetChildPositions(_group.Children[0].Offset()/*_group.Position - _group.Children[0].Position*/, ref list0, -1, GroupType.Tone);

        //        Debug.Assert(_group.Position == _group.Children[0].Position);
        //        var del = _group.Children[0].Offset();
        //        Debug.Assert(VectorConverter.VectorToPitch(del) == 0);

        //        // get chord 1 tone positions
        //        var list1 = new List<Group>();
        //        _group.Children[1].Group.GetChildPositions(_group.Children[1].Offset()/*_group.Position - _group.Children[1].Position*/, ref list1, -1, GroupType.Tone);

        //        del = _group.Children[1].Offset();
        //        Debug.Assert(VectorConverter.VectorToPitch(del) == 0);

        //        //
        //        var distances = new List<int>();
        //        foreach (var tone0 in list0)
        //        {
        //            // Min
        //            int distance = list1.Min(tone1 => VectorConverter.VectorToDistance(tone1.Position - tone0.Position));
        //            distances.Add(distance);
        //        }
        //        foreach (var tone1 in list1)
        //        {
        //            // Min
        //            int distance = list0.Min(tone0 => VectorConverter.VectorToDistance(tone0.Position - tone0.Position));
        //            distances.Add(distance);
        //        }
        //        return distances.Max();
        //    }
        //    else
        //    {
        //        return int.MaxValue;
        //    }
        //}
        //public int MaxPairwiseHorizontalFrame_oldold()
        //{
        //    if (IsHorizontalFrame())
        //    {
        //        var list0 = new List<Group>();
        //        Debug.Assert(_group.Position == _group.Children[0].Position);
        //        var del = _group.Children[0].Offset();
        //        Debug.Assert(VectorConverter.VectorToPitch(del) == 0);
        //        _group.Children[0].Group.GetChildPositions(_group.Children[0].Offset(), ref list0, -1, GroupType.Tone);
        //        var list1 = new List<Group>();
        //        del = _group.Children[1].Offset();
        //        Debug.Assert(VectorConverter.VectorToPitch(del) == 0);
        //        _group.Children[1].Group.GetChildPositions(_group.Children[1].Offset(), ref list1, -1, GroupType.Tone);
        //        var distances = new List<int>();
        //        foreach (var tone0 in list0)
        //        {
        //            // Max
        //            int distance = list1.Max(tone1 => VectorConverter.VectorToDistance(tone1.Position - tone0.Position));
        //            distances.Add(distance);
        //        }
        //        foreach (var tone1 in list1)
        //        {
        //            // Max
        //            int distance = list0.Max(tone0 => VectorConverter.VectorToDistance(tone0.Position - tone0.Position));
        //            distances.Add(distance);
        //        }
        //        return distances.Max();
        //    }
        //    else
        //    {
        //        return int.MaxValue;
        //    }
        //}
        //public int MaxPairwiseHorizontalFrame_oldoldold()
        //{
        //    if (IsHorizontalFrame())
        //    {
        //        int max = int.MinValue;
        //        var offset = _group.Position - _group.Children[0].Position;
        //        var offsetNext = _group.Position - _group.Children[1].Position;
        //        for (int i = 0; i < _group.Children[0].Group.Children.Count; ++i)
        //        {
        //            for (int j = 0; j < _group.Children[1].Group.Children.Count; j++)
        //            {
        //                int distance = VectorConverter.VectorToDistance((_group.Children[0].Group.Children[i].Position + offset) - (_group.Children[1].Group.Children[j].Position + offsetNext));
        //                if (distance > max)
        //                {
        //                    max = distance;
        //                }
        //            }
        //        }
        //        return max;
        //    }
        //    else
        //    {
        //        return int.MaxValue;
        //    }
        //}
        //public static int MaxPairwiseDistance_Frame(Group frame)
        //{
        //    if (frame.Type != GroupType.Frame || frame.Children.Any(c => c.Group.Type != GroupType.Tone))
        //    {
        //        return int.MaxValue;
        //    }
        //    int max = int.MinValue;
        //    for (int i = 0; i < frame.Children.Count - 1; ++i)
        //    {
        //        for (int j = i + 1; j < frame.Children.Count; j++)
        //        {
        //            int distance = VectorConverter.VectorToDistance(frame.Children[i].Position - frame.Children[j].Position);
        //            if (distance > max)
        //            {
        //                max = distance;
        //            }
        //        }
        //    }
        //    return max;
        //}
        //public static int MaxPairwiseDistance_HorizontalFrame(Group frame)
        //{
        //    if (frame.Type != GroupType.HorizontalFrame || frame.Children.Any(c => c.Group.Type != GroupType.Frame) ||
        //        frame.Children.SelectMany(vf => vf.Group.Children).Any(c => c.Group.Type != GroupType.Tone))
        //    {
        //        return int.MaxValue;
        //    }
        //    int max = int.MinValue;
        //    var offset = frame.Position - frame.Children[0].Position;
        //    var offsetNext = frame.Position - frame.Children[1].Position;
        //    for (int i = 0; i < frame.Children[0].Group.Children.Count; ++i)
        //    {
        //        for (int j = 0; j < frame.Children[1].Group.Children.Count; j++)
        //        {
        //            int distance = VectorConverter.VectorToDistance((frame.Children[0].Group.Children[i].Position + offset) - (frame.Children[1].Group.Children[j].Position + offsetNext));
        //            if (distance > max)
        //            {
        //                max = distance;
        //            }
        //        }
        //    }
        //    return max;
        //}
        private bool IsFrame()
        {
            return _group.Type == GroupType.Frame && _group.Children.All(c => c.Group.Type == GroupType.Tone);
        }
        private bool IsVerticalFrame()
        {
            return _group.Type == GroupType.VerticalFrame && _group.Children.All(c => c.Group.Type == GroupType.Frame);
        }
        private bool IsHorizontalFrame()
        {
            return _group.Type == GroupType.HorizontalFrame && _group.Children.All(c => c.Group.Type == GroupType.Frame);
        }

        #endregion
        // the last element is twice the first element
        public static /*private*/ readonly double[][] _weights = new double[][]
        {
                new double[] { },
                new double[] { 1, },
                new double[] { 1, 2 },
                new double[] { 2, 3, 4 },
                new double[] { 3, 4, 5, 6 },
                new double[] { 4, 5, 6, 7, 8 },
                new double[] { 5, 6, 7, 8, 9, 10 },
                new double[] { 6, 7, 8, 9, 10, 11, 12 },
                new double[] { 7, 8, 9, 10, 11, 12, 13, 14 },
        };
    }

    public class DistanceStruct : IDistanceAlgorithm
    {
        public double Pairwise { get; set; }
        public double Average { get; set; }
        public double Weighted { get; set; }
        public double CompoundPairwise { get; set; }
        public double CompoundAverage { get; set; }
        public double CompoundWeighted { get; set; }
        public double DistanceTypeToDouble(DistanceType type)
        {
            switch (type)
            {
                case DistanceType.Pairwise:
                    return this.Pairwise;
                case DistanceType.Average:
                    return this.Average;
                case DistanceType.Weighted:
                    return this.Weighted;
                case DistanceType.CompoundPairwise:
                    return this.CompoundPairwise;
                case DistanceType.CompoundAverage:
                    return this.CompoundAverage;
                case DistanceType.CompoundWeighted:
                    return this.CompoundWeighted;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        public static DistanceStruct Null = 
            new DistanceStruct { Pairwise = -1, Average = -1, Weighted = -1, CompoundPairwise = -1, CompoundAverage = -1, CompoundWeighted = -1 };
    }

    //public class DistanceVerticalGroupMap : IDistanceAlgorithm
    //{
    //    public double Pairwise { get; set; }
    //    public double Average { get; set; }
    //    public double Weighted { get; set; }
    //    public double CompoundPairwise { get; set; }
    //    public double CompoundAverage { get; set; }
    //    public double CompoundWeighted { get; set; }
    //    private VerticalGroupMapItem _item;

    //    public DistanceVerticalGroupMap(VerticalGroupMapItem item)
    //    {
    //        _item = item;
    //    }

    //    public double DistanceTypeToDouble(DistanceType type)
    //    {
    //        switch (type)
    //        {
    //            case DistanceType.Pairwise:
    //                return _item.ToneToGroups.Where(t => t.HorizontalGroup != null).Min(t => t.HorizontalGroup.Distance.Pairwise);
    //            case DistanceType.Average:
    //                return _item.ToneToGroups.Where(t => t.HorizontalGroup != null).Min(t => t.HorizontalGroup.Distance.Average);
    //            case DistanceType.Weighted:
    //                return _item.ToneToGroups.Where(t => t.HorizontalGroup != null).Min(t => t.HorizontalGroup.Distance.Weighted);
    //            case DistanceType.CompoundPairwise:
    //                return _item.ToneToGroups.Where(t => t.HorizontalGroup != null).Min(t => t.HorizontalGroup.Distance.CompoundPairwise);
    //            case DistanceType.CompoundAverage:
    //                return _item.ToneToGroups.Where(t => t.HorizontalGroup != null).Min(t => t.HorizontalGroup.Distance.CompoundAverage);
    //            case DistanceType.CompoundWeighted:
    //                return _item.ToneToGroups.Where(t => t.HorizontalGroup != null).Min(t => t.HorizontalGroup.Distance.CompoundWeighted);
    //            default:
    //                throw new ArgumentOutOfRangeException(nameof(type), type, null);
    //        }
    //    }
    //}
    public enum DistanceType
    {
        Pairwise,
        Average,
        Weighted,
        CompoundPairwise,
        CompoundAverage,
        CompoundWeighted,
    }
}
