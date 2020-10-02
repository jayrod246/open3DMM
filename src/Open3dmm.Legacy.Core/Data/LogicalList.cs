using Open3dmm.Core.IO;
using System;
using System.IO;

namespace Open3dmm.Core.Data
{
    public class LogicalList : LogicalGroupBase
    {
        private int count;
        private int elementSize;
        private readonly MemoryStream memoryStream;

        public override int Count => count;

        public LogicalList()
        {
            memoryStream = IOHelper.CreateMemoryStream();
        }

        public override int ElementSize {
            get => elementSize;
            set {
                if (value <= 0)
                    throw new ArgumentException("ElementSize must be greater than zero.");
                for (int i = 0; i < Count; i++)
                {
                    int dataOffset = ElementSize * i;
                    MemoryMove(dataOffset + ElementSize, checked((int)memoryStream.Length) - dataOffset - ElementSize, dataOffset + value);
                }
                elementSize = value;
            }
        }

        public override bool IsItemAt(int index)
        {
            return index < count && index >= 0;
        }

        public override void AddBytes(Span<byte> newBytes)
        {
            newBytes.CopyTo(InsertBytes(count));
        }

        public override void InsertBytes(int index, Span<byte> newBytes)
        {
            newBytes.CopyTo(InsertBytes(index));
        }

        public Span<byte> AddBytes()
        {
            return InsertBytes(count);
        }

        public Span<byte> InsertBytes(int index)
        {
            if (index < 0 || index > count)
                throw new IndexOutOfRangeException();

            EnsureCapacity(checked((int)memoryStream.Length) + ElementSize);

            // Resize the stream.
            memoryStream.SetLength(memoryStream.Length + ElementSize);

            int dataOffset = index * ElementSize;
            if (index != count)
            {
                // Perform a memory-move to shift elements by one.
                MemoryMove(dataOffset, checked((int)memoryStream.Length) - dataOffset, dataOffset + ElementSize);
            }

            count++;
            return memoryStream.GetBuffer().AsSpan(dataOffset, ElementSize);
        }

        private Span<byte> ResizeBytes(int index, int newSize)
        {
            if (index < 0 || index >= count)
                throw new IndexOutOfRangeException();
            if (GetSize(index) == newSize)
                return GetBytes(index);
            // TODO: ResizeBytes(): This currently uses a naive approach, calls InsertBytes() and RemoveAt(), which both call MoveMemory(). Should be able to refactor so that MoveMemory() gets called only once.
            var newBytes = InsertBytes(index);
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
            int dataOffset = index * ElementSize;
            if (dataOffset >= 0)
            {
                if (index == count - 1)
                {
                    // Simplest code path. Clip data off of the end of stream.
                    memoryStream.SetLength(memoryStream.Length - ElementSize);
                }
                else
                {
                    // Perform a memory-move to shift elements by one.
                    MemoryMove(dataOffset + ElementSize, checked((int)memoryStream.Length) - dataOffset - ElementSize, dataOffset);
                }
            }
            count--;
        }

        public override int GetSize(int index)
        {
            if (index < 0 || index >= count)
                throw new IndexOutOfRangeException();
            return ElementSize;
        }

        public override Span<byte> GetBytes(int index)
        {
            if (index < 0 || index >= count)
                throw new IndexOutOfRangeException();
            return memoryStream.GetBuffer().AsSpan(index * ElementSize, ElementSize);
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

        private void EnsureCapacity(int newDataCapacity)
        {
            if (newDataCapacity > memoryStream.Capacity)
                memoryStream.Capacity = newDataCapacity;
        }

        private void MemoryMove(int fromData, int lenData, int toData)
        {
            // Assumes capacity is ensured by EnsureCapacity().
            memoryStream.GetBuffer().AsSpan(fromData, lenData).CopyTo(memoryStream.GetBuffer().AsSpan(toData, lenData));
        }

        public static LogicalList FromStream(IReadOnlyStream block, LogicalList list = null, int desiredElementSize = -1)
        {
            bool free = false;
            if (list == null)
            {
                list = new LogicalList();
                free = true;
            }

            try
            {
                if (!block.MagicNumber())
                    throw ThrowHelper.IOBadHeaderMagicNumber(block.Peek<int>());
                int elementSize = block.Read<int>();
                if (desiredElementSize != -1 && elementSize != desiredElementSize)
                    throw ThrowHelper.IOBadElementSize(elementSize, desiredElementSize);
                int count = block.Read<int>();
                while (--count >= 0)
                    block.ReadTo(list.AddBytes());
                return list;
            }
            catch
            {
                if (free)
                    list.Dispose();
                throw;
            }
        }

        internal static void ToStream(IStream block, LogicalList list)
        {
            block.Write(0x0303001);
            block.Write(list.ElementSize);
            block.Write(list.Count);
            int count = list.Count;
            while (--count >= 0)
                block.ReadTo(list.AddBytes());
        }

        public override void ToStream(IStream stream) => ToStream(stream, this);

        public override void Clear()
        {
            memoryStream.SetLength(0);
            count = 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                memoryStream.Dispose();
            }
        }
    }
}
