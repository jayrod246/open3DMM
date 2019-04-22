using System;
using System.Collections.Generic;

namespace Open3dmm
{
    public struct IMAGE_IMPORT_DESCRIPTOR : IEquatable<IMAGE_IMPORT_DESCRIPTOR>
    {
        public IntPtr OriginalFirstThunk;
        public IntPtr TimeDateStamp;
        public IntPtr ForwarderChain;
        public IntPtr Name;
        public IntPtr FirstThunk;
        public static readonly IMAGE_IMPORT_DESCRIPTOR Empty = default;

        public override bool Equals(object obj)
        {
            return obj is IMAGE_IMPORT_DESCRIPTOR dESCRIPTOR && Equals(dESCRIPTOR);
        }

        public bool Equals(IMAGE_IMPORT_DESCRIPTOR other)
        {
            return EqualityComparer<IntPtr>.Default.Equals(this.OriginalFirstThunk, other.OriginalFirstThunk) &&
                   EqualityComparer<IntPtr>.Default.Equals(this.TimeDateStamp, other.TimeDateStamp) &&
                   EqualityComparer<IntPtr>.Default.Equals(this.ForwarderChain, other.ForwarderChain) &&
                   EqualityComparer<IntPtr>.Default.Equals(this.Name, other.Name) &&
                   EqualityComparer<IntPtr>.Default.Equals(this.FirstThunk, other.FirstThunk);
        }

        public override int GetHashCode()
        {
            var hashCode = 120373722;
            hashCode = hashCode * -1521134295 + EqualityComparer<IntPtr>.Default.GetHashCode(this.OriginalFirstThunk);
            hashCode = hashCode * -1521134295 + EqualityComparer<IntPtr>.Default.GetHashCode(this.TimeDateStamp);
            hashCode = hashCode * -1521134295 + EqualityComparer<IntPtr>.Default.GetHashCode(this.ForwarderChain);
            hashCode = hashCode * -1521134295 + EqualityComparer<IntPtr>.Default.GetHashCode(this.Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<IntPtr>.Default.GetHashCode(this.FirstThunk);
            return hashCode;
        }

        public static bool operator ==(IMAGE_IMPORT_DESCRIPTOR left, IMAGE_IMPORT_DESCRIPTOR right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IMAGE_IMPORT_DESCRIPTOR left, IMAGE_IMPORT_DESCRIPTOR right)
        {
            return !(left == right);
        }
    }
}
