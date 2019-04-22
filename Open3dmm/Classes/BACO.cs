namespace Open3dmm.Classes
{
    public class BACO : BASE
    {
        public CRF CRF {
            get => GetReference<CRF>(0x08);
            set => SetReference(value, 0x08);
        }
        public ref QuadIDPair QuadIDPair => ref GetField<QuadIDPair>(0x0C);
    }
}
