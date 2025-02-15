using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisLib
{
    public class Child : IEquatable<Child>
    {
        public Group Group { get; set; } = new Group();
        public int Δz { get; set; } = 0;
        public Vector<int> Position
        {
            get
            {
                return Group.Position + VectorConverter.Aliases[Δz + VectorConverter.AliasesOrigin];
            }
        }
        public Child() { }
        public Child(Group group, Vector<int> position)
        {
            int deltaZ = VectorConverter.VectorToDeltaZ(group.Position, position);
            Group = group;
            Δz = deltaZ;
        }

        /// <summary>
        /// Tests whether the child positions are within bounds
        /// </summary>
        /// <param name="glb">greatest lower bound</param>
        /// <param name="lub">least upper bound</param>
        /// <param name="offset">offset to be applied to group</param>
        /// <param name="types">usually Tone, or else Tone and Group</param>
        public bool ChildWithinBounds(Vector<int> glb, Vector<int> lub, Vector<int> offset, params GroupType[] types)
        {
            var inside = true;
            var typeFound = false;
            if (types.Contains(this.Group.Type))
            {
                typeFound = true;
                var p = this.Position + offset;
                if (p[1] < glb[1] || lub[1] < p[1] || p[2] < glb[2] || lub[2] < p[2])
                {
                    inside = false;
                }
            }
            var offsetChild = this.Position + offset - this.Group.Position;
            foreach (var c in this.Group.Children)
            {
                if (types.Contains(c.Group.Type))
                {
                    typeFound = true;
                    var p = c.Position + offsetChild;
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
        /// returns offset of child Position with respect to parent Posistion
        /// </summary>
        public Vector<int> Offset()
        {
            return Position - Group.Position;
        }
        public void DebugWrite(int depth, Vector<int> offset)
        {
            Debug.Write(new string(' ', 2 * depth));
            var position = Position + offset;
            switch (Group.Type)
            {
                case GroupType.Tone:
                    if (Group.Tone is not null)
                    {
                        Debug.WriteLine($"{Group.Type,-15} {string.Empty,2} : {position[0],3} {position[1],2} {position[2],2} : {Group.Tone.PitchHeight,3} {Group.Tone.FifthHeight,2} ");
                    }
                    break;
                case GroupType.Group:
                case GroupType.VerticalGroup:
                case GroupType.HorizontalGroup:
                case GroupType.Frame:
                case GroupType.VerticalFrame:
                case GroupType.HorizontalFrame:
                    Debug.WriteLine($"{Group.Type,-15} {string.Empty,2} : {position[0],3} {position[1],2} {position[2],2} : ");
                    foreach (var child in Group.Children)
                    {
                        child.DebugWrite(depth + 1, offset);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        public bool Validate()
        {
            var valid = true;
            valid = valid && this.Group.Validate() && this.Group.Children.All(c => c.Validate());
            return valid;
        }

        #region IEquatable<Child>
        public bool Equals(Child? other)
        {
            return other is not null &&
                //this.Group == other?.Group &&
                this.Group.Equals(other?.Group) &&
                this.Δz == other.Δz;
        }
        #endregion
    }
}
