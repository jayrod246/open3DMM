using Open3dmm;
using System;

namespace Open3dmm.Core.Data
{
    public struct LogicalReference : IComparable<LogicalReference>, IEquatable<LogicalReference>
    {
        public ChunkIdentifier TargetIdentifier;
        public int Index;

        public LogicalReference(int index, ChunkIdentifier targetIdentifier)
        {
            this.TargetIdentifier = targetIdentifier;
            this.Index = index;
        }

        public static readonly int Size = 12;

        public int CompareTo(LogicalReference other)
        {
            if (Index.CompareTo(other.Index) == 0)
                return TargetIdentifier.CompareTo(other.TargetIdentifier);
            return Index.CompareTo(other.Index);
        }

        public override bool Equals(object obj)
        {
            return obj is LogicalReference reference && Equals(reference);
        }

        public bool Equals(LogicalReference other)
        {
            return this.TargetIdentifier.Equals(other.TargetIdentifier) &&
                   this.Index == other.Index;
        }

        public override int GetHashCode()
        {
            int hashCode = -1410597250;
            hashCode = hashCode * -1521134295 + this.TargetIdentifier.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Index.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(LogicalReference left, LogicalReference right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LogicalReference left, LogicalReference right)
        {
            return !(left == right);
        }
    }
}
