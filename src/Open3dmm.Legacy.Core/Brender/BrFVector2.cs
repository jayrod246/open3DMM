using System.Numerics;

namespace Open3dmm.Core.Brender
{
    /// <summary>
    /// Fraction vector type size 2
    /// </summary>
    public struct BrFVector2
    {
        public BrFraction X;
        public BrFraction Y;

        public static implicit operator Vector2(BrFVector2 f) => new Vector2(f.X, f.Y);
    }
}
