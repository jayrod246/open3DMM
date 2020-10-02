namespace Open3dmm.Core.Graphics
{
    public struct Binding
    {
        public Binding(IGraphicsContext graphicsContext, int id)
        {
            GraphicsContext = graphicsContext;
            Id = id;
        }

        public IGraphicsContext GraphicsContext { get; }
        public int Id { get; }

        public void Free(IBindable target)
        {
            if (GraphicsContext == null) return;
            target.FreeBinding(GraphicsContext);
            this = default;
        }
    }
}
