using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Open3dmm.Core.IO
{
    public class BinaryStream : IStream
    {
        private readonly bool leaveOpen;
        private bool isDisposed;

        public Stream BaseStream { get; }

        protected BinaryStream(Stream baseStream, bool leaveOpen)
        {
            this.BaseStream = baseStream;
            this.leaveOpen = leaveOpen;
        }

        public static BinaryStream Create()
            => new BinaryStream(IOHelper.CreateMemoryStream(), false);

        public static BinaryStream Create(Stream baseStream, bool leaveOpen = false)
            => new BinaryStream(baseStream, leaveOpen);

        public static unsafe BinaryStream Create<T>(Memory<T> memory, bool readOnly = false) where T : unmanaged
            => Create(memory.Span, readOnly);

        public static unsafe BinaryStream Create<T>(ReadOnlyMemory<T> memory) where T : unmanaged
            => Create(memory.Span);

        public static unsafe BinaryStream Create<T>(Span<T> span, bool readOnly = false) where T : unmanaged
            => CreateUnmanaged(span.Length == 0 ? (void*)0xDEADBEEF : Unsafe.AsPointer(ref span[0]), MemoryMarshal.AsBytes(span).Length, readOnly);

        public static unsafe BinaryStream Create<T>(ReadOnlySpan<T> span) where T : unmanaged
            => CreateUnmanaged(span.Length == 0 ? (void*)0xDEADBEEF : Unsafe.AsPointer(ref Unsafe.AsRef(in span[0])), MemoryMarshal.AsBytes(span).Length, true);

        private static unsafe BinaryStream CreateUnmanaged(void* ptr, int length, bool readOnly)
            => Create(new UnmanagedMemoryStream((byte*)ptr, length, length, readOnly ? FileAccess.Read : FileAccess.ReadWrite), false);

        public void SetLength(long value)
        {
            EnsureNotDisposed();
            BaseStream.SetLength(value);
        }

        public bool TryWrite<T>(in T value) where T : unmanaged
        {
            EnsureNotDisposed();
            return TryWriteBytes(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in value), 1)));
        }

        public bool TryWrite<T>(ReadOnlySpan<T> src) where T : unmanaged
        {
            EnsureNotDisposed();
            return TryWriteBytes(MemoryMarshal.AsBytes(src));
        }

        private bool TryWriteBytes(ReadOnlySpan<byte> src)
        {
            EnsureNotDisposed();
            try
            {
                BaseStream.Write(src);
            }
            catch (Exception e) when (e is NotSupportedException || e is IOException)
            {
                return false;
            }
            return true;
        }

        public long Position {
            get {
                EnsureNotDisposed();
                return BaseStream.Position;
            }
            set {
                EnsureNotDisposed();
                BaseStream.Position = value;
            }
        }
        public long Length {
            get {
                EnsureNotDisposed();
                return BaseStream.Length;
            }
        }

        public bool TryRead<T>(out T value) where T : unmanaged
        {
            EnsureNotDisposed();
            value = default;
            return TryReadBytes(IOHelper.GetBytes(ref value));
        }

        public bool TryRead<T>(Span<T> dest) where T : unmanaged
        {
            EnsureNotDisposed();
            return TryReadBytes(MemoryMarshal.AsBytes(dest));
        }

        private bool TryReadBytes(Span<byte> dest)
        {
            if (Length - Position < dest.Length)
                throw ThrowHelper.EndOfStreamReached();
            try
            {
                BaseStream.Read(dest);
            }
            catch (Exception e) when (e is NotSupportedException || e is IOException)
            {
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !leaveOpen)
                BaseStream.Dispose();
        }

        public void CopyTo(BinaryStream dest)
        {
            EnsureNotDisposed();
            BaseStream.CopyTo(dest.BaseStream);
        }

        private void EnsureNotDisposed()
        {
            if (isDisposed) throw ThrowHelper.ObjectDisposed(nameof(BinaryStream));
        }
    }
}
