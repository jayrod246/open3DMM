using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using System.Numerics;

namespace Open3dmm.Core.Actors
{
    public class Txxf : ResolvableObject
    {
        public Txxf() : this(Matrix4x4.Identity)
        {
        }

        public Txxf(Matrix4x4 matrix)
        {
            Matrix = matrix;
        }

        public Matrix4x4 Matrix { get; set; }

        protected override void ResolveCore()
        {
            using var block = Metadata.GetBlock();
            if (!block.MagicNumber())
                throw ThrowHelper.BadMagicNumber(block.Read<int>());
            Matrix = block.Read<FixedMatrix3x2>();
        }
    }
}
