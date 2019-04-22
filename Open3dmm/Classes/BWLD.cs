using Microsoft.Xna.Framework;
using Open3dmm.BRender;
using Open3dmm.Graphics;
using System;

namespace Open3dmm.Classes
{
    public class BWLD : BASE
    {
        private BrWorldRenderer renderer;

        public ref int Width1 => ref GetField<int>(0x10);

        public ref int Height1 => ref GetField<int>(0x14);

        public ref int Width2 => ref GetField<int>(0x20);

        public ref int Height2 => ref GetField<int>(0x24);

        public ref IntPtr RenderHandlers => ref GetField<IntPtr>(0x30);

        public BWLD Field003C {
            get => GetReference<BWLD>(0x003C);
            set => SetReference(value, 0x003C);
        }

        public GPT Bitmap1 {
            get => GetReference<GPT>(0x0100);
            set => SetReference(value, 0x0100);
        }

        public GPT Bitmap2 {
            get => GetReference<GPT>(0x0104);
            set => SetReference(value, 0x0104);
        }

        public ref int Color => ref GetField<int>(0x10C);

        public ZBMP Field0130 {
            get => GetReference<ZBMP>(0x0130);
            set => SetReference(value, 0x0130);
        }

        public ZBMP Field0134 {
            get => GetReference<ZBMP>(0x0134);
            set => SetReference(value, 0x0134);
        }

        public ref int Depth => ref GetField<int>(0x138);

        public REGN Field015C {
            get => GetReference<REGN>(0x015C);
            set => SetReference(value, 0x015C);
        }

        public REGN Field0160 {
            get => GetReference<REGN>(0x0160);
            set => SetReference(value, 0x0160);
        }

        public ref bool DirtyFlag => ref GetField<bool>(0x16C);
        public ref bool SkipHandlers => ref GetField<bool>(0x170);

        public CRF Field0184 {
            get => GetReference<CRF>(0x0184);
            set => SetReference(value, 0x0184);
        }

        public ref BrActor World => ref GetField<BrActor>(0x28);
        public ref BrActor Camera => ref GetField<BrActor>(0x84);
        public ref BrPixelMap PixelMap1 => ref GetField<BrPixelMap>(0x84);
        public ref BrMatrix34 Matrix => ref GetField<BrMatrix34>(0x50);

        protected override void Initialize()
        {
            base.Initialize();
            renderer = new BrWorldRenderer(this);
            NativeAbstraction.GameTimer.Draw += OnRender;
        }

        public void RenderOne()
        {
            rendering = true;
        }

        bool rendering;

        private void OnRender(GameTime gameTime)
        {
            //if (rendering)
            //{
            //    rendering = false;
            //    renderer.Render();
            //}
            renderer.Render();
        }
    }
}
