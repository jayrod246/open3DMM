using Open3dmm.Core;
using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using System;
using Veldrid;

namespace Open3dmm
{
    public class Gorf : Gorp
    {
        public int Field0x8 { get; set; }
        public int Field0xc { get; set; }
        public LTRB Rect { get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public Gorf(IScopedResolver scope, ChunkIdentifier identifier) : base(scope, identifier)
        {
            using var block = BinaryStream.Create(scope.File.GetChunk(identifier).Section.Memory.Span).Decompress();
            if (block.Length == 0x24)
            {
                if (!block.MagicNumber())
                    throw new InvalidOperationException("Bad magic number for FILL.");
                if (block.TryRead(out LTRB rect))
                {
                    Rect = rect;
                    Field0x8 = block.Read<int>();
                    Field0xc = block.Read<int>();
                }
            }
        }

        public override void VirtualFunc3(int width, int height)
        {
            if (width == 0)
                width = Rect.Width;
            if (height == 0)
                height = Rect.Height;
            Width = width;
            Height = height;
        }

        public override LTRB GetRect()
        {
            var rc = new LTRB(0, 0, Width, Height);
            rc.CenterWith(Rect);
            return rc;
        }

        public override void Draw(CommandList commandList, in RectangleF dest)
        {
            //var l = ImGui.GetForegroundDrawList();
            //l.PushClipRect(new Vector2(clip.Left, clip.Top), new Vector2(clip.Right, clip.Bottom));
            //if ((Field0xc & Field0x8) != -1)
            //    l.AddRectFilled(new Vector2(dest.Left, dest.Top), new Vector2(dest.Right, dest.Bottom), (uint)Field0xc);
            //l.PopClipRect();
        }

        public override bool HitTest(PT pt)
        {
            return pt.X >= 0 && pt.Y >= 0 && pt.X < Width && pt.Y < Height;
        }
    }
}