using SpecialTask.Drawing;

namespace SpecialTask.Infrastructure.Extensoins
{
    /// <summary>
    /// Provides some extensions to <see cref="List{T}"/> of <see cref="Point"/>s
    /// </summary>
    public static class PointListExtensions
    {
        public static string PointsToString(this List<Point> points)
        {
            return string.Join(", ", points.Select(p => $"{p.X} {p.Y}"));
        }

        public static List<Point> ParsePoints(this string value)
        {
            StringSplitOptions op = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

            return value.Split(',', op).Select(p => p.Split(' ', op)).Select(pre => new Point(int.Parse(pre[0]), int.Parse(pre[1]))).ToList();
        }

        public static Point Center(this List<Point> points)
        {
            int x = (int)points.Select(p => p.X).Average();
            int y = (int)points.Select(p => p.Y).Average();
            return new Point(x, y);
        }
    }
}
