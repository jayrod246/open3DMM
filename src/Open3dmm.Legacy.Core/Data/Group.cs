using System;

namespace Open3dmm.Core.Data
{
    public class Group : IGenericGroup
    {
        private int chunkSize;
        private int chunkCapacity;
        private int payloadCapacity;
        private byte[] element;

        public int ChunkSize {
            get => chunkSize;
            set {
                if (value <= 0) throw new ArgumentException();
                chunkSize = value;
            }
        }
        public int Count { get; } = 1;
        public int ChunkCapacity {
            get => chunkCapacity;
            set {
                if (value <= 0) throw new ArgumentException("ChunkCapacity must be greater than zero.");
                chunkCapacity = value;
            }
        }
        public int PayloadCapacity {
            get => payloadCapacity;
            set {
                if (value <= 0) throw new ArgumentException("PayloadCapacity must be greater than zero.");
                payloadCapacity = value;
            }
        }

        public Span<byte> GetElement(int index)
        {
            return this.element;
        }

        public Group(int chunkSize)
        {
            this.chunkSize = chunkSize;
            element = new byte[ChunkSize + 4];
        }

        public Span<byte> GetChunk(int index)
        {
            if (index < 0 || index >= Count) throw new ArgumentException("Index was out of bounds.");
            return GetElement(index).Slice(0, ChunkSize);
        }

        public void SetChunk(int index, ReadOnlySpan<byte> src)
        {
            if (index < 0 || index >= Count) throw new ArgumentException("Index was out of bounds.");
            if (src.Length != ChunkSize) throw new ArgumentException("");
            src.CopyTo(GetChunk(index));
        }

        public Span<byte> GetPayload(int index)
        {
            if (index < 0 || index >= Count) throw new ArgumentException("Index was out of bounds.");
            return GetElement(index).Slice(ChunkSize);
        }

        public void SetPayload(int index, int offset, ReadOnlySpan<byte> src)
        {
            if (index < 0 || index >= Count) throw new ArgumentException("Index was out of bounds.");
            if (offset < 0 || offset == 1024) throw new ArgumentException("Offset was out of bounds.");
            if (src.Length == 1024) throw new ArgumentException();
            src.CopyTo(GetElement(index).Slice(ChunkSize));
        }

        public void Add(Span<byte> payload, ReadOnlySpan<byte> chunk)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, ReadOnlySpan<byte> payload, ReadOnlySpan<byte> chunk)
        {
            if (index < 0 || index >= Count) throw new ArgumentException("Index was out of bounds.");
            if (chunk.Length != ChunkSize) throw new ArgumentException("Chunk length not equal to ChunkSize");
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public void InsertPayload(int index, int offset, ReadOnlySpan<byte> src)
        {
            throw new NotImplementedException();
        }

        public void RemovePayload(int index, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public void MovePayload(int fromIndex, int fromOffset, int toIndex, int toOffset, int length)
        {
            throw new NotImplementedException();
        }
    }
}
