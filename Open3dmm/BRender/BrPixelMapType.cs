namespace Open3dmm.BRender
{
    public enum BrPixelMapType : byte
    {
        /*
         * Each pixel is an index into a colour map
         */
        BR_PMT_INDEX_1,
        BR_PMT_INDEX_2,
        BR_PMT_INDEX_4,
        BR_PMT_INDEX_8,

        /*
         * True colour RGB
         */
        BR_PMT_RGB_555,     /* 16 bits per pixel */
        BR_PMT_RGB_565,     /* 16 bits per pixel */
        BR_PMT_RGB_888,     /* 24 bits per pixel */
        BR_PMT_RGBX_888,    /* 32 bits per pixel */
        BR_PMT_RGBA_8888,   /* 32 bits per pixel */

        /*
         * YUV
         */
        BR_PMT_YUYV_8888,   /* YU YV YU YV ... */
        BR_PMT_YUV_888,

        /*
         * Depth
         */
        BR_PMT_DEPTH_16,
        BR_PMT_DEPTH_32,

        /*
         * Opacity
         */
        BR_PMT_ALPHA_8,

        /*
         * Opacity + Index
         */
        BR_PMT_INDEXA_88
    };
}