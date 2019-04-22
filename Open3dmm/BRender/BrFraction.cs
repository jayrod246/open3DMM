using System;
using System.Diagnostics;

namespace Open3dmm.BRender
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct BrFraction
    {
        private readonly short value;

        private string DebuggerDisplay => ToString();

        private BrFraction(short x)
        {
            value = x;
        }

        public static BrFraction FromScalar(BrScalar scalar)
        {
            return new BrFraction((short)(scalar.ToFixed() / 2));
        }

        public static BrFraction FromFloat(float value)
        {
            return FromScalar(BrScalar.FromFloat(value));
        }

        public BrScalar ToScalar()
        {
            return BrScalar.FromFixed(value * 2);
        }

        public float ToFloat()
        {
            return ToScalar().ToFloat();
        }

        public static BrFraction Add(BrFraction a, BrFraction b)
        {
            return FromScalar(a + b);
        }

        public static BrFraction Subtract(BrFraction a, BrFraction b)
        {
            return FromScalar(a - b);
        }

        public static BrFraction Multiply(BrFraction a, BrFraction b)
        {
            return FromScalar(a * b);
        }

        public static BrFraction Divide(BrFraction a, BrFraction b)
        {
            return FromScalar(a / b);
        }

        public static implicit operator BrScalar(BrFraction a)
        {
            return a.ToScalar();
        }

        public static explicit operator float(BrFraction a)
        {
            return (float)a.ToScalar();
        }

        public static explicit operator BrFraction(BrScalar a)
        {
            return FromScalar(a);
        }

        public static BrFraction operator +(BrFraction a, BrFraction b)
        {
            return Add(a, b);
        }

        public static BrFraction operator -(BrFraction a, BrFraction b)
        {
            return Subtract(a, b);
        }

        public static BrFraction operator *(BrFraction a, BrFraction b)
        {
            return Multiply(a, b);
        }

        public static BrFraction operator /(BrFraction a, BrFraction b)
        {
            return Divide(a, b);
        }

        public override string ToString()
        {
            return ToScalar().ToString();
        }
    }
}