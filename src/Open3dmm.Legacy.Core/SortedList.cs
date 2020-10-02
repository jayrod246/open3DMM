using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Open3dmm.Core
{
    public static class SortedList
    {
        public static ISortedList<TKey, TValue> Create<TKey, TValue>()
        {
            return new SortedListImpl<TKey, TValue>();
        }

        public static ISortedList<TKey, TValue> Create<TKey, TValue>(int capacity)
        {
            return new SortedListImpl<TKey, TValue>(capacity);
        }

        public static ISortedList<TValue> Create<TValue>()
        {
            return new SortedListImpl<TValue>();
        }

        public static ISortedList<TValue> Create<TValue>(int capacity)
        {
            return new SortedListImpl<TValue>(capacity);
        }

        private class SortedListImpl<TKey, TValue> : ISortedList<TKey, TValue>
        {
            private readonly List<TKey> keys;
            private readonly List<TValue> values;
            private static readonly IComparer<KeyValuePair<TKey, TValue>> keyComparer = CreateKeyComparer();

            public SortedListImpl() : this(0)
            {
            }

            public SortedListImpl(int capacity)
            {
                keys = new List<TKey>(capacity);
                values = new List<TValue>(capacity);
            }

            private static Comparer<KeyValuePair<TKey, TValue>> CreateKeyComparer()
            {
                return Comparer<KeyValuePair<TKey, TValue>>.Create((x, y) => Comparer<TKey>.Default.Compare(x.Key, y.Key));
            }

            public int BinarySearch(TKey key)
            {
                return keys.BinarySearch(key);
            }

            public bool ContainsKey(TKey key)
            {
                return BinarySearch(key) >= 0;
            }

            public void Add(TKey key, TValue value)
            {
                int index = BinarySearch(key);
                if (index >= 0)
                    throw new ArgumentException("Key already exists");
                keys.Insert(~index, key);
                values.Insert(~index, value);
            }

            public bool Remove(TKey key)
            {
                int index = BinarySearch(key);
                if (index < 0)
                    return false;
                keys.RemoveAt(index);
                values.RemoveAt(index);
                return true;
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                int index = keys.BinarySearch(key);
                if (index < 0)
                {
                    value = default;
                    return false;
                }
                value = values[index];
                return true;
            }

            public TValue this[TKey key] {
                get {
                    int index = keys.BinarySearch(key);
                    if (index < 0)
                        throw new KeyNotFoundException();
                    return values[index];
                }
                set {
                    int index = keys.BinarySearch(key);
                    if (index < 0)
                    {
                        keys.Insert(~index, key);
                        values.Insert(~index, value);
                    }
                    else
                        this.values[index] = value;
                }
            }

            public ICollection<TKey> Keys => keys.AsReadOnly();
            public ICollection<TValue> Values => values.AsReadOnly();

            public void Add(KeyValuePair<TKey, TValue> item)
            {
                Add(item.Key, item.Value);
            }

            public void Clear()
            {
                keys.Clear();
                values.Clear();
            }

            public bool Contains(KeyValuePair<TKey, TValue> item)
            {
                return IndexOf(item) >= 0;
            }

            private int IndexOf(KeyValuePair<TKey, TValue> item)
            {
                int index = keys.BinarySearch(item.Key);
                if (index >= 0 && !(EqualityComparer<TKey>.Default.Equals(keys[index], item.Key) && EqualityComparer<TValue>.Default.Equals(values[index], item.Value)))
                    return ~index;
                return index;
            }

            public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            {
                for (int i = 0; i < keys.Count; i++)
                    array[arrayIndex + i] = new KeyValuePair<TKey, TValue>(keys[i], values[i]);
            }

            public bool Remove(KeyValuePair<TKey, TValue> item)
            {
                int index = IndexOf(item);
                if (index < 0)
                    return false;
                keys.RemoveAt(index);
                values.RemoveAt(index);
                return true;
            }

            public int Count => keys.Count;
            public bool IsReadOnly => false;

            public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
            {
                return keys.Select((key, i) => new KeyValuePair<TKey, TValue>(key, values[i])).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public KeyValuePair<TKey, TValue> this[int index] => new KeyValuePair<TKey, TValue>(keys[index], values[index]);

            public void RemoveAt(int index)
            {
                keys.RemoveAt(index);
                values.RemoveAt(index);
            }

            public void SetAt(int index, TValue value)
            {
                values[index] = value;
            }

            public void Set(TKey key, TValue value)
            {
                this[key] = value;
            }

            public KeyValuePair<TKey, TValue> GetAt(int index)
            {
                return this[index];
            }

            public TValue Get(TKey key)
            {
                return this[key];
            }
        }


        private class SortedListImpl<TValue> : ISortedList<TValue>
        {
            private readonly SortedListImpl<TValue, TValue> wrappedList;

            public SortedListImpl()
            {
                wrappedList = new SortedListImpl<TValue, TValue>();
            }

            public SortedListImpl(int capacity)
            {
                wrappedList = new SortedListImpl<TValue, TValue>(capacity);
            }

            public int Count => wrappedList.Count;

            public int BinarySearch(TValue value)
            {
                return wrappedList.BinarySearch(value);
            }

            public void Add(TValue item)
            {
                wrappedList.Add(item, item);
            }

            public void Clear()
            {
                wrappedList.Clear();
            }

            public bool Contains(TValue item)
            {
                return wrappedList.ContainsKey(item);
            }

            public void CopyTo(TValue[] array, int arrayIndex)
            {
                foreach (var item in wrappedList)
                    array[arrayIndex++] = item.Key;
            }

            public bool Remove(TValue item)
            {
                return wrappedList.Remove(item);
            }

            public bool IsReadOnly => false;

            public TValue this[int index] => wrappedList[index].Key;

            public IEnumerator<TValue> GetEnumerator()
            {
                return wrappedList.Select(item => item.Key).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void RemoveAt(int index)
            {
                wrappedList.RemoveAt(index);
            }
        }
    }
}
