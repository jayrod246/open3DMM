using Open3dmm.BRender;

namespace Open3dmm.Classes
{
    public class BKGD : BACO
    {
        public ref BrMatrix34 WorldMatrix => ref GetField<BrMatrix34>(0x34);
    }
}
