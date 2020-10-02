using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Open3dmm.Core.Data
{
    public class GenericGroup<T> : GenericGroup where T : unmanaged
    {
        public ref T this[int index] => ref GetChunk(index);

        public GenericGroup() : base(Unsafe.SizeOf<T>())
        {
        }

        public override void ToSequencer(IContiguousBlockSequencer sequencer)
        {
            base.ToSequencer(sequencer);
            if (ElementSize != Unsafe.SizeOf<T>())
                throw new InvalidOperationException();
        }

        public new ref T GetChunk(int index)
            => ref Unsafe.As<byte, T>(ref base.GetChunk(index)[0]);
    }

    public class GenericStrings<T> : GenericStrings, IEnumerable<KeyValuePair<T, string>> where T : unmanaged
    {
        public new KeyValuePair<T, string> this[int index] => new KeyValuePair<T, string>(GetKey(index), base[index]);
        public IReadOnlyList<T> Keys => new Indexer<T>(i => this[i].Key, () => Count);

        public GenericStrings() : base(Unsafe.SizeOf<T>())
        {
        }

        public override void ToSequencer(IContiguousBlockSequencer sequencer)
        {
            base.ToSequencer(sequencer);
            if (ElementSize != Unsafe.SizeOf<T>())
                throw new InvalidOperationException();
        }

        public ref T GetKey(int index)
            => ref Unsafe.As<byte, T>(ref GetBuffer(index)[0]);

        public bool TryGetString(T key, out string value)
        {
            for (int i = 0; i < Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(key, GetKey(i)))
                {
                    value = base[i];
                    return true;
                }
            }
            value = null;
            return false;
        }

        public IEnumerator<KeyValuePair<T, string>> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class GenericList<T> : GenericList, IList<T> where T : unmanaged
    {
        public T this[int index] {
            get => MemoryMarshal.Read<T>(GetBuffer(index));
            set => MemoryMarshal.Write(GetBuffer(index), ref value);
        }

        public GenericList() : base(Unsafe.SizeOf<T>())
        {
        }

        public override void ToSequencer(IContiguousBlockSequencer sequencer)
        {
            base.ToSequencer(sequencer);
            if (ElementSize != Unsafe.SizeOf<T>())
                throw new InvalidOperationException();
        }

        public void Add(T item) => Add(IOHelper.GetBytes(ref item));
        public void Insert(int index, T item) => Insert(index, IOHelper.GetBytes(ref item));

        public int IndexOf(T item)
        {
            for (int i = 0; i < Count; i++)
                if (EqualityComparer<T>.Default.Equals(this[i], item))
                    return i;
            return -1;
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
            => IndexOf(item) >= 0;

        public void CopyTo(T[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
                array[arrayIndex + i] = this[i];
        }

        public bool Remove(T item)
        {
            int index = IndexOf(item);
            if (index < 0)
                return false;
            RemoveAt(index);
            return true;
        }

        public bool IsReadOnly => false;

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class GenericStrings : GroupBase
    {
        private byte[] dataBuffer;
        private int dataLength;
        private byte[] indexBuffer;
        private int count;
        private int actualElementSize;
        private int magicNumber = 0x03030001;
        private int someIndex = -1;

        public string this[int index] {
            get => CreateString(GetStringSpan(index));
            set {
                RemoveStringOffset(GetStringOffset(index));
                GetStringOffset(index) = AddStringOffset(value);
            }
        }

        public override int Count => count;

        public IReadOnlyList<string> Strings => new Indexer<string>(i => this[i], () => Count);

        private GenericStrings()
        {
            dataBuffer = Array.Empty<byte>();
            indexBuffer = Array.Empty<byte>();
        }

        public GenericStrings(int elementSize) : this()
        {
            this.actualElementSize = elementSize + 4;
        }

        protected override bool IsItemAt(int index)
        {
            return index >= 0 && index < Count;
        }

        public override void ToSequencer(IContiguousBlockSequencer sequencer)
        {
            sequencer.Next(ref magicNumber);
            sequencer.Next(ref actualElementSize);
            sequencer.Next(ref count);
            sequencer.Next(ref dataLength);
            sequencer.Next(ref someIndex);
            sequencer.Next(ref dataBuffer, dataLength);
            sequencer.Next(ref indexBuffer, actualElementSize * Count);
        }

        public override int CalculateLogicalSize()
            => actualElementSize * Count + 20 + dataLength;

        public void Add(string value, ReadOnlySpan<byte> chunk)
        {
            Insert(Count, value, chunk);
        }

        public void Insert(int index, string value, ReadOnlySpan<byte> chunk)
        {
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (chunk.Length != ElementSize)
                throw new ArgumentException("Span length was not equal to ElementSize.", nameof(chunk));
            EnsureCapacity(ref indexBuffer, (Count + 1) * actualElementSize);

            MemoryMove(indexBuffer.AsSpan(), index * actualElementSize, (index + 1) * actualElementSize, (Count - index) * actualElementSize);
            chunk.CopyTo(GetBuffer(index));
            GetStringOffset(index) = AddStringOffset(value);
            count++;
        }

        public override void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            RemoveStringOffset(GetStringOffset(index));
            MemoryMove(indexBuffer.AsSpan(), (index + 1) * actualElementSize, index * actualElementSize, (Count - index - 1) * actualElementSize);
            count--;
        }

        public int IndexOf(string value)
        {
            for (int i = 0; i < Count; i++)
            {
                if (value.Equals(this[i], StringComparison.InvariantCulture))
                    return i;
            }
            return -1;
        }

        private Span<byte> GetStringSpan(int index)
        {
            int offset = GetStringOffset(index);
            int length = dataBuffer[offset];
            return dataBuffer.AsSpan(offset + 1, length);
        }

        private string CreateString(ReadOnlySpan<byte> bytes) => Encoding.ASCII.GetString(bytes);

        private void CopyString(ReadOnlySpan<char> chrs, Span<byte> dest)
        {
            dest[0] = (byte)chrs.Length;
            int i = 1;
            foreach (var c in chrs)
                dest[i++] = (byte)c;
        }

        private int AddStringOffset(string value)
        {
            int offset = dataLength;
            int length = value.Length + 1;
            EnsureCapacity(ref dataBuffer, dataLength + length);
            CopyString(value.AsSpan(), dataBuffer.AsSpan(dataLength, length));
            dataLength += length;
            return offset;
        }

        private void RemoveStringOffset(int offset)
        {
            int length = dataBuffer[offset] + 1;
            for (int i = 0; i < Count; i++)
            {
                ref var other = ref GetStringOffset(i);
                if (other > offset)
                    other -= length;
            }
            MemoryMove(dataBuffer.AsSpan(), offset + length, offset, dataLength - offset - length);
            dataLength -= length;
        }

        private ref int GetStringOffset(int index)
        {
            return ref Unsafe.As<byte, int>(ref indexBuffer[index * actualElementSize]);
        }

        public override Span<byte> GetBuffer(int index)
            => indexBuffer.AsSpan(index * actualElementSize + 4, ElementSize);

        public override int ElementSize => actualElementSize - 4;

        protected override void ResolveCore()
        {
            using var stream = Metadata.GetBlock();
            this.FromStream(stream);
        }
    }

    public class GenericList : GroupBase
    {
        private byte[] dataBuffer;
        private int count;
        private int elementSize;
        private int magicNumber = 0x03030001;

        public override int Count => count;

        private GenericList()
        {
            dataBuffer = Array.Empty<byte>();
        }

        public GenericList(int elementSize) : this()
        {
            this.elementSize = elementSize;
        }

        protected override bool IsItemAt(int index)
        {
            return index >= 0 && index < Count;
        }

        public override void ToSequencer(IContiguousBlockSequencer sequencer)
        {
            sequencer.Next(ref magicNumber);
            sequencer.Next(ref elementSize);
            sequencer.Next(ref count);
            sequencer.Next(ref dataBuffer, ElementSize * Count);
        }

        public override int CalculateLogicalSize()
            => ElementSize * Count + 12;

        public void Add(ReadOnlySpan<byte> bytes)
        {
            Insert(Count, bytes);
        }

        public void Insert(int index, ReadOnlySpan<byte> bytes)
        {
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (bytes.Length != ElementSize)
                throw new ArgumentException("Span length was not equal to ElementSize.", nameof(bytes));
            int offset = index * ElementSize;
            int dataLength = ElementSize * Count;
            EnsureCapacity(ref dataBuffer, dataLength + ElementSize);
            MemoryMove(dataBuffer.AsSpan(), offset, dataLength - offset, offset + ElementSize);
            bytes.CopyTo(GetBuffer(index));
            count++;
        }

        public override void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            int offset = index * ElementSize;
            int dataLength = ElementSize * Count;
            MemoryMove(dataBuffer.AsSpan(), offset + ElementSize, offset, dataLength - offset - ElementSize);
            count--;
        }

        public override Span<byte> GetBuffer(int index)
            => dataBuffer.AsSpan(index * ElementSize, ElementSize);

        public override int ElementSize => elementSize;

        protected override void ResolveCore()
        {
            using var stream = Metadata.GetBlock();
            this.FromStream(stream);
        }
    }

    public class GenericGroup : GroupBase
    {
        private byte[] dataBuffer;
        private int dataLength;
        private Entry[] entries;
        private int count;
        private int elementSize;
        private int magicNumber = 0x03030001;
        private int someIndex = -1;

        private GenericGroup()
        {
            dataBuffer = Array.Empty<byte>();
            entries = Array.Empty<Entry>();
        }

        public GenericGroup(int elementSize) : this()
        {
            this.elementSize = elementSize;
        }

        private struct Entry
        {
            public int Offset;
            public int Length;

            public Entry(int offset, int length)
            {
                this.Offset = offset;
                this.Length = length;
            }

            public void Deconstruct(out int offset, out int length)
            {
                offset = Offset;
                length = Length;
            }
        }

        public override int Count => count;

        protected override bool IsItemAt(int index)
        {
            return index >= 0 && index < Count && entries[index].Offset != -1;
        }

        public override void ToSequencer(IContiguousBlockSequencer sequencer)
        {
            sequencer.Next(ref magicNumber);
            sequencer.Next(ref count);
            sequencer.Next(ref dataLength);
            sequencer.Next(ref someIndex);
            sequencer.Next(ref elementSize);
            sequencer.Next(ref dataBuffer, dataLength);
            sequencer.Next(ref entries, Count);
        }

        public override int CalculateLogicalSize()
            => dataLength + 20 + Count * 8;

        public void Add(ReadOnlySpan<byte> payload, ReadOnlySpan<byte> chunk)
        {
            Insert(Count, payload, chunk);
        }

        public void Insert(int index, ReadOnlySpan<byte> payload, ReadOnlySpan<byte> chunk)
        {
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (chunk.Length != ElementSize)
                throw new ArgumentException("Span length was not equal to ElementSize.", nameof(chunk));
            int offset = GetInsertDataOffset(index, out _);
            int length = ElementSize + payload.Length;
            EnsureCapacity(ref entries, Count + 1);
            EnsureCapacity(ref dataBuffer, dataLength + length);
            FixOffsets(index, length);
            MemoryMove(dataBuffer.AsSpan(), offset, dataLength - offset, offset + length);
            MemoryMove(entries.AsSpan(), index, index + 1, Count - index);
            chunk.CopyTo(GetChunk(index));
            payload.CopyTo(GetPayload(index));
            entries[index] = new Entry(offset, length);
            dataLength += length;
            count++;
        }

        public void InsertPayload(int index, int offset, ReadOnlySpan<byte> src)
        {
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            var originalOffset = offset + GetInsertDataOffset(index, out int originalLength);
            if (offset < 0 || offset > originalLength)
                throw new ArgumentOutOfRangeException(nameof(offset));
            EnsureCapacity(ref dataBuffer, dataLength + src.Length);
            FixOffsets(index, src.Length);
            MemoryMove(dataBuffer.AsSpan(), originalOffset + offset, dataLength - originalOffset - offset, originalOffset + src.Length);
            src.CopyTo(GetPayload(index).Slice(offset));
            entries[index] = new Entry(originalOffset, originalLength + src.Length);
            dataLength += src.Length;
        }

        public void RemovePayload(int index, int offset, int length)
        {
            throw new NotImplementedException();
        }

        public override void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            int offset = GetInsertDataOffset(index, out int length);
            if (length > 0)
            {
                FixOffsets(index, -length);
                MemoryMove(dataBuffer.AsSpan(), offset + length, offset, dataLength - offset - length);
                dataLength -= length;
            }
            MemoryMove(entries.AsSpan(), index + 1, index, Count - index - 1);
            count--;
        }

        public override Span<byte> GetBuffer(int index)
            => dataBuffer.AsSpan(entries[index].Offset, entries[index].Length);

        public Span<byte> GetChunk(int index)
            => GetBuffer(index).Slice(0, ElementSize);

        public Span<byte> GetPayload(int index)
            => GetBuffer(index).Slice(ElementSize);

        public override int ElementSize => elementSize;

        private int GetInsertDataOffset(int index, out int length)
        {
            while (index >= 0)
            {
                if (entries[index].Offset >= 0)
                {
                    entries[index].Deconstruct(out int offset, out length);
                    return offset;
                }
                index--;
            }

            length = 0;
            return dataLength;
        }

        private void FixOffsets(int start, int amount)
        {
            for (int i = start; i < Count; i++)
            {
                if (entries[i].Offset < 0)
                    continue;
                entries[i].Offset += amount;
            }
        }

        public static GenericGroup FromStream(IReadOnlyStream stream)
        {
            var group = new GenericGroup();
            group.FromStream(stream);
            return group;
        }

        protected override void ResolveCore()
        {
            using var stream = Metadata.GetBlock();
            this.FromStream(stream);
        }
    }

    public abstract class GroupBase : ResolvableObject, IContiguousBlock
    {
        private const int defaultCapacity = 4;
        private const int maxArrayLength = 0X7FEFFFFF;

        public abstract int Count { get; }
        public abstract int ElementSize { get; }

        protected static void EnsureCapacity<T>(ref T[] arr, int min)
        {
            if (arr.Length < min)
            {
                int newCapacity = arr.Length == 0 ? defaultCapacity : arr.Length * 2;
                if ((uint)newCapacity > maxArrayLength) newCapacity = maxArrayLength;
                newCapacity = Math.Max(newCapacity, min);
                Array.Resize(ref arr, newCapacity);
            }
        }

        protected static void MemoryMove<T>(Span<T> source, int from, int to, int length)
        {
            source.Slice(from, length).CopyTo(source.Slice(to, length));
        }

        protected abstract bool IsItemAt(int index);

        public abstract Span<byte> GetBuffer(int index);

        public abstract void RemoveAt(int index);

        public abstract void ToSequencer(IContiguousBlockSequencer sequencer);

        public abstract int CalculateLogicalSize();

        public void Save()
        {
            var section = Metadata.Resolver.File.GetChunk(Metadata.Key).Section;
            section.Resize(CalculateLogicalSize());
            using var stream = BinaryStream.Create(section.Span);
            this.ToStream(stream);
        }

        public bool IsIndexOutOfBounds(int index) => !IsItemAt(index);
    }

    public class Indexer<T> : IReadOnlyList<T>
    {
        public Indexer(Func<int, T> selector, Func<int> getCount)
        {
            Selector = selector;
            GetCount = getCount;
        }

        public Func<int, T> Selector { get; }
        public Func<int> GetCount { get; }

        public T this[int index] => Selector(index);

        public int Count => GetCount();

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
