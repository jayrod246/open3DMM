namespace Open3dmm
{
    public enum FunctionNames : int
    {
        __WinMainCRTStartup = 0x004C5E88,
        __ioinit = 0x004C7850,
        __initmbctable = 0x004C7845,
        __setargv = 0x004C73B9,
        __setenvp = 0x004C72EE,
        __cinit = 0x004C6772,
        WinMain = 0x00427FB0,
        Malloc = 0x00419130,
        Free = 0x00419160,

        // StdCall
        AllocateGPT = 0x00429EE0,
        GDIFlush = 0x00429680,

        // ThisCall
        BWLD_Render = 0x00474590,
        BWLD_RenderBackground = 0x00474740,
        GOB__0045CD30 = 0x0045CD30,
        GOB__0045D0B0 = 0x0045D0B0,
        GOB__0045CFD0 = 0x0045CFD0,
        GPT__0042A550 = 0x0042A550,
        GPT__BlitMBMP = 0x0042B330,
        Rectangle_HitTest = 0x0041A170,
        Rectangle_Copy = 0x0041A230,
        Rectangle_TopLeftOrigin = 0x0041A100,
        Rectangle_Translate = 0x0041A0E0,
        Rectangle_CopyAtOffset = 0x0041A0B0,
        Rectangle_Union = 0x00419D90,
        Rectangle_CalculateIntersectionBetween = 0x00419E10,
        Rectangle_CalculateIntersection = 0x00419E90,
        Rectangle_method00419F30 = 0x00419F30,
        Rectangle_SizeLimit = 0x0041A250,
        MBMP_Blit = 0x00425300,
        MBMP_GetRect = 0x0043F7D0,
        MBMP_00425850 = 0x00425850,
    }
}
