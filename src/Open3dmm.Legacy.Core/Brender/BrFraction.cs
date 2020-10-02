using System.Diagnostics;

namespace Open3dmm.Core.Brender
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct BrFraction
    {
        public readonly short PackedValue;

        private string DebuggerDisplay => ToString();

        private BrFraction(short packedValue)
        {
            PackedValue = packedValue;
        }

        public static implicit operator Fixed(BrFraction a)
        {
            return new Fixed(a.PackedValue * 2);
        }

        public static implicit operator float(BrFraction a)
        {
            return (Fixed)a;
        }

        public static explicit operator BrFraction(Fixed a)
        {
            return new BrFraction((short)(a.PackedValue / 2));
        }

        public static explicit operator BrFraction(float a)
        {
            return (BrFraction)(Fixed)a;
        }

        public override string ToString()
        {
            return ((float)this).ToString();
        }
    }
}