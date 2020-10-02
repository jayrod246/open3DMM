using System.Collections.Generic;

namespace Open3dmm.Core.Actors
{
    public struct PathIndex
    {
        public int Base { get; set; }
        private Fixed tween;
        public float Tween { get => this.tween; set => this.tween = (Fixed)value; }
        public int Delay { get; set; }
        public static bool operator <(PathIndex a, PathIndex b) => a.Base < b.Base && a.Tween < b.Tween;
        public static bool operator >(PathIndex a, PathIndex b) => a.Base > b.Base && a.Tween > b.Tween;
        public static bool operator <=(PathIndex a, PathIndex b) => a < b || a == b;
        public static bool operator >=(PathIndex a, PathIndex b) => a > b || a == b;
        public static bool operator ==(PathIndex a, PathIndex b) => a.Base == b.Base && a.Tween == b.Tween;
        public static bool operator !=(PathIndex a, PathIndex b) => a.Base != b.Base && a.Tween != b.Tween;

        public override bool Equals(object obj)
        {
            return obj is PathIndex index &&
                   Base == index.Base &&
                   Tween == index.Tween;
        }

        public override int GetHashCode()
        {
            int hashCode = -2040625740;
            hashCode = hashCode * -1521134295 + Base.GetHashCode();
            hashCode = hashCode * -1521134295 + Tween.GetHashCode();
            return hashCode;
        }
    }
}
