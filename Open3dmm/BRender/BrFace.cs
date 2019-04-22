using System;
using System.Runtime.InteropServices;

namespace Open3dmm.BRender
{
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 32)]
    public unsafe struct BrFace
    {
        // Vertices
        public ushort A;
        public ushort B;
        public ushort C;

        // Edges
        public ushort EdgeA;
        public ushort EdgeB;
        public ushort EdgeC;

        /* BrMaterial */
        public BrMaterial* Material;
        public ushort Smoothing;
        public BrFaceFlags Flags;
        internal Byte _pad0;
        public BrFVector3 Normal;
        public BrScalar D;

        public BrFace(ushort a, ushort b, ushort c) : this()
        {
            A = a;
            B = b;
            C = c;
        }

        public override string ToString()
        {
            return $"{A}, {B}, {C}";
        }
    }
}
