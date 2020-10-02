namespace Open3dmm.Core.Graphics
{
    public interface IBindable
    {
        int GetOrCreateBinding(IGraphicsContext graphicsContext);
        void FreeBinding(IGraphicsContext graphicsContext);
    }
}
