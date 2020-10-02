using Open3dmm.Core.Resolvers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Open3dmm.Core.IO
{
    public class Gst<T> : IEnumerable<KeyValuePair<T, string>> where T : unmanaged
    {
        private readonly List<T> list;
        private readonly List<string> strings;

        public int Count => list.Count;

        public KeyValuePair<T, string> this[int index] {
            get => new KeyValuePair<T, string>(GetKey(index), GetString(index));
            set {
                list[index] = value.Key;
                strings[index] = value.Value;
            }
        }

        public Gst()
        {
            list = new List<T>();
            strings = new List<string>();
        }

        public Gst(int capacity)
        {
            list = new List<T>(capacity);
            strings = new List<string>(capacity);
        }

        public ref T GetKey(int index)
        {
            return ref list.GetInternalArray()[index];
        }

        public void SetKey(int index, in T newValue)
        {
            list[index] = newValue;
        }

        public bool TryGetString(in T key, out string str)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(key, list[i]))
                    continue;
                str = strings[i];
                return true;
            }
            str = default;
            return false;
        }

        public string GetString(int index)
        {
            return strings[index];
        }

        public void SetString(int index, string newStr)
        {
            strings[index] = newStr;
        }

        public void Add(in T key, string str)
        {
            list.Add(key);
            strings.Add(str);
        }

        public void Insert(int index, in T key, string str)
        {
            list.Insert(index, key);
            strings.Insert(index, str);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
            strings.RemoveAt(index);
        }

        public void LoadBlock(IReadOnlyStream block)
        {
            throw new NotImplementedException();
            //Clear();
            //block.MagicNumber();
            //int elementSize = block.Read<int>();
            //if (elementSize - 4 != Unsafe.SizeOf<T>())
            //    throw new InvalidOperationException("The size per element inside the block does not match the size per element of the Gst.");
            //int count = block.Read<int>();
            //strings.Capacity = list.Capacity = count;
            //int dataLength = block.Read<int>();
            //block.Assert(-1);
            //var data = block.SliceAtCursor(dataLength, skip: true);
            //while (--count >= 0)
            //{
            //    data.Cursor = block.Read<int>();
            //    Add(block.Read<T>(), data.LengthPrefixedString());
            //}
        }

        public void Clear()
        {
            list.Clear();
            strings.Clear();
        }

        public IEnumerator<KeyValuePair<T, string>> GetEnumerator()
        {
            var e1 = list.GetEnumerator();
            var e2 = strings.GetEnumerator();
            while (e1.MoveNext() && e2.MoveNext())
                yield return new KeyValuePair<T, string>(e1.Current, e2.Current);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
