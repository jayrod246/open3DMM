using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using System;

namespace Open3dmm.Core.Actors
{
    public class Tmap : ResolvableObject
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] PixelBuffer { get; set; }

        protected override void ResolveCore()
        {
            using var block = Metadata.GetBlock();
            if (!block.MagicNumber() || !block.TryRead(out short w) || !block.Assert<short>(0x0203) && !block.Assert<short>(0x0003) || !block.TrySkip(4) || !block.Assert(w) || !block.TryRead(out short h) || !block.TrySkip(4))
                throw ThrowHelper.BadSection(Metadata.Key);
            PixelBuffer = new byte[w * h];
            while (--h >= 0)
                block.ReadTo(PixelBuffer.AsSpan(h * w, w));

            Width = w;
            Height = PixelBuffer.Length / w;
        }
    }
}
