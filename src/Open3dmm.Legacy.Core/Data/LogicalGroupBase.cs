using Open3dmm.Core.IO;
using System;

namespace Open3dmm.Core.Data
{
    public abstract class LogicalGroupBase : IDisposable
    {
        private bool isDisposed;

        public abstract int Count { get; }

        public abstract int ElementSize { get; set; }
        public abstract bool IsItemAt(int index);

        public abstract void AddBytes(Span<byte> newBytes);

        public abstract void InsertBytes(int index, Span<byte> newBytes);

        public abstract void RemoveAt(int index);

        public abstract Span<byte> GetBytes(int index);

        public abstract int GetSize(int index);

        public abstract void SetBytes(int index, Span<byte> newBytes);

        public abstract void Clear();

        public abstract void ToStream(IStream stream);

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
                isDisposed = true;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
