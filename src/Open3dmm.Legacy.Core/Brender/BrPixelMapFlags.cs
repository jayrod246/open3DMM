using System;

namespace Open3dmm.Core.Brender
{
    [Flags]
    public enum BrPixelMapFlags : byte
    {
        /*
		 * No direct access to pixels
		 */
        BR_PMF_NO_ACCESS = 0x01,

        BR_PMF_LINEAR = 0x02,
        BR_PMF_ROW_WHOLEPIXELS = 0x04
    };
}