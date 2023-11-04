namespace SpecialTask.Helpers.Extensoins
{
    /// <summary>
    /// Provides some extensions to <see cref="List{T}"/> of <see cref="Point"/>s
    /// </summary>
    public static class PointListExtensions
    {
        public static string PointsToString(this List<Point> points)
        {
            return string.Join(", ", from p in points select $"{p.X} {p.Y}");
        }

        public static List<Point> ParsePoints(this string value)
        {
            List<string[]> prePoints = (from prePreP
                                    in value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                        select prePreP.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)).ToList();

            return (from preP in prePoints select new Point(int.Parse(preP[0]), int.Parse(preP[1]))).ToList();
        }

        public static Point Center(this List<Point> points)
        {
            int x = (int)(from p in points select p.X).Average();
            int y = (int)(from p in points select p.Y).Average();
            return new Point(x, y);
        }
    }
}
