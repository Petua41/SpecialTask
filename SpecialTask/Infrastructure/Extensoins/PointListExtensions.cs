using SpecialTask.Drawing;

namespace SpecialTask.Infrastructure.Extensoins
{
    /// <summary>
    /// Provides some extensions to <see cref="List{T}"/> of <see cref="Point"/>s
    /// </summary>
    public static class PointListExtensions
    {
        public static string PointsToString(this List<Point> points)        // I cannot overwrite ToString() for List<Point>, so it will be named like that
        {
            return string.Join(", ", points.Select(p => $"{p.X} {p.Y}"));
        }

        public static List<Point> ParsePoints(this string value)
        {
            return value.SplitInsensitive(',').Select(st => st.SplitInsensitive(' ')).Select(arr => new Point(int.Parse(arr[0]), int.Parse(arr[1]))).ToList();
        }

        public static Point Center(this List<Point> points)
        {
            int x = (int)points.Select(p => p.X).Average();
            int y = (int)points.Select(p => p.Y).Average();
            return new Point(x, y);
        }
    }
}
