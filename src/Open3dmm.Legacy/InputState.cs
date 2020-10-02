using System;

namespace Open3dmm
{
    [Flags]
    public enum InputState : byte
    {
        NoInput = 0,
        Control = 1,
        Shift = 2,
        Alt = 4,
        LeftButton = 8
    }
}
