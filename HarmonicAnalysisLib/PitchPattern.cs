using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisLib
{
    /// <summary>
    /// PitchPattern represents a chord
    /// </summary>
    public class PitchPattern
    {
        /// <summary>
        /// each of the Pitches is a chord tone
        /// </summary>
        public List<IPitch> Pitches /* [iPitch] */ { get; set; } = new List<IPitch>();
    }
    /// <summary>
    /// the three positions of a VerticalGroup or HorizontalGroup
    /// </summary>
    public class PitchTriplet
    {
        public Vector<int> Position { get; set; } = Vector<int>.Zero;
        public IPitch Pitch1 { get; set; } = new Pitch(0, 0);
        public IPitch Pitch2 { get; set; } = new Pitch(0, 0);

        public PitchTriplet(Vector<int> position, IPitch pitch1, IPitch pitch2)
        {
            Position = position;
            Pitch1 = pitch1;
            Pitch2 = pitch2;
        }
    }
    public class VectorTriplet : IEqualityComparer<VectorTriplet>, IComparable<VectorTriplet>
    {
        public Vector<int> Position { get; set; } = Vector<int>.Zero;
        public Vector<int> Child0 { get; set; } = Vector<int>.Zero;
        public Vector<int> Child1 { get; set; } = Vector<int>.Zero;

        public VectorTriplet(Vector<int> position, Vector<int> child0, Vector<int> child1)
        {
            Position = position;
            Child0 = child0;
            Child1 = child1;
        }

        #region IEqualityComparer<VectorTriplet>
        public bool Equals(VectorTriplet? x, VectorTriplet? y)
        {
            return x?.Position == y?.Position && x?.Child0 == y?.Child0 && x?.Child1 == y?.Child1;
        }
        public override bool Equals(object obj)
        {
            return Equals(this, (VectorTriplet)obj);
        }
        public int GetHashCode([DisallowNull] VectorTriplet obj)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region IComparable<VectorTriplet>
        public int CompareTo(VectorTriplet? other)
        {
            if (this.Position != other?.Position)
            {
                if (this.Position[1] != other.Position[1])
                {
                    return this.Position[1].CompareTo(other.Position[1]);
                }
                return this.Position[2].CompareTo(other.Position[2]);
            }
            else if (this.Child0 != other.Child0)
            {
                if (this.Child0[1] != other.Child0[1])
                {
                    return this.Child0[1].CompareTo(other.Child0[1]);
                }
                return this.Child0[2].CompareTo(other.Child0[2]);
            }
            else 
            {
                if (this.Child1[1] != other.Child1[1])
                {
                    return this.Child1[1].CompareTo(other.Child1[1]);
                }
                return this.Child1[2].CompareTo(other.Child1[2]);
            }
        }
        #endregion
    }
}
