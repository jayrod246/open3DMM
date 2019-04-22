namespace Open3dmm.BRender
{
    /// <summary>
    /// Vector type size 4
    /// </summary>
    public struct BrVector4
    {
        public BrScalar X;
        public BrScalar Y;
        public BrScalar Z;
        public BrScalar W;

        public BrVector4(BrScalar x, BrScalar y, BrScalar z, BrScalar w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }
    }
}
