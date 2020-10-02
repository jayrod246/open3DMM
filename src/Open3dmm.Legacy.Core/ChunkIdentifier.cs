using System;

namespace Open3dmm
{
    /// <summary>
    /// A data pair (chunk type, chunk number).
    /// </summary>
    public struct ChunkIdentifier : IComparable<ChunkIdentifier>, IEquatable<ChunkIdentifier>
    {
        public Tag Tag { get; set; }
        // TODO: IMPORTANT: Need to change this to UINT type!
        public int Number { get; set; }

        public ChunkIdentifier(Tag tag, int number)
            => (Tag, Number) = (tag, number);

        #region Overrides/Operators
        public static implicit operator ChunkIdentifier((Tag tag, int num) _) => new(_.tag, _.num);
        public static implicit operator (Tag, int)(ChunkIdentifier _) => (_.Tag, _.Number);
        public int CompareTo(ChunkIdentifier other)
            => Tag.CompareTo(other.Tag) == 0 ? Number.CompareTo(other.Number) : Tag.CompareTo(other.Tag);

        public override bool Equals(object obj)
            => obj is ChunkIdentifier identifier && Equals(identifier);

        public bool Equals(ChunkIdentifier other)
            => Tag == other.Tag && Number == other.Number;

        public override int GetHashCode()
            => HashCode.Combine(Tag, Number);

        public static bool operator ==(ChunkIdentifier left, ChunkIdentifier right)
            => left.Equals(right);

        public static bool operator !=(ChunkIdentifier left, ChunkIdentifier right)
            => !(left == right);

        public static bool operator <(ChunkIdentifier left, ChunkIdentifier right)
            => left.CompareTo(right) < 0;

        public static bool operator <=(ChunkIdentifier left, ChunkIdentifier right)
            => left.CompareTo(right) <= 0;

        public static bool operator >(ChunkIdentifier left, ChunkIdentifier right)
            => left.CompareTo(right) > 0;

        public static bool operator >=(ChunkIdentifier left, ChunkIdentifier right)
            => left.CompareTo(right) >= 0;
        #endregion
    }

    /// <summary>
    /// A chunk child (chunk type, chunk number, child ID).
    /// </summary>
    public readonly struct ChunkChild : IComparable<ChunkChild>, IEquatable<ChunkChild>
    {
        public ChunkIdentifier Identifier { get; init; }
        public int Chid { get; init; }

        public ChunkChild(ChunkIdentifier identifier, int childID)
            => (Identifier, Chid) = (identifier, childID);

        public ChunkChild(Tag childTag, int childNumber, int childID) : this(new(childTag, childNumber), childID) { }

        #region Overrides/Operators
        public override bool Equals(object obj)
            => obj is ChunkChild child && Equals(child);

        public bool Equals(ChunkChild other)
            => (Identifier, Chid).Equals((other.Identifier, other.Chid));

        public override int GetHashCode()
            => HashCode.Combine(Identifier, Chid);

        public int CompareTo(ChunkChild other)
            => Chid.CompareTo(other.Chid) == 0 ? Identifier.CompareTo(other.Identifier) : Chid.CompareTo(other.Chid);

        public static bool operator ==(ChunkChild left, ChunkChild right)
            => left.Equals(right);

        public static bool operator !=(ChunkChild left, ChunkChild right)
            => !(left == right);

        public static bool operator <(ChunkChild left, ChunkChild right)
            => left.CompareTo(right) < 0;

        public static bool operator <=(ChunkChild left, ChunkChild right)
            => left.CompareTo(right) <= 0;

        public static bool operator >(ChunkChild left, ChunkChild right)
            => left.CompareTo(right) > 0;

        public static bool operator >=(ChunkChild left, ChunkChild right)
            => left.CompareTo(right) >= 0;
        #endregion
    }

    /// <summary>
    /// A data pair (chunk child type, chunk child ID).
    /// </summary>
    public struct ChunkChildIdentifier
    {
        public Tag Tag { get; set; }
        // TODO: IMPORTANT: Need to change this to UINT type!
        public int Chid { get; set; }

        public ChunkChildIdentifier(int chid, Tag tag)
        {
            Chid = chid;
            Tag = tag;
        }

        public static implicit operator ChunkChildIdentifier((int id, Tag tag) _) => new(_.id, _.tag);
        public void Deconstruct(out Tag tag, out int id) => (tag, id) = (Tag, Chid);
    }
}
