using System;

namespace Open3dmm.Core.Brender
{
    [Flags]
    public enum BrMaterialUpdateFlags
    {
        BR_MATU_MAP_TRANSFORM = 0x0001,
        BR_MATU_RENDERING = 0x0002,
        BR_MATU_LIGHTING = 0x0004,
        BR_MATU_COLOURMAP = 0x0008,
        BR_MATU_ALL = 0x7fff
    }
}