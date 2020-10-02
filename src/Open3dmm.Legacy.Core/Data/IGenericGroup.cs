using System;

namespace Open3dmm.Core.Data
{
    public interface IGenericGroup
    {
        int ChunkSize { get; set; }
        int Count { get; }
        int ChunkCapacity { get; set; }
        int PayloadCapacity { get; set; }

        Span<byte> GetChunk(int index);
        void SetChunk(int index, ReadOnlySpan<byte> src);
        Span<byte> GetPayload(int index);
        void SetPayload(int index, int offset, ReadOnlySpan<byte> src);
        void Add(Span<byte> payload, ReadOnlySpan<byte> chunk);
        void Insert(int index, ReadOnlySpan<byte> payload, ReadOnlySpan<byte> chunk);
        void RemoveAt(int index);
        void InsertPayload(int index, int offset, ReadOnlySpan<byte> src);
        void RemovePayload(int index, int offset, int length);
        void MovePayload(int fromIndex, int fromOffset, int toIndex, int toOffset, int length);
    }
}
