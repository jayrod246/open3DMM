namespace Open3dmm.Core.Brender {
    public enum BrEulerOrder : byte {
        /// <summary>
        /// (BR_EULER_FIRST_X | BR_EULER_PARITY_EVEN | BR_EULER_REPEAT_NO | BR_EULER_FRAME_STATIC)
        /// </summary>
        XYZ_S = 0,
        /// <summary>
        /// (BR_EULER_FIRST_X | BR_EULER_PARITY_EVEN | BR_EULER_REPEAT_YES | BR_EULER_FRAME_STATIC)
        /// </summary>
        XYX_S = 8,
        /// <summary>
        /// (BR_EULER_FIRST_X | BR_EULER_PARITY_ODD | BR_EULER_REPEAT_NO | BR_EULER_FRAME_STATIC)
        /// </summary>
        XZY_S = 4,
        /// <summary>
        /// (BR_EULER_FIRST_X | BR_EULER_PARITY_ODD | BR_EULER_REPEAT_YES | BR_EULER_FRAME_STATIC)
        /// </summary>
        XZX_S = 12,
        /// <summary>
        /// (BR_EULER_FIRST_Y | BR_EULER_PARITY_EVEN | BR_EULER_REPEAT_NO | BR_EULER_FRAME_STATIC)
        /// </summary>
        YZX_S = 1,
        /// <summary>
        /// (BR_EULER_FIRST_Y | BR_EULER_PARITY_EVEN | BR_EULER_REPEAT_YES | BR_EULER_FRAME_STATIC)
        /// </summary>
        YZY_S = 9,
        /// <summary>
        /// (BR_EULER_FIRST_Y | BR_EULER_PARITY_ODD | BR_EULER_REPEAT_NO | BR_EULER_FRAME_STATIC)
        /// </summary>
        YXZ_S = 5,
        /// <summary>
        /// (BR_EULER_FIRST_Y | BR_EULER_PARITY_ODD | BR_EULER_REPEAT_YES | BR_EULER_FRAME_STATIC)
        /// </summary>
        YXY_S = 13,
        /// <summary>
        /// (BR_EULER_FIRST_Z | BR_EULER_PARITY_EVEN | BR_EULER_REPEAT_NO | BR_EULER_FRAME_STATIC)
        /// </summary>
        ZXY_S = 2,
        /// <summary>
        /// (BR_EULER_FIRST_Z | BR_EULER_PARITY_EVEN | BR_EULER_REPEAT_YES | BR_EULER_FRAME_STATIC)
        /// </summary>
        ZXZ_S = 10,
        /// <summary>
        /// (BR_EULER_FIRST_Z | BR_EULER_PARITY_ODD | BR_EULER_REPEAT_NO | BR_EULER_FRAME_STATIC)
        /// </summary>
        ZYX_S = 6,
        /// <summary>
        /// (BR_EULER_FIRST_Z | BR_EULER_PARITY_ODD | BR_EULER_REPEAT_YES | BR_EULER_FRAME_STATIC)
        /// </summary>
        ZYZ_S = 14,

        /// <summary>
        /// (BR_EULER_FIRST_X | BR_EULER_PARITY_EVEN | BR_EULER_REPEAT_NO | BR_EULER_FRAME_ROTATING)
        /// </summary>
        ZYX_R = 16,
        /// <summary>
        /// (BR_EULER_FIRST_X | BR_EULER_PARITY_EVEN | BR_EULER_REPEAT_YES | BR_EULER_FRAME_ROTATING)
        /// </summary>
        XYX_R = 24,
        /// <summary>
        /// (BR_EULER_FIRST_X | BR_EULER_PARITY_ODD | BR_EULER_REPEAT_NO | BR_EULER_FRAME_ROTATING)
        /// </summary>
        YZX_R = 20,
        /// <summary>
        /// (BR_EULER_FIRST_X | BR_EULER_PARITY_ODD | BR_EULER_REPEAT_YES | BR_EULER_FRAME_ROTATING)
        /// </summary>
        XZX_R = 28,
        /// <summary>
        /// (BR_EULER_FIRST_Y | BR_EULER_PARITY_EVEN | BR_EULER_REPEAT_NO | BR_EULER_FRAME_ROTATING)
        /// </summary>
        XZY_R = 17,
        /// <summary>
        /// (BR_EULER_FIRST_Y | BR_EULER_PARITY_EVEN | BR_EULER_REPEAT_YES | BR_EULER_FRAME_ROTATING)
        /// </summary>
        YZY_R = 25,
        /// <summary>
        /// (BR_EULER_FIRST_Y | BR_EULER_PARITY_ODD | BR_EULER_REPEAT_NO | BR_EULER_FRAME_ROTATING)
        /// </summary>
        ZXY_R = 21,
        /// <summary>
        /// (BR_EULER_FIRST_Y | BR_EULER_PARITY_ODD | BR_EULER_REPEAT_YES | BR_EULER_FRAME_ROTATING)
        /// </summary>
        YXY_R = 29,
        /// <summary>
        /// (BR_EULER_FIRST_Z | BR_EULER_PARITY_EVEN | BR_EULER_REPEAT_NO | BR_EULER_FRAME_ROTATING)
        /// </summary>
        YXZ_R = 18,
        /// <summary>
        /// (BR_EULER_FIRST_Z | BR_EULER_PARITY_EVEN | BR_EULER_REPEAT_YES | BR_EULER_FRAME_ROTATING)
        /// </summary>
        ZXZ_R = 26,
        /// <summary>
        /// (BR_EULER_FIRST_Z | BR_EULER_PARITY_ODD | BR_EULER_REPEAT_NO | BR_EULER_FRAME_ROTATING)
        /// </summary>
        XYZ_R = 22,
        /// <summary>
        /// (BR_EULER_FIRST_Z | BR_EULER_PARITY_ODD | BR_EULER_REPEAT_YES | BR_EULER_FRAME_ROTATING)
        /// </summary>
        ZYZ_R = 30
    };
}