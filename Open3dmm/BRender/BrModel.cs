using System;

namespace Open3dmm.BRender
{
    public unsafe struct BrModel
    {
        public byte* Identifier;
        public BrVertex* Vertices;
        public BrFace* Faces;
        public ushort NumVertices;
        public ushort NumFaces;
        public BrVector3 Pivot;
        public ushort Flags;
        public void* CustomCall;
        public void* User;
        public BrScalar Radius;
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
