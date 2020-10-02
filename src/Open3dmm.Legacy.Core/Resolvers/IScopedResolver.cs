using Open3dmm;
using Open3dmm.Core.IO;
using System;

namespace Open3dmm.Core.Resolvers
{
    public interface IScopedResolver : IResolver
    {
        ChunkyFile File { get; }
        bool FreeItem(ChunkIdentifier identifier);
        bool RemoveItem(IResolvableObject item);
    }
}
