using System.Numerics;

namespace Open3dmm.Core.Veldrid
{
    public struct VertexPositionNormalTexture
    {
        public const uint SizeInBytes = 32;
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 Texture;

        public VertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 texture)
        {
            this.Position = position;
            this.Normal = normal;
            this.Texture = texture;
        }
    }
}
