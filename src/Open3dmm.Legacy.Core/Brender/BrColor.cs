using System.Numerics;

namespace Open3dmm.Core.Brender
{
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct BrColor
    {
        internal byte b;
        internal byte g;
        internal byte r;
        internal byte a;

        public BrColor(byte r, byte g, byte b) : this(r, g, b, 0xFF)
        {
        }

        public BrColor(byte r, byte g, byte b, byte a)
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

        public Fixed ScR {
            get => (Fixed)(r / 255f);
            set => r = (byte)(value * 255f);
        }

        public Fixed ScG {
            get => (Fixed)(g / 255f);
            set => g = (byte)(value * 255f);
        }

        public Fixed ScB {
            get => (Fixed)(b / 255f);
            set => b = (byte)(value * 255f);
        }

        public Fixed ScA {
            get => (Fixed)(a / 255f);
            set => a = (byte)(value * 255f);
        }

        public static explicit operator Vector3(BrColor c)
        {
            return new Vector3(c.R / 255f, c.G / 255f, c.B / 255f);
        }

        public static explicit operator Vector4(BrColor c)
        {
            return new Vector4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        }
    }
}
