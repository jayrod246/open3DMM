namespace Open3dmm.BRender {
    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct BrColor {
        internal byte b;
        internal byte g;
        internal byte r;
        internal byte a;

        public BrColor(byte r, byte g, byte b) : this(r, g, b, 0xFF) {
        }

        public BrColor(byte r, byte g, byte b, byte a) {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public int ToRGBA() {
            return r | g << 8 | b << 16 | a << 24;
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

        public BrScalar ScR {
            get => BrScalar.FromFloat(r / 255f);
            set => r = (byte)(value.Clamped(0f, 1f) * 255f);
        }

        public BrScalar ScG {
            get => BrScalar.FromFloat(g / 255f);
            set => g = (byte)(value.Clamped(0f, 1f) * 255f);
        }

        public BrScalar ScB {
            get => BrScalar.FromFloat(b / 255f);
            set => b = (byte)(value.Clamped(0f, 1f) * 255f);
        }

        public BrScalar ScA {
            get => BrScalar.FromFloat(a / 255f);
            set => a = (byte)(value.Clamped(0f, 1f) * 255f);
        }

        public static explicit operator BrVector3(BrColor c) {
            return new BrVector3(c.R / 255f, c.G / 255f, c.B / 255f);
        }

        public static explicit operator BrVector4(BrColor c) {
            return new BrVector4(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        }
    }
}
