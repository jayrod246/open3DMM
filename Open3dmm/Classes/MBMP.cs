using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.InteropServices;

namespace Open3dmm.Classes
{
    public class MBMP : BACO
    {
        public IntPtr Data => GetField<IntPtr>(0x1C);

        public unsafe bool Blit(IntPtr dest, int stride, int height, int offsetX, int offsetY, RECTANGLE* rect, REGN regn)
        {
            return UnmanagedFunctionCall.ThisCall((IntPtr)FunctionNames.MBMP_Blit, NativeHandle.Address, dest, new IntPtr(stride), new IntPtr(height), new IntPtr(offsetX), new IntPtr(offsetY), new IntPtr(rect), regn?.NativeHandle.Address ?? IntPtr.Zero) != IntPtr.Zero;
        }

        public bool GetRect(out RECTANGLE rect)
        {
            if (Data != IntPtr.Zero)
            {
                rect = Marshal.PtrToStructure<RECTANGLE>(Data + 8);
                return true;
            }
            rect = default;
            return false;
        }

        Texture2D tex;
        private Color[] colorBuf;
        int paletteVersion;
        public Texture2D GetTexture(GraphicsDevice graphicsDevice)
        {
            if (GetRect(out var rc))
            {
                if (tex == null || tex.Width != rc.X2 || tex.Height != rc.Y2 || paletteVersion != APP.PaletteVersion)
                {
                    tex?.Dispose();
                    tex = null;
                    paletteVersion = APP.PaletteVersion;
                    unsafe
                    {
                        rc.TopLeftOrigin();
                        tex = new Texture2D(graphicsDevice, rc.X2, rc.Y2, true, SurfaceFormat.ColorSRgb);
                        var buf = new byte[rc.X2 * rc.Y2];
                        colorBuf = new Color[rc.X2 * rc.Y2];
                        fixed (byte* b = buf)
                        {
                            Blit(new IntPtr(b), rc.X2, rc.Y2, 0, 0, &rc, null);
                            for (int i = 0; i < colorBuf.Length; i++)
                            {
                                colorBuf[i] = APP.Palette[b[i]].ToXNA();
                                colorBuf[i].A = byte.MaxValue;
                            }
                            tex.SetData(colorBuf);
                        }
                    }
                }
            }
            return tex;
        }
    }
}
