using System.Diagnostics;

namespace Open3dmm.Core.Brender
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct BrUFraction
    {
        public readonly ushort PackedValue;

        private string DebuggerDisplay => ToString();

        private BrUFraction(ushort packedValue)
        {
            PackedValue = packedValue;
        }

        public static implicit operator Fixed(BrUFraction a)
        {
            return new Fixed(a.PackedValue);
        }

        public static implicit operator float(BrUFraction a)
        {
            return (Fixed)a;
        }

        public static explicit operator BrUFraction(Fixed a)
        {
            return new BrUFraction((ushort)(a.PackedValue));
        }

        public static explicit operator BrUFraction(float a)
        {
            return (BrUFraction)(Fixed)a;
        }

        public override string ToString()
        {
            return ((float)this).ToString();
        }
    }
}