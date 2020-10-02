using Open3dmm;
using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using System.Numerics;

namespace Open3dmm.Core.Actors
{
    public class Mtrl : ResolvableObject
    {
        public Vector2 PaletteSlice { get; set; }
        public Txxf Transform => ResolveReferenceOrDefault(new ReferenceIdentifier(0, Tags.TXXF), new Txxf());
        public Tmap Tmap => ResolveReferenceOrDefault<Tmap>(new ReferenceIdentifier(0, Tags.TMAP));
        public object Tag { get; set; }

        protected override void ResolveCore()
        {
            using var block = Metadata.GetBlock();
            if (!block.MagicNumber())
                throw ThrowHelper.BadMagicNumber(block.Read<int>());
            block.Skip(10);
            PaletteSlice = new Vector2(block.Read<byte>(), block.Read<byte>() + 1);
        }
    }
}
