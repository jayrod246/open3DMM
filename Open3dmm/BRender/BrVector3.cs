namespace Open3dmm.BRender {
    /// <summary>
    /// Vector type size 3
    /// </summary>
    public struct BrVector3
    {
        public BrScalar X;
        public BrScalar Y;
        public BrScalar Z;

        public BrVector3(BrScalar x, BrScalar y, BrScalar z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }
}
