namespace Open3dmm.Classes
{
    public class ACTN : BACO
    {
        public GG Cells {
            get => GetReference<GG>(0x0018);
            set => SetReference(value, 0x0018);
        }

        public GL Transforms {
            get => GetReference<GL>(0x001C);
            set => SetReference(value, 0x001C);
        }
    }
}
