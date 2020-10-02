using Open3dmm.Core.IO;

namespace Open3dmm.Core.Actors
{
    public struct LogicalActor
    {
        public int MagicNumber;
        public FixedVector3 Position;
        public int Index;
        public int Start;
        public int End;
        public GlobalReference Reference;
    }
}
