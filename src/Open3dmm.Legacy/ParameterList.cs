using Open3dmm.Core;
using Open3dmm.Core.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Open3dmm
{
    public class ParameterList : ISortedList<ulong, int>
    {
        static ISortedList<ulong, string> knownHashes = Open3dmm.Core.SortedList.Create<ulong, string>();

        ISortedList<ulong, int> list = Open3dmm.Core.SortedList.Create<ulong, int>();

        public int Count => list.Count;

        public ParameterList Set(string name, int value, int index = 0)
        {
            Set(GetParameterId(name, index), value);
            return this;
        }

        public ulong GetParameterId(ReadOnlySpan<byte> name, int index = 0)
        {
            int src = 0;
            int dest = 0;
            int len = name.Length;
            int num = 8;
            Span<byte> buf = stackalloc byte[8];
            do
            {
                int chr;
                if (src < len)
                {
                    chr = name[src];
                    if (chr != '\0')
                    {
                        if (chr >= '0' && '9' >= chr)
                            chr -= '/';
                        else if (chr >= 'A' && 'Z' >= chr)
                            chr -= '6';
                        else if (chr >= 'a' && 'z' >= chr)
                            chr -= '<';
                        else
                            chr = '?';
                    }
                }
                else
                    chr = '\0';
                var nextValue = (uint)((byte)chr << ((num + 2) & 0x1f));
                var t = (0xFFFFFFFF << ((num & 0x1f)) & buf[dest]) | (nextValue >> 8);
                buf[dest] = (byte)t;
                if (num + 2 < 9)
                {
                    buf[++dest] = (byte)nextValue;
                    num += 2;
                }
                else
                    num -= 6;
                src++;
            } while (src < 8);

            // 0   1   2   3   4   5   6   7
            // 1   0   .   .   5   4  2|3  .
            // 1   0   7   6   5   4   3   2
            buf.Slice(0, 2).Reverse();
            buf.Slice(2).Reverse();
            if (index > 0)
                index <<= 16;
            ulong result = Unsafe.ReadUnaligned<ulong>(ref buf[0]) | (uint)index;
            return result;
        }

        public static ulong GetParameterId(string name, int index = 0)
        {
            int src = 0;
            int dest = 0;
            int len = name.Length;
            int num = 8;
            Span<byte> buf = stackalloc byte[8];
            do
            {
                char chr;
                if (src < len)
                {
                    chr = name[src];
                    if (chr >= '0' && '9' >= chr)
                        chr -= '/';
                    else if (chr >= 'A' && 'Z' >= chr)
                        chr -= '6';
                    else if (chr >= 'a' && 'z' >= chr)
                        chr -= '<';
                    else
                        chr = '?';
                }
                else
                    chr = '\0';
                var nextValue = (uint)((byte)chr << ((num + 2) & 0x1f));
                var t = (0xFFFFFFFF << ((num & 0x1f)) & buf[dest]) | (nextValue >> 8);
                buf[dest] = (byte)t;
                if (num + 2 < 9)
                {
                    buf[++dest] = (byte)nextValue;
                    num += 2;
                }
                else
                    num -= 6;
                src++;
            } while (src < 8);

            // 0   1   2   3   4   5   6   7
            // 1   0   .   .   5   4  2|3  .
            // 1   0   7   6   5   4   3   2
            buf.Slice(0, 2).Reverse();
            buf.Slice(2).Reverse();
            return Unsafe.ReadUnaligned<ulong>(ref buf[0]) | (uint)(index << 16);
        }

        public int BinarySearch(ulong key)
        {
            return list.BinarySearch(key);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        public bool ContainsKey(ulong key)
        {
            return list.ContainsKey(key);
        }

        public void Add(ulong key, int value)
        {
            list.Add(key, value);
        }

        public bool Remove(ulong key)
        {
            return list.Remove(key);
        }

        public int GetValueOrDefault(ulong key) => list.ContainsKey(key) ? list[key] : 0;

        public bool TryGetValue(ulong key, out int value) => list.TryGetValue(key, out value);

        public int this[ulong key] { get => Get(key); set => Set(key, value); }

        public ICollection<ulong> Keys => list.Keys;

        public ICollection<int> Values => list.Values;

        public void Add(KeyValuePair<ulong, int> item)
        {
            list.Add(item);
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(KeyValuePair<ulong, int> item)
        {
            return list.Contains(item);
        }

        public void CopyTo(KeyValuePair<ulong, int>[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<ulong, int> item)
        {
            return list.Remove(item);
        }

        public bool IsReadOnly => list.IsReadOnly;

        public KeyValuePair<ulong, int> this[int index] => list[index];

        public IEnumerator<KeyValuePair<ulong, int>> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public void SetAt(int index, int value)
        {
            list.SetAt(index, value);
        }

        static char[] ValidChars =
            Enumerable.Range('0', '9' - '0' + 1)
            .Concat(Enumerable.Range('A', 'Z' - 'A' + 1))
            .Concat(Enumerable.Range('a', 'z' - 'a' + 1))
            .Concat(Enumerable.Range('?', 1))
            .Select(i => (char)i)
            .ToArray();

        public void Set(ulong key, int value)
        {
            var name = VariableNameParser.Parse(key);
            list.Set(key, value);
        }

        public int Get(ulong key)
        {
            var value = list.Get(key);
            var name = VariableNameParser.Parse(key);
            return value;
        }

        public KeyValuePair<ulong, int> GetAt(int index)
        {
            return list.GetAt(index);
        }
    }
}
