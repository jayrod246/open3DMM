using Microsoft.Xna.Framework;
using Open3dmm.BRender;

namespace Open3dmm
{
    internal unsafe class Helper
    {
        public static Matrix XNAMatrixFromBrMatrix34(BrMatrix34* mat)
        {
            var m = (BrScalar*)mat;
            return new Matrix(m[0].ToFloat(), m[1].ToFloat(), m[2].ToFloat(), 0f, m[3].ToFloat(), m[4].ToFloat(), m[5].ToFloat(), 0f, m[6].ToFloat(), m[7].ToFloat(), m[8].ToFloat(), 0f, m[9].ToFloat(), m[10].ToFloat(), m[11].ToFloat(), 1f);
        }

        public static Vector3 XNAVector3FromBrVector3(BrVector3* vec3)
        {
            var s = (BrScalar*)vec3;
            return new Vector3(s[0].ToFloat(), s[1].ToFloat(), s[2].ToFloat());
        }

        public static Vector3 XNAVector3FromBrFVector3(BrFVector3* fvec3)
        {
            var f = (BrFraction*)fvec3;
            return new Vector3(f[0].ToFloat(), f[1].ToFloat(), f[2].ToFloat());
        }

        public static Vector2 XNAVector2FromBrVector2(BrVector2* vec2)
        {
            var s = (BrScalar*)vec2;
            return new Vector2(s[0].ToFloat(), s[1].ToFloat());
        }

        public static Vector2 XNAVector2FromBrFVector2(BrFVector2* fvec2)
        {
            var f = (BrFraction*)fvec2;
            return new Vector2(f[0].ToFloat(), f[1].ToFloat());
        }
    }
}
