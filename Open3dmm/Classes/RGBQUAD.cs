using Microsoft.Xna.Framework;

namespace Open3dmm.Classes
{
    public struct RGBQUAD
    {
        public byte B;
        public byte G;
        public byte R;
        public byte A;

        public Color ToXNA()
        {
            return new Color(R, G, B, A);
        }
    }
}
