using Open3dmm.Core.Actors;
using System;
using System.Numerics;
using Veldrid;

namespace Open3dmm.Core.Veldrid
{
    public class BmdlModel
    {
        private DeviceBuffer vertexBuffer;
        private DeviceBuffer indexBuffer;

        public VertexPositionNormalTexture[] Vertices { get; set; }
        public ushort[] Indices { get; set; }
        public Bounds Bounds { get; set; }
        public Bmdl Bmdl { get; }

        public BmdlModel(Bmdl bmdl)
        {
            Bmdl = bmdl;
            Vertices = new VertexPositionNormalTexture[bmdl.Vertices.Length];
            Indices = new ushort[bmdl.Faces.Length * 3];

            for (int i = 0; i < bmdl.Faces.Length; i++)
            {
                ref readonly var src = ref bmdl.Faces[i];
                Indices[i * 3] = src.A;
                Indices[i * 3 + 1] = src.B;
                Indices[i * 3 + 2] = src.C;

                var u = (Vector3)bmdl.Vertices[src.B].Position - bmdl.Vertices[src.A].Position;
                var v = (Vector3)bmdl.Vertices[src.B].Position - bmdl.Vertices[src.C].Position;
                var n = Vector3.Cross(u, v);
                Vertices[src.A].Normal += n;
                Vertices[src.B].Normal += n;
                Vertices[src.C].Normal += n;
            }

            for (int i = 0; i < bmdl.Vertices.Length; i++)
            {
                ref readonly var src = ref bmdl.Vertices[i];
                ref var dest = ref Vertices[i];
                dest.Position = src.Position;
                dest.Texture = src.TextureCoordinate;
                // Normalize normals
                dest.Normal = Vector3.Normalize(dest.Normal);
            }

            if (Vertices.Length > 0)
                Bounds = Bounds.CalculateBounds(ref Vertices[0].Position, new IntPtr(VertexPositionNormalTexture.SizeInBytes), Vertices.Length);
            else
                Bounds = default;
        }

        public DeviceBuffer GetOrCreateVertexBuffer(GraphicsDevice graphicsDevice)
        {
            if (vertexBuffer == null && Vertices.Length > 0)
            {
                var vbDescription = new BufferDescription(
                (uint)Vertices.Length * VertexPositionNormalTexture.SizeInBytes,
                BufferUsage.VertexBuffer);
                vertexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(vbDescription);
                graphicsDevice.UpdateBuffer(vertexBuffer, 0, Vertices);
            }
            return vertexBuffer;
        }

        public DeviceBuffer GetOrCreateIndexBuffer(GraphicsDevice graphicsDevice)
        {
            if (indexBuffer == null && Indices.Length > 0)
            {
                var ibDescription = new BufferDescription(
                (uint)Indices.Length * sizeof(ushort),
                BufferUsage.IndexBuffer);
                indexBuffer = graphicsDevice.ResourceFactory.CreateBuffer(ibDescription);
                graphicsDevice.UpdateBuffer(indexBuffer, 0, Indices);
            }
            return indexBuffer;
        }

        public void Remap(GraphicsDevice graphicsDevice, BmdlModel other)
        {
            if (other != this && Indices != other.Indices && Indices.Length == other.Indices.Length && Vertices.Length == other.Vertices.Length)
            {
                Console.WriteLine("Created new vertex array");
                var verts = new VertexPositionNormalTexture[Vertices.Length];
                for (int i = 0; i < Indices.Length; i++)
                {
                    verts[other.Indices[i]] = Vertices[Indices[i]];
                }

                Vertices = verts;

                if (vertexBuffer != null)
                    graphicsDevice.UpdateBuffer(vertexBuffer, 0, Vertices);

                Indices = other.Indices;
                if (indexBuffer != null)
                    graphicsDevice.UpdateBuffer(indexBuffer, 0, Indices);
            }
        }
    }
}
