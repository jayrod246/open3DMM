using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Open3dmm
{
    public class TextGob : Gob
    {
        public string Text { get; set; }
        public int ColorMaybe { get; set; }
        public int Field_0x17c { get; }
        public string TextFont { get; }
        public int Field_0x184 { get; set; }
        public int Field_0x188 { get; }
        public int TextSize { get; set; }

        public TextGob(GobOptions options) : base(options)
        {
            Text = string.Empty;
            Field_0x184 = 0x1000000;
            Field_0x188 = -1;
            ColorMaybe = 0;
            Field_0x17c = 1;
            TextFont = Application.Current.GetTextFont();
            TextSize = Application.Current.GetTextSize();
        }

        public override void Draw(CommandList commandList, in RectangleF dest)
        {
            if (Text != null)
            {
                var pos = (-ImGui.CalcTextSize(Text) / 2f + new Vector2(dest.Width, dest.Height) / 2f);
                ImGui.GetBackgroundDrawList().AddText(new Vector2(dest.X + pos.X, dest.Y + pos.Y), 0xff000000, Text);
            }
        }
    }
}
