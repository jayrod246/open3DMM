using Open3dmm.Core;
using Open3dmm.Core.Actors;
using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using System;
using System.Numerics;
using Veldrid;

namespace Open3dmm.Core.Veldrid
{
    public class MtrlMaterial
    {
        ResourceSet textureSet;
        Texture texture;
        static ResourceSet shadedTextureSet;
        private Texture indexedTexture;
        private ColorSwap.ColorSwapToken? token;

        public Texture Texture => texture;
        public Texture IndexedTexture => indexedTexture;

        public Mtrl Mtrl { get; }

        public MtrlMaterial(Mtrl mtrl)
        {
            Mtrl = mtrl;
        }

        public void Load(GraphicsDevice graphicsDevice, CommandList commandList, Texture palette, CustomRenderer renderer)
        {
            var tmap = Mtrl.Tmap;
            if (tmap != null)
            {
                if (indexedTexture == null)
                {
                    indexedTexture = graphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)tmap.Width, (uint)tmap.Height, 1, 1, PixelFormat.R8_UNorm, TextureUsage.Sampled | TextureUsage.Storage));
                    graphicsDevice.UpdateTexture(indexedTexture, tmap.PixelBuffer, 0, 0, 0, indexedTexture.Width, indexedTexture.Height, indexedTexture.Depth, 0, 0);
                }
                texture ??= graphicsDevice.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)tmap.Width, (uint)tmap.Height, 1, 1, PixelFormat.B8_G8_R8_A8_UNorm, TextureUsage.Sampled | TextureUsage.Storage));
                renderer.ColorSwap.Swap(commandList, token ??= renderer.ColorSwap.CreateToken(palette, indexedTexture, texture));
                renderer.Use(commandList, false);
                //if (texture == null)
                //{
                //    buf ??= new int[tmap.PixelBuffer.Length];
                //    for (int i = 0; i < buf.Length; i++)
                //    {
                //        var c = pal[tmap.PixelBuffer[i]];
                //        c.A = 0xff;
                //        buf[i] = c.ToBgra();
                //    }
                //    graphicsDevice.UpdateTexture(texture, buf, 0, 0, 0, texture.Width, texture.Height, texture.Depth, 0, 0);
                //}
                textureSet ??= renderer.CreateTextureSet(texture, graphicsDevice.LinearSampler);
                commandList.SetGraphicsResourceSet(2, textureSet);
            }
            else
            {
                renderer.Use(commandList, true);
                shadedTextureSet ??= renderer.CreateTextureSet(palette, graphicsDevice.PointSampler);
                renderer.PaletteSlice.Set(commandList, Mtrl.PaletteSlice);
                commandList.SetGraphicsResourceSet(2, shadedTextureSet);
            }

            renderer.TextureTransform.Set(commandList, Mtrl.Transform.Matrix);
        }
    }
}
