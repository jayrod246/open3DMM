using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace Open3dmm
{
    public abstract class Txtg : Ddg
    {
        public Txtb Txtb { get; set; }
        public int DocumentLineStart { get; set; }
        public object Gnv { get; set; }

        public Txtg(Txtb txtb, GobOptions options) : base(txtb, options)
        {
            Txtb = txtb;
            Gnv = null;
        }

        public override bool VirtualFunc28()
        {
            if (base.VirtualFunc28())
            {
                throw null;
                //SomeList = new 
            }
            return false;
        }

        public bool HitTestTextMaybe(int x, int y, out int i, bool closest)
        {
            var rc = GetRectangle(CoordinateSpace.None);
            if (y >= 0 && y < rc.Bottom)
            {
                var m = GetMeasureDataByLineStart(DocumentLineStart + y, out _);
                if (m.LineHeight_0_2 + m.LineStart <= DocumentLineStart + y)
                {
                    i = Txtb.Text.Length - 1;
                    return true;
                }
                return HitTestLine(x, m, out i, closest);
            }
            i = 0;
            return false;
        }

        private bool HitTestLine(int x, TextMeasureData line, out int i, bool closest)
        {
            i = line.CharIndex;
            x += DocumentX - line.Length_2_2 - 9;
            if (x < 1)
            {
                return closest;
            }
            while (i < Txtb.Text.Length && line.Length_2_2 + GetMarginX(line.CharIndex, i) < x)
            {
                i++;
                line = GetMeasureDataByChar(i, out _);
            }
            if (i >= Txtb.Text.Length)
                i = Txtb.Text.Length - 1;
            return true;
        }

        public override void Draw(CommandList commandList, in RectangleF dest)
        {
            // TODO: Fix text!
            var pt = new Vector2(dest.X, dest.Y);
            var l = ImGui.GetBackgroundDrawList();
            var scale = Application.Current.UiScale;
            for (int i = 0; i < Txtb.Text.Length; i++)
            {
                var measure = GetMeasureDataByChar(i, out _);
                var format = GetFormatData(i, out _, out _);
                int x = measure.Length_2_2 + GetMarginX(measure.CharIndex, i);
                int y = measure.LineStart;
                var dpt = pt + (new Vector2(x, y) * scale);
                var height = measure.LineHeight_0_2 * scale;
                var chr = Txtb.Text[i];
                var font = format.StyleFlags == 0 ? LegacyEntrypoint.ComicSans : LegacyEntrypoint.ComicSansBold;
                var bg = format.BackgroundColor;
                if (bg != 0xffffffff)
                {
                    bg = ComputeColor(bg);
                    l.AddRectFilled(dpt, dpt + new Vector2(font.GetCharAdvance(chr) * scale, height), bg);
                }

                var color = ComputeColor(format.TextColor);


                if (!char.IsWhiteSpace(chr))
                    font.RenderChar(l, LegacyEntrypoint.ComicSans.FontSize * scale, dpt, color, chr);

                //l.AddText(pt + new Vector2(x, y), color, Txtb.Text.Substring(i, 1));
                //l.AddText(Program.ComicSans, format.Size, pt + new Vector2(x, y), 0xff0000ff, Txtb.Text.Substring(i, 1));
            }
        }

        private uint ComputeColor(uint color)
        {
            if ((sbyte)(color >> 24) != -2)
            {
                color = (color & 255 | 512) << 16 | (color >> 8 & 255) << 8 | color >> 16 & 255;
            }
            else
            {
                color = color & 255 | 16777216;
                if (true) // is paletted device?
                {
                    color = unchecked((uint)(Application.Current as App).Palette[unchecked((int)color & 255)].ToRgba());
                }
            }

            return color | 0xff000000u;
        }

        private void DrawOriginal(in RectangleF dest)
        {
            var pt = new Vector2(dest.X, dest.Y);
            var l = ImGui.GetBackgroundDrawList();
            int ofs = 0;
            while (ofs < Txtb.Text.Length)
            {
                var data = GetMeasureDataByChar(ofs, out _);
                l.AddText(pt, 0xff0000ff, Txtb.Text.Substring(ofs, data.Length_0_2));
                ofs += data.Length_0_2;
                pt.Y += data.LineHeight_0_2;
            }
        }

        public int GetMarginX(int start, int end)
        {
            var autoText = new AutoText();
            var alignment = GetAlignmentData(start, out _, out _);
            var lineStart = 0;
            int index = start;
            int tmp = index;
            int _end = end;
            if (_end == start)
                _end++;

            while (autoText.DocumentCharIndex < end && (autoText.Flags & 2) == 0)
            {
                if (tmp <= index)
                {
                    var format = GetFormatData(index, out _, out tmp);
                    if (_end <= tmp)
                        tmp = _end;
                    autoText.Prepare(in format, in alignment, Txtb, index, tmp, lineStart, 16777216, 16777216);
                }
                autoText.ProcessWords(false);
                index = autoText.DocumentCharIndex;
                lineStart = autoText.CalculateExt;
            }
            int result = 9;
            if (end != start)
                result += autoText.CalculateExt;
            else
                result += autoText.MarginSize;
            return result;
        }
        private readonly List<TextMeasureData> cache = new List<TextMeasureData>();

        public TextMeasureData GetMeasureDataByLineStart(int lineStart, out int group)
        {
            int index = 0;
            int ls = 0;
            TextMeasureData result = default;
            int n = 0;

            while (ls <= lineStart)
            {
                if (n >= cache.Count)
                {
                    var measure = GetMeasureData(index, ls);
                    if (measure.Length_0_2 == 0)
                        break;
                    cache.Insert(n, measure);
                }
                result = cache[n];
                index += result.Length_0_2;
                ls += result.LineHeight_0_2;
                n++;
            }

            group = n - 1;
            return result;
        }

        public TextMeasureData GetMeasureDataByGroup(int groupIndex, out int group)
        {
            int index = 0;
            int lineStart = 0;
            TextMeasureData result = default;
            int n = 0;

            if (groupIndex >= 0 && groupIndex < cache.Count)
                return cache[group = groupIndex];

            while (n <= groupIndex)
            {
                if (n >= cache.Count)
                {
                    var measure = GetMeasureData(index, lineStart);
                    if (measure.Length_0_2 == 0)
                        break;
                    cache.Insert(n, measure);
                }
                result = cache[n];
                index += result.Length_0_2;
                lineStart += result.LineHeight_0_2;
                n++;
            }

            group = n - 1;
            return result;
        }

        public TextMeasureData GetMeasureDataByChar(int charIndex, out int group)
        {
            int index = 0;
            int lineStart = 0;
            TextMeasureData result = default;
            int n = 0;

            while (index <= charIndex)
            {
                if (n >= cache.Count)
                {
                    var measure = GetMeasureData(index, lineStart);
                    if (measure.Length_0_2 == 0)
                        break;
                    cache.Insert(n, measure);
                }
                result = cache[n];
                index += result.Length_0_2;
                lineStart += result.LineHeight_0_2;
                n++;
            }

            group = n - 1;
            return result;
        }

        private TextMeasureData GetMeasureData(int charIndex, int y)
        {
            var result = new TextMeasureData();
            int width = GetWrapWidth();
            var alignmentData = GetAlignmentData(charIndex, out _, out var alignmentEnd);
            var measure1 = new TextMeasureData();
            var measure2 = new TextMeasureData { CharIndex = charIndex };
            var measure3 = new TextMeasureData();
            int endOfFormatting = charIndex;
            int tmp = charIndex;
            var autoText = new AutoText();
            int n1 = 0, n2 = 0, n3;
            int tmp_00;

            while (true)
            {
                n3 = n1;
                if (endOfFormatting <= tmp)
                {
                    var formatData = GetFormatData(tmp, out _, out endOfFormatting);
                    endOfFormatting = Math.Min(endOfFormatting, alignmentEnd);
                    tmp_00 = tmp;
                    if (endOfFormatting <= tmp)
                        break;
                    autoText.Prepare(in formatData, in alignmentData, Txtb, tmp, endOfFormatting, measure1.LineStart, width, width);
                }
                autoText.ProcessWords(charIndex == measure2.CharIndex);
                if (tmp == autoText.DocumentCharIndex)
                {
                    tmp_00 = measure2.CharIndex;
                    measure1.Length_0_2 = (short)n2;
                    measure1.LineHeight_0_2 = (short)n3;
                    measure1.LineStart = measure2.LineStart;
                    break;
                }
                measure3.CharIndex = tmp;
                measure3.LineStart = measure1.LineStart;
                measure3.Length = measure1.Length;
                measure1.LineStart = autoText.CalculateResult;
                measure3.LineHeight = measure1.LineHeight;
                measure1.Length = Math.Max(measure1.Length, autoText.BaseLineOffset);
                measure1.LineHeight = Math.Max(measure1.LineHeight, autoText.TextBottom);
                if ((autoText.Flags & 2) != 0)
                {
                    tmp_00 = autoText.DocumentCharIndex;
                    if (autoText.Width < autoText.CalculateResult && charIndex < tmp)
                    {
                        tmp_00 = measure3.CharIndex;
                        measure1.Length_0_2 = measure3.Length_0_2;
                        measure1.LineHeight_0_2 = measure3.LineHeight_0_2;
                        measure1.LineStart = measure3.LineStart;
                    }
                    break;
                }

                if (tmp < autoText.DocumentPreviousWord)
                {
                    measure2.CharIndex = autoText.DocumentPreviousWord;
                    measure2.LineStart = autoText.__WorkingLineWidth;
                    n2 = Math.Max(autoText.BaseLineOffset, n2);
                    n1 = Math.Max(autoText.TextBottom, n3);
                }
                else
                {
                    n1 = n3;
                }
                tmp = autoText.DocumentCharIndex;
            }

            if (alignmentData.TextAlignment == TextAlignment.Oppose)
                result.Length_2_2 = (short)(width - measure1.LineStart_0_2);
            else if (alignmentData.TextAlignment == TextAlignment.Center)
                result.Length_2_2 = (short)((width - measure1.LineStart_0_2) / 2);
            else
                result.Length_2_2 = 0;

            result.Length_0_2 = (short)(tmp_00 - charIndex);
            result.LineHeight_0_2 = (short)(measure1.LineHeight_0_2 + measure1.Length_0_2);
            if (alignmentData.Field0x4 != 256)
                result.LineHeight_0_2 = (short)((long)result.LineHeight_0_2 * alignmentData.Field0x4 / 256);
            result.LineHeight_0_2 += alignmentData.LineHeight;
            if (Txtb.ShouldReturnCarriage(tmp_00) || tmp_00 == Txtb.Text.Length)
            {
                if (alignmentData.Field0x8 != 256)
                    result.LineHeight_0_2 = (short)((long)result.LineHeight_0_2 * alignmentData.Field0x8 / 256);
                result.LineHeight_0_2 += alignmentData.Field0xA;
            }

            result.LineHeight_0_2 = Math.Max(result.LineHeight_0_2, (short)1);
            result.LineHeight_2_2 = Math.Max(measure1.Length_0_2, (short)1);
            result.CharIndex = charIndex;
            result.LineStart = y;
            return result;
        }

        protected abstract TextAlignmentData GetAlignmentData(int charIndex, out int start, out int end);

        protected abstract TextFormatData GetFormatData(int charIndex, out int start, out int end);

        public virtual int GetWrapWidth()
        {
            return Txtb.Field0x3c;
            //return Parent.GetRect(CoordinateSpace.None).Right;
        }

        public virtual PT GetTextBounds()
        {
            var measure = GetMeasureDataByGroup(134217727, out _);
            return new(GetWrapWidth() + 18, measure.LineStart + measure.LineHeight_0_2);
        }
    }

    public struct AutoText
    {
        public TextFormatData FormatData;
        public TextAlignmentData AlignmentData;
        public Txtb Document;
        public int GNV;
        public int Flags;
        public int DocumentStartIndex;
        public int DocumentEndIndexTarget;
        public int DocumentEndIndex;
        public int MarginSize;
        public int Width;
        public int DocumentCharIndex;
        public int DocumentLineChar;
        public int CalculateExt;
        public int CalculateResult;
        public int DocumentPreviousWord;
        public int WorkingEndIndex;
        public int CurrentIndentValue;
        public int __WorkingLineWidth;
        public int BaseLineOffset;
        public int TextBottom;
        private string Text;
        public void Prepare(in TextFormatData formatData, in TextAlignmentData alignmentData, Txtb doc, int start, int end, int linestart, int width1, int width2)
        {
            FormatData = formatData;
            AlignmentData = alignmentData;
            Document = doc;
            DocumentStartIndex = start;
            DocumentEndIndexTarget = end;
            DocumentEndIndex = start;
            Flags &= ~2;
            MarginSize = linestart;
            Width = Math.Min(width1, width2);

            if (linestart == 0)
            {
                switch (AlignmentData.IndentValue)
                {
                    case 1:
                        if (doc.ShouldReturnCarriage(start))
                            MarginSize += AlignmentData.WhitespaceSize;
                        break;
                    case 2:
                        if (!doc.ShouldReturnCarriage(start))
                            MarginSize += AlignmentData.WhitespaceSize;
                        break;
                    case 3:
                        MarginSize = AlignmentData.WhitespaceSize;
                        break;
                }
            }

            if (AlignmentData.IndentValue == 3)
                Width = Math.Min(Width, width1 - AlignmentData.WhitespaceSize);
            DocumentCharIndex = DocumentLineChar = DocumentStartIndex;
            __WorkingLineWidth = CurrentIndentValue = CalculateExt = CalculateResult = MarginSize;
            DocumentPreviousWord = DocumentCharIndex;
            WorkingEndIndex = DocumentLineChar;
            // SetTextAppe
            var bounds = GetTextBounds("");
            BaseLineOffset = Math.Min(0, -(bounds.Top + FormatData.BaseLineOffset));
            var tmp = Math.Max(0, bounds.Bottom + FormatData.BaseLineOffset);
            if (tmp - TextBottom > 32)
            {

            }
            TextBottom = tmp;
        }

        private Rectangle GetTextBounds(string str)
        {
            var font = LegacyEntrypoint.ComicSansBold;
            var v = Vector2.Zero; // ImGui.CalcTextSize(str);
            v.Y = font.FontSize;
            v.X = str.Sum(c => font.GetCharAdvance(c));
            return new Rectangle(0, 0, (int)v.X, (int)v.Y);
        }

        public void ProcessWords(bool wrap)
        {
            WorkingEndIndex = DocumentPreviousWord = DocumentStartIndex = DocumentLineChar = DocumentCharIndex;
            __WorkingLineWidth = CurrentIndentValue = MarginSize = CalculateResult = CalculateExt;
            Width = Math.Max(Width, CalculateExt);
            if (DocumentEndIndexTarget <= DocumentCharIndex)
                return;

            DocumentEndIndex = Math.Min(DocumentEndIndexTarget, DocumentStartIndex + 128);
            Text = Document.Text[DocumentStartIndex..DocumentEndIndex];
            if (wrap)
                Flags |= 1;
            else
                Flags &= ~1;
            Flags &= ~6;
            // SetTextAppe

            while (DocumentCharIndex < DocumentEndIndex)
            {
                var c = Text[DocumentCharIndex - DocumentStartIndex];
                var cflags = ParseCharFlags(c);
                if (cflags.HasFlag(CharFlags.NewLine))
                    break;
                if (cflags.HasFlag(CharFlags.Indent))
                {
                    IndentMaybe();
                    return;
                }
                if (cflags.HasFlag(CharFlags.ReturnCarriage))
                {
                    if (!CalculateLineWidth())
                    {
                        DoWordWrapping();
                        return;
                    }
                    DocumentCharIndex++;
                    AdvanceDocumentSkippingNewLines();
                    DocumentPreviousWord = DocumentCharIndex;
                    WorkingEndIndex = DocumentLineChar;
                    Flags |= 2;
                    CurrentIndentValue = CalculateExt;
                    __WorkingLineWidth = CalculateResult;
                    return;
                }
                if (cflags.HasFlag(CharFlags.WhiteSpace))
                {
                    DocumentLineChar = ++DocumentCharIndex;
                    if (!CalculateLineWidth())
                    {
                        Flags |= 2;
                        if (cflags.HasFlag(CharFlags.Space))
                        {
                            DocumentLineChar--;
                            var restore = CalculateExt;
                            if (CalculateLineWidth())
                            {
                                CurrentIndentValue = CalculateExt = restore;
                                DocumentPreviousWord = DocumentCharIndex;
                                WorkingEndIndex = DocumentLineChar;
                                __WorkingLineWidth = CalculateResult;
                                AdvanceDocumentSkippingNewLines();
                                return;
                            }
                        }
                        DoWordWrapping();
                        return;
                    }
                    DocumentPreviousWord = DocumentCharIndex;
                    WorkingEndIndex = DocumentLineChar;
                    CurrentIndentValue = CalculateExt;
                    __WorkingLineWidth = CalculateResult;
                }
                else
                {
                    if (c == '\x01')
                    {
                        if (DocumentStartIndex < DocumentCharIndex)
                        {
                            if (!CalculateLineWidth())
                            {
                                DoWordWrapping();
                                return;
                            }

                            DocumentPreviousWord = DocumentCharIndex;
                            WorkingEndIndex = DocumentLineChar;
                            CurrentIndentValue = CalculateExt;
                            __WorkingLineWidth = CalculateResult;
                            return;
                        }

                        if (Document.VirtualFunc43(DocumentCharIndex, ref this, out var bounds))
                        {
                            bounds.Offset(0, FormatData.BaseLineOffset);
                            var right = bounds.Right + MarginSize;
                            if (!wrap && Width < right)
                                return;
                            Flags |= 4;
                            DocumentPreviousWord = DocumentLineChar = DocumentCharIndex + 1;
                            CalculateExt = CalculateResult = right;
                            WorkingEndIndex = DocumentLineChar;
                            CurrentIndentValue = CalculateExt;
                            __WorkingLineWidth = CalculateResult;
                            BaseLineOffset = Math.Max(BaseLineOffset, -bounds.Top);
                            TextBottom = Math.Max(bounds.Bottom, TextBottom);
                            if (Width <= right)
                                Flags |= 6;
                            AdvanceDocumentSkippingNewLines();
                        }
                    }

                    DocumentLineChar = ++DocumentCharIndex;
                }
            }

            if (!CalculateLineWidth())
            {
                DoWordWrapping();
            }
            else
            {
                AdvanceDocumentSkippingNewLines();
            }
        }

        private void AdvanceDocumentSkippingNewLines()
        {
            char c = default;
            while (DocumentCharIndex < DocumentEndIndex)
            {
                c = Text[DocumentCharIndex - DocumentStartIndex];
                if (!ParseCharFlags(c).HasFlag(CharFlags.NewLine))
                    break;
                DocumentCharIndex++;
            }

            if (DocumentEndIndex == DocumentCharIndex)
            {
                int length = Document.Text.Length;
                while (DocumentCharIndex < length)
                {
                    Document.VirtualFunc38(DocumentCharIndex, MemoryMarshal.CreateSpan<char>(ref c, 1));
                    if (!ParseCharFlags(c).HasFlag(CharFlags.NewLine))
                        break;
                    DocumentCharIndex++;
                }
            }
        }

        private bool CalculateLineWidth()
        {
            var right = MarginSize;
            if (DocumentLineChar != DocumentStartIndex)
                right += GetTextBounds(Text.Substring(0, DocumentLineChar - DocumentStartIndex)).Width;
            return (CalculateExt = CalculateResult = right) <= Width;
        }

        private void DoWordWrapping()
        {
            Flags |= 2;
            if (DocumentPreviousWord <= DocumentStartIndex && (Flags & 1) != 0)
            {
                int w = Width - MarginSize;
                int n = DocumentLineChar - DocumentStartIndex;
                int x = 0;
                while (x < n)
                {
                    int mid = (x + n) / 2;
                    if (GetTextBounds(Text.Substring(0, mid + 1)).Width <= w)
                        x = mid + 1;
                    else
                        n = mid;
                }
                if (x == 0)
                    x = 1;
                DocumentCharIndex = DocumentLineChar = DocumentStartIndex + x;
                CalculateLineWidth();
            }
            else
            {
                DocumentCharIndex = DocumentPreviousWord;
                DocumentLineChar = WorkingEndIndex;
                CalculateExt = CurrentIndentValue;
                CalculateResult = __WorkingLineWidth;
            }
            AdvanceDocumentSkippingNewLines();
        }

        private void IndentMaybe()
        {
            if (!CalculateLineWidth())
            {
                DoWordWrapping();
                return;
            }

            while (DocumentCharIndex < DocumentEndIndex)
            {
                var cflags = ParseCharFlags(Text[DocumentCharIndex - DocumentStartIndex]);
                if (!cflags.HasFlag(CharFlags.Indent))
                    break;
                DocumentCharIndex++;
                CalculateExt = Pack(CalculateExt + 1, AlignmentData.WhitespaceSize);
                if (Width < CalculateExt)
                {
                    if (DocumentStartIndex - DocumentCharIndex == -1 && (Flags & 1) != 0)
                    {
                        AdvanceDocumentSkippingNewLines();
                        Flags |= 2;
                        return;
                    }
                    DoWordWrapping();
                    return;
                }
                CalculateResult = CalculateExt;
                DocumentPreviousWord = DocumentCharIndex;
                WorkingEndIndex = DocumentLineChar;
                CurrentIndentValue = CalculateExt;
                __WorkingLineWidth = CalculateResult;
            }

            AdvanceDocumentSkippingNewLines();
        }

        private int Pack(int x, int y)
        {
            if (y < 0)
            {
                y = -y;
            }
            if (x < 0)
            {
                x = (x - y) + 1;
            }
            else
            {
                x = y + -1 + x;
            }
            return x - x % y;
        }

        public static CharFlags ParseCharFlags(char c)
        {
            switch (c)
            {
                case '\t':
                    return CharFlags.WhiteSpace | CharFlags.ControlChar | CharFlags.Indent;
                case '\n':
                    return CharFlags.NewLine | CharFlags.ControlChar;
                default:
                    if (c < 32)
                        return CharFlags.ControlChar;
                    return CharFlags.None;
                case '\r':
                    return CharFlags.ReturnCarriage | CharFlags.WhiteSpace | CharFlags.ControlChar;
                case ' ':
                    return CharFlags.Space | CharFlags.WhiteSpace;
                case '\x7f':
                    return CharFlags.ControlChar;
            }
        }
    }

    [Flags]
    public enum CharFlags : int
    {
        None = 0x0,
        Space = 0x1,
        ReturnCarriage = 0x2,
        WhiteSpace = 0x4,
        NewLine = 0x8,
        ControlChar = 0x10,
        Indent = 0x20,
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct TextMeasureData
    {
        [FieldOffset(0)]
        public int CharIndex;
        [FieldOffset(4)]
        public int LineStart;
        [FieldOffset(4)]
        public short LineStart_0_2;
        [FieldOffset(6)]
        public short LineStart_2_2;
        [FieldOffset(8)]
        public int Length;
        [FieldOffset(8)]
        public short Length_0_2;
        [FieldOffset(10)]
        public short Length_2_2;
        [FieldOffset(12)]
        public int LineHeight;
        [FieldOffset(12)]
        public short LineHeight_0_2;
        [FieldOffset(14)]
        public short LineHeight_2_2;
    }

    public struct TextAlignmentData
    {
        public TextAlignment TextAlignment;
        public byte IndentValue;
        public short WhitespaceSize;
        public short Field0x4;
        public short LineHeight;
        public short Field0x8;
        public short Field0xA;
    }

    public struct TextFormatData
    {
        public int StyleFlags;
        public int Font;
        public int Size;
        public int BaseLineOffset;
        public uint TextColor;
        public uint BackgroundColor;
    }

    public enum TextAlignment : byte
    {
        Default = 0,
        Oppose = 1,
        Center = 2,
    }
}