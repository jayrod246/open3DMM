using Microsoft.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Open3dmm.Core.IO
{
    public static class IOHelper
    {
        private static readonly RecyclableMemoryStreamManager RecyclableMemoryStreamManager = new RecyclableMemoryStreamManager();

        /// <summary>
        /// Determines if a string can be represented with single-byte encoding.
        /// </summary>
        /// <param name="str">The string to check.</param>
        /// <returns>
        /// If the string is single-byte or not.
        /// </returns>
        public static bool IsSingleByte(string str)
            => Encoding.UTF8.GetByteCount(str) == str.Length;

        public static Encoding GetSuitableEncoding(string str)
            => IsSingleByte(str) ? Encoding.ASCII : Encoding.Unicode;

        public static Encoding ReadEncoding(IReadOnlyStream stream)
        {
            var encodingTypeCode = stream.Read<ushort>();
            if (encodingTypeCode == 0x0303)
                return Encoding.ASCII;
            if (encodingTypeCode == 0x0505)
                return Encoding.Unicode;
            throw ThrowHelper.BadEncodingTypeCode(encodingTypeCode);
        }

        public static void WriteEncoding(IStream stream, Encoding encoding)
        {
            if (encoding == Encoding.ASCII)
                stream.Write<ushort>(0x0303);
            else if (encoding == Encoding.Unicode)
                stream.Write<ushort>(0x0505);
            else
                throw new ArgumentException("Encoding not supported.");
        }

        public static string ReadString(IReadOnlyStream stream, Encoding encoding)
        {
            int length;
            if (encoding.IsSingleByte)
                length = stream.Read<byte>();
            else
                length = stream.Read<ushort>() * 2;
            Span<byte> span = stackalloc byte[length];
            stream.ReadTo(span);
            return encoding.GetString(span);
        }

        public static void WriteString(IStream stream, Encoding encoding, string str)
        {
            int length = str.Length;
            if (encoding.IsSingleByte)
                stream.Write((byte)length);
            else
            {
                stream.Write((ushort)length);
                length *= 2;
            }
            Span<byte> span = stackalloc byte[length];
            encoding.GetBytes(str.AsSpan(), span);
            stream.Write<byte>(span);
        }

        public static int PadToAlignment(int size, int align) => size + CalculatePadding(size, align);

        public static int CalculatePadding(int offset, int align) => (align - (offset % align)) % align;

        public static string String(this IReadOnlyStream stream, int length)
        {
            unsafe
            {
                sbyte* str = stackalloc sbyte[length];
                stream.ReadTo(new Span<sbyte>(str, length));
                return new string(str, 0, length);
            }
        }

        public static string LengthPrefixedString(this IReadOnlyStream stream)
        {
            int length = stream.Read<byte>();
            return String(stream, length);
        }

        /// <summary>
        /// Factory method for getting a new MemoryStream.
        /// </summary>
        /// <returns></returns>
        public static MemoryStream CreateMemoryStream() => RecyclableMemoryStreamManager.GetStream();

        public static bool MagicNumber(this IReadOnlyStream stream)
        {
            return stream.Assert(0x03030001) || stream.Assert(0x05050001);
        }

        public static void GL<T>(this IReadOnlyStream stream, ICollection<T> output) where T : unmanaged
        {
            int elementSize = Unsafe.SizeOf<T>();
            if (!stream.MagicNumber())
                throw ThrowHelper.IOBadHeaderMagicNumber(stream.Peek<int>());
            if (!stream.Assert(elementSize))
                throw ThrowHelper.IOBadElementSize(stream.Peek<int>(), elementSize);
            int count = stream.Read<int>();
            while (--count >= 0)
                output.Add(stream.Read<T>());
        }

        public static T[] GL<T>(this IReadOnlyStream stream) where T : unmanaged
        {
            int elementSize = Unsafe.SizeOf<T>();
            if (!(stream.MagicNumber() && stream.Assert(elementSize)))
                throw new InvalidOperationException("Incorrect header for generic list");
            int count = stream.Read<int>();
            var arr = new T[count];
            stream.ReadTo<T>(arr);
            return arr;
        }

        public static void GL<TFrom, TTo>(this IReadOnlyStream stream, ICollection<TTo> output, Func<TFrom, TTo> transform)
            where TFrom : unmanaged
            where TTo : unmanaged
        {
            GL(stream, DataCollector<TFrom, TTo>.Rent(output, transform));
        }

        public static Span<byte> GetBytes<T>(ref T value)
        {
            unsafe
            {
                return new Span<byte>(Unsafe.AsPointer(ref value), Unsafe.SizeOf<T>());
            }
        }

        public static ReadOnlySpan<byte> GetBytesReadOnly<T>(in T value)
        {
            return GetBytes(ref Unsafe.AsRef(in value));
        }

        private class DataCollector<TFrom, TTo> : ICollection<TFrom>
        {
            [ThreadStatic]
            static readonly DataCollector<TFrom, TTo> instance = new DataCollector<TFrom, TTo>();

            ICollection<TTo> dest;
            Func<TFrom, TTo> transform;

            private DataCollector()
            {
            }

            public static ICollection<TFrom> Rent(ICollection<TTo> dest, Func<TFrom, TTo> transform)
            {
                instance.dest = dest;
                instance.transform = transform;
                return instance;
            }

            public void Add(TFrom item)
            {
                dest.Add(transform(item));
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(TFrom item)
            {
                throw new NotImplementedException();
            }

            public void CopyTo(TFrom[] array, int arrayIndex)
            {
                throw new NotImplementedException();
            }

            public bool Remove(TFrom item)
            {
                throw new NotImplementedException();
            }

            public int Count => throw new NotImplementedException();
            public bool IsReadOnly => throw new NotImplementedException();

            public IEnumerator<TFrom> GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }

        public static unsafe IntPtr SpanToPointer<T>(Span<T> span) where T : unmanaged
        {
            return new IntPtr(Unsafe.AsPointer(ref span[0]));
        }

        public static unsafe IntPtr SpanToPointer<T>(ReadOnlySpan<T> span) where T : unmanaged
        {
            return new IntPtr(Unsafe.AsPointer(ref Unsafe.AsRef(in span[0])));
        }
    }

    public struct StringGrouping
    {
        public string Value;
        public Memory<byte> GroupData;
        public IStream Open() => BinaryStream.Create(GroupData.Span);
    }
}
