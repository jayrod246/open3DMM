using System;
using System.Numerics;

namespace Open3dmm
{
    public struct PT : IEquatable<PT>
    {
        public PT(int x, int y)
        {
            (X, Y) = (x, y);
        }

        public int X { get; set; }
        public int Y { get; set; }

        public void Deconstruct(out int x, out int y)
        {
            (x, y) = (X, Y);
        }

        public override bool Equals(object obj)
        {
            return obj is PT pT && Equals(pT);
        }

        public bool Equals(PT other)
        {
            return (X, Y) == (other.X, other.Y);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public static bool operator ==(PT left, PT right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PT left, PT right)
        {
            return !(left == right);
        }

        public static PT operator +(PT left, PT right)
        {
            return new(left.X + right.X, left.Y + right.Y);
        }

        public static PT operator -(PT left, PT right)
        {
            return new(left.X - right.X, left.Y - right.Y);
        }

        public static implicit operator Vector2(PT pt) => new(pt.X, pt.Y);

        public static explicit operator PT(Vector2 v) => new((int)MathF.Round(v.X), (int)MathF.Round(v.Y));
    }

    public struct LTRB : IEquatable<LTRB>
    {
        public LTRB(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Width => Right - Left;
        public int Height => Bottom - Top;

        public int Left { get; set; }
        public int Top { get; set; }
        public int Right { get; set; }
        public int Bottom { get; set; }

        public void Offset(int x, int y)
        {
            if (x != 0)
            {
                Left += x;
                Right += x;
            }

            if (y != 0)
            {
                Top += y;
                Bottom += y;
            }
        }

        public void CenterWith(LTRB other)
        {
            var (x, y) = (Width, Height);
            Left = (other.Left + other.Right - x) / 2;
            Top = (other.Top + other.Bottom - y) / 2;
            Right = Left + x;
            Bottom = Top + y;
        }

        public void KeepInsideOf(LTRB other)
        {
            int x, y;
            x = Math.Max(Math.Min(other.Right - Right, 0), other.Left - Left);
            y = Math.Max(Math.Min(other.Bottom - Bottom, 0), other.Top - Top);

            if (x != 0 || y != 0)
            {
                var (w, h) = (Width, Height);
                Left += x;
                Top += y;
                Right = Left + w;
                Bottom = Top + h;
            }
        }

        public readonly bool IsValid()
        {
            return Top < Bottom && Left < Right;
        }

        public readonly bool IsValidIntersection(LTRB other)
        {
            if (Top < Bottom && Left < Right)
                return this != other;
            return other.Top < other.Bottom && other.Left < other.Right;
        }

        public readonly bool TryGetIntersection(LTRB other, out LTRB result)
        {
            result = this;
            return result.Intersect(other);
        }

        public bool Intersect(LTRB other)
        {
            if (Left < other.Left)
            {
                Left = other.Left;
            }

            if (Right > other.Right)
            {
                Right = other.Right;
            }

            if (Top < other.Top)
            {
                Top = other.Top;
            }

            if (Bottom > other.Bottom)
            {
                Bottom = other.Bottom;
            }

            if (Top < Bottom && Left < Right)
                return true;

            this = default;
            return false;
        }

        public readonly bool Contains(PT point)
        {
            var (x, y) = point;
            return x <= Right
                && x >= Left
                && y <= Bottom
                && y >= Top;
        }

        public override bool Equals(object obj)
        {
            return obj is LTRB other && Equals(other);
        }

        public bool Equals(LTRB other)
        {
            return (Left, Top, Right, Bottom) == (other.Left, other.Top, other.Right, other.Bottom);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Left, Top, Right, Bottom);
        }

        public static bool operator ==(LTRB left, LTRB right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LTRB left, LTRB right)
        {
            return !(left == right);
        }

        public void AnchorTo(LTRB other, Anchor anchor)
        {
            var (width, height) = (other.Width, other.Height);
            Left += (int)(width * anchor.Left);
            Top += (int)(height * anchor.Top);
            Right += (int)(width * anchor.Right);
            Bottom += (int)(height * anchor.Bottom);
        }

        public void Transform(LTRB from, LTRB to)
        {
            float scale;

            Offset(-from.Left, -from.Top);

            if (to.Width == from.Width)
            {
                Offset(to.Left, 0);
            }
            else
            {
                scale = (float)to.Width / from.Width;
                Left = (int)MathF.Round(MathF.FusedMultiplyAdd(Left, scale, to.Left));
                Right = (int)MathF.Round(MathF.FusedMultiplyAdd(Right, scale, to.Left));
            }

            if (to.Height == from.Height)
            {
                Offset(0, to.Top);
            }
            else
            {
                scale = (float)to.Height / from.Height;
                Top = (int)MathF.Round(MathF.FusedMultiplyAdd(Top, scale, to.Top));
                Bottom = (int)MathF.Round(MathF.FusedMultiplyAdd(Bottom, scale, to.Top));
            }
        }
    }

    public struct Anchor : IEquatable<Anchor>
    {
        LTRB _ltrb;

        public Anchor(float left, float top, float right, float bottom) : this()
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        private static float Out(int value)
        {
            return value / 65536f;
        }

        private static int In(float value)
        {
            return (int)(value * 65536f);
        }

        public override bool Equals(object obj)
        {
            return obj is Anchor anchor && Equals(anchor);
        }

        public bool Equals(Anchor other)
        {
            return _ltrb.Equals(other._ltrb);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_ltrb);
        }

        public float Left { get => Out(_ltrb.Left); set => _ltrb.Left = In(value); }
        public float Top { get => Out(_ltrb.Top); set => _ltrb.Top = In(value); }
        public float Right { get => Out(_ltrb.Right); set => _ltrb.Right = In(value); }
        public float Bottom { get => Out(_ltrb.Bottom); set => _ltrb.Bottom = In(value); }

        public static bool operator ==(Anchor left, Anchor right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Anchor left, Anchor right)
        {
            return !(left == right);
        }
    }
}
