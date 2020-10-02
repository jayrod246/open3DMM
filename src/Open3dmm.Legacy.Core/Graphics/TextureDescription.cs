namespace Open3dmm.Core.Graphics
{
    public struct TextureDescription
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Stride { get; set; }
        public int ChannelCount { get; set; }

        public TextureDescription(int width, int height, int stride, int channelCount)
        {
            Width = width;
            Height = height;
            Stride = stride;
            ChannelCount = channelCount;
        }
    }
}
