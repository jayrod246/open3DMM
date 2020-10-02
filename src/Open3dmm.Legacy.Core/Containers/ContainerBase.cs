using System;
using System.Collections;
using System.Collections.Generic;

namespace Open3dmm.Core.Containers
{
    public abstract class ContainerBase<TKey, TItem> : IContainer<TKey, TItem>
    {
        private readonly List<TItem> items = new List<TItem>();

        protected IList<TItem> Items => items;

        protected virtual void ClearItems()
        {
            items.Clear();
        }

        protected abstract TKey GetKeyForItem(TItem item);

        protected virtual void ChangeItemKeyCore(TItem item, TKey newKey)
        {
            if (EqualityComparer<TKey>.Default.Equals(GetKeyForItem(item), newKey))
                return;
            int index = IndexOf(item);
            if (index < 0)
                throw new ArgumentException("The item could not be found in the collection. Ensure that the key returned by GetKeyForItem() is valid until after ChangeItemKey() returns.");
            int newIndex = BinarySearch(newKey);
            if (newIndex >= 0)
                throw GetKeyAlreadyExistsException(newKey);
            items.RemoveAt(index);
            items.Insert(index, item);
        }

        protected virtual void InsertItem(int index, TItem item)
        {
            items.Insert(index, item);
        }

        protected virtual void RemoveItem(int index)
        {
            items.RemoveAt(index);
        }

        public int BinarySearch(TKey key)
        {
            int lo = 0;
            int hi = Count - 1;
            var comparer = Comparer<TKey>.Default;
            while (lo <= hi)
            {
                int mid = lo + (hi - lo) / 2;
                int compare = comparer.Compare(key, GetKeyForItem(this[mid]));
                if (compare == 0)
                    return mid;
                else if (compare < 0)
                    hi = mid - 1;
                else
                    lo = mid + 1;
            }

            return ~lo;
        }

        public bool Contains(TKey key) => BinarySearch(key) >= 0;

        public bool TryGetItem(TKey key, out TItem value)
        {
            int index = BinarySearch(key);
            if (index >= 0)
            {
                value = this[index];
                return true;
            }
            value = default;
            return false;
        }

        public bool Remove(TKey key)
        {
            int index = BinarySearch(key);
            if (index < 0)
                return false;
            RemoveItem(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            RemoveItem(index);
        }

        public int IndexOf(TItem item)
        {
            int index = BinarySearch(GetKeyForItem(item));
            if (index < 0 || !EqualityComparer<TItem>.Default.Equals(this[index], item))
                return -1;
            return index;
        }

        public void Add(TItem item)
        {
            int index = BinarySearch(GetKeyForItem(item));
            if (index >= 0)
                throw GetKeyAlreadyExistsException(GetKeyForItem(item));
            InsertItem(~index, item);
        }

        protected virtual Exception GetKeyAlreadyExistsException(TKey key)
        {
            return new ArgumentException("An item with the same key already exists.");
        }

        protected virtual Exception GetKeyNotFoundException(TKey key)
        {
            return new KeyNotFoundException();
        }

        public void Clear()
        {
            ClearItems();
        }

        public bool Contains(TItem item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(TItem[] array, int arrayIndex)
        {
            items.CopyTo(array, arrayIndex);
        }

        public bool Remove(TItem item)
        {
            int index = IndexOf(item);
            if (index < 0)
                return false;
            RemoveItem(index);
            return true;
        }

        public int Count => items.Count;
        public bool IsReadOnly => false;

        public TItem this[int index] => items[index];

        public IEnumerator<TItem> GetEnumerator() => items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)items).GetEnumerator();

        public TItem this[TKey key] {
            get {
                int index = BinarySearch(key);
                if (index < 0)
                    throw GetKeyNotFoundException(key);
                return this[index];
            }
        }
    }
}
