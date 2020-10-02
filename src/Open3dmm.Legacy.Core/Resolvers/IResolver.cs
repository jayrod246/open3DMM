using Open3dmm;
using Open3dmm.Core.IO;
using System;
using System.Collections.Generic;

namespace Open3dmm.Core.Resolvers
{
    public interface IResolver
    {
        bool TryResolve<T>(ChunkIdentifier identifier, Tag refTag, int index, out T item) where T : IResolvableObject;
        bool TryResolve<T>(ChunkIdentifier identifier, out T item) where T : IResolvableObject;
        bool TryGetIdentifier(ChunkIdentifier identifier, Tag refTag, int index, out ChunkIdentifier result);
        IScopedResolver ScopeOf(ChunkIdentifier identifier);
    }

    public interface IResolvable { }

    public interface IResolvable<TValue> : IResolvable
    {
        bool Resolve(IScopedResolver resolver, ChunkIdentifier identifier, out TValue value);
    }
}
