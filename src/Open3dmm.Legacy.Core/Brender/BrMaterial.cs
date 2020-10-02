namespace Open3dmm.Core.Brender
{
    public unsafe struct BrMaterial
    {
        public byte* Identifier;
        public BrColor Color;
        public byte Opacity;
        private byte padding0;
        public BrUFraction KA;
        public BrUFraction KD;
        public BrUFraction KS;
        public Fixed Power;
        public BrMaterialFlags Flags;
        public FixedMatrix3x2 MapTransform;
        public byte IndexBase;
        public byte IndexRange;
        public BrPixelMap* ColorMap;
        public BrPixelMap* Screendoor;
        public BrPixelMap* IndexShade;
        public BrPixelMap* IndexBlend;
        public byte PrepFlags;
        public void* RPointer;
    }
}