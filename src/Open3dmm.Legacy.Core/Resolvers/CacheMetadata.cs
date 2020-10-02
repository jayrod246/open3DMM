using Open3dmm;
using Open3dmm.Core.IO;

namespace Open3dmm.Core.Resolvers
{
    public interface ICache
    {
        void SetValue(CacheMetadata metadata, object value);
        void Evict(CacheMetadata metadata);
    }

    public record CacheMetadata
    {
        public IResolvable Resolvable { get; init; }
        public IScopedResolver Resolver { get; init; }
        public ChunkIdentifier Key { get; init; }

        public Chunk GetItem() => Resolver.File.GetChunk(Key);
        public IReadOnlyStream GetBlock() => BinaryStream.Create(GetItem().Section.Memory.Span).Decompress();

        public bool TryGetChild(ChunkChildIdentifier child, out Chunk chunk)
        {
            return Resolver.File.TryGetChunk(Key, child, out chunk);
        }
    }
}
