namespace Open3dmm.Core.Scenes
{
    public struct LogicalScene
    {
        public LogicalScene(int start, int end, uint transition)
        {
            MagicNumber = 0x03030001;
            this.End = end;
            this.Start = start;
            this.Transition = transition;
        }

        public uint MagicNumber { get; set; }
        public int End { get; set; }
        public int Start { get; set; }
        public uint Transition { get; set; }
    }
}
