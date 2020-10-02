namespace Open3dmm.Core.Graphics
{
    public interface IGraphicsContext
    {
        int GetOrCreateTexture<T>(ref Binding binding, in TextureDescription textureDescription, ref T firstPixel);
        void FreeTexture(ref Binding binding);
    }
}
