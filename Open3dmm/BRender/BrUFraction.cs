using System.Diagnostics;

namespace Open3dmm.BRender
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct BrUFraction
    {
        private readonly ushort value;

        private string DebuggerDisplay => ToString();

        private BrUFraction(ushort x)
        {
            value = x;
        }

        public static BrUFraction FromScalar(BrScalar scalar)
        {
            return new BrUFraction((ushort)(scalar.ToFixed()));
        }

        public BrScalar ToScalar()
        {
            return BrScalar.FromFixed(value);
        }

        public static BrUFraction Add(BrUFraction a, BrUFraction b)
        {
            return FromScalar(a + b);
        }

        public static BrUFraction Subtract(BrUFraction a, BrUFraction b)
        {
            return FromScalar(a - b);
        }

        public static BrUFraction Multiply(BrUFraction a, BrUFraction b)
        {
            return FromScalar(a * b);
        }

        public static BrUFraction Divide(BrUFraction a, BrUFraction b)
        {
            return FromScalar(a / b);
        }

        public static implicit operator BrScalar(BrUFraction a)
        {
            return a.ToScalar();
        }

        public static explicit operator float(BrUFraction a)
        {
            return (float)a.ToScalar();
        }

        public static explicit operator BrUFraction(BrScalar a)
        {
            return FromScalar(a);
        }

        public static BrUFraction operator +(BrUFraction a, BrUFraction b)
        {
            return Add(a, b);
        }

        public static BrUFraction operator -(BrUFraction a, BrUFraction b)
        {
            return Subtract(a, b);
        }

        public static BrUFraction operator *(BrUFraction a, BrUFraction b)
        {
            return Multiply(a, b);
        }

        public static BrUFraction operator /(BrUFraction a, BrUFraction b)
        {
            return Divide(a, b);
        }

        public override string ToString()
        {
            return ToScalar().ToString();
        }
    }
}