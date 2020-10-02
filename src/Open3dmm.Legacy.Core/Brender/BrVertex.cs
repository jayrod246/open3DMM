using System;
using System.Runtime.InteropServices;

namespace Open3dmm.Core.Brender {
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 32)]
    public partial struct BrVertex {
        public FixedVector3 Position;
        public FixedVector2 TextureCoordinate;
        public Byte PaletteIndex;
        public Byte Red;
        public Byte Green;
        public Byte Blue;
        public UInt16 Unk;
        public BrFVector3 Normal;

        public BrVertex(Fixed x, Fixed y, Fixed z, Fixed u, Fixed v) : this() {
            Position = new FixedVector3(x, y, z);
            TextureCoordinate = new FixedVector2(u, v);
        }

        public override string ToString() {
            return Position.ToString();
        }

        public static implicit operator FixedVector3(BrVertex vertex) {
            return vertex.Position;
        }
    }
}
