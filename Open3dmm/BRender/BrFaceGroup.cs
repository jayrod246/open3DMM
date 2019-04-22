using System;
using System.Runtime.InteropServices;

namespace Open3dmm.BRender
{
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
    public unsafe struct BrFaceGroup
    {
        /* BrMaterial */
        public BrMaterial* Material;
        /* BrFace */
        public BrFace* Faces;
        internal ushort NumFaces;
    }
}
