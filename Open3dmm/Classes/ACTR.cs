using Open3dmm.BRender;

namespace Open3dmm.Classes
{
    public class ACTR : BASE
    {
        public ref BrMatrix34 OtherMatrix => ref GetField<BrMatrix34>(0xB0);

        public ref BrMatrix34 TransformMatrix => ref GetField<BrMatrix34>(0xF0);
    }
}
