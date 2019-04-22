using System.Runtime.InteropServices;

namespace Open3dmm.BRender
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct BrLight
    {
        public BrMatrix34 TransformMatrix;

        public BrScalar Attenuation;

        public BrLightType LightType {
            get => lightType;
            set => lightType = value;
        }

        public bool IsViewSpace => (LightType & BrLightType.BR_LIGHT_VIEW) == BrLightType.BR_LIGHT_VIEW;

        private BrLightType lightType;
    }
}
