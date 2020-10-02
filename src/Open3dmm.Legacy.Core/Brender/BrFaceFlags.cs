using System;

namespace Open3dmm.Core.Brender
{
    [Flags]
    public enum BrFaceFlags : byte
    {
        BrFaceF_COPLANAR_0 = 0x01,
        BrFaceF_COPLANAR_1 = 0x02,
        BrFaceF_COPLANAR_2 = 0x04
    }
}
