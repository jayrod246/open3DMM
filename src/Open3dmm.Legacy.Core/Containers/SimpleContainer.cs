using System;

namespace Open3dmm.Core.Containers
{
    public class SimpleContainer<TKey, TItem> : ContainerBase<TKey, TItem>
    {
        readonly Func<TItem, TKey> keySelector;

        public SimpleContainer(Func<TItem, TKey> keySelector)
        {
            this.keySelector = keySelector;
        }

        protected override TKey GetKeyForItem(TItem item)
        {
            return keySelector(item);
        }
    }
}
