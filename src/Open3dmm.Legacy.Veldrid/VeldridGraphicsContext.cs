using Open3dmm.Core.Graphics;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Veldrid;
using VeldridTextureDescription = Veldrid.TextureDescription;
using CoreTextureDescription = Open3dmm.Core.Graphics.TextureDescription;

namespace Open3dmm.Core.Veldrid
{
    public class VeldridGraphicsContext : IGraphicsContext
    {
        const PixelFormat INDEXED = PixelFormat.R8_UNorm;
        const PixelFormat FULL_COLOR = PixelFormat.B8_G8_R8_A8_UNorm;

        private int textureCounter = int.MinValue;
        private int paletteCounter = int.MinValue;
        private readonly Dictionary<int, TextureBlob> textures;

        public GraphicsDevice GraphicsDevice { get; }
        public ColorSwap ColorSwap { get; }
        public Texture Palette { get; }
        public VeldridGraphicsContext(GraphicsDevice graphicsDevice)
        {
            textures = new Dictionary<int, TextureBlob>();
            GraphicsDevice = graphicsDevice;
            ColorSwap = new ColorSwap(graphicsDevice);
            Palette = graphicsDevice.ResourceFactory.CreateTexture(VeldridTextureDescription.Texture2D(256u, 1u, 1, 1, FULL_COLOR, TextureUsage.Sampled | TextureUsage.Storage));
        }

        public Texture GetTexture(int id) => textures[id].Texture;
        public Texture GetTexture(IBindable obj) => GetTexture(obj.GetOrCreateBinding(this));
        public Texture GetColorOutputTexture(IBindable obj) => textures[obj.GetOrCreateBinding(this)].ColorOutputTexture;

        public void NotifyPaletteChanged()
        {
            unchecked
            {
                paletteCounter++;
                if (paletteCounter == 0)
                    paletteCounter++;
            }
        }

        public void PaletteSwap(CommandList commandList, IBindable obj)
        {
            var binding = obj.GetOrCreateBinding(this);
            if (textures[binding].PaletteVersion != paletteCounter)
            {
                ColorSwap.Swap(commandList, textures[binding].ColorSwapToken);
                textures[binding].PaletteVersion = paletteCounter;
            }
        }

        public int GetOrCreateTexture<T>(ref Binding binding, in CoreTextureDescription textureDescription, ref T firstPixel)
        {
            EnsureNotBoundToOther(ref binding);
            if (binding.GraphicsContext == null)
                binding = CreateBinding(CreateTexture(in textureDescription, ref firstPixel));
            return binding.Id;
        }

        private Binding CreateBinding(int id)
        {
            return new Binding(this, id);
        }

        private int CreateTexture<T>(in CoreTextureDescription textureDescription, ref T firstPixel)
        {
            while (textures.ContainsKey(textureCounter) || textureCounter == 0)
                unchecked { textureCounter++; }

            var format = textureDescription switch
            {
                { ChannelCount: 1, Stride: 1 } => PixelFormat.R8_UNorm,
                { ChannelCount: 1, Stride: 2 } => PixelFormat.R16_UNorm,
                _ => throw new NotSupportedException("Stride and ChannelCount combination not supported."),
            };

            var texture = GraphicsDevice.ResourceFactory.CreateTexture(VeldridTextureDescription.Texture2D((uint)textureDescription.Width, (uint)textureDescription.Height, 1, 1, format, TextureUsage.Sampled | TextureUsage.Storage));
            var colorOutputTexture = format != INDEXED ? null : GraphicsDevice.ResourceFactory.CreateTexture(VeldridTextureDescription.Texture2D((uint)textureDescription.Width, (uint)textureDescription.Height, 1, 1, FULL_COLOR, TextureUsage.Sampled | TextureUsage.Storage));

            uint sizeInBytes = (uint)(textureDescription.Width * textureDescription.Height * textureDescription.Stride);
            unsafe
            {
                GraphicsDevice.UpdateTexture(texture, new IntPtr(Unsafe.AsPointer(ref firstPixel)), sizeInBytes, 0, 0, 0, texture.Width, texture.Height, 1, 0, 0);
            }
            textures[textureCounter] = new TextureBlob()
            {
                Texture = texture,
                ColorOutputTexture = colorOutputTexture,
                ColorSwapToken = colorOutputTexture == null ? default : ColorSwap.CreateToken(Palette, texture, colorOutputTexture)
            };

            return textureCounter;
        }

        public void FreeTexture(ref Binding binding)
        {
            EnsureNotBoundToOther(ref binding);
            if (binding.Id == 0) return;
            textures[binding.Id].Dispose();
            textures.Remove(binding.Id);
            binding = default;
        }

        private void EnsureNotBoundToOther(ref Binding binding)
        {
            if (binding.GraphicsContext != null && binding.GraphicsContext != this)
                throw new InvalidOperationException("Graphics object is already bound to a context.");
        }

        private class TextureBlob : IDisposable
        {
            public Texture Texture { get; set; }
            public Texture ColorOutputTexture { get; set; }
            public ColorSwap.ColorSwapToken ColorSwapToken { get; set; }
            public int PaletteVersion { get; set; }

            public void Dispose()
            {
                Texture.Dispose();
                if (ColorOutputTexture == null) return;
                ColorOutputTexture.Dispose();
                ColorSwapToken.ComputeResourceSet.Dispose();
            }
        }
    }
}
