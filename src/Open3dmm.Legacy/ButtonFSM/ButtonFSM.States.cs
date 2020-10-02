namespace Open3dmm
{
    partial class ButtonFSM
    {
        public static class States
        {
            /// <summary>Button is ready.</summary>
            public const uint Ready = 1;
            /// <summary>Button is not pressed. Cursor is not hovering.</summary>
            public const uint UpOff = 2;
            /// <summary>Button is not pressed. Cursor is entering.</summary>
            public const uint UpOffOn = 3;
            /// <summary>Button is not pressed. Cursor is exiting.</summary>
            public const uint UpOnOff = 4;
            /// <summary>Button is not pressed. Cursor is hovering.</summary>
            public const uint UpOn = 5;
            /// <summary>Button is being released. Cursor is not hovering.</summary>
            public const uint DownUpOff = 6;
            /// <summary>Button is being pressed. Cursor is hovering.</summary>
            public const uint UpDownOn = 7;
            /// <summary>Button is being released. Cursor is hovering.</summary>
            public const uint DownUpOn = 8;
            /// <summary>Button is pressed. Cursor is hovering.</summary>
            public const uint DownOn = 9;
            /// <summary>Button is pressed. Cursor is exiting.</summary>
            public const uint DownOnOff = 10;
            /// <summary>Button is pressed. Cursor is entering.</summary>
            public const uint DownOffOn = 11;
            /// <summary>Button is pressed. Cursor is not hovering.</summary>
            public const uint DownOff = 12;
            /// <summary>Mouse click state.</summary>
            public const uint Click = 13;
        }
    }
}
