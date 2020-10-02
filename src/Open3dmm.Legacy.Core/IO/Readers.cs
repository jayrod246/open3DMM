namespace Open3dmm.Core.IO
{
    public readonly ref struct GroupItem
    {
        public readonly int Index;
        public readonly IReadOnlyStream Head;
        public readonly IReadOnlyStream Body;

        public GroupItem(int index, IReadOnlyStream head, IReadOnlyStream body)
        {
            this.Index = index;
            this.Head = head;
            this.Body = body;
        }
    }

    public delegate void BlockReader(IReadOnlyStream block);
    public delegate void GroupItemReader(GroupItem item);
}
