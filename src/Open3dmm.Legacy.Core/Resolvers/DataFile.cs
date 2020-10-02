using Open3dmm;
using Open3dmm.Core.Containers;
using Open3dmm.Core.Data;
using Open3dmm.Core.IO;
using System;
using System.Linq;

namespace Open3dmm.Core.Resolvers
{
    public class DataFile : IScopedResolver
    {
        private readonly IContainer<ChunkIdentifier, ResolvedItem> items;
        private readonly IFactory factory;

        public ChunkyFile File { get; }
        public int Key { get; }

        public DataFile(ChunkyFile file, IFactory factory, int key, int product = -1)
        {
            this.File = file;
            this.Key = key;
            items = new SimpleContainer<ChunkIdentifier, ResolvedItem>(x => x.Identifier);
            this.factory = factory;
        }

        struct ResolvedItem
        {
            public ResolvedItem(ChunkIdentifier identifier, IResolvableObject obj)
            {
                Identifier = identifier;
                Object = obj;
            }

            public ChunkIdentifier Identifier { get; set; }
            public IResolvableObject Object { get; set; }
        }

        private bool TryGetOrCreate<T>(ChunkIdentifier identifier, out T item, bool resolving) where T : IResolvableObject
        {
            item = default;

            if (items.TryGetItem(identifier, out var resolvedItem))
            {
                if (resolvedItem.Object.IsDisposed)
                {
                    items.Remove(identifier);
                    return false;
                }
                item = (T)resolvedItem.Object;
                return true;
            }
            if (File.TryGetChunk(identifier, out var chunk))
            {
                CacheMetadata metadata = new()
                {
                    Resolvable = Resolvable<T>.Default,
                    Key = identifier,
                    Resolver = this,
                };

                if (Resolvable<T>.Default?.Resolve(this, identifier, out item) is null)
                    item = factory.Create<T>(metadata);

                if (resolving && chunk.Section.Length != 0)
                    item.Resolve();

                items.Add(new ResolvedItem(identifier, item));
                return true;
            }

            return false;
        }

        public bool TryResolve<T>(ChunkIdentifier identifier, out T item) where T : IResolvableObject
            => TryGetOrCreate(identifier, out item, true);

        public bool TryResolve<T>(ChunkIdentifier identifier, Tag refTag, int index, out T item) where T : IResolvableObject
        {
            item = default;
            return TryGetIdentifier(identifier, refTag, index, out identifier) && TryResolve(identifier, out item);
        }

        public IScopedResolver ScopeOf(ChunkIdentifier identifier)
        {
            if (File.TryGetChunk(identifier, out _))
                return this;
            return null;
        }

        public bool TryGetIdentifier(ChunkIdentifier identifier, Tag refTag, int index, out ChunkIdentifier result)
        {
            if (File.TryGetChunk(identifier, (index, refTag), out var item))
            {
                result = item.Identifier;
                return true;
            }

            result = default;
            return false;
        }

        public bool FreeItem(ChunkIdentifier identifier)
        {
            if (!items.TryGetItem(identifier, out var resolvedItem))
                return false;
            items.Remove(identifier);
            if (resolvedItem.Object.IsDisposed)
                return false;
            resolvedItem.Object.Dispose();
            return true;
        }

        public bool RemoveItem(IResolvableObject item)
        {
            var identifier = item.Metadata.Key;
            return FreeItem(identifier) && File.Remove(identifier);
        }
    }
}
