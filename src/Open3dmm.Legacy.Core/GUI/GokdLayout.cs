namespace Open3dmm.Core.GUI
{
    public struct GokdLayout
    {
        public int ParentId;
        public int X;
        public int Y;
        public int Z;

        public GokdLayout(int parentId, int x, int y, int z)
        {
            this.ParentId = parentId;
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }
}
