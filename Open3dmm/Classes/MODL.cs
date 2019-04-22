using Microsoft.Xna.Framework.Graphics;
using Open3dmm.BRender;

namespace Open3dmm.Classes
{
    using static NativeAbstraction;
    public class MODL : BACO
    {
        IndexBuffer indexBuffer;
        VertexBuffer vertexBuffer;

        public unsafe void Render(BrModel* model)
        {
            EnsureCreated(model);
            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;
            if (indexBuffer != null)
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indexBuffer.IndexCount / 3);
        }

        private unsafe void EnsureCreated(BrModel* model)
        {
            if (model->NumPreparedFaces == 0 || model->NumPreparedVertices == 0 || indexBuffer != null || vertexBuffer != null)
                return;
            vertexBuffer = CreateVertexBuffer(model);
            indexBuffer = CreateIndexBuffer(model);
        }

        private unsafe IndexBuffer CreateIndexBuffer(BrModel* model)
        {
            var indices = new ushort[model->NumPreparedFaces * 3];
            for (int f = 0, i = 0; i < indices.Length; f++, i += 3)
            {
                var face = model->PreparedFaces[f];
                indices[i] = face.A;
                indices[i + 1] = face.B;
                indices[i + 2] = face.C;
            }
            var indexBuffer = new IndexBuffer(GraphicsDevice, typeof(ushort), indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
            return indexBuffer;
        }

        private unsafe VertexBuffer CreateVertexBuffer(BrModel* model)
        {
            var verts = new VertexPositionNormalTexture[model->NumPreparedVertices];
            for (int i = 0; i < verts.Length; i++)
            {
                var v = model->PreparedVertices[i];
                verts[i] = new VertexPositionNormalTexture(Helper.XNAVector3FromBrVector3(&v.Position), Helper.XNAVector3FromBrFVector3(&v.Normal), Helper.XNAVector2FromBrVector2(&v.TextureCoordinate));
            }
            var vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionNormalTexture), verts.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(verts);
            return vertexBuffer;
        }
    }
}
