namespace Open3dmm.Classes
{
    public class TBXG : TXRG
    {
        public ref RECTANGLE Rectangle => ref GetField<RECTANGLE>(0x00D4);
    }
}
