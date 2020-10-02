using System;

namespace Open3dmm.Core.Resolvers
{
    public interface IResolvableObject : IDisposable
    {
        CacheMetadata Metadata { get; }
        bool IsDisposed { get; }
        void Initialize(CacheMetadata metadata);
        void Resolve();
    }
}
