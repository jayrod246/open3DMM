using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using System;
using System.Runtime.CompilerServices;

namespace Open3dmm
{
    public class MBMP : ResolvableObject
    {
        public bool IsMask { get; set; }
        public LTRB Bounds { get; set; }
        public int OffsetX => Bounds.Left;
        public int OffsetY => Bounds.Top;
        public int Width => Bounds.Width;
        public int Height => Bounds.Height;
        public short[] RowLengths { get; set; }
        public byte[] RowData { get; set; }

        public void ProcessRows(RowProcessor processor)
        {
            if (processor is null)
                throw new ArgumentNullException(nameof(processor));

            if (RowData is null || RowLengths is null)
                return;

            int row = 0;
            int offset = 0;

            foreach (var rowLength in RowLengths)
            {
                int end = offset + rowLength;

                if (end > RowData.Length)
                    end = RowData.Length;

                if (IsMask)
                    ProcessRowMask(row, offset, end, processor);
                else
                    ProcessRowBmp(row, offset, end, processor);

                if (end == RowData.Length)
                    break;

                offset = end;
                row++;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        private void ProcessRowBmp(int row, int offset, int end, RowProcessor processor)
        {
            int start = 0, n = 0;
            bool skip = true;
            while (offset < end)
            {
                start += n;
                n = RowData[offset++];

                if (!skip)
                {
                    processor(new(row, start, n, RowData.AsSpan(offset, n)));
                    offset += n;
                }

                skip = !skip;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
        private void ProcessRowMask(int row, int offset, int end, RowProcessor processor)
        {
            int start = 0, n = 0;

            while (offset < end)
            {
                start += RowData[offset++] + n;
                n = RowData[offset++];
                processor(new(row, start, n, ReadOnlySpan<byte>.Empty));
            }
        }

        public delegate void RowProcessor(Segment segment);
        public readonly ref struct Segment
        {
            public Segment(int row, int start, int length, ReadOnlySpan<byte> indices)
            {
                Row = row;
                Start = start;
                Length = length;
                Indices = indices;
            }

            /// <summary>
            /// The row of the <seealso cref="MBMP"/> which the segment belongs to.
            /// </summary>
            public int Row { get; }

            /// <summary>
            /// The start of the segment.
            /// </summary>
            public int Start { get; }

            /// <summary>
            /// The length of the segment.
            /// </summary>
            public int Length { get; }

            /// <summary>
            /// The indices of the pixels themselves. Use indices with a palette to get color.
            /// </summary>
            /// 
            /// <remarks>
            /// Unused if the <seealso cref="MBMP"/> is a mask.
            /// </remarks>
            public ReadOnlySpan<byte> Indices { get; }
        }

        class Resolvable : IResolvable<MBMP>
        {
            public bool Resolve(IScopedResolver resolver, ChunkIdentifier identifier, out MBMP value)
            {
                if (resolver.File.TryGetBlock(identifier, out var src) && (src = src.Decompress()).Remainder >= 28)
                {
                    src.TryRead(out int magic);
                    src.TryRead(out int isMask);
                    src.TryRead(out LTRB bounds);
                    src.TryRead(out int length);

                    if (length == src.Length && bounds.IsValid())
                    {
                        var rowLengths = new short[bounds.Height];
                        src.TryRead(rowLengths.AsSpan());
                        var rowData = GC.AllocateUninitializedArray<byte>((int)src.Remainder, true);
                        src.TryRead(rowData.AsSpan());
                        value = new()
                        {
                            IsMask = (byte)isMask != 0,
                            Bounds = bounds,
                            RowLengths = rowLengths,
                            RowData = rowData,
                        };
                        return true;
                    }
                }

                value = null;
                return false;
            }
        }
    }
}
