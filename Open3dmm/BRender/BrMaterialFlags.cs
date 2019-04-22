using System;

namespace Open3dmm.BRender
{
    [Flags]
    public enum BrMaterialFlags
    {
        BR_MATF_LIGHT = 0x00000001,
        BR_MATF_PRELIT = 0x00000002,

        BR_MATF_SMOOTH = 0x00000004,

        BR_MATF_ENVIRONMENT_I = 0x00000008,
        BR_MATF_ENVIRONMENT_L = 0x00000010,
        BR_MATF_PERSPECTIVE = 0x00000020,
        BR_MATF_DECAL = 0x00000040,

        BR_MATF_I_FROM_U = 0x00000080,
        BR_MATF_I_FROM_V = 0x00000100,
        BR_MATF_U_FROM_I = 0x00000200,
        BR_MATF_V_FROM_I = 0x00000400,

        BR_MATF_ALWAYS_VISIBLE = 0x00000800,
        BR_MATF_TWO_SIDED = 0x00001000,

        BR_MATF_FORCE_Z_0 = 0x00002000,

        BR_MATF_DITHER = 0x00004000
    }
}