using Open3dmm.Core.Actors;
using System;
using System.Numerics;
using Veldrid;

namespace Open3dmm.Core.Veldrid
{
    // Maths borrowed from: https://github.com/MonoGame/MonoGame/blob/develop/MonoGame.Framework/Ray.cs
    public struct Ray
    {
        public Vector3 Origin;
        public Vector3 Direction;

        public Ray(Vector3 origin, Vector3 direction)
        {
            this.Origin = origin;
            this.Direction = direction;
        }

        public float? Intersects(Plane plane)
        {
            Intersects(in plane, out var result);
            return result;
        }

        public void Intersects(in Plane plane, out float? result)
        {
            var den = Vector3.Dot(Direction, plane.Normal);
            if (Math.Abs(den) < 0.00001f)
            {
                result = null;
                return;
            }

            result = (-plane.D - Vector3.Dot(plane.Normal, Origin)) / den;

            if (result < 0.0f)
            {
                if (result < -0.00001f)
                {
                    result = null;
                    return;
                }

                result = 0.0f;
            }
        }

        public float? Intersects(Bounds bounds)
        {
            Intersects(in bounds, out var result);
            return result;
        }

        public void Intersects(in Bounds bounds, out float? result)
        {
            const float Epsilon = 1e-6f;

            float? tMin = null, tMax = null;

            if (Math.Abs(Direction.X) < Epsilon)
            {
                if (Origin.X < bounds.Min.X || Origin.X > bounds.Max.X)
                {
                    result = null;
                    return;
                }
            }
            else
            {
                tMin = (bounds.Min.X - Origin.X) / Direction.X;
                tMax = (bounds.Max.X - Origin.X) / Direction.X;

                if (tMin > tMax)
                {
                    var temp = tMin;
                    tMin = tMax;
                    tMax = temp;
                }
            }

            if (Math.Abs(Direction.Y) < Epsilon)
            {
                if (Origin.Y < bounds.Min.Y || Origin.Y > bounds.Max.Y)
                {
                    result = null;
                    return;
                }
            }
            else
            {
                var tMinY = (bounds.Min.Y - Origin.Y) / Direction.Y;
                var tMaxY = (bounds.Max.Y - Origin.Y) / Direction.Y;

                if (tMinY > tMaxY)
                {
                    var temp = tMinY;
                    tMinY = tMaxY;
                    tMaxY = temp;
                }

                if ((tMin.HasValue && tMin > tMaxY) || (tMax.HasValue && tMinY > tMax))
                {
                    result = null;
                    return;
                }

                if (!tMin.HasValue || tMinY > tMin) tMin = tMinY;
                if (!tMax.HasValue || tMaxY < tMax) tMax = tMaxY;
            }

            if (Math.Abs(Direction.Z) < Epsilon)
            {
                if (Origin.Z < bounds.Min.Z || Origin.Z > bounds.Max.Z)
                {
                    result = null;
                    return;
                }
            }
            else
            {
                var tMinZ = (bounds.Min.Z - Origin.Z) / Direction.Z;
                var tMaxZ = (bounds.Max.Z - Origin.Z) / Direction.Z;

                if (tMinZ > tMaxZ)
                {
                    var temp = tMinZ;
                    tMinZ = tMaxZ;
                    tMaxZ = temp;
                }

                if ((tMin.HasValue && tMin > tMaxZ) || (tMax.HasValue && tMinZ > tMax))
                {
                    result = null;
                    return;
                }

                if (!tMin.HasValue || tMinZ > tMin) tMin = tMinZ;
                if (!tMax.HasValue || tMaxZ < tMax) tMax = tMaxZ;
            }

            // having a positive tMin and a negative tMax means the ray is inside the box
            // we expect the intesection distance to be 0 in that case
            if ((tMin.HasValue && tMin < 0) && tMax > 0)
            {
                result = 0f;
                return;
            }

            // a negative tMin means that the intersection point is behind the ray's origin
            // we discard these as not hitting the AABB
            if (tMin < 0)
            {
                result = null;
                return;
            }

            result = tMin;
        }

        public static Ray CalculateRay(Vector2 mouseLocation, Matrix4x4 view, Matrix4x4 projection, Viewport viewport) => CalculateRay(mouseLocation, view, projection, viewport, Matrix4x4.Identity);

        public static Ray CalculateRay(Vector2 mouseLocation, Matrix4x4 view, Matrix4x4 projection, Viewport viewport, Matrix4x4 world)
        {
            var nearPoint = viewport.Unproject(new Vector3(mouseLocation.X,
                    mouseLocation.Y, 0.0f),
                    projection,
                    view,
                    world);

            var farPoint = viewport.Unproject(new Vector3(mouseLocation.X,
                    mouseLocation.Y, 1.0f),
                    projection,
                    view,
                    world);

            var direction = Vector3.Normalize(farPoint - nearPoint);

            return new Ray(nearPoint, direction);
        }

        public static Ray Transform(Ray ray, Matrix4x4 matrix)
        {
            return new Ray(Vector3.Transform(ray.Origin, matrix), Vector3.Normalize(Vector3.TransformNormal(ray.Direction, matrix)));
        }
    }
}
