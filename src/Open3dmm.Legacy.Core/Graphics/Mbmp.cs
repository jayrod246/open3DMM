using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using System;
using System.Drawing;

namespace Open3dmm.Core.Graphics
{
    public class Mbmp : ResolvableObject, IBindable
    {
        private Binding binding;
        private TextureDescription textureDescription;
        private bool isTextureDescriptionValid;

        protected override void ResolveCore()
        {
            var b = Metadata.GetBlock();
            if (!b.MagicNumber())
                throw new InvalidOperationException("Bad magic number");
            b.Read<int>();
            Rect = Rectangle.FromLTRB(b.Read<int>(), b.Read<int>(), b.Read<int>(), b.Read<int>());

            PixelBuffer = new byte[Rect.Width * Rect.Height];

            if (b.Read<int>() != b.Length)
                throw new InvalidOperationException("Mbmp lengths don't match.");

            if (Metadata.Key.Tag != Tags.MBMP)
                return;

            var lineLengths = b.ReadTo(stackalloc short[Rect.Height]);
            for (int line = 0; line < Rect.Height; line++)
            {
                int x = line * Rect.Width;
                bool skip = true;
                var end = b.Position + lineLengths[line];
                while (b.Position < end)
                {
                    int num = b.Read<byte>();
                    if (!skip)
                        b.ReadTo(PixelBuffer.Span.Slice(x, num));

                    x += num;

                    skip = !skip;
                }
                if (b.Position > end)
                    b.Position = end;
            }
        }

        public Rectangle Rect { get; private set; }
        public Memory<byte> PixelBuffer { get; private set; }
        public bool UsesTransparency { get; private set; }

        private void EnsureTextureDescriptionValid()
        {
            if (isTextureDescriptionValid) return;
            binding.Free(this);
            textureDescription = new TextureDescription(Rect.Width, Rect.Height, 1, 1);
            isTextureDescriptionValid = true;
        }

        public int GetOrCreateBinding(IGraphicsContext graphicsContext)
        {
            EnsureTextureDescriptionValid();
            if (!PixelBuffer.Span.IsEmpty)
                return graphicsContext.GetOrCreateTexture(ref binding, in textureDescription, ref PixelBuffer.Span[0]);
            byte tmp = 0;
            return graphicsContext.GetOrCreateTexture(ref binding, new(1, 1, 1, 1), ref tmp);
        }

        public void FreeBinding(IGraphicsContext graphicsContext)
        {
            graphicsContext.FreeTexture(ref binding);
        }
    }
}
