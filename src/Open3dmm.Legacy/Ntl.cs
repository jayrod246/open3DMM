using System;
using System.Collections.Generic;

namespace Open3dmm
{
    public static class Ntl
    {
        static List<string> fonts = new List<string>();

        public static int Default => LoadFont("Comic Sans MS");

        public static int LoadFont(string fontName)
        {
            fontName = fontName.ToLowerInvariant();
            int index = fonts.BinarySearch(fontName);
            if (index < 0)
            {
                index = ~index;
                fonts.Insert(index, fontName);
            }
            return index;
        }

        public static int GetFont(string fontName)
        {
            fontName = fontName.ToLowerInvariant();
            int index = fonts.BinarySearch(fontName);
            if (index < 0)
                return Default;
            return index;
        }
    }
}
