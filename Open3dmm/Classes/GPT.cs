using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Open3dmm.WinApi;
using System;
using System.Collections.Generic;

namespace Open3dmm.Classes
{
    public class GPT : BASE
    {
        public REGN Region => GetReference<REGN>(0x08);
        public ref RECTANGLE Field000C => ref GetField<RECTANGLE>(0x0C);
        public ref IntPtr DC => ref GetField<IntPtr>(0x24);
        public ref IntPtr DIBPointer => ref GetField<IntPtr>(0x2C);
        public ref IntPtr PixelBuffer => ref GetField<IntPtr>(0x30);
        public ref RECTANGLE Bounds => ref GetField<RECTANGLE>(0x3C);
        public ref int BitDepth => ref GetField<int>(0x34);
        public ref int Stride => ref GetField<int>(0x38);
        public ref int FlushCounter => ref GetField<int>(0x50);
        public int Width => Bounds.X2 - Bounds.X1;
        public int Height => Bounds.Y2 - Bounds.Y1;

        private Texture2D tex;
        private Color[] colorBuf;
        private GraphicsDevice graphicsDevice;
        private SpriteBatch spriteBatch;
        private RenderTarget2D renderTarget;
        public Queue<Action<RECTANGLE, RECTANGLE>> RenderActions = new Queue<Action<RECTANGLE, RECTANGLE>>();
        public RenderTarget2D RenderTarget => renderTarget;

        private static byte[] temp;

        protected override void Initialize()
        {
            base.Initialize();
            graphicsDevice = NativeAbstraction.GraphicsDevice;
            spriteBatch = NativeAbstraction.SpriteBatch;
            renderTarget = new RenderTarget2D(graphicsDevice, 640, 480);
        }

        public void Blit(GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, GPT other, RECTANGLE src, RECTANGLE dest)
        {
            var otherTex = other.GetTexture(graphicsDevice);
            if (renderTarget == null)
                renderTarget = new RenderTarget2D(graphicsDevice, 640, 480, true, SurfaceFormat.Bgra32SRgb, DepthFormat.None);
            graphicsDevice.SetRenderTarget(renderTarget);
            unsafe
            {
                var _dest = *(Rectangle*)&dest;
                _dest.Width -= _dest.X;
                _dest.Height -= _dest.Y;
                var _src = *(Rectangle*)&src;
                _src.Width -= _src.X;
                _src.Height -= _src.Y;
                spriteBatch.Draw(otherTex, _dest, _src, Color.White);
            }
            graphicsDevice.SetRenderTarget(null);
        }

        public Texture2D GetTexture(GraphicsDevice graphicsDevice)
        {
            //if (RenderTarget != null)
            //    return RenderTarget;
            if (Width <= 0 || Height <= 0)
                return null;
            if (tex == null || (tex.Width != Width || tex.Height != Height))
            {
                tex?.Dispose();
                tex = new Texture2D(graphicsDevice, Width, Height, true, SurfaceFormat.ColorSRgb);
                colorBuf = new Color[Width * Height];
            }

            unsafe
            {
                byte* b = (byte*)PixelBuffer;
                for (int i = 0; i < colorBuf.Length; i++)
                {
                    colorBuf[i] = APP.Palette[b[i]].ToXNA();
                    colorBuf[i].A = byte.MaxValue;
                    //b[i] = 0;
                }
            }
            tex.SetData(colorBuf);
            return tex;
        }

        public unsafe void BlitMBMP(MBMP mbmp, RECTANGLE dest, GPT_UnkStruct1* unkStruct)
        {
            RECTANGLE clip = default;

            if (unkStruct->Clip != null)
            {
                clip.Copy(in *unkStruct->Clip);
                if (!clip.CalculateIntersection(in dest))
                    return;
            }
            else
            {
                clip.Copy(in dest);
            }

            mbmp.GetRect(out var mbmpRect); // call 0x0043F7D0 
            if (mbmpRect.Y1 < mbmpRect.Y2 && mbmpRect.X1 < mbmpRect.X2)
            {
                if (BitDepth == 8)
                {
                    if (dest.X1 - dest.X2 + mbmpRect.X2 != mbmpRect.X1 || dest.Y1 - dest.Y2 + mbmpRect.Y2 != mbmpRect.Y1)
                    {
                        // FIXME: The file dialog screens are black for some reason.
                        // 0042B451
                        PInvoke.Call(LibraryNames.GDI32, "SetTextColor", DC, new IntPtr(0x2FFFFFF));
                        PInvoke.Call(LibraryNames.GDI32, "SetBkColor", DC, new IntPtr(0x2000000));
                        mbmpRect.TopLeftOrigin();
                        if (FromPointer<GPT>(UnmanagedFunctionCall.StdCall((IntPtr)FunctionNames.AllocateGPT, new IntPtr(&mbmpRect), new IntPtr(1))) is GPT gptMaskMaybe)
                        {
                            UnmanagedFunctionCall.ThisCall((IntPtr)FunctionNames.MBMP_00425850, mbmp.NativeHandle.Address, gptMaskMaybe.PixelBuffer, (IntPtr)gptMaskMaybe.Stride, new IntPtr(mbmpRect.Height), new IntPtr(-mbmpRect.X1), new IntPtr(-mbmpRect.Y1), new IntPtr(&mbmpRect)); // call 0x00425850
                            UnmanagedFunctionCall.ThisCall((IntPtr)FunctionNames.GPT__0042A550, NativeHandle.Address, new IntPtr(&unkStruct->Clip));
                            PInvoke.Call(LibraryNames.GDI32, "StretchBlt", DC, new IntPtr(dest.X1), new IntPtr(dest.X2), new IntPtr(dest.Width), new IntPtr(dest.Height), gptMaskMaybe.DC, IntPtr.Zero, IntPtr.Zero, new IntPtr(mbmpRect.Width), new IntPtr(mbmpRect.Height), new IntPtr(0xEE0086));
                            gptMaskMaybe.VirtualCall(0x10); // Free?

                            if (FromPointer<GPT>(UnmanagedFunctionCall.StdCall((IntPtr)FunctionNames.AllocateGPT, new IntPtr(&mbmpRect), new IntPtr(8))) is GPT gptColor)
                            {
                                RECTANGLE fill = default;
                                mbmpRect.SizeLimit(ref fill);

                                PInvoke.Call(LibraryNames.USER32, "FillRect", gptColor.DC, new IntPtr(&fill), PInvoke.Call(LibraryNames.GDI32, "GetStockObject", IntPtr.Zero));
                                UnmanagedFunctionCall.StdCall((IntPtr)FunctionNames.GDIFlush);

                                mbmp.Blit(gptColor.PixelBuffer, gptColor.Stride, mbmpRect.Height, -mbmpRect.X1, -mbmpRect.Y1, &mbmpRect, null);

                                PInvoke.Call(LibraryNames.GDI32, "StretchBlt", DC, new IntPtr(dest.X1), new IntPtr(dest.Y1), new IntPtr(dest.Width), new IntPtr(dest.Height), gptColor.DC, IntPtr.Zero, IntPtr.Zero, new IntPtr(mbmpRect.Width), new IntPtr(mbmpRect.Height), new IntPtr(0x8800C6));
                                gptColor.VirtualCall(0x10); // Free?
                            }
                        }
                    }
                    else
                    {
                        // 0042B3F0

                        if (clip.CalculateIntersection(in Bounds))
                        {
                            if (FlushCounter >= APP.FlushCounter)
                                UnmanagedFunctionCall.StdCall((IntPtr)FunctionNames.GDIFlush);
                            int offsetX = dest.X1 - mbmpRect.X1;
                            int offsetY = dest.Y1 - mbmpRect.Y1;
                            mbmp.Blit(PixelBuffer, Stride, Bounds.Height, offsetX, offsetY, &clip, Region);
                            //var tex = mbmp.GetTexture(graphicsDevice);
                            //if (tex != null)
                            //{
                            //    var _dest = new Rectangle(offsetX, offsetY, mbmpRect.Width, mbmpRect.Height);
                            //    var _clip = new Rectangle(clip.X1, clip.Y1, clip.Width, clip.Height);
                            //    RenderTexture(tex, _dest, _clip);
                            //}
                        }
                    }
                }
                else if (BitDepth == 32)
                {
                    // TODO: Implement 32 bit GPT
                    if (clip.CalculateIntersection(in Bounds))
                    {
                        if (FlushCounter >= APP.FlushCounter)
                            UnmanagedFunctionCall.StdCall((IntPtr)FunctionNames.GDIFlush);
                        int offsetX = dest.X1 - mbmpRect.X1;
                        int offsetY = dest.Y1 - mbmpRect.Y1;
                        if (temp == null || temp.Length != Width * Height)
                            temp = new byte[Width * Height];
                        fixed (byte* b = temp)
                        {
                            mbmp.Blit(new IntPtr(b), 8, Bounds.Height, offsetX, offsetY, &clip, null);
                            RGBQUAD* p = (RGBQUAD*)PixelBuffer;
                            for (int y = clip.Y1; y < clip.Y2; y++)
                            {
                                for (int x = clip.X1; x < clip.X2; x++)
                                {
                                    int i = y * Width + x;
                                    *(p + i) = APP.Palette[b[i]];
                                }
                            }
                        }
                    }
                }
            }
        }

        private unsafe void RenderTexture(Texture2D tex, Rectangle dest, Rectangle clip)
        {
            var src = new Rectangle(clip.X - dest.X, clip.Y - dest.Y, dest.Width, dest.Height);
            Rectangle.Intersect(ref dest, ref clip, out dest);
            src.Width = dest.Width;
            src.Height = dest.Height;
            graphicsDevice.SetRenderTarget(renderTarget);
            try
            {
                spriteBatch.Draw(tex, dest, src, Color.White);
            }
            finally
            {
                graphicsDevice.SetRenderTarget(null);
            }
        }

        public void RenderGPT(GPT gptSrc, RECTANGLE dest, RECTANGLE clip)
        {
            var _dest = new Rectangle(dest.X1, dest.Y1, dest.Width, dest.Height);
            var _clip = new Rectangle(clip.X1, clip.Y1, clip.Width, clip.Height);
            RenderTexture(tex, _dest, _clip);
        }
    }
}
