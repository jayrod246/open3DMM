using Open3dmm;
using System;

namespace Open3dmm.Core
{
    public struct ReferenceIdentifier : IComparable<ReferenceIdentifier>
    {
        public Tag Tag;
        public int Index;

        public ReferenceIdentifier(int index, Tag tag)
        {
            this.Index = index;
            this.Tag = tag;
        }

        public int CompareTo(ReferenceIdentifier other)
        {
            if (Index.CompareTo(other.Index) == 0)
                return Tag.CompareTo(other.Tag);
            return Index.CompareTo(other.Index);
        }
    }
}
