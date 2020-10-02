namespace Open3dmm.Core.Containers
{
    public interface IItemProvider<TKey, TItem>
    {
        bool TryGetItem(TKey key, out TItem item);
    }
}
