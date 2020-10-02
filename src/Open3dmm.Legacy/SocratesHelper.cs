using System;

namespace Open3dmm
{
    public struct Rectangle : IEquatable<Rectangle>
    {
        public Rectangle(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public int Right => X + Width;
        public int Left => X;
        public int Bottom => Y + Height;
        public int Top => Y;

        public override bool Equals(object obj)
        {
            return obj is Rectangle rectangle && Equals(rectangle);
        }

        public bool Equals(Rectangle other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   Width == other.Width &&
                   Height == other.Height;
        }

        public static bool operator ==(Rectangle left, Rectangle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Rectangle left, Rectangle right)
        {
            return !(left == right);
        }

        [Obsolete(null, true)]
        public static implicit operator Veldrid.Rectangle(Rectangle rectangle)
        {
            return new Veldrid.Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        [Obsolete(null, true)]
        public static implicit operator System.Drawing.Rectangle(Rectangle rectangle)
        {
            return new System.Drawing.Rectangle(rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom);
        }

        public static implicit operator Rectangle(Veldrid.Rectangle rectangle)
        {
            return new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }

        public static implicit operator Rectangle(System.Drawing.Rectangle rectangle)
        {
            if (rectangle.Right < rectangle.Left || rectangle.Bottom < rectangle.Top)
                throw new InvalidOperationException($"{typeof(System.Drawing.Rectangle).FullName} is invalid.");
            return new Rectangle(rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height);
        }

        public bool Contains(Veldrid.Point p)
            => p.X >= Left && p.X <= Right && p.Y >= Top && p.Y <= Bottom;

        public static bool OneValidAndBothNotSame(in Rectangle a, in Rectangle b)
        {
            if (a.Top < a.Bottom && a.Left < a.Right)
                return a != b;
            return b.Top < b.Bottom && b.Left < b.Right;
        }
    }
}
