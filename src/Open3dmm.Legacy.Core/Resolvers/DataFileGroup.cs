using Open3dmm;
using Open3dmm.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Open3dmm.Core.Resolvers
{
    public class DataFileGroup : IResolver
    {
        private readonly List<DataFile> children;

        public IReadOnlyList<DataFile> Files => children.AsReadOnly();

        public DataFileGroup()
        {
            children = new List<DataFile>();
        }

        public DataFileGroup(int capacity)
        {
            children = new List<DataFile>(capacity);
        }

        public bool TryResolve<T>(ChunkIdentifier identifier, out T item) where T : IResolvableObject
        {
            item = default;
            return ScopeOf(identifier)?.TryResolve(identifier, out item) == true;
        }

        public bool TryResolve<T>(ChunkIdentifier identifier, Tag refTag, int index, out T item) where T : IResolvableObject
        {
            item = default;
            return ScopeOf(identifier)?.TryResolve(identifier, refTag, index, out item) == true;
        }

        public void AddFile(DataFile resolver)
            => children.Add(resolver);

        public bool RemoveFile(DataFile resolver)
            => children.Remove(resolver);

        public IScopedResolver ScopeOf(ChunkIdentifier identifier)
        {
            foreach (var resolver in children)
            {
                var scope = resolver.ScopeOf(identifier);
                if (scope != null)
                    return scope;
            }
            return null;
        }

        public bool TryGetIdentifier(ChunkIdentifier identifier, Tag refTag, int index, out ChunkIdentifier result)
        {
            result = default;
            return ScopeOf(identifier)?.TryGetIdentifier(identifier, refTag, index, out result) == true;
        }
    }
}
