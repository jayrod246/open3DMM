using System.Numerics;

namespace Open3dmm.Core
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct ColorBgra
    {
        internal byte b;
        internal byte g;
        internal byte r;
        internal byte a;

        public ColorBgra(byte r, byte g, byte b) : this(r, g, b, 0xFF)
        {
        }

        public ColorBgra(byte r, byte g, byte b, byte a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public int ToRgba()
        {
            return r | g << 8 | b << 16 | a << 24;
        }

        public int ToBgra()
        {
            return b | g << 8 | r << 16 | a << 24;
        }

        public Vector4 ToVector()
        {
            return new Vector4((float)b / byte.MaxValue, (float)g / byte.MaxValue, (float)r / byte.MaxValue, (float)a / byte.MaxValue);
        }

        public byte R {
            get => r;
            set => r = value;
        }

        public byte G {
            get => g;
            set => g = value;
        }

        public byte B {
            get => b;
            set => b = value;
        }

        public byte A {
            get => a;
            set => a = value;
        }
    }
}
