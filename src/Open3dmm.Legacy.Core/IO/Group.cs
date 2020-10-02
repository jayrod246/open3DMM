using Open3dmm.Core.Resolvers;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Open3dmm.Core.IO
{
    public class Group<TItem> : IReadOnlyCollection<KeyValuePair<TItem, Memory<byte>>> where TItem : unmanaged
    {
        private static Stack<Blob> freed = new Stack<Blob>(128);
        private readonly List<Blob> blobs;

        public Group() : this(0)
        {
        }

        public Group(int capacity)
        {
            blobs = new List<Blob>(capacity);
        }

        public ref TItem this[int index] => ref GetItem(index);

        public int Count => blobs.Count;

        class Blob
        {
            public IMemoryOwner<byte> MemoryOwner;
            public int Length;

            public KeyValuePair<TItem, Memory<byte>> MakeKeyValuePair()
            {
                var memory = MemoryOwner.Memory;
                var key = Unsafe.ReadUnaligned<TItem>(ref memory.Span[0]);
                int size = Unsafe.SizeOf<TItem>();
                memory = memory.Slice(size, Length - size);
                return new KeyValuePair<TItem, Memory<byte>>(key, memory);
            }

            public static Blob OutOfBounds = new Blob();
        }

        private static IComparer<Blob> BlobComparer(IComparer<TItem> comparer) => Comparer<Blob>.Create((Blob x, Blob y) =>
           {
               var a = Unsafe.ReadUnaligned<TItem>(ref x.MemoryOwner.Memory.Span[0]);
               var b = Unsafe.ReadUnaligned<TItem>(ref y.MemoryOwner.Memory.Span[0]);
               return comparer.Compare(a, b);
           });

        public void Add(ReadOnlySpan<byte> data)
        {
            Insert(blobs.Count, data);
        }

        public void AddEmpty(int length)
        {
            InsertEmpty(blobs.Count, length);
        }

        public void Insert(int index, ReadOnlySpan<byte> data)
        {
            if (index < 0 || index > Count)
                throw ThrowHelper.GroupIndexOutOfBounds();
            if (data.Length < Unsafe.SizeOf<TItem>())
                throw ThrowHelper.GroupDataLength();
            var blob = CreateBlob(data.Length);
            var dest = blob.MemoryOwner.Memory.Span;
            data.CopyTo(dest);
            blobs.Insert(index, blob);
        }

        public void InsertEmpty(int index, int length)
        {
            if (index < 0 || index > Count)
                throw ThrowHelper.GroupIndexOutOfBounds();
            if (length < Unsafe.SizeOf<TItem>())
                throw ThrowHelper.GroupDataLength();
            var blob = CreateBlob(length);
            blobs.Insert(index, blob);
        }

        public void Add(in TItem item)
        {
            Insert(blobs.Count, in item);
        }

        public void Insert(int index, in TItem item)
        {
            Insert(index, IOHelper.GetBytesReadOnly(in item));
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
                throw ThrowHelper.GroupIndexOutOfBounds();
            FreeBlob(blobs[index]);
            blobs.RemoveAt(index);
        }

        public ref TItem GetItem(int index)
        {
            if (IsOutOfBounds(index))
                throw ThrowHelper.GroupIndexOutOfBounds();
            return ref Unsafe.As<byte, TItem>(ref blobs[index].MemoryOwner.Memory.Span[0]);
        }

        public ref TItem GetItem(int index, out Span<byte> data)
        {
            data = GetData(index);
            return ref GetItem(index);
        }

        public Span<byte> GetData(int index)
        {
            if (IsOutOfBounds(index))
                throw ThrowHelper.GroupIndexOutOfBounds();
            var blob = blobs[index];
            int size = Unsafe.SizeOf<TItem>();
            return blob.MemoryOwner.Memory.Span.Slice(size, blob.Length - size);
        }

        public ref TData GetData<TData>(int index)
        {
            if (IsOutOfBounds(index))
                throw ThrowHelper.GroupIndexOutOfBounds();
            var blob = blobs[index];
            if (blob.Length < Unsafe.SizeOf<TItem>() + Unsafe.SizeOf<TData>())
                throw ThrowHelper.GroupDataLength();
            return ref Unsafe.As<byte, TData>(ref blob.MemoryOwner.Memory.Span[Unsafe.SizeOf<TItem>()]);
        }

        public IStream OpenData(int index)
        {
            return BinaryStream.Create(GetData(index));
        }

        public void SetItem(int index, in TItem item)
        {
            bool preserve = !ReferenceEquals(blobs[index], Blob.OutOfBounds);
            if (preserve)
                GetItem(index) = item;
            else
            {
                RemoveAt(index);
                Insert(index, in item);
            }
        }

        public void SetItem(int index, in TItem item, ReadOnlySpan<byte> data)
        {
            RemoveAt(index);
            InsertEmpty(index, data.Length + Unsafe.SizeOf<TItem>());
            SetItem(index, in item);
            SetData(index, data);
        }

        public static Group<TItem> Factory(CacheMetadata info)
        {
            using var block = BinaryStream.Create(info.Resolver.File.GetChunk(info.Key).Section.Memory.Span).Decompress();
            return FromBlock(block);
        }

        public void SetData(int index, ReadOnlySpan<byte> data)
        {
            if (data.Length > 0)
            {
                if (data.Length != blobs[index].Length + Unsafe.SizeOf<TItem>())
                {
                    bool preserve = !ReferenceEquals(blobs[index], Blob.OutOfBounds);
                    if (preserve)
                    {
                        var item = GetItem(index);
                        RemoveAt(index);
                        InsertEmpty(index, data.Length + Unsafe.SizeOf<TItem>());
                        SetItem(index, in item);
                    }
                    else
                    {
                        RemoveAt(index);
                        InsertEmpty(index, data.Length + Unsafe.SizeOf<TItem>());
                    }
                }
                data.CopyTo(blobs[index].MemoryOwner.Memory.Span.Slice(Unsafe.SizeOf<TItem>()));
            }
        }

        public void SetData<TData>(int index, in TData data) => SetData(index, IOHelper.GetBytesReadOnly(in data));

        public void Sort(IComparer<TItem> comparer)
        {
            blobs.Sort(BlobComparer(comparer));
        }

        public void Clear()
        {
            foreach (var blob in blobs)
                FreeBlob(blob);
            blobs.Clear();
        }

        public bool IsOutOfBounds(int index)
        {
            return index < 0 || index >= Count || ReferenceEquals(blobs[index], Blob.OutOfBounds);
        }

        public void Sort() => Sort(Comparer<TItem>.Default);

        public int BinarySearch(in TItem key) => BinarySearch(0, Count, in key, Comparer<TItem>.Default);

        public int BinarySearch(in TItem key, IComparer<TItem> comparer) => BinarySearch(0, Count, in key, comparer);

        public int BinarySearch(int index, int count, in TItem key) => BinarySearch(index, count, in key);

        public int BinarySearch(int index, int count, in TItem key, IComparer<TItem> comparer)
        {
            int lo = index;
            int hi = index + count;
            int mid = (lo + hi) / 2;
            while (lo <= hi)
            {
                int c = comparer.Compare(GetItem(mid), key);
                if (c < 0)
                {
                    lo = mid + 1;
                }
                else if (c == 0)
                {
                    return mid;
                }
                else
                {
                    hi = mid - 1;
                }
                mid = (lo + hi) / 2;
            }
            return ~mid;
        }

        private Blob CreateBlob(int length)
        {
            var blob = freed.Count == 0 ? new Blob() : freed.Pop();
            blob.MemoryOwner = MemoryPool<byte>.Shared.Rent(length);
            blob.Length = length;
            return blob;
        }

        public void LoadBlock(IReadOnlyStream block)
        {
            Clear();
            LoadBlock(this, block);
        }

        public static Group<TItem> FromBlock(IReadOnlyStream block)
        {
            return LoadBlock(null, block);
        }

        private static Group<TItem> LoadBlock(Group<TItem> group, IReadOnlyStream block)
        {
            throw null;
            //if (!block.MagicNumber())
            //    throw ThrowHelper.IOBadHeaderMagicNumber(block.Peek<int>());
            //int count = block.Read<int>();
            //if (group == null)
            //    group = new Group<TItem>(count);
            //else
            //    group.blobs.Capacity = count;
            //int dataLength = block.Read<int>();
            //block.Skip(4);

            //if (!block.Assert(Unsafe.SizeOf<TItem>()))
            //    throw ThrowHelper.IOBadElementSize(block.Peek<int>(), Unsafe.SizeOf<TItem>());
            //var data = block.Read<byte>(dataLength);
            //var index = block.Read<int>(count * 2);

            //for (int i = 0; i < count; i++)
            //    group.Add(data.Slice(index[i * 2], index[i * 2 + 1]));
            //return group;
        }

        private void FreeBlob(Blob blob)
        {
            if (ReferenceEquals(blob, Blob.OutOfBounds))
                return;
            blob.MemoryOwner.Dispose();
            if (freed.Count < 128)
                freed.Push(blob);
        }

        public IEnumerator<KeyValuePair<TItem, Memory<byte>>> GetEnumerator()
        {
            foreach (var blob in blobs)
            {
                yield return blob.MakeKeyValuePair();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
