using Open3dmm;
using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;

namespace Open3dmm.Core.Actors
{
    public class CostumeInfo : ResolvableObject
    {
        public int BodySet { get; set; }

        protected override void ResolveCore()
        {
            using var block = Metadata.GetBlock();
            if (!block.MagicNumber())
                throw ThrowHelper.BadMagicNumber(block.Read<int>());
            BodySet = block.Read<int>();
        }

        public bool TryGetMaterial(int number, out Mtrl material)
            => TryResolveReference(new ReferenceIdentifier(number, Tags.MTRL), out material);

        public bool TryGetModel(int number, out Bmdl model)
            => TryResolveReference(new ReferenceIdentifier(number, Tags.BMDL), out model);
    }
}
