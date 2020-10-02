using Open3dmm.Core.Graphics;
using System.Linq;
using Veldrid;
using VeldridTextureDescription = Veldrid.TextureDescription;
namespace Open3dmm
{
    public class MbmpTexture
    {
        private VeldridTextureDescription textureDescription;
        private Texture texture;

        public Mbmp Mbmp { get; }

        public MbmpTexture(Mbmp mbmp)
        {
            Mbmp = mbmp;
            textureDescription = VeldridTextureDescription.Texture2D((uint)Mbmp.Rect.Width, (uint)Mbmp.Rect.Height, 1, 1, PixelFormat.B8_G8_R8_A8_UNorm, TextureUsage.Sampled);
        }

        public Texture GetTexture(GraphicsDevice g)
        {
            if (texture == null)
            {
                if (textureDescription.Width == 0 || textureDescription.Height == 0)
                    return null;
                texture = g.ResourceFactory.CreateTexture(textureDescription);
                var buf = Mbmp.PixelBuffer.ToArray().Select(i => i == 0 ? default : (Application.Current as App).Palette[i].ToBgra() | 0xff << 24).ToArray();
                g.UpdateTexture(texture, buf, 0, 0, 0, texture.Width, texture.Height, texture.Depth, 0, 0);
            }

            return texture;
        }
    }
}
