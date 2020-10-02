using System.Diagnostics;
using System.Numerics;

namespace Open3dmm.Core
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public readonly struct Fixed
    {
        public readonly int PackedValue;
        private string DebuggerDisplay => ToString();

        public Fixed(int packedValue)
        {
            this.PackedValue = packedValue;
        }

        public static implicit operator float(Fixed f) => f.PackedValue / 65536f;
        public static implicit operator double(Fixed f) => f.PackedValue / 65536d;
        public static explicit operator Fixed(float f) => new Fixed((int)(f * 65536f));
        public static explicit operator Fixed(double d) => new Fixed((int)(d * 65536d));

        public override string ToString()
        {
            return ((float)this).ToString();
        }
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct FixedVector3
    {
        public Fixed X;
        public Fixed Y;
        public Fixed Z;

        private string DebuggerDisplay => ToString();

        public FixedVector3(Fixed x, Fixed y, Fixed z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public static implicit operator Vector3(FixedVector3 f) => new Vector3(f.X, f.Y, f.Z);
        public static explicit operator FixedVector3(Vector3 v) => new FixedVector3((Fixed)v.X, (Fixed)v.Y, (Fixed)v.Z);

        public override string ToString()
        {
            return ((Vector3)this).ToString();
        }
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct FixedVector2
    {
        public Fixed X;
        public Fixed Y;

        private string DebuggerDisplay => ToString();

        public FixedVector2(Fixed x, Fixed y)
        {
            this.X = x;
            this.Y = y;
        }

        public static implicit operator Vector2(FixedVector2 f) => new Vector2(f.X, f.Y);
        public static explicit operator FixedVector2(Vector2 v) => new FixedVector2((Fixed)v.X, (Fixed)v.Y);

        public override string ToString()
        {
            return ((Vector2)this).ToString();
        }
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct FixedMatrix4x3
    {
        // [11][12][13] {0}
        // [21][22][23] {0}
        // [31][32][33] {0}
        // [41][42][43] {1}

        // [11][12][13][14]
        // [21][22][23][24]
        // [31][32][33][34]
        // {0} {0} {0} {1}

        // [11][21][31]{0}
        // [12][22][32]{0}
        // [13][23][33]{0}
        // [14][24][34]{1}

        public Fixed M11;
        public Fixed M12;
        public Fixed M13;
        public Fixed M21;
        public Fixed M22;
        public Fixed M23;
        public Fixed M31;
        public Fixed M32;
        public Fixed M33;
        public Fixed M41;
        public Fixed M42;
        public Fixed M43;

        private string DebuggerDisplay => ToString();

        public FixedMatrix4x3(Fixed m11, Fixed m12, Fixed m13, Fixed m21, Fixed m22, Fixed m23, Fixed m31, Fixed m32, Fixed m33, Fixed m41, Fixed m42, Fixed m43)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M13 = m13;
            this.M21 = m21;
            this.M22 = m22;
            this.M23 = m23;
            this.M31 = m31;
            this.M32 = m32;
            this.M33 = m33;
            this.M41 = m41;
            this.M42 = m42;
            this.M43 = m43;
        }

        public static implicit operator Matrix4x4(FixedMatrix4x3 m) => new Matrix4x4(m.M11, m.M12, m.M13, 0f, m.M21, m.M22, m.M23, 0f, m.M31, m.M32, m.M33, 0f, m.M41, m.M42, m.M43, 1f);
        public static explicit operator FixedMatrix4x3(Matrix4x4 m) => new FixedMatrix4x3((Fixed)m.M11, (Fixed)m.M12, (Fixed)m.M13, (Fixed)m.M21, (Fixed)m.M22, (Fixed)m.M23, (Fixed)m.M31, (Fixed)m.M32, (Fixed)m.M33, (Fixed)m.M41, (Fixed)m.M42, (Fixed)m.M43);

        public override string ToString()
        {
            return ((Matrix4x4)this).ToString();
        }
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct FixedMatrix3x2
    {
        // [11][12]{0} {0}
        // [21][22]{0} {0}
        // [31][32]{1} {0}
        // {0} {0} {0} {1}

        // [11][21][31]{0}
        // [12][22][32]{0}
        // {0} {0} {1} {0}
        // {0} {0} {0} {1}

        public Fixed M11;
        public Fixed M12;
        public Fixed M21;
        public Fixed M22;
        public Fixed M31;
        public Fixed M32;

        //public Fixed M11;
        //public Fixed M21;
        //public Fixed M31;
        //public Fixed M12;
        //public Fixed M22;
        //public Fixed M32;
        private string DebuggerDisplay => ToString();

        public FixedMatrix3x2(Fixed m11, Fixed m12, Fixed m21, Fixed m22, Fixed m31, Fixed m32)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M21 = m21;
            this.M22 = m22;
            this.M31 = m31;
            this.M32 = m32;
        }

        public static implicit operator Matrix3x2(FixedMatrix3x2 m) => new Matrix3x2(m.M11, m.M12, m.M21, m.M22, m.M31, m.M32);
        public static implicit operator Matrix4x4(FixedMatrix3x2 m) => new Matrix4x4(m);

        public static explicit operator FixedMatrix3x2(Matrix3x2 m) => new FixedMatrix3x2((Fixed)m.M11, (Fixed)m.M12, (Fixed)m.M21, (Fixed)m.M22, (Fixed)m.M31, (Fixed)m.M32);
        public static explicit operator FixedMatrix3x2(Matrix4x4 m) => new FixedMatrix3x2((Fixed)m.M11, (Fixed)m.M12, (Fixed)m.M21, (Fixed)m.M22, (Fixed)m.M31, (Fixed)m.M32);

        public override string ToString()
        {
            return ((Matrix3x2)this).ToString();
        }
    }
}
