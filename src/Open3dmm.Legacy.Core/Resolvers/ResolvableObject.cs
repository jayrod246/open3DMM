namespace Open3dmm.Core.Resolvers
{
    public abstract class ResolvableObject : IResolvableObject
    {
        private CacheMetadata info;
        private bool disposedValue;

        public bool IsDisposed => disposedValue;

        public CacheMetadata Metadata => this.info;

        public void Initialize(CacheMetadata info)
        {
            this.info = info;
        }

        public void Resolve()
        {
            ResolveCore();
        }

        protected virtual void ResolveCore() { }

        protected T ResolveReference<T>(ReferenceIdentifier identifier) where T : IResolvableObject
        {
            if (!TryResolveReference<T>(identifier, out var item))
                throw ThrowHelper.MissingReference(info.Key, identifier);
            return item;
        }

        protected T ResolveReferenceOrDefault<T>(ReferenceIdentifier identifier, T @default = default) where T : IResolvableObject
        {
            if (TryResolveReference<T>(identifier, out var item))
                return item;
            return @default;
        }

        protected bool TryResolveReference<T>(ReferenceIdentifier identifier, out T item) where T : IResolvableObject
        {
            return info.Resolver.TryResolve(info.Key, identifier.Tag, identifier.Index, out item);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue) return;
            disposedValue = true;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }
    }
}
