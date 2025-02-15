using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisLib
{
    /// <summary>
    /// GroupMap distance algorithm
    /// </summary>
    /// <remarks>The distance returned by this algorithm depends on GroupMap SelectedSnapshot </remarks>
    public class GroupMapDistanceAlgorithm : IDistanceAlgorithm
    {
        private GroupMap _group;

        public double Pairwise
        {
            get
            {
                return CalculateAverage(2, false);
            }
        }
        public double Average
        {
            get
            {
                return CalculateAverage(int.MaxValue, false);
            }
        }
        public double Weighted
        {
            get
            {
                return CalculateWeightedAverage(false);
            }
        }
        public double CompoundPairwise
        {
            get
            {
                return CalculateAverage(2, true);
            }
        }
        public double CompoundAverage
        {
            get
            {
                return CalculateAverage(int.MaxValue, true);
            }
        }
        public double CompoundWeighted
        {
            get
            {
                return CalculateWeightedAverage(true);
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

        public GroupMapDistanceAlgorithm(GroupMap group)
        {
            _group = group;
        }

        private double CalculateAverage(int N, bool compound)
        {
            List<Group> distinctGroups;
            switch (_group.Type)
            {
                case GroupMapType.GroupMap:
                case GroupMapType.VerticalGroupMap:
                    distinctGroups = _group.GetDistinctGroups(GroupType.Group, GroupType.VerticalGroup);
                    break;
                case GroupMapType.HorizontalGroupMap:
                    distinctGroups = _group.GetDistinctGroups(GroupType.HorizontalGroup);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            var count = distinctGroups.Count;
            double sum;
            if (count == 0)
            {
                return 0;
            }
            else if (!compound)
            {
                sum = distinctGroups
                    .OrderBy(g => g.Distance.Average)
                    // take the last N elements
                    .Skip(Math.Max(0, count - N))
                    .Sum(g => g.Distance.Average) / count;
            }
            else
            {
                sum = distinctGroups
                    .OrderBy(g => g.Distance.CompoundAverage)
                    // take the last N elements
                    .Skip(Math.Max(0, count - N))
                    .Sum(g => g.Distance.CompoundAverage) / count;
            }
            return sum;
        }
        private double CalculateWeightedAverage(bool compound)
        {
            List<Group> distinctGroups;
            switch (_group.Type)
            {
                case GroupMapType.GroupMap:
                case GroupMapType.VerticalGroupMap:
                    distinctGroups = _group.GetDistinctGroups(GroupType.Group, GroupType.VerticalGroup);
                    break;
                case GroupMapType.HorizontalGroupMap:
                    distinctGroups = _group.GetDistinctGroups(GroupType.HorizontalGroup);
                    break;
                default:
                    throw new InvalidOperationException();
            }
            var count = distinctGroups.Count;
            if (count == 0)
            {
                return 0;
            }
            else if (count == 1)
            {
                // avoid double rounding errors
                if (!compound)
                {
                   return distinctGroups[0].Distance.Weighted;
                }
                else
                {
                    return distinctGroups[0].Distance.CompoundWeighted;
                }
            }
            else if (!compound)
            {
                var denominator = DistanceAlgorithm._weights[count].Sum();
                var sum = Enumerable.Range(0, count)
                    .OrderBy(i => distinctGroups[i].Distance.Weighted)
                    .Select(i => DistanceAlgorithm._weights[count][i] * distinctGroups[i].Distance.Weighted).Sum();
                // facter of 2 already applied
                double average = sum/*(2 * sum)*/ / (double)denominator;
                return average;
            }
            else
            {
                var denominator = DistanceAlgorithm._weights[count].Sum();
                var sum = Enumerable.Range(0, count)
                    .OrderBy(i => distinctGroups[i].Distance.Weighted)
                    .Select(i => DistanceAlgorithm._weights[count][i] * (
                    distinctGroups[i].Distance.Weighted + distinctGroups[i].Children.Sum(c => c.Group.Distance.Weighted)
                    )).Sum();
                // facter of 2 already applied
                double average = sum/*(2 * sum)*/ / (double)denominator;
                return average;
            }
        }
    }
}
