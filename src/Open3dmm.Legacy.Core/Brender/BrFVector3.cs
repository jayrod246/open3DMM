using System.Numerics;

namespace Open3dmm.Core.Brender
{
    /// <summary>
    /// Fraction vector type size 3
    /// </summary>
    public struct BrFVector3
    {
        public BrFraction X;
        public BrFraction Y;
        public BrFraction Z;

        public static implicit operator Vector3(BrFVector3 f) => new Vector3(f.X, f.Y, f.Z);
    }
}
