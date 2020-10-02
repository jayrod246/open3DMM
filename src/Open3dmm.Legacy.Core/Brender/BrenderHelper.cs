using System.Numerics;

namespace Open3dmm.Core.Brender
{
    public static class BrenderHelper
    {
        public static Matrix4x4 MatrixFromEuler(BrEuler euler)
        {
            return Matrix4x4.CreateRotationX(euler.A.ToRadians())
                   * Matrix4x4.CreateRotationY(euler.B.ToRadians())
                   * Matrix4x4.CreateRotationZ(euler.C.ToRadians());
        }
    }
}
