using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Open3dmm
{
    public static class RectangleExtensions
    {
        public static bool Contains(this Rectangle rc, in Rectangle other)
        {
            return rc.Intersect(in other);
        }

        public static bool TryGetIntersection(this in Rectangle a, in Rectangle b, out Rectangle result)
        {
            result = a;
            return result.Intersect(in b);
        }

        public static bool Intersect(ref this Rectangle rc, in Rectangle other)
        {
            int bottom;
            int top;
            int left;
            int right;

            left = rc.Left;
            if (rc.Left <= other.Left)
            {
                left = other.Left;
            }
            right = other.Right;
            if (rc.Right <= other.Right)
            {
                right = rc.Right;
            }
            top = other.Top;
            if (other.Top <= rc.Top)
            {
                top = rc.Top;
            }
            bottom = other.Bottom;
            if (rc.Bottom <= other.Bottom)
            {
                bottom = rc.Bottom;
            }
            if ((top < bottom) && (left < right))
            {
                rc.X = left;
                rc.Y = top;
                rc.Width = right - left;
                rc.Height = bottom - top;
                return true;
            }
            rc = default;
            return false;
        }

        public static void CenterWith(ref this Rectangle rc, in Rectangle other)
        {
            rc.X = (other.Left + other.Right - rc.Width) / 2;
            rc.Y = (other.Top + other.Bottom - rc.Height) / 2;
        }

        public static void KeepInsideOf(ref this Rectangle rc, in Rectangle other)
        {
            int tmp;
            int offsetX = other.Right - rc.Right;
            if (-1 < offsetX)
            {
                offsetX = 0;
            }
            tmp = other.Left - rc.Left;
            if (offsetX <= tmp)
            {
                offsetX = tmp;
            }
            int offsetY = other.Bottom - rc.Bottom;
            if (-1 < offsetY)
            {
                offsetY = 0;
            }
            tmp = other.Top - rc.Top;
            if (offsetY <= tmp)
            {
                offsetY = tmp;
            }
            if ((offsetX != 0) || (offsetY != 0))
            {
                rc.X += offsetX;
                rc.Y += offsetY;
            }
        }

        public static void Offset(ref this Rectangle rc, int x, int y)
        {
            rc = new Rectangle(rc.X + x, rc.Y + y, rc.Width, rc.Height);
        }
    }
}
