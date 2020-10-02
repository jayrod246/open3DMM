using Open3dmm.Core.IO;

namespace Open3dmm.Core.Actors
{
    public struct LogicalActorInfo
    {
        public int Index { get; set; }
        public int UseCount { get; set; }
        public ActorFlags Flags { get; set; }
        public GlobalReference Reference { get; set; }

        public bool IsProp()
        {
            return Flags.HasFlag(ActorFlags.IsProp);
        }
    }
}
