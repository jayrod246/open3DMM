using Open3dmm.Core.IO;
using System;
using System.Buffers;
using System.IO;

namespace Open3dmm.Core.Data
{
    public class LogicalGroup : LogicalGroupBase
    {
        private bool isDisposed;
        private Entry[] entries;
        private int count;
        private readonly MemoryStream memoryStream;

        public override int Count => count;

        private struct Entry
        {
            public int Offset;
            public int Length;
        }

        public LogicalGroup()
        {
            memoryStream = IOHelper.CreateMemoryStream();
            entries = ArrayPool<Entry>.Shared.Rent(128);
        }

        public override int ElementSize { get; set; }

        public override bool IsItemAt(int index)
        {
            return index < count && index >= 0 && entries[index].Offset >= 0;
        }

        public override void AddBytes(Span<byte> newBytes)
        {
            newBytes.CopyTo(InsertBytes(count, newBytes.Length));
        }

        public override void InsertBytes(int index, Span<byte> newBytes)
        {
            newBytes.CopyTo(InsertBytes(index, newBytes.Length));
        }

        public virtual Span<byte> AddBytes(int size)
        {
            return InsertBytes(count, size);
        }

        public virtual Span<byte> InsertBytes(int index, int size)
        {
            if (index < 0 || index > count)
                throw new IndexOutOfRangeException();
            Entry newEntry;
            if (size == 0)
            {
                newEntry.Offset = -1;
                newEntry.Length = 0;
            }
            else
            {
                EnsureCapacity(count + 1, checked((int)memoryStream.Length) + size);

                // Resize the stream.
                memoryStream.SetLength(memoryStream.Length + size);

                if (index == count)
                {
                    // Simplest code path. Append data to the end of stream.
                    newEntry.Offset = checked((int)(memoryStream.Length - size));
                }
                else
                {
                    int dataOffset = GetInsertDataOffset(index);
                    newEntry.Offset = dataOffset;
                    // Fix offsets of elements beyond insertion point.
                    for (int i = index; i < count; i++)
                    {
                        if (entries[i].Offset < 0)
                            continue;
                        entries[i].Offset += size;
                    }
                    // Perform a memory-move to shift elements by one.
                    MemoryMove(index, count - index, index + 1, dataOffset, checked((int)memoryStream.Length) - dataOffset, dataOffset + size);
                }
            }
            newEntry.Length = size;
            entries[index] = newEntry;
            count++;
            if (newEntry.Offset < 0)
                return Span<byte>.Empty;
            return memoryStream.GetBuffer().AsSpan(newEntry.Offset, newEntry.Length);
        }

        public void Resize(int newSize)
        {
            if (Count == newSize)
                return;
            // TODO: Resize(): This currently uses a naive approach, calls AddBytes() and RemoveAt(), which both call MoveMemory(). Should be able to refactor so that MoveMemory() gets called only once.
            if (newSize < Count)
            {
                while (newSize < Count)
                    RemoveAt(Count - 1);
            }
            else
            {
                while (newSize > Count)
                    AddBytes(0);
            }
        }

        public virtual Span<byte> ResizeBytes(int index, int newSize)
        {
            if (index < 0 || index >= count)
                throw new IndexOutOfRangeException();
            if (GetSize(index) == newSize)
                return GetBytes(index);
            // TODO: ResizeBytes(): This currently uses a naive approach, calls InsertBytes() and RemoveAt(), which both call MoveMemory(). Should be able to refactor so that MoveMemory() gets called only once.
            var newBytes = InsertBytes(index, newSize);
            var src = GetBytes(index + 1);
            if (src.Length > newSize)
                src.Slice(0, newSize).CopyTo(newBytes);
            else
            {
                src.CopyTo(newBytes.Slice(0, src.Length));
                newBytes.Slice(src.Length).Fill(0); // Zero-mem
            }

            RemoveAt(index + 1); // Remove original bytes
            return GetBytes(index);
        }

        public override void RemoveAt(int index)
        {
            if (index < 0 || index >= count)
                throw new IndexOutOfRangeException();
            int dataLength = entries[index].Length;
            int dataOffset = entries[index].Offset;
            if (dataOffset >= 0)
            {
                if (index == count - 1)
                {
                    // Simplest code path. Clip data off of the end of stream.
                    memoryStream.SetLength(memoryStream.Length - entries[index].Length);
                }
                else
                {
                    // Fix offsets of elements beyond index of removal.
                    for (int i = index + 1; i < count; i++)
                    {
                        if (entries[i].Offset < 0)
                            continue;
                        entries[i].Offset -= dataLength;
                    }
                    // Perform a memory-move to shift elements by one.
                    MemoryMove(index + 1, count - index - 1, index, dataOffset + dataLength, checked((int)memoryStream.Length) - dataOffset - dataLength, dataOffset);
                }
            }
            count--;
        }

        public override int GetSize(int index)
        {
            if (index < 0 || index >= count)
                throw new IndexOutOfRangeException();
            int size = entries[index].Length;
            return Math.Max(size, 0);
        }

        public override Span<byte> GetBytes(int index)
        {
            if (index < 0 || index >= count)
                throw new IndexOutOfRangeException();
            var entry = entries[index];
            if (entry.Offset < 0)
                return Span<byte>.Empty;
            return memoryStream.GetBuffer().AsSpan(entry.Offset, entry.Length);
        }

        public override void SetBytes(int index, Span<byte> newBytes)
        {
            if (index < 0 || index >= count)
                throw new IndexOutOfRangeException();
            var oldBytes = GetBytes(index);
            if (newBytes.Length == oldBytes.Length)
                newBytes.CopyTo(oldBytes);
            else
                newBytes.CopyTo(ResizeBytes(index, newBytes.Length));
        }

        public static LogicalGroup FromStream(IReadOnlyStream stream, LogicalGroup group = null, int desiredElementSize = -1)
        {
            bool free = false;
            if (group == null)
            {
                group = new LogicalGroup();
                free = true;
            }

            try
            {
                int magicNumber;
                int unknown;

                if ((magicNumber = stream.Read<int>()) != 0x03030001)
                    throw ThrowHelper.BadMagicNumber(magicNumber);
                int count = stream.Read<int>();
                int dataLength = stream.Read<int>();
                if ((unknown = stream.Read<int>()) != -1)
                    throw ThrowHelper.BadMagicNumber(unknown);
                int elementSize = stream.Read<int>();
                if (desiredElementSize != -1 && desiredElementSize != elementSize)
                    throw ThrowHelper.BadGroupKeySize(elementSize, desiredElementSize);
                group.Clear();
                group.ElementSize = elementSize;
                using var data = MemoryPool<byte>.Shared.Rent(dataLength);
                using var index = MemoryPool<int>.Shared.Rent(count * 2);
                stream.ReadTo(data.Memory.Span.Slice(0, dataLength));
                stream.ReadTo(index.Memory.Span.Slice(0, count * 2));

                for (int i = 0; i < count; i++)
                {
                    int offset = index.Memory.Span[i * 2];
                    if (offset < 0)
                        group.AddBytes(0); // Add empty
                    else
                    {
                        int length = index.Memory.Span[i * 2 + 1];
                        data.Memory.Span.Slice(offset, length).CopyTo(group.AddBytes(length));
                    }
                }
                return group;
            }
            catch
            {
                if (free)
                    group.Dispose();
                throw;
            }
        }

        internal static void ToStream(IStream stream, LogicalGroup group)
        {
            int dataLength = checked((int)group.memoryStream.Length);
            stream.Write(0x03030001);
            stream.Write(group.Count);
            stream.Write(dataLength);
            stream.Write(-1);
            stream.Write(group.ElementSize);
            stream.Write(group.memoryStream.GetBuffer().AsSpan(0, checked((int)group.memoryStream.Length)));
            stream.Write(group.entries.AsSpan(0, group.Count));
        }

        public override void ToStream(IStream stream) => ToStream(stream, this);

        public override void Clear()
        {
            memoryStream.SetLength(0);
            ArrayPool<Entry>.Shared.Return(entries);
            entries = ArrayPool<Entry>.Shared.Rent(128);
            count = 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (isDisposed) return;
            memoryStream.Dispose();
            ArrayPool<Entry>.Shared.Return(entries);
            isDisposed = true;
        }

        private int GetInsertDataOffset(int index)
        {
            int dataOffset = 0;
            while (index >= 0)
            {
                if ((dataOffset = entries[index].Offset) >= 0)
                    break;
                index--;
            }
            if (dataOffset < 0)
                return 0;
            return dataOffset;
        }

        private void EnsureCapacity(int newEntryCapacity, int newDataCapacity)
        {
            if (newEntryCapacity > entries.Length)
            {
                var newEntries = ArrayPool<Entry>.Shared.Rent(newEntryCapacity);
                entries.AsSpan(0, count).CopyTo(newEntries.AsSpan(0, count));
                ArrayPool<Entry>.Shared.Return(entries);
                entries = newEntries;
            }

            if (newDataCapacity > memoryStream.Capacity)
                memoryStream.Capacity = newDataCapacity;
        }

        private void MemoryMove(int fromEntry, int numEntries, int toEntry, int fromData, int lenData, int toData)
        {
            // Assumes capacity is ensured by EnsureCapacity().
            entries.AsSpan(fromEntry, numEntries).CopyTo(entries.AsSpan(toEntry, numEntries));
            var buffer = memoryStream.GetBuffer();
            buffer.AsSpan(fromData, lenData).CopyTo(buffer.AsSpan(toData, lenData));
        }
    }
}
