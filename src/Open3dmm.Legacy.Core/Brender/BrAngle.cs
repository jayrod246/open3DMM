using System;
using System.Diagnostics;

namespace Open3dmm.Core.Brender
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct BrAngle
    {
        public readonly ushort PackedValue;

        private string DebuggerDisplay => ToString();

        public BrAngle(ushort packedValue)
        {
            PackedValue = packedValue;
        }

        public float ToDegrees()
        {
            return 360f * new Fixed(PackedValue);
        }

        public float ToRadians()
        {
            return 2f * (float)Math.PI * new Fixed(PackedValue);
        }

        public static BrAngle AngleDegrees(float deg)
        {
            const float maxdeg = 360f;
            deg = (float)Math.IEEERemainder(deg, maxdeg);
            if (deg < 0)
                deg += maxdeg;
            return new BrAngle((ushort)(deg * 182));
        }

        public static BrAngle AngleRadians(float rad)
        {
            const float maxrad = 6.28319f;
            rad = (float)Math.IEEERemainder(rad, maxrad);
            if (rad < 0)
                rad += maxrad;
            return new BrAngle((ushort)(rad * 10430));
        }

        public override string ToString()
        {
            return $"{ToDegrees().ToString()}º";
        }
    }
}