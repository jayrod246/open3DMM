namespace Open3dmm.Core.Resolvers
{
    public interface IFactory
    {
        public T Create<T>(CacheMetadata info);
    }
}
