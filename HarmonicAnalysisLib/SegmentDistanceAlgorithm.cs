using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisLib
{
    /// <summary>
    /// Segment distance algorithm
    /// </summary>
    /// <remarks>The distance returned by this algorithm depends on GroupMap SelectedSnapshot </remarks>
    public class SegmentDistanceAlgorithm : IDistanceAlgorithm
    {
        // Segments are one of the following forms (G GroupMap V VerticalGroupMap H HorizontalGroupMap):
        // G   G  G   G
        // H   V  V   H
        // G   G  G   G
        //        H   V
        //        G   G
        // My reasoning is that:
        // the first G and the H are weighted about equally.
        // Half the weight should be distributed over all the Gs, and half of the weight should be distributed over the H and V.
        // V is part of diagonal H, so V is grouped with H.
        // The V should be weighted little, less than H.
        // The middle G should be weighted little.
        // The last G should be weighted little, to avoid adding its weight twice, since it is also the first G of the next segment.
        private Segment _group;

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
        public bool IsDefined
        {
            get
            {
                return _group.FrameStructs.All(frameStruct => frameStruct.GroupMap != null && frameStruct.GroupMap.Snapshots.Count != 0);
            }
        }

        public SegmentDistanceAlgorithm(Segment group)
        {
            _group = group;
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
        private double CalculateAverage(int N, bool compound)
        {
            var count = _group.FrameStructs.Count;
            // GHG | GVG | GHGVG | GVGHG
            string tag = _group.FrameStructs.Select(f => f.GroupMap).Select(g => g.Type == GroupMapType.GroupMap ? "G" : g.Type == GroupMapType.VerticalGroupMap ? "V" : "H").Aggregate((a, b) => a + b);
            var G = _group.FrameStructs.Where(fs => fs.GroupMap.Type == GroupMapType.GroupMap).Select(fs => fs.GroupMap).ToList();
            var V = _group.FrameStructs.Where(fs => fs.GroupMap.Type == GroupMapType.VerticalGroupMap).Select(fs => fs.GroupMap).ToList();
            var H = _group.FrameStructs.Where(fs => fs.GroupMap.Type == GroupMapType.HorizontalGroupMap).Select(fs => fs.GroupMap).ToList();
            Debug.Assert(G.Count >= 2 && G.Count <= 3);
            Debug.Assert(V.Count <= 1);
            Debug.Assert(H.Count <= 1);
            Debug.Assert(count == 5 || (V.Count == 0 && H.Count == 1) || (V.Count == 1 && H.Count == 0));

            // the last chord has no horizontal group
            bool lastChord = (V.Count == 1 && H.Count == 0);

            double sum = 0;
            if (!lastChord)
            {
                if (!compound)
                {
                    if (N == 2)
                    {
                        sum = 0.5 * G[0].Distance.Average 
                            + 0.5 * H[0].Distance.Average;
                    }
                    else
                    {
                        switch (tag)
                        {
                            case "GHG":
                                sum = 0.4 * G[0].Distance.Average + 0.5 * H[0].Distance.Average 
                                    + 0.1 * G[1].Distance.Average;
                                break;
                            case "GHGVG":
                            case "GVGHG":
                                sum = 0.3 * G[0].Distance.Average + 0.4 * H[0].Distance.Average 
                                    + 0.1 * G[1].Distance.Average + 0.1 * V[0].Distance.Average 
                                    + 0.1 * G[2].Distance.Average;
                                break;
                            default: throw new InvalidOperationException();
                        }
                    }
                }
                else
                {
                    if (N == 2)
                    {
                        sum = 0.5 * G[0].Distance.CompoundAverage
                            + 0.5 * H[0].Distance.CompoundAverage;
                    }
                    else
                    {
                        switch (tag)
                        {
                            case "GHG":
                                sum = 0.4 * G[0].Distance.CompoundAverage + 0.5 * H[0].Distance.CompoundAverage
                                    + 0.1 * G[1].Distance.CompoundAverage;
                                break;
                            case "GHGVG":
                            case "GVGHG":
                                sum = 0.3 * G[0].Distance.CompoundAverage + 0.4 * H[0].Distance.CompoundAverage
                                    + 0.1 * G[1].Distance.CompoundAverage + 0.1 * V[0].Distance.CompoundAverage
                                    + 0.1 * G[2].Distance.CompoundAverage;
                                break;
                            default: throw new InvalidOperationException();
                        }
                    }
                }
            }
            else if (lastChord)
            {
                if (!compound)
                {
                    if (N == 2)
                    {
                        sum = 0.5 * G[0].Distance.Average 
                            + 0.5 * V[0].Distance.Average;
                    }
                    else
                    {
                        switch (tag)
                        {
                            case "GVG":
                                sum = 0.6 * G[0].Distance.Average
                                    + 0.2 * V[0].Distance.Average
                                    + 0.2 * G[0].Distance.Average;
                                break;
                            default: throw new InvalidOperationException();
                        }
                    }
                }
                else
                {
                    if (N == 2)
                    {
                        sum = 0.5 * G[0].Distance.CompoundAverage
                            + 0.5 * V[0].Distance.CompoundAverage;
                    }
                    else
                    {
                        switch (tag)
                        {
                            case "GVG":
                                sum = 0.6 * G[0].Distance.CompoundAverage
                                    + 0.2 * V[0].Distance.CompoundAverage
                                    + 0.2 * G[0].Distance.CompoundAverage;
                                break;
                            default: throw new InvalidOperationException();
                        }
                    }
                }
            }
            else throw new InvalidOperationException();

            return sum;
        }
        private double CalculateWeightedAverage(bool compound)
        {
            var count = _group.FrameStructs.Count;
            // GHG | GVG | GHGVG | GVGHG
            string tag = _group.FrameStructs.Select(f => f.GroupMap).Select(g => g.Type == GroupMapType.GroupMap ? "G" : g.Type == GroupMapType.VerticalGroupMap ? "V" : "H").Aggregate((a, b) => a + b);
            var G = _group.FrameStructs.Where(fs => fs.GroupMap.Type == GroupMapType.GroupMap).Select(fs => fs.GroupMap).ToList();
            var V = _group.FrameStructs.Where(fs => fs.GroupMap.Type == GroupMapType.VerticalGroupMap).Select(fs => fs.GroupMap).ToList();
            var H = _group.FrameStructs.Where(fs => fs.GroupMap.Type == GroupMapType.HorizontalGroupMap).Select(fs => fs.GroupMap).ToList();
            Debug.Assert(G.Count >= 2 && G.Count <= 3);
            Debug.Assert(V.Count <= 1);
            Debug.Assert(H.Count <= 1);
            Debug.Assert(count == 5 || (V.Count == 0 && H.Count == 1) || (V.Count == 1 && H.Count == 0));

            // the last chord has no horizontal group
            bool lastChord = (V.Count == 1 && H.Count == 0);

            double sum = 0;
            if (!lastChord)
            {
                if (!compound)
                {
                    switch (tag)
                    {
                        case "GHG":
                            sum = 0.4 * G[0].Distance.Weighted + 0.5 * H[0].Distance.Weighted
                                + 0.1 * G[1].Distance.Weighted;
                            break;
                        case "GHGVG":
                        case "GVGHG":
                            sum = 0.3 * G[0].Distance.Weighted + 0.4 * H[0].Distance.Weighted
                                + 0.1 * G[1].Distance.Weighted + 0.1 * V[0].Distance.Weighted
                                + 0.1 * G[2].Distance.Weighted;
                            break;
                        default: throw new InvalidOperationException();
                    }
                }
                else
                {
                    switch (tag)
                    {
                        case "GHG":
                            sum = 0.4 * G[0].Distance.CompoundWeighted + 0.5 * H[0].Distance.CompoundWeighted
                                + 0.1 * G[1].Distance.CompoundWeighted;
                            break;
                        case "GHGVG":
                        case "GVGHG":
                            sum = 0.3 * G[0].Distance.CompoundWeighted + 0.4 * H[0].Distance.CompoundWeighted
                                + 0.1 * G[1].Distance.CompoundWeighted + 0.1 * V[0].Distance.CompoundWeighted
                                + 0.1 * G[2].Distance.CompoundWeighted;
                            break;
                        default: throw new InvalidOperationException();
                    }
                }
            }
            else if (lastChord)
            {
                if (!compound)
                {
                    switch (tag)
                    {
                        case "GVG":
                            sum = 0.6 * G[0].Distance.Weighted
                                + 0.2 * V[0].Distance.Weighted
                                + 0.2 * G[0].Distance.Weighted;
                            break;
                        default: throw new InvalidOperationException();
                    }
                }
                else
                {
                    switch (tag)
                    {
                        case "GVG":
                            sum = 0.6 * G[0].Distance.CompoundWeighted
                                + 0.2 * V[0].Distance.CompoundWeighted
                                + 0.2 * G[0].Distance.CompoundWeighted;
                            break;
                        default: throw new InvalidOperationException();
                    }
                }
            }
            else throw new InvalidOperationException();

            return sum;
        }
    }
}