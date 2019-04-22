using System;
using System.Runtime.InteropServices;

namespace Open3dmm.Classes
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct GPT_UnkStruct1
    {
        [FieldOffset(0)]
        public IntPtr SomePointer;
        [FieldOffset(0x1C)]
        public RECTANGLE* Clip;
    }
}
