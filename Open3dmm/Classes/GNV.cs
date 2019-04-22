namespace Open3dmm.Classes
{
    public class GNV : BASE
    {
        public GPT GPT {
            get => GetReference<GPT>(0x008);
            set => SetReference(value, 0x008);
        }
        public ref RECTANGLE Field001C => ref GetField<RECTANGLE>(0x1C);
    }
}
