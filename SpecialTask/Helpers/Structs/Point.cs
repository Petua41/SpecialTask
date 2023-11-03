namespace SpecialTask.Helpers
{
    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Point operator +(Point a, Point b)
        {
            return new(a.X + b.X, a.Y + b.Y);
        }

        public static implicit operator (int, int)(Point p) => (p.X, p.Y);

        public static explicit operator System.Windows.Point(Point p) => new(p.X, p.Y);

        public static implicit operator Point((int, int) tp) => new(tp.Item1, tp.Item2);

        public readonly void Deconstruct(out int x, out int y)
        {
            x = X;
            y = Y;
        }
    }
}
