namespace Open3dmm.Core.Brender {
    public enum BrLightType : byte {
        /// <summary>
        /// Mask used for getting the light type.
        /// </summary>
        BR_LIGHT_TYPE = 0x0003,

        /// <summary>
        /// Point light.
        /// </summary>
        BR_LIGHT_POINT = 0x0000,
        /// <summary>
        /// Directional light.
        /// </summary>
        BR_LIGHT_DIRECT = 0x0001,

        /// <summary>
        /// Spot light.
        /// </summary>
        BR_LIGHT_SPOT = 0x0002,

        /// <summary>
        /// Flag indicating that calculations are to be done in view space.
        /// </summary>
        BR_LIGHT_VIEW = 0x0004
    };
}
