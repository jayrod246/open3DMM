using System;
using System.Collections.Generic;

namespace Open3dmm
{
    public class Txrg : Txtg
    {
        protected Txrg(Txrd txrd, GobOptions options) : base(txrd, options)
        {
            Field0xc4 = 0;
            Field0xc8 = 0;
        }

        public int Field0xc4 { get; set; }
        public int Field0xc8 { get; set; }

        protected override TextAlignmentData GetAlignmentData(int charIndex, out int start, out int end)
        {
            var doc = (Txrd)Txtb;
            var result = new TextAlignmentData
            {
                WhitespaceSize = 18,
                Field0x4 = 256,
                Field0x8 = 256,
            };
            var type = 128;
            start = 0;
            end = doc.Text.Length;

            while (type < 131)
            {
                var found = doc.BinarySearch(type++, charIndex, out var info);
                end = Math.Min(info.End, end);
                if (!found)
                    continue;
                start = Math.Max(info.Start, start);
                switch (info.Type)
                {
                    case 128:
                        result.TextAlignment = (TextAlignment)(info.Value >> 24);
                        result.IndentValue = (byte)(info.Value >> 16);
                        result.WhitespaceSize += (short)info.Value;
                        break;
                    case 129:
                        result.Field0x4 += (short)(info.Value >> 16);
                        result.LineHeight = (short)info.Value;
                        break;
                    case 130:
                        result.Field0x8 += (short)(info.Value >> 16);
                        result.Field0xA = (short)info.Value;
                        break;
                }
            }
            return result;
        }

        const uint TextColorDefault = 0x1000000;
        const uint BackgroundColorNone = 0xffffffff;

        protected override TextFormatData GetFormatData(int charIndex, out int start, out int end)
        {
            var doc = (Txrd)Txtb;
            var result = new TextFormatData
            {
                TextColor = TextColorDefault,
                BackgroundColor = BackgroundColorNone,
                Size = doc.FontSize,
                Font = doc.FontIndex,
            };
            var type = 1;
            start = 0;
            end = doc.Text.Length;

            while (type < 5)
            {
                var found = doc.BinarySearch(type++, charIndex, out var info);
                end = Math.Min(info.End, end);
                if (!found)
                    continue;
                start = Math.Max(info.Start, start);
                switch (info.Type)
                {
                    case 1:
                        result.BaseLineOffset = (sbyte)(info.Value >> 8);
                        result.StyleFlags = (byte)info.Value;
                        result.Size += info.Value >> 16;
                        break;
                    case 2:
                        // TODO: Handle this.
                        break;
                    case 3:
                        if (info.Value != 0)
                            result.TextColor = unchecked((uint)info.Value + TextColorDefault);
                        break;
                    case 4:
                        if (info.Value != 0)
                            result.BackgroundColor = unchecked((uint)info.Value + BackgroundColorNone);
                        break;
                }
            }
            return result;
        }
    }
}