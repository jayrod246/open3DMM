namespace Open3dmm.Classes
{
    public class TBXB : GOB
    {
        public ref RECTANGLE Rectangle => ref GetField<RECTANGLE>(0x0084);
    }
}
