using Open3dmm.Core.IO.Compression;
using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Open3dmm.Core.IO
{
    public interface IContiguousBlock
    {
        int CalculateLogicalSize();
        void ToSequencer(IContiguousBlockSequencer sequencer);
    }

    public interface IContiguousBlockSequencer
    {
        void Next<T>(ref T reference) where T : unmanaged;
        void Next<T>(Span<T> span) where T : unmanaged;
        void Next<T>(ref T[] reference, int length) where T : unmanaged;
    }

    public static class IOExtensions
    {
        public static void Write<T>(this IStream stream, Span<T> span) where T : unmanaged
            => stream.Write((ReadOnlySpan<T>)span);

        public static void FromStream(this IContiguousBlock block, IReadOnlyStream source)
        {
            block.ToSequencer(new BlockSequenceReader(source));
        }

        public static void ToStream(this IContiguousBlock block, IStream dest)
        {
            block.ToSequencer(new BlockSequenceWriter(dest));
        }

        public static IReadOnlyStream Decompress(this IReadOnlyStream stream)
        {
            if (stream.Length < 9)
                return stream;
            const int KCD2 = 0x3244434B;
            const int KCDC = 0x4344434B;
            DecompressionAlgorithm decompressionAlgorithm;
            var originalPosition = stream.Position;

            switch (stream.Read<int>())
            {
                case KCDC:
                    decompressionAlgorithm = CompressionAlgorithms.KCDC;
                    break;
                case KCD2:
                    decompressionAlgorithm = CompressionAlgorithms.KCD2;
                    break;
                default:
                    // Already decompressed, rewind the stream back to original position.
                    stream.Position = originalPosition;
                    return stream;
            }

            // Original stream gets freed.
            using (stream)
            {
                int decompressedLength = stream.Read<int>();
                IOHelper.GetBytes(ref decompressedLength).Reverse(); // big endian
                var result = IOHelper.CreateMemoryStream();
                result.SetLength(decompressedLength);
                decompressionAlgorithm(stream, result.GetBuffer());
                return BinaryStream.Create(result);
            }
        }
    }

    internal class BlockSequenceWriter : IContiguousBlockSequencer
    {
        private readonly IStream stream;

        public BlockSequenceWriter(IStream stream)
        {
            this.stream = stream;
        }

        public void Next<T>(ref T reference) where T : unmanaged
        {
            stream.Write(in reference);
        }

        public void Next<T>(Span<T> span) where T : unmanaged
        {
            stream.Write(span);
        }

        public void Next<T>(ref T[] reference, int length) where T : unmanaged
        {
            stream.Write(reference.AsSpan(0, length));
        }
    }

    internal class BlockSequenceReader : IContiguousBlockSequencer
    {
        private readonly IReadOnlyStream stream;

        public BlockSequenceReader(IReadOnlyStream stream)
        {
            this.stream = stream;
        }

        public void Next<T>(ref T reference) where T : unmanaged
        {
            reference = stream.Read<T>();
        }

        public void Next<T>(Span<T> span) where T : unmanaged
        {
            stream.ReadTo(span);
        }

        public void Next<T>(ref T[] reference, int length) where T : unmanaged
        {
            if (reference.Length < length)
                reference = new T[length];
            stream.ReadTo(reference.AsSpan(0, length));
        }
    }
}
