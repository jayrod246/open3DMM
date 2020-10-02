using System;

namespace Open3dmm.Core.Brender
{
    public unsafe struct BrModel
    {
        public byte* Identifier;
        public BrVertex* Vertices;
        public BrFace* Faces;
        public ushort NumVertices;
        public ushort NumFaces;
        public FixedVector3 Pivot;
        public ushort Flags;
        public void* CustomCall;
        public void* User;
        public Fixed Radius;
        public BrBounds Bounds;
        public ushort NumPreparedVertices;
        public ushort NumPreparedFaces;
        public BrFace* PreparedFaces;
        public BrVertex* PreparedVertices;
        public ushort NumFaceGroups;
        public ushort NumVertexGroups;
        public BrFaceGroup* FaceGroups;
        public BrVertexGroup* VertexGroups;
    }
}
