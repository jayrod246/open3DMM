using System;
using System.Runtime.InteropServices;

namespace Open3dmm.Core.Brender
{
    public unsafe struct BrPixelMap
    {
        public byte* Identifier;
        public void* Pixels;
        private uint _reserved0;
        public BrPixelMap* Map;
        public short RowBytes;
        public BrPixelMapType Type;
        public BrPixelMapFlags Flags;
        public ushort BaseX;
        public ushort BaseY;
        public ushort Width;
        public ushort Height;
        public short OriginX;
        public short OriginY;
        public void* Device;

        delegate BrPixelMap* BrPixelmapAllocateFunction(BrPixelMapType type, ushort w, ushort h, void* pixels, BrPixelMapFlags flags);

        static readonly BrPixelmapAllocateFunction BrPixelmapAllocate;

        static BrPixelMap()
        {
            BrPixelmapAllocate = Marshal.GetDelegateForFunctionPointer<BrPixelmapAllocateFunction>((IntPtr)0x00486610);
        }
        public static BrPixelMap* Allocate(BrPixelMapType type, ushort w, ushort h, void* pixels, BrPixelMapFlags flags)
        {
            return BrPixelmapAllocate(type, w, h, pixels, flags);
        }
    }
}