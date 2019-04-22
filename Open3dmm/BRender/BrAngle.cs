using System.Diagnostics;

namespace Open3dmm.BRender
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public struct BrAngle
    {
        private readonly ushort value;

        private string DebuggerDisplay => ToString();

        private BrAngle(ushort x)
        {
            value = x;
        }

        public int ToFixed()
        {
            return value;
        }

        public BrScalar ToDegrees()
        {
            return BrScalar.Multiply(BrScalar.FromFixed(value), BrScalar.FromInt(360));
        }

        public BrScalar ToRadians()
        {
            return BrScalar.Multiply(BrScalar.FromFixed(value), BrScalar.FromInt(2) * BrScalar.PI);
        }

        public static BrAngle AngleDegrees(BrScalar deg)
        {
            return new BrAngle((ushort)(deg * 182));
        }

        public static BrAngle AngleRadians(BrScalar rad)
        {
            return new BrAngle((ushort)(rad * 10430));
        }

        public static BrAngle FromScalar(BrScalar s)
        {
            return new BrAngle((ushort)s.ToFixed());
        }

        public static BrAngle FromRadians(BrScalar r)
        {
            return FromScalar(r * (0.5f / BrScalar.PI));
        }

        public static BrAngle FromDegrees(BrScalar d)
        {
            return FromScalar(d / 360f);
        }

        public override string ToString()
        {
            return $"{ToDegrees().ToString()}º";
        }
    }
}