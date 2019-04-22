using System.Runtime.InteropServices;

namespace Open3dmm.BRender
{
    [StructLayout(LayoutKind.Explicit)]
    public struct BrTransform
    {
        [FieldOffset(0)]
        public ushort Type;
        [FieldOffset(4)]
        public BrMatrix34 Matrix;
    }
}