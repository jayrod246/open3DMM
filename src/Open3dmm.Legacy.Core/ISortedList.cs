using System.Collections.Generic;

namespace Open3dmm.Core
{
    public interface ISortedList<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyList<KeyValuePair<TKey, TValue>>
    {
        new int Count { get; }
        int BinarySearch(TKey key);
        void RemoveAt(int index);
        void SetAt(int index, TValue value);
        void Set(TKey key, TValue value);
        TValue Get(TKey key);
        KeyValuePair<TKey, TValue> GetAt(int index);
    }

    public interface ISortedList<TValue> : ICollection<TValue>, IEnumerable<TValue>, IReadOnlyList<TValue>
    {
        new int Count { get; }
        int BinarySearch(TValue value);
        void RemoveAt(int index);
    }
}
