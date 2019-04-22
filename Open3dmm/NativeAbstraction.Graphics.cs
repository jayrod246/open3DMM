using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Open3dmm.WinApi;
using System;
using System.Threading;

namespace Open3dmm
{
    internal partial class NativeAbstraction
    {
        public static GraphicsDevice GraphicsDevice;
        public static SpriteBatch SpriteBatch;
        public static Texture2D SimpleTexture;
        public static GameTimer GameTimer;

        private static Texture2D uiTexture;
        private static byte[] uiBuffer;
        private static Thread timerThread;

        public static void InitGraphics()
        {
            var presentationParameters = new PresentationParameters()
            {
                BackBufferFormat = SurfaceFormat.Bgra32SRgb,
                BackBufferWidth = 640,
                BackBufferHeight = 480,
                DepthStencilFormat = DepthFormat.Depth16,
                IsFullScreen = false,
                HardwareModeSwitch = false,
                PresentationInterval = PresentInterval.Default,
                DisplayOrientation = DisplayOrientation.Default,
                DeviceWindowHandle = MainWindowHandle,
                MultiSampleCount = 32,
                RenderTargetUsage = RenderTargetUsage.DiscardContents
            };
            GraphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, presentationParameters);
            GraphicsDevice.Clear(Color.White);
            SimpleTexture = new Texture2D(GraphicsDevice, 1, 1, true, SurfaceFormat.Bgra32SRgb);
            uiTexture = new Texture2D(GraphicsDevice, 640, 480, true, SurfaceFormat.Bgr32SRgb);
            uiBuffer = new byte[640 * 480 * 4];
            SimpleTexture.SetData(new Color[] { Color.White });
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            GameTimer.Updated += ProcessUI;
            GameTimer.Draw += RenderUI;
            timerThread = new Thread(() => { while (true) GameTimer.Tick(); });
            timerThread.Start();
        }

        const int bmiSz = 40 + 4 * 256;
        private static byte[] uiBitmapInfo = new byte[bmiSz];

        private static TimeSpan lastUpdate;

        private static void ProcessUI(GameTime gameTime)
        {
            if ((gameTime.TotalGameTime - lastUpdate).TotalSeconds < 0.33)
                return;

            unsafe
            {
                fixed (byte* bmi = uiBitmapInfo)
                {
                    *(int*)bmi = bmiSz;
                    PInvoke.Call(LibraryNames.GDI32, "GetDIBits", MainWindowDC, MainWindowBitmap, IntPtr.Zero, new IntPtr(480), IntPtr.Zero, (IntPtr)bmi, IntPtr.Zero);
                    fixed (void* b = uiBuffer)
                    {
                        // Redraw window until successful.
                        while (PInvoke.Call(LibraryNames.USER32, "RedrawWindow", MainWindowHandle, IntPtr.Zero, IntPtr.Zero, new IntPtr(4 | 1)) == IntPtr.Zero) ;
                        PInvoke.Call(LibraryNames.GDI32, "GetDIBits", MainWindowDC, MainWindowBitmap, IntPtr.Zero, new IntPtr(480), (IntPtr)b, (IntPtr)bmi, IntPtr.Zero);
                        uiTexture.SetData(uiBuffer);
                    }
                }
            }
        }

        private static void RenderUI(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            SpriteBatch.Begin();
            SpriteBatch.Draw(uiTexture, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.FlipVertically, 0f);
            SpriteBatch.End();
        }
    }
}
