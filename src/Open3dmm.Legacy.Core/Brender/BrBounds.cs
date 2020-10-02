using System.Collections.Generic;

namespace Open3dmm.Core.Brender
{
    /// <summary>
    /// Axis aligned bounding box.
    /// </summary>
    public struct BrBounds
    {
        public BrBounds(FixedVector3 min, FixedVector3 max)
        {
            Minimum = min;
            Maximum = max;
        }

        /// <summary>
        /// Minimum corner of bounds.
        /// </summary>
        public FixedVector3 Minimum;
        /// <summary>
        /// Maximum corner of bounds.
        /// </summary>
        public FixedVector3 Maximum;

        public static BrBounds FromPoints(IEnumerable<FixedVector3> points)
        {
            FixedVector3 min = default;
            FixedVector3 max = default;

            foreach (var p in points)
            {
                if (p.X < min.X)
                    min.X = p.X;
                if (p.Y < min.Y)
                    min.Y = p.Y;
                if (p.Z < min.Z)
                    min.Z = p.Z;
                if (p.X > max.X)
                    max.X = p.X;
                if (p.Y > max.Y)
                    max.Y = p.Y;
                if (p.Z > max.Z)
                    max.Z = p.Z;
            }

            return new BrBounds(min, max);
        }
    }
}
