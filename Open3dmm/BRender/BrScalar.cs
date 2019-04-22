using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Open3dmm.BRender
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct BrScalar : IEquatable<BrScalar>
    {
        public const int FixedOne = 65536;
        private readonly int value;

        private string DebuggerDisplay => ToString();

        public static readonly BrScalar PI = FromFloat(3.14159265358979323846f);
        public static readonly BrScalar One = FromFixed(FixedOne);
        public static readonly BrScalar E = FromFloat((float)Math.E);
        public static readonly BrScalar Epsilon = FromFixed(0x0001);
        public static readonly BrScalar MaxValue = FromFixed(0x7fffffff);
        public static readonly BrScalar MinValue = FromFixed(unchecked((int)0x80000000));

        private BrScalar(int x)
        {
            value = x;
        }

        public BrScalar Clamped(BrScalar min, BrScalar max)
        {
            if (this < min) return min;
            if (this > max) return max;
            return this;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BrScalar Make(int x)
        {
            return new BrScalar(x);
        }

        public static BrScalar Add(BrScalar a, BrScalar b)
        {
            return FromFloat(a.ToFloat() + b.ToFloat());
        }

        public static BrScalar Subtract(BrScalar a, BrScalar b)
        {
            return FromFloat(a.ToFloat() - b.ToFloat());
        }

        public static BrScalar Multiply(BrScalar a, BrScalar b)
        {
            return FromFloat(a.ToFloat() * b.ToFloat());
        }

        public static BrScalar Divide(BrScalar a, BrScalar b)
        {
            return FromFloat(a.ToFloat() / b.ToFloat());
        }

        public static BrScalar FromFloat(float f)
        {
            return Make((int)(f * FixedOne));
        }

        public static BrScalar FromInt(int i)
        {
            return FromFloat((float)i);
        }

        public static BrScalar FromFixed(int x)
        {
            return Make(x);
        }

        public float ToFloat()
        {
            return (float)value / FixedOne;
        }

        public int ToInt()
        {
            return (int)ToFloat();
        }

        public int ToFixed()
        {
            return (int)value;
        }

        public BrAngle ToAngle()
        {
            return BrAngle.FromScalar(this);
        }

        public BrFraction ToFraction()
        {
            return BrFraction.FromScalar(this);
        }

        public BrUFraction ToUFraction()
        {
            return BrUFraction.FromScalar(this);
        }

        public override bool Equals(object obj)
        {
            return obj is BrScalar && Equals((BrScalar)obj);
        }

        public bool Equals(BrScalar other)
        {
            return EqualityComparer<int>.Default.Equals(this.value, other.value);
        }

        public override int GetHashCode()
        {
            return -1584136870 + EqualityComparer<int>.Default.GetHashCode(this.value);
        }

        public static explicit operator float(BrScalar a)
        {
            return a.ToFloat();
        }

        public static implicit operator BrScalar(float a)
        {
            return FromFloat(a);
        }

        public static BrScalar operator +(BrScalar a, BrScalar b)
        {
            return Add(a, b);
        }

        public static BrScalar operator -(BrScalar a, BrScalar b)
        {
            return Subtract(a, b);
        }

        public static BrScalar operator *(BrScalar a, BrScalar b)
        {
            return Multiply(a, b);
        }

        public static BrScalar operator /(BrScalar a, BrScalar b)
        {
            return Divide(a, b);
        }

        public static bool operator ==(BrScalar a, BrScalar b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(BrScalar a, BrScalar b)
        {
            return !Equals(a, b);
        }

        public static bool operator <=(BrScalar a, BrScalar b)
        {
            return a <= b;
        }

        public static bool operator >=(BrScalar a, BrScalar b)
        {
            return a >= b;
        }

        public static bool operator <(BrScalar a, BrScalar b)
        {
            return a < b;
        }

        public static bool operator >(BrScalar a, BrScalar b)
        {
            return a > b;
        }

        public override string ToString()
        {
            return ToFloat().ToString();
        }
    }
}