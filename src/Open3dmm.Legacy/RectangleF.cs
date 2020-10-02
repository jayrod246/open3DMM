using System;
using Veldrid;

namespace Open3dmm
{
    public struct RectangleF
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;

        public RectangleF(float x, float y, float width, float height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public readonly float Left => X;
        public readonly float Top => Y;
        public readonly float Right => X + Width;
        public readonly float Bottom => Y + Height;

        public readonly bool Contains(in RectangleF other)
        {
            return Left <= other.Left && Top <= other.Top && Bottom >= other.Bottom && Right >= other.Right;
        }

        public readonly bool TryGetIntersection(in RectangleF other, out RectangleF result)
        {
            result = this;
            return result.Intersect(in other);
        }

        public bool Intersect(in RectangleF other)
        {
            float bottom;
            float top;
            float left;
            float right;

            left = Left;
            if (Left <= other.Left)
            {
                left = other.Left;
            }
            right = other.Right;
            if (Right <= other.Right)
            {
                right = Right;
            }
            top = other.Top;
            if (other.Top <= Top)
            {
                top = Top;
            }
            bottom = other.Bottom;
            if (Bottom <= other.Bottom)
            {
                bottom = Bottom;
            }
            if ((top < bottom) && (left < right))
            {
                X = left;
                Y = top;
                Width = right - left;
                Height = bottom - top;
                return true;
            }
            this = default;
            return false;
        }

        public void CenterWith(in RectangleF other)
        {
            X = other.X + other.Width / 2f;
            Y = other.Y + other.Height / 2f;
            X -= Width / 2f;
            Y -= Height / 2f;
        }

        public void KeepInsideOf(in RectangleF other)
        {
            float tmp;
            float offsetX = other.Right - Right;
            if (offsetX >= 0f)
            {
                offsetX = 0f;
            }
            tmp = other.Left - Left;
            if (offsetX <= tmp)
            {
                offsetX = tmp;
            }
            float offsetY = other.Bottom - Bottom;
            if (offsetY >= 0f)
            {
                offsetY = 0f;
            }
            tmp = other.Top - Top;
            if (offsetY <= tmp)
            {
                offsetY = tmp;
            }
            if (offsetX != 0f || offsetY != 0f)
            {
                X += offsetX;
                Y += offsetY;
            }
        }

        public void Offset(float x, float y)
        {
            X += x;
            Y += y;
        }

        public static implicit operator RectangleF(Rectangle rc) => new RectangleF(rc.X, rc.Y, rc.Width, rc.Height);
        public static implicit operator RectangleF(System.Drawing.Rectangle rc) => new RectangleF(rc.X, rc.Y, rc.Width, rc.Height);

        public static explicit operator RectangleF(LTRB v) => new(v.Left, v.Top, v.Width, v.Height);
    }
}
