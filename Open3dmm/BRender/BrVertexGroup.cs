using System.Runtime.InteropServices;

namespace Open3dmm.BRender
{
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
    public unsafe struct BrVertexGroup
    {
        /* BrMaterial */
        public BrMaterial* Material;
        /* BrFace */
        public BrVertex* Vertices;
        internal ushort NumFaces;
    }
}