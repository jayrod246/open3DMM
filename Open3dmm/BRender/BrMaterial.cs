namespace Open3dmm.BRender
{
    public unsafe struct BrMaterial
    {
        public byte* Identifier;
        public BrColor Color;
        public byte Opacity;
        public BrUFraction KA;
        public BrUFraction KD;
        public BrUFraction KS;
        public BrScalar Power;
        public BrMaterialFlags Flags;
        public BrMatrix23 MapTransform;
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