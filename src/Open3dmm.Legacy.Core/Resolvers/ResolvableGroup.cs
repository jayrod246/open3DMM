using Open3dmm.Core.Data;

namespace Open3dmm.Core.Resolvers
{
    public class ResolvableGroup : ResolvableObject
    {
        private LogicalGroup logicalGroup;

        public LogicalGroup GetLogicalGroup() => logicalGroup;

        protected override void ResolveCore()
        {
            logicalGroup?.Dispose();
            logicalGroup = LogicalGroup.FromStream(Metadata.GetBlock());
        }
    }
}
