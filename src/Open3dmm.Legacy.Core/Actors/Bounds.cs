using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Open3dmm.Core.Actors
{
    public struct Bounds
    {
        public Vector3 Min;
        public Vector3 Max;

        public Bounds(Vector3 min, Vector3 max)
        {
            this.Min = min;
            this.Max = max;
        }

        public static Bounds CalculateBounds(Vector3[] points)
        {
            return CalculateBounds(ref points[0], new IntPtr(Unsafe.SizeOf<Vector3>()), points.Length);
        }

        public static Bounds CalculateBounds(ref Vector3 firstPoint, IntPtr byteOffset, int count)
        {
            if (count <= 0)
                throw new ArgumentException(nameof(count));
            if (byteOffset.ToInt32() < Unsafe.SizeOf<Vector3>())
                throw new ArgumentException(nameof(byteOffset));

            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            while (--count >= 0)
            {
                min = Vector3.Min(min, firstPoint);
                max = Vector3.Max(max, firstPoint);
                firstPoint = ref Unsafe.AddByteOffset(ref firstPoint, byteOffset);
            }
            return new Bounds(min, max);
        }

        public static Bounds Add(Bounds a, Bounds b)
        {
            return new Bounds(Vector3.Min(a.Min, b.Min), Vector3.Max(a.Max, b.Max));
        }

        public static Bounds Transform(Bounds bounds, Matrix4x4 matrix)
        {
            var pts = bounds.GetPoints();
            for (int i = 0; i < pts.Length; i++)
                pts[i] = Vector3.Transform(pts[i], matrix);
            return CalculateBounds(pts);
        }

        public Vector3[] GetPoints()
        {
            var arr = new Vector3[8];
            GetPoints(arr);
            return arr;
        }

        public void GetPoints(Vector3[] dest)
        {
            dest[0] = Min;
            dest[1] = new Vector3(Max.X, Min.Y, Min.Z);
            dest[2] = new Vector3(Max.X, Max.Y, Min.Z);
            dest[3] = new Vector3(Min.X, Max.Y, Min.Z);

            dest[4] = new Vector3(Min.X, Min.Y, Max.Z);
            dest[5] = new Vector3(Max.X, Min.Y, Max.Z);
            dest[6] = Max;
            dest[7] = new Vector3(Min.X, Max.Y, Max.Z);
        }

        public static Bounds Inflate(Bounds bounds, float value)
        {
            var unit = Vector3.One * value;
            return new Bounds(bounds.Min - unit, bounds.Max + unit);
        }
    }
}
