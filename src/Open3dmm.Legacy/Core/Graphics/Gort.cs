using ImGuiNET;
using Open3dmm.Core.Graphics;
using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using System;
using System.Numerics;
using Veldrid;

namespace Open3dmm
{
    public class Gort : Gorp
    {
        short[] xValues;
        short[] yValues;
        int Width;
        int Height;
        int x1;
        int y1;
        int x2;
        int y2;

        public Gort(IScopedResolver scope, ChunkIdentifier identifier) : base(scope, identifier)
        {
            using var block = BinaryStream.Create(scope.File.GetChunk(identifier).Section.Memory.Span).Decompress();
            if (block.Length == 40)
            {
                if (!block.MagicNumber())
                    throw new InvalidOperationException("Bad magic number for TILE.");
                xValues = new short[9];
                yValues = new short[9];
                if (block.TryRead(xValues.AsSpan()) &&
                    block.TryRead(yValues.AsSpan()))
                {
                    Width = xValues[4] + xValues[1] + xValues[5];
                    Height = xValues[8] + xValues[2] + xValues[5];
                }
            }
        }

        public override void VirtualFunc3(int width, int height)
        {
            FUN_0045f140(ref x1, ref x2, width + xValues[1] + xValues[0], xValues);
            FUN_0045f140(ref y1, ref y2, height + yValues[1] + yValues[0], yValues);
            Width = xValues[8] + xValues[2] + xValues[5] + x1 + x2;
            Height = yValues[8] + yValues[2] + yValues[5] + y2 + y1;
        }

        private static void FUN_0045f140(ref int a, ref int b, int total, Span<short> span)
        {
            int i;
            int j;
            int x;

            x = ((total - span[5]) - span[8]) - span[2];
            if (x < 1)
            {
                x = 0;
            }
            b = x;
            x = (int)(((long)x * span[3]) / (span[6] + span[3]));
            a = x;
            if (0 < x)
            {
                i = Align(x, span[3]);
                j = Pack(a - i, span[4]);
                x = span[3];
                if (j <= span[3])
                {
                    x = j;
                }
                a = x + i;
            }
            x = b - a;
            if (x < 1)
            {
                x = 0;
            }
            b = x;
            if (0 < x)
            {
                i = Align(x, span[6]);
                j = Pack(b - i, span[7]);
                x = span[6];
                if (j <= span[6])
                {
                    x = j;
                }
                b = x + i;
            }
            return;
        }

        private static int Pack(int x, int y)
        {
            if (y < 0)
            {
                y = -y;
            }
            if (x < 0)
            {
                x = x - y + 1;
            }
            else
            {
                x = y + -1 + x;
            }
            return x - x % y;
        }

        private static int Align(int x, int y)
        {
            if (y < 0)
                y = -y;
            return x - x % y;
        }

        public override LTRB GetRect()
        {
            var rect = new LTRB(0, 0, Width, Height);
            if (TryGetMbmp(out var mbmp))
            {
                int offsetX = -mbmp.Rect.X;
                int offsetY = -mbmp.Rect.Y;
                FUN_0045f2d0(ref offsetX, xValues, x1, x2);
                FUN_0045f2d0(ref offsetY, yValues, y1, y2);
                rect.Offset(-offsetX, -offsetY);
            }
            return rect;
        }

        public override LTRB GetRectOrigin()
        {
            return new(xValues[0], yValues[0], Width - xValues[1], Height - yValues[1]);
        }

        private static void FUN_0045f2d0(ref int val, Span<short> span, int a, int b)
        {
            int iVar1;
            int iVar2;
            int iVar3;

            iVar3 = span[2];
            iVar1 = val;
            if (iVar3 <= iVar1)
            {
                iVar2 = span[3];
                if (iVar1 < iVar2 + iVar3)
                {
                    val = iVar3 + (int)((long)(iVar1 - iVar3) * a / iVar2);
                    return;
                }
                iVar1 = iVar1 - iVar2 + a;
                val = iVar1;
                iVar3 += span[5] + a;
                if (iVar3 <= iVar1)
                {
                    iVar2 = span[6];
                    if (iVar1 < iVar2 + iVar3)
                    {
                        val = (int)((long)(iVar1 - iVar3) * b / iVar2) + iVar3;
                        return;
                    }
                    val = iVar1 - iVar2 + b;
                }
            }
        }

        private void FUN_0045ec80(Action<RectangleF, RectangleF> renderDelegate, Mbmp mbmp, in RectangleF src, in RectangleF dest, int x, int y)
        {
            var rc = src;
            rc.Offset(dest.X, dest.Y);
            if (rc.TryGetIntersection(in dest, out _))
            {
                var rcMbmp = (RectangleF)mbmp.Rect;
                rc = new RectangleF(dest.X + src.X, dest.Y + src.Y, xValues[3] + xValues[2], src.Height);
                float newRight = xValues[2] + x1 + rc.X;

                if (newRight <= rc.Right)
                {
                    rc.Width = newRight - rc.X;
                }
                y += (int)rc.Y;
                if (dest.TryGetIntersection(in rc, out var intersect))
                {
                    rcMbmp.Offset(x - rcMbmp.X + rc.X, y - rcMbmp.Y);
                    renderDelegate(rcMbmp, intersect);
                }
                x -= xValues[2];
                while ((rc.X = rc.Right) < newRight)
                {
                    rc.Width = xValues[3];
                    if (newRight <= rc.Right)
                    {
                        rc.Width = newRight - rc.X;
                    }
                    if (dest.TryGetIntersection(in rc, out intersect))
                    {
                        rcMbmp.Offset(x - rcMbmp.X + rc.X, y - rcMbmp.Y);
                        renderDelegate(rcMbmp, intersect);
                    }
                }
                newRight = x2 + xValues[5] + rc.X;
                rc.Width = xValues[6] + xValues[5];
                if (newRight <= rc.Right)
                {
                    rc.Width = newRight - rc.X;
                }
                x -= xValues[3];
                if (dest.TryGetIntersection(in rc, out intersect))
                {
                    rcMbmp.Offset(x - rcMbmp.X + rc.X, y - rcMbmp.Y);
                    renderDelegate(rcMbmp, intersect);
                }
                x -= xValues[5];
                while ((rc.X = rc.Right) < newRight)
                {
                    rc.Width = xValues[6];
                    if (newRight <= rc.Right)
                    {
                        rc.Width = newRight - rc.X;
                    }
                    if (dest.TryGetIntersection(in rc, out intersect))
                    {
                        rcMbmp.Offset(x - rcMbmp.X + rc.X, y - rcMbmp.Y);
                        renderDelegate(rcMbmp, intersect);
                    }
                }
                rc.Width = src.Right - rc.X;
                if (dest.TryGetIntersection(in rc, out intersect))
                {
                    rcMbmp.Offset(x - xValues[6] - rcMbmp.X + rc.X, y - rcMbmp.Y);
                    renderDelegate(rcMbmp, intersect);
                }
            }
        }

        public override void Draw(CommandList commandList, in RectangleF dest)
        {
            short sVar1;
            int iVar2;
            RectangleF src;

            if (TryGetMbmp(out var mbmp))
            {
                Application.Current.GraphicsContext.PaletteSwap(commandList, mbmp);
                var tex = Application.Current.GraphicsContext.GetColorOutputTexture(mbmp);
                var btex = Application.Current.GetOrCreateImGuiBinding(tex);
                var l = ImGui.GetBackgroundDrawList();
                var scale = Application.Current.UiScale;
                var ox = dest.X / scale;
                var oy = dest.Y / scale;
                var localDest = dest;
                localDest.X = localDest.Y = 0;

                Vector2 rounded(Vector2 vec)
                    => new(MathF.Round(vec.X), MathF.Round(vec.Y));

                void renderDelegate(RectangleF rc, RectangleF clip)
                {
                    l.PushClipRect(rounded(new Vector2(ox + clip.X, oy + clip.Y) * scale), rounded(new Vector2(ox + clip.Right, oy + clip.Bottom) * scale));
                    l.AddImage(btex, rounded(new Vector2(ox + rc.Left, oy + rc.Top) * scale), rounded(new Vector2(ox + rc.Right, oy + rc.Bottom) * scale));
                    l.PopClipRect();
                }

                src = new(0, 0, Width, yValues[3] + yValues[2]);
                float newBottom = y1 + yValues[2] + src.Y;
                if (newBottom <= src.Bottom)
                {
                    src.Height = newBottom - src.Y;
                }
                FUN_0045ec80(renderDelegate, mbmp, in src, in localDest, 0, 0);
                sVar1 = yValues[2];
                while ((src.Y = src.Bottom) < newBottom)
                {
                    src.Height = yValues[3];
                    if (newBottom <= src.Bottom)
                    {
                        src.Height = newBottom - src.Y;
                    }
                    FUN_0045ec80(renderDelegate, mbmp, in src, in localDest, 0, -sVar1);
                }
                newBottom = y2 + yValues[5] + src.Y;
                src.Height = yValues[6] + yValues[5];
                if (newBottom <= src.Bottom)
                {
                    src.Height = newBottom - src.Y;
                }
                iVar2 = -sVar1 - yValues[3];
                FUN_0045ec80(renderDelegate, mbmp, in src, in localDest, 0, iVar2);
                iVar2 -= yValues[5];
                while ((src.Y = src.Bottom) < newBottom)
                {
                    src.Height = yValues[6];
                    if (newBottom <= src.Bottom)
                    {
                        src.Height = newBottom - src.Y;
                    }
                    FUN_0045ec80(renderDelegate, mbmp, in src, in localDest, 0, iVar2);
                }
                src.Height = Height;
                FUN_0045ec80(renderDelegate, mbmp, in src, in localDest, 0, iVar2 - yValues[6]);
            }
        }

        private bool TryGetMbmp(out Mbmp mbmp)
            => Resolver.TryResolve(new ChunkIdentifier(Tag, Number), Tags.MBMP, 0, out mbmp);

        public override bool HitTest(PT pt)
        {
            int x = pt.X, y = pt.Y;

            if (x >= 0 && y >= 0 && x < Width && y < Height)
            {
                if (!TryGetMbmp(out var mbmp))
                    return true;

                TransformValueToTileBitmapRegion(ref x, xValues, x1, x2);
                TransformValueToTileBitmapRegion(ref y, yValues, y1, y2);

                int index = x + y * mbmp.Rect.Width;
                if (index >= 0 && index < mbmp.PixelBuffer.Length)
                {
                    return mbmp.PixelBuffer.Span[index] != 0;
                }
            }

            return false;
        }

        private int TransformValueToTileBitmapRegion(ref int val, Span<short> range, int a, int b)
        {
            short sVar1;
            int computedVal;
            int t;

            t = range[2];
            computedVal = val;
            if (t <= computedVal)
            {
                if (computedVal < t + a)
                {
                    sVar1 = range[3];
                    val = t + (computedVal - t) % sVar1;
                    return (computedVal - t) / sVar1;
                }
                computedVal = (computedVal - a) + range[3];
                val = computedVal;
                t += range[5] + range[3];
                if (t <= computedVal)
                {
                    if (computedVal < t + b)
                    {
                        sVar1 = range[6];
                        val = t + (computedVal - t) % sVar1;
                        return (computedVal - t) / sVar1;
                    }
                    val = (range[6] - b) + computedVal;
                }
            }

            return computedVal;
        }
    }
}