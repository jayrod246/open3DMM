using Open3dmm.Core.IO;
using System;

namespace Open3dmm.Core.Data
{
    public class Section
    {
        public Section()
        {
        }

        public Section(ReadOnlySpan<byte> data)
        {
            this.data = data.ToArray();
        }

        private Chunk owner;
        private Memory<byte> data = Memory<byte>.Empty;

        public Chunk Owner => owner;

        public virtual Memory<byte> Memory => data;
        public virtual Span<byte> Span => data.Span;

        internal int FileOffset { get; set; }
        public int Length => Memory.Length;

        public virtual void Resize(int newSize)
        {
            if (newSize == 0)
                data = Array.Empty<byte>();
            else if (newSize < Memory.Length)
                data = data.Slice(0, newSize);
            else
            {
                var newData = new byte[newSize];
                if (!data.IsEmpty)
                    data.Span.CopyTo(newData.AsSpan(0, data.Length));
                data = newData;
            }
        }

        internal void ChangeOwner(Chunk newOwner)
        {
            if (newOwner != null && owner != null)
                throw new InvalidOperationException("The section is owned by another chunk.");
            owner = newOwner;
        }
    }
}
