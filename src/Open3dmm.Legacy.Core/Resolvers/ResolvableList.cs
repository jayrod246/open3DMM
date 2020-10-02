using Open3dmm.Core.Data;

namespace Open3dmm.Core.Resolvers
{
    public abstract class ResolvableList : ResolvableObject
    {
        private LogicalList logicalList;

        public LogicalList GetLogicalList() => logicalList;

        protected override void ResolveCore()
        {
            logicalList?.Dispose();
            logicalList = LogicalList.FromStream(Metadata.GetBlock());
        }
    }
}
