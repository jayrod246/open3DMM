namespace Open3dmm.Core.Brender
{
    public unsafe struct BrLight
    {
        public byte* Identifier;
        public BrLightType Type;
        public BrColor Color;
        public Fixed AttenuationConstant;
        public Fixed AttenuationLinear;
        public Fixed AttenuationQuadratic;
        public BrAngle ConeOuter;
        public BrAngle ConeInner;
    }
}
