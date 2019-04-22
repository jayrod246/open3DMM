namespace Open3dmm.BRender
{
    /// <summary>
    /// Vector type size 2
    /// </summary>
    public struct BrVector2
    {
        public BrScalar X;
        public BrScalar Y;

        public BrVector2(BrScalar x, BrScalar y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
