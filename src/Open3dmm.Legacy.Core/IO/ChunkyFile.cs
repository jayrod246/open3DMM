using Open3dmm;
using Open3dmm.Core.Data;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Open3dmm.Core.IO
{
    public struct ChunkyFileHeader
    {
        public int Identifier;
        public int Signature;
        public int Version;
        public int MagicNum;
        public int Length1;
        public int GroupOffset;
        public int GroupLength;
        public int Length2;
    }

    public static class IOUtil
    {
        public static Span<byte> AsBytes<T>(ref T value) where T : unmanaged
        {
            return MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref value, 1));
        }

        public static void Read<T>(Stream stream, out T value) where T : unmanaged
        {
            Unsafe.SkipInit(out value);
            stream.Read(AsBytes(ref value));
        }

        public static unsafe Stream ToStream(Span<byte> span)
        {
            return new UnmanagedMemoryStream((byte*)Unsafe.AsPointer(ref span[0]), span.Length);
        }
    }

    public class Chunk
    {
        internal bool Exists { get; set; }

        public ChunkFlags Flags { get; set; }

        internal Chunk(ChunkyFile file, ChunkIdentifier identifier)
        {
            File = file;
            Identifier = identifier;
        }

        private readonly List<ChunkChild> _children = new();
        public IEnumerable<ChunkChild> Children => _children;
        public ChunkyFile File { get; }
        public ChunkIdentifier Identifier { get; }
        public Section Section { get; set; } = new();
        public int TimesReferenced { get; internal set; }
        public string Label { get; set; }

        internal bool GetChild(ChunkChildIdentifier child, out ChunkIdentifier identifier)
        {
            ChunkChild c;

            int index = _children.BinarySearch(new(child.Tag, 0, child.Chid));

            if (index < 0)
                index = ~index;

            if (index < _children.Count)
            {
                c = _children[index < 0 ? ~index : index];
                identifier = c.Identifier;

                return (child.Chid, child.Tag) == (c.Chid, c.Identifier.Tag);
            }

            identifier = default;
            return false;
        }

        public int GetChildID(Chunk child)
        {
            if (child is null)
                throw new ArgumentNullException(nameof(child));
            var entry = _children.FirstOrDefault(c => c.Identifier == child.Identifier);

            if (entry.Identifier != child.Identifier)
                throw new ArgumentException("Chunk has no child that matches the argument given.");
            return entry.Chid;
        }

        internal void AddChild(ChunkChild newChild)
        {
            int index = _children.BinarySearch(newChild);
            if (index < 0)
                index = ~index;
            _children.Insert(index, newChild);
        }

        internal void Invalidate()
        {
        }
    }

    public class ChunkyFile
    {
        public const int MAGIC_FILE_IDENTIFIER = 0x324E4843;

        public const int MAGIC_NUMBER = 0x03030001;
        private readonly List<Chunk> _list = new(0);

        public Tag Signature { get; set; } = Tag.Parse("CHMP");
        public int Version { get; private set; }
        public int Capacity { get => _list.Capacity; set => _list.Capacity = value; }

        public bool TryGetBlock(ChunkIdentifier identifier, out IReadOnlyStream block)
        {
            if (TryGetChunk(identifier, out var chunk))
            {
                block = BinaryStream.Create(chunk.Section.Memory.Span);
                return true;
            }

            block = null;
            return false;
        }

        public Chunk GetChunk(ChunkIdentifier identifier)
        {
            if (!GetChunkInternal(identifier, out var chunk, true))
                throw new ArgumentException("File had no chunks that matched the argument given.", nameof(identifier));
            return chunk;
        }

        public Chunk GetChunk(ChunkIdentifier identifier, Tag childType, int childID)
        {
            Chunk parent = GetChunk(identifier);

            if (!GetChunk(identifier).GetChild((childID, childType), out var c))
                throw new ArgumentException("No children matched the arguments given.", nameof(identifier));

            return GetChunk(c);
        }

        public bool TryGetChunk(ChunkIdentifier identifier, ChunkChildIdentifier child, out Chunk chunk)
        {
            chunk = null;

            if (!GetChunkInternal(identifier, out var parent, true) || !parent.GetChild(child, out var c))
                return false;

            return GetChunkInternal(c, out chunk, true);
        }

        public bool TryGetChunk(ChunkIdentifier identifier, out Chunk chunk) => GetChunkInternal(identifier, out chunk, true);

        internal bool GetChunkInternal(ChunkIdentifier identifier, out Chunk chunk, bool mustExist)
        {
            int index = BinarySearch(identifier);
            chunk = index < 0 ? null : _list[index];

            if (chunk is null)
                return false;

            if (chunk.Exists || !mustExist)
                return true;

            chunk = null;
            return false;
        }

        internal Chunk GetOrCreateChunk(ChunkIdentifier identifier)
        {
            int index = BinarySearch(identifier);
            if (index >= 0)
                return _list[index];
            index = ~index;
            _list.Insert(index, new(this, identifier));
            return _list[index];
        }

        readonly struct Comparable : IComparable<Chunk>
        {
            private readonly ChunkIdentifier _identifier;

            public Comparable(ChunkIdentifier identifier)
            {
                _identifier = identifier;
            }
            public int CompareTo(Chunk other)
            {
                return _identifier.CompareTo(other.Identifier);
            }
        }

        internal int BinarySearch(ChunkIdentifier identifier)
        {
            return CollectionsMarshal.AsSpan(_list).BinarySearch(new Comparable(identifier));
        }

        public void AddParentChild(ChunkIdentifier identifier, ChunkChild newChild)
        {
            GetChunk(identifier).AddChild(newChild);
        }

        public ChunkyFile()
        {
        }

        public ChunkyFile(int capacity)
        {
            _list.Capacity = capacity;
        }

        public ChunkyFile(FileInfo info) : this()
        {
            LoadFile(info);
            Path = info.FullName;
        }

        public ChunkyFile(string path) : this(new FileInfo(path))
        {
        }

        private void LoadFile(FileInfo info)
        {
            if (!info.Exists)
                throw ThrowHelper.MissingFile(info.FullName);
            using IReadOnlyStream fileStream = BinaryStream.Create(info.OpenRead());
            var fileStart = 0;
            fileStream.Position = 0;
            if (info.Extension.ToLowerInvariant() == ".vmm")
                throw new NotImplementedException();
            var header = fileStream.Read<ChunkyFileHeader>();
            if (header.Identifier != MAGIC_FILE_IDENTIFIER)
                throw ThrowHelper.BadMagicNumber(header.Identifier);
            fileStream.Position = fileStart + header.GroupOffset;
            var group = GenericGroup.FromStream(fileStream);
            Capacity = group.Count;
            for (int i = 0; i < group.Count; i++)
            {
                var logicalChunk = MemoryMarshal.Read<LogicalChunk>(group.GetChunk(i));
                var chunk = Add(logicalChunk.Identifier);
                chunk.Flags = logicalChunk.Flags;
                chunk.TimesReferenced = logicalChunk.TimesReferenced;
                fileStream.Position = logicalChunk.SectionOffset;
                chunk.Section = new Section(fileStream.ReadTo<byte>(new byte[logicalChunk.SectionLength]))
                {
                    FileOffset = logicalChunk.SectionOffset
                };
                foreach (var r in MemoryMarshal.Cast<byte, LogicalReference>(group.GetPayload(i)).Slice(0, logicalChunk.NumReferences))
                {
                    GetOrCreateChunk(r.TargetIdentifier).TimesReferenced++;
                    chunk.AddChild(new(r.TargetIdentifier, r.Index));
                }
                if (group.GetPayload(i).Length > logicalChunk.NumReferences * 12)
                {
                    using IReadOnlyStream stream = BinaryStream.Create(group.GetPayload(i).Slice(logicalChunk.NumReferences * 12));
                    var labelEncoding = IOHelper.ReadEncoding(stream);
                    chunk.Label = IOHelper.ReadString(stream, labelEncoding);
                }
            }
        }

        public bool Remove(ChunkIdentifier identifier)
        {
            if (!GetChunkInternal(identifier, out var chunk, true))
                return false;
            chunk.Exists = false;
            return true;
        }

        public Chunk Add(ChunkIdentifier identifier)
        {
            var chunk = GetOrCreateChunk(identifier);
            if (chunk.Exists)
                throw new ArgumentException("File already contains chunk with an identifier matching the argument given.", nameof(identifier));
            chunk.Exists = true;
            return chunk;
        }

        public Chunk Add(Tag tag)
        {
            var id = new ChunkIdentifier(tag, 0);
            while (true)
            {
                var chunk = GetOrCreateChunk(id);
                if (!chunk.Exists)
                {
                    chunk.Exists = true;
                    return chunk;
                }
                id.Number++;
            }
        }

        public void Write(ChunkIdentifier identifier, ReadOnlySpan<byte> source)
        {
            GetChunk(identifier).Section = new(source);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public int Count => _list.Count;

        public string Path { get; }

        public IEnumerable<Chunk> GetChunksOfType(Tag type)
        {
            int index = BinarySearch(new(type, 0));
            if (index < 0)
                index = ~index;
            while (true)
            {
                if (index >= _list.Count || _list[index].Identifier.Tag != type)
                    break;
                yield return _list[index];
                index++;
            }
        }

        public IEnumerable<ChunkChild> GetChildrenOfType(ChunkIdentifier parent, Tag type)
        {
            if (TryGetChunk(parent, out var chunk))
                return chunk.Children.Where(c => c.Identifier.Tag == type);

            return Enumerable.Empty<ChunkChild>();
        }

        public ChunkIdentifier NextFree(Tag type)
        {
            var id = new ChunkIdentifier(type, 0);

            while (GetChunkInternal(id, out _, true))
                id.Number++;

            return id;
        }
    }
}
