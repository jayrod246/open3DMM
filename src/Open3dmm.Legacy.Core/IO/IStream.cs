using System;

namespace Open3dmm.Core.IO
{
    public interface IStream : IReadOnlyStream
    {
        void SetLength(long value);
        bool TryWrite<T>(in T value) where T : unmanaged;
        bool TryWrite<T>(ReadOnlySpan<T> src) where T : unmanaged;

        public void Write<T>(in T value) where T : unmanaged
        {
            if (!TryWrite(in value))
                throw ThrowHelper.EndOfStreamReached();
        }

        public void Write<T>(ReadOnlySpan<T> src) where T : unmanaged
        {
            if (!TryWrite(src))
                throw ThrowHelper.EndOfStreamReached();
        }
    }
}
