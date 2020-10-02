using Open3dmm;
using System;

namespace Open3dmm.Core.IO
{
    public struct GlobalReference : IComparable<GlobalReference>
    {
        public GlobalReference(int product, Tag tag, int number) : this(product, new ChunkIdentifier(tag, number))
        {
        }

        public GlobalReference(int product, ChunkIdentifier identifier)
        {
            ProductKey = product;
            Identifier = identifier;
            resolverPtr = 0;
        }

        public int ProductKey;
        private int resolverPtr;
        public ChunkIdentifier Identifier { get; set; }

        public int CompareTo(GlobalReference other)
        {
            if (ProductKey.CompareTo(other.ProductKey) == 0)
                return Identifier.CompareTo(other.Identifier);
            return ProductKey.CompareTo(other.ProductKey);
        }
    }
}
