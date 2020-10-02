using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using System;
using System.Drawing;

namespace Open3dmm.Core.Graphics
{
    public class Zbmp : ResolvableObject, IBindable
    {
        protected override void ResolveCore()
        {
            var b = Metadata.GetBlock();
            if (!b.MagicNumber())
                throw new InvalidOperationException("Bad magic number");
            b.Read<int>();
            SetRect(b.Read<ushort>(), b.Read<ushort>());

            PixelBuffer = new ushort[Rect.Width * Rect.Height];
            b.ReadTo(PixelBuffer.Span);
        }

        private bool isTextureDescriptionValid;
        private Binding binding;
        private TextureDescription textureDescription;

        public Rectangle Rect { get; private set; }
        public Memory<ushort> PixelBuffer { get; set; }
        public bool UsesTransparency { get; set; }
        public object Tag { get; set; }

        public void SetRect(ushort width, ushort height)
        {
            isTextureDescriptionValid = false;
            Rect = new Rectangle(0, 0, width, height);
        }

        private void EnsureTextureDescriptionValid()
        {
            if (isTextureDescriptionValid) return;
            binding.Free(this);
            textureDescription = new TextureDescription(Rect.Width, Rect.Height, 2, 1);
            isTextureDescriptionValid = true;
        }

        public int GetOrCreateBinding(IGraphicsContext graphicsContext)
        {
            EnsureTextureDescriptionValid();
            return graphicsContext.GetOrCreateTexture(ref binding, in textureDescription, ref PixelBuffer.Span[0]);
        }

        public void FreeBinding(IGraphicsContext graphicsContext)
        {
            graphicsContext.FreeTexture(ref binding);
        }
    }
}
