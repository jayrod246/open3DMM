using System;
using System.Runtime.InteropServices;

namespace Open3dmm.BRender {
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 32)]
    public partial struct BrVertex {
        public BrVector3 Position;
        public BrVector2 TextureCoordinate;
        public Byte PaletteIndex;
        public Byte Red;
        public Byte Green;
        public Byte Blue;
        public UInt16 Unk;
        public BrFVector3 Normal;

        public BrVertex(BrScalar x, BrScalar y, BrScalar z, BrScalar u, BrScalar v) : this() {
            Position = new BrVector3(x, y, z);
            TextureCoordinate = new BrVector2(u, v);
        }

        public override string ToString() {
            return Position.ToString();
        }

        public static implicit operator BrVector3(BrVertex vertex) {
            return vertex.Position;
        }
    }
}
