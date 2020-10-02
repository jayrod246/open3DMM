using ImGuiNET;
using Open3dmm.Core.Graphics;
using Open3dmm.Core.Resolvers;
using System.Numerics;
using Veldrid;

namespace Open3dmm
{
    public class Gorb : Gorp
    {
        public Gorb(IScopedResolver scope, ChunkIdentifier identifier) : base(scope, identifier)
        {
        }

        public override LTRB GetRect()
        {
            if (Tag == Tags.MASK && Resolver.TryResolve<MBMP>(new(Tag, Number), out var mask))
            {
                return mask.Bounds;
            }

            if (Tag == Tags.MBMP && Resolver.TryResolve<Mbmp>(new(Tag, Number), out var mbmp))
            {
                var drect = mbmp.Rect;
                return new(drect.Left, drect.Top, drect.Right, drect.Bottom);
            }
            return default;
        }

        public override void Draw(CommandList commandList, in RectangleF dest)
        {
            if (Tag == Tags.MBMP && Resolver.TryResolve<Mbmp>(new(Tag, Number), out var mbmp))
            {
                Application.Current.GraphicsContext.PaletteSwap(commandList, mbmp);
                var tex = Application.Current.GraphicsContext.GetColorOutputTexture(mbmp);
                var btex = Application.Current.GetOrCreateImGuiBinding(tex);
                var l = ImGui.GetBackgroundDrawList();
                l.AddImage(btex, new Vector2(dest.Left, dest.Top), new Vector2(dest.Right, dest.Bottom));
                //ImGui.Image(btex, new Vector2(rect.Width, rect.Height));
            }
        }

        public override bool HitTest(PT pt)
        {
            if (Tag == Tags.MASK && Resolver.TryResolve<MBMP>(new(Tag, Number), out var mask))
            {
                bool hit = false;
                if (pt.X >= 0 && pt.Y >= 0 && pt.X < mask.Bounds.Width && pt.Y < mask.Bounds.Height)
                {
                    int testRow = pt.Y;
                    int index = pt.X;

                    mask.ProcessRows(s =>
                    {
                        if (s.Row != testRow || hit)
                            return;

                        if (index - s.Start < s.Length && index - s.Start >= 0)
                            hit = true;
                    });
                }

                return hit;
            }
            else if (Tag == Tags.MBMP && Resolver.TryResolve<Mbmp>(new(Tag, Number), out var mbmp))
            {
                if (pt.X >= 0 && pt.Y >= 0 && pt.X < mbmp.Rect.Width && pt.Y < mbmp.Rect.Height)
                {
                    int index = pt.X + pt.Y * mbmp.Rect.Width;
                    if (index >= 0 && index < mbmp.PixelBuffer.Length)
                        return mbmp.PixelBuffer.Span[index] != 0;
                }
            }
            return false;
        }
    }
}