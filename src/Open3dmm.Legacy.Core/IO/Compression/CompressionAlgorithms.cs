using System;
using System.Buffers;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Open3dmm.Core.IO.Compression
{
    public delegate void DecompressionAlgorithm(IReadOnlyStream input, Span<byte> output);

    internal readonly ref struct BitStream
    {
        public readonly IReadOnlyStream BaseStream;
        private readonly Span<byte> buffer;
        public readonly int Length;

        public BitStream(IReadOnlyStream baseStream, Span<byte> buffer)
        {
            BaseStream = baseStream;
            this.buffer = buffer;
            this.Length = buffer.Length * 8;
        }

        public int NextInt(ref int index, int n)
        {
            int num = 0;
            for (int i = 0; i < n; i++)
            {
                if (NextBit(ref index))
                    num |= 1 << i;
            }
            return num;
        }

        public bool NextBit(ref int index)
        {
            int ofs = index++ % Length;
            if (ofs == 0)
            {
                int n = Math.Min(this.buffer.Length, checked((int)(BaseStream.Length - BaseStream.Position)));
                BaseStream.ReadTo(this.buffer.Slice(0, n));
            }
            return (buffer[ofs / 8] & (1 << (ofs % 8))) != 0;
        }
    }

    internal static class CompressionAlgorithms
    {
        public static DecompressionAlgorithm KCDC = DecompressKcdc_fast_original;

        //static Dictionary<string, float> averageTimes = new Dictionary<string, float>();

        //private static void TestKcdcPerformance(IReadOnlyStream input, Span<byte> output)
        //{
        //    PeformTest(DecompressKcdc_fast_original, input, output);
        //    //PeformTest(DecompressKcdc_fast, input, output);
        //}

        //private static void Report(string name, long elapsedTime)
        //{
        //    if (!averageTimes.TryGetValue(name, out var avg))
        //        averageTimes[name] = avg = elapsedTime;
        //    else
        //        averageTimes[name] = avg = (avg + elapsedTime) / 2f;
        //    Console.WriteLine($"KCDC PERFORMANCE TEST: {name} average: {avg:.00}");
        //}

        //private static void PeformTest(DecompressionAlgorithm a, IReadOnlyStream input, Span<byte> output)
        //{
        //    var startPos = input.Position;
        //    var watch = System.Diagnostics.Stopwatch.StartNew();
        //    a.Invoke(input, output);
        //    watch.Stop();
        //    var elapsedMs = watch.ElapsedMilliseconds;
        //    input.Position = startPos;
        //    Report(a.GetMethodInfo().Name, elapsedMs);
        //}

        public static DecompressionAlgorithm KCD2 = DecompressKcd2;

        static void DecompressKcd2(IReadOnlyStream input, Span<byte> output)
        {
            input.Position++;
            int i = 0;
            int outIndex = 0;
            Span<byte> buffer = stackalloc byte[1024];
            var bits = new BitStream(input, buffer);

            unchecked
            {
                while (outIndex < output.Length)
                {
                    int n = 0;
                    while (bits.NextBit(ref i))
                        if (++n >= 40)
                            return;

                    uint copySize;
                    if (n > 0)
                        copySize = (uint)(((1 << n) - 1) + bits.NextInt(ref i, n));
                    else copySize = 0;

                    if (bits.NextBit(ref i))
                    {
                        uint lookBack;
                        if (bits.NextBit(ref i))
                        {
                            if (bits.NextBit(ref i))
                            {
                                if (bits.NextBit(ref i))
                                {
                                    // 0b__1111
                                    lookBack = (uint)(4673 + bits.NextInt(ref i, 20));
                                    copySize += 3;
                                }
                                else
                                {
                                    // 0b__0111
                                    lookBack = (uint)(577 + bits.NextInt(ref i, 12));
                                    copySize += 2;
                                }
                            }
                            else
                            {
                                // 0b__011
                                lookBack = (uint)(65 + bits.NextInt(ref i, 9));
                                copySize += 2;
                            }
                        }
                        else
                        {
                            // 0b__01
                            lookBack = (uint)(1 + bits.NextInt(ref i, 6));
                            copySize += 2;
                        }

                        for (n = 0; n < copySize; n++)
                        {
                            output[outIndex] = output[outIndex - (int)lookBack];
                            outIndex++;
                        }
                    }
                    else
                    {
                        // 0b__0

                        int b = i % 8;

                        if (b == 0)
                        {
                            for (n = 0; n < copySize + 1; n++)
                                output[outIndex++] = (byte)bits.NextInt(ref i, 8);
                        }
                        else
                        {
                            var half1 = b;
                            var half2 = 8 - half1;
                            var partialByte = bits.NextInt(ref i, half2);
                            for (n = 0; n < copySize; n++) output[outIndex++] = (byte)bits.NextInt(ref i, 8);
                            partialByte |= bits.NextInt(ref i, half1) << half2;
                            output[outIndex++] = (byte)partialByte;
                        }
                    }
                }
            }
        }

        static void DecompressKcdc(IReadOnlyStream input, Span<byte> output)
        {
            const int MAX_LOOKBACK = 1053248;
            var bits = new BitProvider(input);
            bits.AdvanceBytes(1);

            int lookBack,
                copySize,
                outIndex = 0;

            while (true)
            {
                if (!bits.NextBit())
                {
                    output[outIndex++] = bits.NextByte();
                    continue;
                }
                if (!bits.NextBit())
                {
                    lookBack = 1 + bits.NextInt(6);
                    copySize = 2;
                }
                else if (!bits.NextBit())
                {
                    lookBack = 65 + bits.NextInt(9);
                    copySize = 2;
                }
                else if (!bits.NextBit())
                {
                    lookBack = 577 + bits.NextInt(12);
                    copySize = 2;
                }
                else
                {
                    lookBack = 4673 + bits.NextInt(20);
                    if (lookBack == MAX_LOOKBACK)
                        break;
                    copySize = 3;
                }

                int buf = ~bits.PeekInt32();

                int n;
                if ((buf & 1) != 0)
                {
                    n = 0;
                }
                else
                {
                    n = 1;
                    if ((buf & 0xffff) == 0)
                    {
                        buf >>= 16;
                        n += 16;
                    }
                    if ((buf & 0xff) == 0)
                    {
                        buf >>= 8;
                        n += 8;
                    }
                    if ((buf & 0xf) == 0)
                    {
                        buf >>= 4;
                        n += 4;
                    }
                    if ((buf & 0x3) == 0)
                    {
                        buf >>= 2;
                        n += 2;
                    }
                    n -= buf & 0x1;
                }

                bits.AdvanceBits(n + 1);
                buf = bits.PeekInt32();

                int m = (1 << n) - 1;
                copySize += (buf & m) + m;
                while (--copySize >= 0)
                {
                    output[outIndex] = output[outIndex - lookBack];
                    outIndex++;
                }

                bits.AdvanceBits(n);
            }
        }

        static void DecompressKcdc_fast(IReadOnlyStream stream, Span<byte> output)
        {
            var input = new byte[stream.Remainder].AsSpan();
            stream.ReadTo(input);
            input = input.Slice(1);

            int lookBack,
                copySize,
                buf,
                bit = 0,
                index = 0,
                outIndex = 0;

            while (true)
            {
                buf = Unsafe.ReadUnaligned<int>(ref Unsafe.AsRef(in input[index])) >> bit;
                if ((buf & 1) == 0)
                {
                    output[outIndex++] = (byte)(buf >> 1);
                    if (bit == 7)
                    {
                        bit = 0;
                        index += 2;
                    }
                    else
                    {
                        bit++;
                        index++;
                    }
                    continue;
                }
                if ((buf & 0b10) == 0)
                {
                    lookBack = 1 + ((buf >> 2) & 63);
                    copySize = 2;
                    index++;
                }
                else if ((buf & 0b100) == 0)
                {
                    lookBack = 65 + ((buf >> 3) & 511);
                    copySize = 2;
                    bit += 12;
                }
                else if ((buf & 0b1000) == 0)
                {
                    lookBack = 577 + ((buf >> 4) & 4095);
                    copySize = 2;
                    bit += 16;
                }
                else
                {
                    lookBack = 4673 + ((buf >> 4) & 1048575);
                    copySize = 3;
                    bit += 24;
                }

                if (bit > 7)
                {
                    index += bit / 8;
                    bit &= 7;
                }
                buf = ~(Unsafe.ReadUnaligned<int>(ref Unsafe.AsRef(in input[index])) >> bit);
                if ((buf & 0xffffff) == 0)
                    break;
                int n;
                if ((buf & 1) != 0)
                {
                    n = 0;
                }
                else
                {
                    n = 1;
                    if ((buf & 0xffff) == 0)
                    {
                        buf >>= 16;
                        n += 16;
                    }
                    if ((buf & 0xff) == 0)
                    {
                        buf >>= 8;
                        n += 8;
                    }
                    if ((buf & 0xf) == 0)
                    {
                        buf >>= 4;
                        n += 4;
                    }
                    if ((buf & 0x3) == 0)
                    {
                        buf >>= 2;
                        n += 2;
                    }
                    n -= buf & 0x1;
                }

                bit += n + 1;
                if (bit > 7)
                {
                    index += bit / 8;
                    bit &= 7;
                }
                buf = Unsafe.ReadUnaligned<int>(ref Unsafe.AsRef(in input[index])) >> bit;

                int m = (1 << n) - 1;
                copySize += (buf & m) + m;
                while (--copySize >= 0)
                {
                    output[outIndex] = output[outIndex - lookBack];
                    outIndex++;
                }

                if (n > 0)
                {
                    bit += n;
                    if (bit > 7)
                    {
                        index += bit / 8;
                        bit &= 7;
                    }
                }
            }
        }

        static void DecompressKcdc_fast_original(IReadOnlyStream stream, Span<byte> output)
        {
            var input = new byte[stream.Remainder].AsSpan();
            stream.ReadTo(input);
            input = input.Slice(1);

            int lookBack,
                copySize,
                buf,
                bit = 0,
                index = 0,
                outIndex = 0;

            while (true)
            {
                buf = Unsafe.ReadUnaligned<int>(ref Unsafe.AsRef(in input[index])) >> bit;
                if ((buf & 1) == 0)
                {
                    output[outIndex++] = (byte)(buf >> 1);
                    if (bit == 7)
                    {
                        bit = 0;
                        index += 2;
                    }
                    else
                    {
                        bit++;
                        index++;
                    }
                    continue;
                }
                if ((buf & 0b10) == 0)
                {
                    lookBack = 1 + ((buf >> 2) & 63);
                    copySize = 2;
                    index++;
                }
                else if ((buf & 0b100) == 0)
                {
                    lookBack = 65 + ((buf >> 3) & 511);
                    copySize = 2;
                    bit += 12;
                }
                else if ((buf & 0b1000) == 0)
                {
                    lookBack = 577 + ((buf >> 4) & 4095);
                    copySize = 2;
                    bit += 16;
                }
                else
                {
                    lookBack = 4673 + ((buf >> 4) & 1048575);
                    copySize = 3;
                    bit += 24;
                }

                if (bit > 7)
                {
                    index += bit / 8;
                    bit &= 7;
                }
                buf = ~(Unsafe.ReadUnaligned<int>(ref Unsafe.AsRef(in input[index])) >> bit);
                if ((buf & 0xffffff) == 0)
                    break;
                int n;
                if ((buf & 1) != 0)
                {
                    n = 0;
                }
                else
                {
                    n = 1;
                    if ((buf & 0xffff) == 0)
                    {
                        buf >>= 16;
                        n += 16;
                    }
                    if ((buf & 0xff) == 0)
                    {
                        buf >>= 8;
                        n += 8;
                    }
                    if ((buf & 0xf) == 0)
                    {
                        buf >>= 4;
                        n += 4;
                    }
                    if ((buf & 0x3) == 0)
                    {
                        buf >>= 2;
                        n += 2;
                    }
                    n -= buf & 0x1;
                }

                bit += n + 1;
                if (bit > 7)
                {
                    index += bit / 8;
                    bit &= 7;
                }
                buf = Unsafe.ReadUnaligned<int>(ref Unsafe.AsRef(in input[index])) >> bit;

                int m = (1 << n) - 1;
                copySize += (buf & m) + m;
                while (--copySize >= 0)
                {
                    output[outIndex] = output[outIndex - lookBack];
                    outIndex++;
                }

                if (n > 0)
                {
                    bit += n;
                    if (bit > 7)
                    {
                        index += bit / 8;
                        bit &= 7;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int NextInt(this ReadOnlySpan<byte> input, ref int index, int n)
        {
            int num = 0;
            for (int i = 0; i < n; i++)
            {
                if (input.NextBit(ref index))
                    num |= 1 << i;
            }
            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool NextBit(this ReadOnlySpan<byte> input, ref int index)
        {
            return (input[index / 8] & (1 << (index++ % 8))) != 0;
        }

        static int ReadInt32(ref ReadOnlySpan<byte> input, ref int i, ref int b, int n)
        {
            if (n > 32) throw new InvalidOperationException();

            if (b == 0)
            {
                if (n > 24)
                {
                    i += 4;
                    b += n - 24;

                    if (b == 8)
                    {
                        i++;
                        b = 0;
                    }

                    return input[i - 4] | input[i - 3] << 8 | input[i - 2] << 16 | input[i - 1] << 24;
                }
                else if (n > 16)
                {
                    i += 3;
                    b += n - 16;

                    if (b == 8)
                    {
                        i++;
                        b = 0;
                    }
                    return input[i - 3] | input[i - 2] << 8 | input[i - 1] << 16;
                }
                else if (n > 8)
                {
                    i += 2;
                    b += n - 8;

                    if (b == 8)
                    {
                        i++;
                        b = 0;
                    }

                    return input[i - 2] | input[i - 1] << 8;
                }
                else
                {
                    if (n == 8)
                        i++;
                    else b = n;

                    return input[i++];
                }
            }
            else
            {
                if (n > 24)
                {
                    i += 4;
                    b -= n - 24;

                    if (b >= 8)
                    {
                        i++;
                        b = 0;
                    }

                    return (input[i - 4] | input[i - 3] << 8 | input[i - 2] << 16 | input[i - 1] << 24) >> b;
                }
                else if (n > 16)
                {
                    i += 3;
                    b += n - 16;

                    if (b == 8)
                    {
                        i++;
                        b = 0;
                    }
                    return input[i - 3] | input[i - 2] << 8 | input[i - 1] << 16;
                }
                else if (n > 8)
                {
                    i += 2;
                    b += n - 8;

                    if (b == 8)
                    {
                        i++;
                        b = 0;
                    }

                    return input[i - 2] | input[i - 1] << 8;
                }
                else
                {
                    if (n == 8)
                        i++;
                    else b = n;

                    return input[i++];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int CountOnes(ref ReadOnlySpan<byte> input, ref int i, ref int b)
        {
            int n = 0;

            unchecked
            {
            Recurs:
                int v = b == 0 ? input[i] : v = (input[i] | input[i + 1] << 8) >> b;
                switch (v)
                {
                    case 0x01:
                        n += 1;
                        b += 2;
                        break;
                    case 0x03:
                        n += 2;
                        b += 3;
                        break;
                    case 0x07:
                        n += 3;
                        b += 4;
                        break;
                    case 0x0F:
                        n += 4;
                        b += 5;
                        break;
                    case 0x1F:
                        n += 5;
                        b += 6;
                        break;
                    case 0x3F:
                        n += 6;
                        b += 7;
                        break;
                    case 0x7F:
                        n += 7;
                        i++;
                        break;
                    case 0xFF:
                        n += 8;
                        i++;
                        goto Recurs;
                }

                if (b >= 8)
                {
                    i++;
                    b -= 8;
                }

                return n;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void ReadLookBack(ref ReadOnlySpan<byte> input, ref int i, ref int b, ref uint lookBack, ref uint copySize)
        {
            unchecked
            {
                if (b == 0)
                {
                    switch (input[i] & 0x0F)
                    {
                        default:
                        case 0b__0000:
                            lookBack = 0;
                            b = 1;
                            //b++;
                            break;
                        case 0b__0001:
                            copySize += 2;
                            lookBack = (uint)(1 + (input[i] >> 2)); //& 0b__111111;
                            i++;
                            //b += 8;
                            break;
                        case 0b__0011:
                            copySize += 2;
                            lookBack = (uint)(65 + ((input[i] | input[i + 1] << 8) >> 3)); // & 0b__11111111_1;
                            i++;
                            b = 4;
                            //b += 12;
                            break;
                        case 0b__0111:
                            copySize += 2;
                            lookBack = (uint)(577 + ((input[i] | input[i + 1] << 8) >> 4)); // & 0b__11111111_1111;
                            i += 2;
                            //b += 16;
                            break;
                        case 0b__1111:
                            copySize += 3;
                            lookBack = (uint)(4673 + ((input[i] | input[i + 1] << 8 | input[i + 2] << 16) >> 4)); // & 0b__11111111_11111111_1111;
                            i += 3;
                            //b += 24;
                            break;
                    }
                }
                else
                {
                    int v;

                    if (b <= 4)
                        v = (input[i] >> b) & 0x0F;
                    else v = ((input[i] | input[i + 1] << 8) >> b) & 0x0F;

                    switch (v)
                    {
                        default:
                        case 0b__0000:
                            lookBack = 0;
                            if (++b == 8)
                            {
                                i++;
                                b = 0;
                            }
                            break;
                        case 0b__0001:
                            copySize += 2;
                            lookBack = (uint)(1 + ((input[i] | input[i + 1]) >> 2 + b) & 0b__111111);
                            i++;
                            //b += 8;
                            break;
                        case 0b__0011:
                            copySize += 2;
                            lookBack = (uint)(65 + ((input[i] | input[i + 1] << 8 | input[i + 2] << 16) >> 3 + b) & 0b__11111111_1);

                            if (b + 4 >= 8)
                            {
                                i += 2;
                                b -= 4;
                            }
                            else
                            {
                                i++;
                                b += 4;
                            }

                            //b += 12;
                            break;
                        case 0b__0111:
                            copySize += 2;
                            lookBack = (uint)(577 + ((input[i] | input[i + 1] << 8 | input[i + 1] << 16) >> 4 + b) & 0b__11111111_1111);
                            i += 2;
                            //b += 16;
                            break;
                        case 0b__1111:
                            copySize += 3;
                            lookBack = 4673 + (uint)(((input[i] | input[i + 1] << 8 | input[i + 2] << 16 | input[i + 3] << 24) >> 4 + b) & 0b__11111111_11111111_1111);
                            i += 3;
                            //b += 24;
                            break;
                    }
                }
            }
        }
    }

    internal class BitProvider
    {
        [StructLayout(LayoutKind.Sequential, Size = 5)]
        private readonly struct Int
        {
            private readonly int baseInt;
            private readonly byte ext;

            public Int(int baseInt, byte ext)
            {
                this.baseInt = baseInt;
                this.ext = ext;
            }

            public int ToInt32()
            {
                return baseInt;
            }

            public Int Shift(int offset)
            {
                if (offset > 8 || offset < 0)
                    throw new ArgumentException(nameof(offset));
                if (offset == 0)
                    return this;
                return new Int(baseInt >> offset | ext << (32 - offset), (byte)(ext >> offset));
            }
        }

        private int iBit;
        private int iByte;
        private Int current;
        private bool dirty;
        private readonly IReadOnlyStream stream;
        private readonly long startPos;

        public BitProvider(IReadOnlyStream stream)
        {
            this.stream = stream;
            startPos = stream.Position;
        }

        private void AdvanceOne()
        {
            if (++iBit <= 7)
                current = current.Shift(1);
            else
            {
                iByte++;
                iBit = 0;
                dirty = true;
            }
        }

        public void AdvanceBits(int nBits)
        {
            if (nBits < 0)
                throw new ArgumentException(nameof(nBits));
            if (nBits == 0) return;
            iBit += nBits;
            if (iBit <= 7)
                current = current.Shift(nBits);
            else
            {
                AdvanceBytes(iBit / 8);
                iBit %= 8;
                dirty = true;
            }
        }

        public void AdvanceBytes(int nBytes)
        {
            if (nBytes < 0)
                throw new ArgumentException(nameof(nBytes));
            if (nBytes == 0) return;
            iByte += nBytes;
            dirty = true;
        }

        public bool NextBit()
        {
            Flush();
            var value = (current.ToInt32() & 1) == 1;
            AdvanceOne();
            return value;
        }

        public byte NextByte()
        {
            Flush();
            AdvanceBytes(1);
            return (byte)current.ToInt32();
        }

        public int NextInt(int nBits)
        {
            Flush();
            var value = current.ToInt32() & (1 << nBits) - 1;
            AdvanceBits(nBits);
            return value;
        }

        public byte PeekByte()
        {
            Flush();
            return (byte)current.ToInt32();
        }

        public int PeekInt32()
        {
            Flush();
            return current.ToInt32();
        }

        private void Flush()
        {
            if (!dirty) return;
            stream.Position = startPos + iByte;
            stream.ReadTo(IOHelper.GetBytes(ref current));
            current = current.Shift((byte)iBit);
        }
    }
}
