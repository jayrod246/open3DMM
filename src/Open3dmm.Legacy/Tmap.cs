using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using System;

namespace Open3dmm
{
    public class Tmap
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] PixelBuffer { get; set; }

        public static Tmap Factory(IScopedResolver scope, CacheMetadata info)
        {
            using var b = BinaryStream.Create(scope.File.GetChunk(info.Key).Section.Memory.Span).Decompress();
            if (b.MagicNumber() && b.TryRead(out short w) && (b.Assert<short>(0x0203) || b.Assert<short>(0x0003)) && b.TrySkip(4) && b.Assert(w) && b.TryRead(out short h) && b.TrySkip(4))
            {
                var pixels = new byte[w * h];
                while (--h >= 0)
                    b.ReadTo(pixels.AsSpan(h * w, w));
                return new Tmap()
                {
                    Width = w,
                    Height = pixels.Length / w,
                    PixelBuffer = pixels,
                };
            }
            return null;
        }
    }
}
