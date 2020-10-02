using System.Collections.Generic;

namespace Open3dmm.Core.Containers
{
    public interface IContainer<TKey, TItem> : IItemProvider<TKey, TItem>, ICollection<TItem>, IReadOnlyList<TItem>
    {
        TItem this[TKey key] { get; }
        bool Contains(TKey key);
        int BinarySearch(TKey key);
        int IndexOf(TItem item);
        bool Remove(TKey key);
        void RemoveAt(int index);
    }
}
