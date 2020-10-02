using Open3dmm.Core.Brender;
using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using System;

namespace Open3dmm.Core.Actors
{
    public class Bmdl : ResolvableObject
    {
        public BrVertex[] Vertices { get; set; }
        public BrFace[] Faces { get; set; }
        public BrBounds Bounds { get; set; }
        public object Tag { get; set; }

        protected override void ResolveCore()
        {
            using var block = Metadata.GetBlock();
            if (!block.MagicNumber() || !block.TryRead(out ushort nVerts) || !block.TryRead(out ushort nFaces))
                throw ThrowHelper.BadSection(Metadata.Key);

            if (nVerts == 0)
            {
                Vertices = Array.Empty<BrVertex>();
                Faces = Array.Empty<BrFace>();
                Bounds = default;
            }
            else
            {
                Vertices = new BrVertex[nVerts];
                Faces = new BrFace[nFaces];
                Bounds = block.Read<BrBounds>();
                var radius = block.Read<Fixed>();
                var pivot = block.Read<FixedVector3>();
                block.ReadTo(Vertices.AsSpan());
                block.ReadTo(Faces.AsSpan());
            }
        }
    }
}
