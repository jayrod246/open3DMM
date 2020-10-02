using Open3dmm.Core.Actors;
using Open3dmm.Core.Veldrid;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Veldrid;

namespace Open3dmm.Core
{
    public static class VeldridExtensions
    {
        public static Bounds CalculateBounds(this Body body, Matrix4x4 world)
        {
            var boundsCalc = new BoundsCalculator(world);
            body.AcceptVisitor(boundsCalc);
            return boundsCalc.Result;
        }

        #region BMDL Extensions
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BmdlModel GetVeldridModel(Bmdl bmdl)
        {
            bmdl.Tag ??= new BmdlModel(bmdl);
            return (BmdlModel)bmdl.Tag;
        }

        public static DeviceBuffer GetOrCreateVertexBuffer(this Bmdl bmdl, GraphicsDevice graphicsDevice)
            => GetVeldridModel(bmdl).GetOrCreateVertexBuffer(graphicsDevice);

        public static DeviceBuffer GetOrCreateIndexBuffer(this Bmdl bmdl, GraphicsDevice graphicsDevice)
            => GetVeldridModel(bmdl).GetOrCreateIndexBuffer(graphicsDevice);

        public static void Remap(this Bmdl bmdl, GraphicsDevice graphicsDevice, Bmdl other)
            => GetVeldridModel(bmdl).Remap(graphicsDevice, GetVeldridModel(other));

        public static Bounds GetBounds(this Bmdl bmdl)
            => GetVeldridModel(bmdl).Bounds;

        public static int GetIndexCount(this Bmdl bmdl)
            => GetVeldridModel(bmdl).Indices.Length;
        #endregion

        #region MTRL Extensions
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static MtrlMaterial GetVeldridMaterial(Mtrl mtrl)
        {
            mtrl.Tag ??= new MtrlMaterial(mtrl);
            return (MtrlMaterial)mtrl.Tag;
        }

        public static void Load(this Mtrl mtrl, GraphicsDevice graphicsDevice, CommandList commandList, Texture palette, CustomRenderer renderer)
            => GetVeldridMaterial(mtrl).Load(graphicsDevice, commandList, palette, renderer);
        #endregion

        public static Vector3 Project(this Viewport vp, Vector3 source, Matrix4x4 projection, Matrix4x4 view, Matrix4x4 world)
        {
            var matrix = Matrix4x4.Multiply(Matrix4x4.Multiply(world, view), projection);
            var vector = Vector3.Transform(source, matrix);
            float a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
            if (!WithinEpsilon(a, 1f))
            {
                vector.X /= a;
                vector.Y /= a;
                vector.Z /= a;
            }
            vector.X = (((vector.X + 1f) * 0.5f) * vp.Width) + vp.X;
            vector.Y = (((-vector.Y + 1f) * 0.5f) * vp.Height) + vp.Y;
            vector.Z = (vector.Z * (vp.MaxDepth - vp.MinDepth)) + vp.MinDepth;
            return vector;
        }

        public static Vector3 Unproject(this Viewport vp, Vector3 source, Matrix4x4 projection, Matrix4x4 view, Matrix4x4 world)
        {
            if (!Matrix4x4.Invert(Matrix4x4.Multiply(Matrix4x4.Multiply(world, view), projection), out var matrix))
                throw new InvalidOperationException();
            source.X = (((source.X - vp.X) / vp.Width) * 2f) - 1f;
            source.Y = -((((source.Y - vp.Y) / vp.Height) * 2f) - 1f);
            source.Z = (source.Z - vp.MinDepth) / (vp.MaxDepth - vp.MinDepth);
            var vector = Vector3.Transform(source, matrix);
            float a = (((source.X * matrix.M14) + (source.Y * matrix.M24)) + (source.Z * matrix.M34)) + matrix.M44;
            if (!WithinEpsilon(a, 1f))
            {
                vector.X /= a;
                vector.Y /= a;
                vector.Z /= a;
            }
            return vector;

        }

        private static bool WithinEpsilon(float a, float b)
        {
            float num = a - b;
            return ((-1.401298E-45f <= num) && (num <= float.Epsilon));
        }
    }
}
