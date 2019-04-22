using System;
using System.Runtime.InteropServices;

namespace Open3dmm.Classes
{
    public partial struct RECTANGLE
    {
        public int X1;
        public int Y1;
        public int X2;
        public int Y2;

        public POINT TopLeft {
            get => new POINT(X1, Y1);
        }

        public POINT TopRight {
            get => new POINT(X2, Y1);
        }

        public POINT BottomLeft {
            get => new POINT(X1, Y2);
        }

        public POINT BottomRight {
            get => new POINT(X2, Y2);
        }

        public int Width => Math.Abs(X2 - X1);

        public int Height => Math.Abs(Y2 - Y1);

        public static readonly RECTANGLE Empty = default;

        public RECTANGLE(int x1, int y1, int x2, int y2)
        {
            this.X1 = x1;
            this.Y1 = y1;
            this.X2 = x2;
            this.Y2 = y2;
        }

        public static int MaxExtent => Marshal.ReadInt32(new IntPtr(0x4D5328));

        partial void DetectRectangles();

#if false
        partial void DetectRectangles()
        {
            unsafe
            {
                fixed (void* p = &this)
                {
                    StructDetector.Invoke(this, new IntPtr(p));
                }
            }
        }
#endif

        [HookFunction(FunctionNames.Rectangle_SizeLimit, CallingConvention = CallingConvention.ThisCall)]
        public ref RECTANGLE SizeLimit(ref RECTANGLE dest)
        {
            dest.X1 = Math.Max(Math.Min(32767, X1), -MaxExtent);
            dest.Y1 = Math.Max(Math.Min(32767, Y1), -MaxExtent);
            dest.X2 = Math.Max(Math.Min(32767, X2), -MaxExtent);
            dest.Y2 = Math.Max(Math.Min(32767, Y2), -MaxExtent);
            return ref dest;
        }

        [HookFunction(FunctionNames.Rectangle_CalculateIntersection, CallingConvention = CallingConvention.ThisCall)]
        public bool CalculateIntersection(in RECTANGLE other)
        {
            return CalculateIntersection(in this, in other);
        }

        [HookFunction(FunctionNames.Rectangle_CalculateIntersectionBetween, CallingConvention = CallingConvention.ThisCall)]
        public bool CalculateIntersection(in RECTANGLE a, in RECTANGLE b)
        {
            DetectRectangles();
            a.DetectRectangles();
            b.DetectRectangles();
            X1 = Math.Max(a.X1, b.X1);
            X2 = Math.Min(a.X2, b.X2);
            Y1 = Math.Max(a.Y1, b.Y1);
            Y2 = Math.Min(a.Y2, b.Y2);

            if (X2 >= X1
                && Y2 >= Y1)
            {
                return true;
            }
            this = Empty;
            return false;
        }

        [HookFunction(FunctionNames.Rectangle_Copy, CallingConvention = CallingConvention.ThisCall)]
        public void Copy(in RECTANGLE source)
        {
            DetectRectangles();
            source.DetectRectangles();
            this = source;
        }

        [HookFunction(FunctionNames.Rectangle_CopyAtOffset, CallingConvention = CallingConvention.ThisCall)]
        public void Copy(in RECTANGLE source, int offsetX, int offsetY)
        {
            DetectRectangles();
            source.DetectRectangles();
            Copy(in source);
            Translate(offsetX, offsetY);
        }

        [HookFunction(FunctionNames.Rectangle_HitTest, CallingConvention = CallingConvention.ThisCall)]
        public bool HitTest(int x, int y)
        {
            DetectRectangles();
            if (X1 > x || X2 <= x)
                return false;
            if (Y1 > y || Y2 <= y)
                return false;
            return true;
        }

        [HookFunction(FunctionNames.Rectangle_Union, CallingConvention = CallingConvention.ThisCall)]
        public void Union(in RECTANGLE other)
        {
            DetectRectangles();
            other.DetectRectangles();
            if (other.X1 < other.X2 && other.Y1 < other.Y2)
            {
                if (X1 < X2 && Y1 < Y2)
                {
                    X1 = Math.Min(X1, other.X1);
                    Y1 = Math.Min(Y1, other.Y1);
                    X2 = Math.Max(X2, other.X2);
                    Y2 = Math.Max(Y2, other.Y2);
                }
                else
                {
                    X1 = other.X1;
                    Y1 = other.Y1;
                    X2 = other.X2;
                    Y2 = other.Y2;
                }
            }
        }

        [HookFunction(FunctionNames.Rectangle_TopLeftOrigin, CallingConvention = CallingConvention.ThisCall)]
        public void TopLeftOrigin()
        {
            DetectRectangles();
            X2 -= X1;
            Y2 -= Y1;
            X1 = Y1 = 0;
        }

        [HookFunction(FunctionNames.Rectangle_Translate, CallingConvention = CallingConvention.ThisCall)]
        public void Translate(int distanceX, int distanceY)
        {
            DetectRectangles();
            X1 += distanceX;
            X2 += distanceX;
            Y1 += distanceY;
            Y2 += distanceY;
        }
    }
}
