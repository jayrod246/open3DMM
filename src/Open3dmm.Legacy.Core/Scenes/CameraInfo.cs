using Open3dmm;
using Open3dmm.Core.Brender;
using Open3dmm.Core.Graphics;
using Open3dmm.Core.IO;
using Open3dmm.Core.Resolvers;
using System;
using System.Numerics;

namespace Open3dmm.Core.Scenes
{
    public class CameraInfo : ResolvableObject
    {
        private Matrix4x4 viewInverse;
        private Matrix4x4 viewMatrix;

        public float NearClipPlane { get; set; }
        public float FarClipPlane { get; set; }
        public float FieldOfView { get; set; }
        public short Unk5 { get; set; }
        public Matrix4x4 ViewMatrix {
            get => viewMatrix;
            set {
                viewMatrix = value;
                Matrix4x4.Invert(ViewMatrix, out viewInverse);
            }
        }
        public Matrix4x4 ViewInverse => viewInverse;
        public Vector3[] SpawnPoints { get; set; }
        public Mbmp Mbmp => ResolveReference<Mbmp>(new ReferenceIdentifier(0, Tags.MBMP));
        public Zbmp Zbmp => ResolveReference<Zbmp>(new ReferenceIdentifier(0, Tags.ZBMP));

        protected override void ResolveCore()
        {
            using var block = Metadata.GetBlock();
            if (!block.MagicNumber() || block.Length < 76 || (block.Length - 76) % 12 != 0)
                throw ThrowHelper.BadSection(Metadata.Key);
            int spawnCount = checked((int)(1 + (block.Length - 76) / 12));
            Span<Vector3> spawnPoints = stackalloc Vector3[spawnCount];
            NearClipPlane = block.Read<Fixed>();
            FarClipPlane = block.Read<Fixed>();
            FieldOfView = block.Read<BrAngle>().ToRadians();
            Unk5 = block.Read<short>();
            spawnPoints[0] = block.Read<FixedVector3>();
            ViewMatrix = block.Read<FixedMatrix4x3>();

            for (int i = 1; i < spawnCount; i++)
                spawnPoints[i] = block.Read<FixedVector3>();

            SpawnPoints = spawnPoints.ToArray();
        }
    }
}
