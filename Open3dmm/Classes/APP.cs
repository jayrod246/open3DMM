using Microsoft.Xna.Framework;
using System;
using System.Runtime.InteropServices;

namespace Open3dmm.Classes
{
    public class APP : APPB
    {
        private static RGBQUAD[] palette;

        public static APP Instance => FromPointer<APP>(new IntPtr(0x4D4120));
        public static RGBQUAD[] Palette => palette ?? (palette = ManagedArray.FromPointer<RGBQUAD>(new IntPtr(0x004E3CA4), 256));
        public static int PaletteVersion => Marshal.ReadInt32(new IntPtr(0x004E3CA8));
        public static int FlushCounter => Marshal.ReadInt32(new IntPtr(0x004E3CAC));

        public void Method0041CFB0(GOB button, REGN reg, int flags, int arg4)
        {
            if (!reg.RegionIsWhack(out var rect))
            {
                var v = RECTANGLE.Empty;
                button.Method004241B0(ref v, 0);
                if(v.CalculateIntersection(in rect))
                {
                    if((flags & 1) !=0 || (Flags00C & 2) != 0 && (flags & 2) == 0)
                    {
                        if (arg4 == 0 && button.Field000C != 0)
                        {
                            VirtualCall(0x50);
                        }
                        else
                        {

                        }
                    }
                }
            }
        }
    }
}
