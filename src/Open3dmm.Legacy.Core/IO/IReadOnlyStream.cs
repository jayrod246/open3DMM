using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Open3dmm.Core.IO
{
    public interface IReadOnlyStream : IDisposable
    {
        Stream BaseStream { get; }
        long Position { get; set; }
        long Length { get; }
        bool TryRead<T>(out T value) where T : unmanaged;
        bool TryRead<T>(Span<T> dest) where T : unmanaged;
        void CopyTo(BinaryStream dest);

        public long Remainder => Length - Position;

        public bool Assert<T>(T value) where T : unmanaged
        {
            if (EqualityComparer<T>.Default.Equals(Read<T>(), value))
                return true;
            Position -= Unsafe.SizeOf<T>();
            return false;
        }

        public T Peek<T>() where T : unmanaged
        {
            if (!TryRead<T>(out var value))
                throw ThrowHelper.EndOfStreamReached();
            Position -= Unsafe.SizeOf<T>();
            return value;
        }

        public bool TrySkip(int skip)
        {
            if (Position + skip > Length)
                return false;
            Position += skip;
            return true;
        }

        public void Skip(int skip)
        {
            Position += skip;
        }

        public T Read<T>() where T : unmanaged
        {
            return TryRead<T>(out var value) ? value : throw ThrowHelper.EndOfStreamReached();
        }

        public Span<T> ReadTo<T>(Span<T> dest) where T : unmanaged
        {
            if (!TryRead(dest))
                throw ThrowHelper.EndOfStreamReached();
            return dest;
        }
    }
}
