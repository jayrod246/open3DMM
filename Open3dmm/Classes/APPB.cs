namespace Open3dmm.Classes
{
    public class APPB : CMH
    {
        public int Flags00C {
            get => GetField<int>(0x00C);
            set => GetField<int>(0x00C) = value;
        }

        public ref int RenderMode => ref GetField<int>(0x05C);
        public GPT GPT {
            get => GetReference<GPT>(0x01C);
            set => SetReference(value, 0x01C);
        }

        public STIO STIO {
            get => GetReference<STIO>(0x08C);
            set => SetReference(value, 0x08C);
        }
        public KWA KWA {
            get => GetReference<KWA>(0x01BC);
            set => SetReference(value, 0x01BC);
        }

        public GL ListSometimes {
            get => GetReference<GL>(0x68);
            set => SetReference(value, 0x68);
        }
    }
}
