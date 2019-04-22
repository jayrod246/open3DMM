using Open3dmm.BRender;

namespace Open3dmm.Classes
{
    public class APE : GOB
    {
        public ref BrActor Light => ref GetField<BrActor>(0x00EC);
        public ref BrMatrix34 Matrix => ref GetField<BrMatrix34>(0x0114);
    }
}
