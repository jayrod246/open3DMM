using System.Numerics;

namespace Open3dmm.Core.Veldrid
{
    public struct VertexPositionTexture
    {
        public const uint SizeInBytes = 20;
        public Vector3 Position;
        public Vector2 Texture;

        public VertexPositionTexture(Vector3 position, Vector2 texture)
        {
            this.Position = position;
            this.Texture = texture;
        }
    }
}
