using SpecialTask.Drawing.Shapes;
using SpecialTask.Infrastructure.WindowSystem;

namespace SpecialTask.Infrastructure.Iterators
{
    internal class CoordinatesIterator : IIterator
    {

        private static readonly object syncLock = new();
        private static volatile CoordinatesIterator? singleton;

        private CoordinatesIterator() { }

        public static IIterator Instance
        {
            get
            {
                if (singleton is not null)
                {
                    return singleton;
                }

                lock (syncLock)
                {
                    singleton ??= new();
                }
                return singleton;
            }
        }

        public IReadOnlyList<Shape> GetCompleteResult()
        {
            List<Shape> rawList = CurrentWindow.Shapes;
            rawList.Sort(new CoordinatesComparer());
            return rawList.Where(sh => sh is not SelectionMarker).ToList();    // Maybe do it manually will be more efficient
        }

        private class CoordinatesComparer : IComparer<Shape>
        {
            int IComparer<Shape>.Compare(Shape? x, Shape? y)
            {
                if (x is null && y is null)
                {
                    return 0;
                }

                if (x is null)
                {
                    return -1;
                }

                if (y is null)
                {
                    return 1;
                }

                (int firstX, int firstY) = x.Center;
                (int secondX, int secondY) = y.Center;

                if (firstY < secondY)
                {
                    return -1;
                }

                if (firstY > secondY)
                {
                    return 1;
                }

                if (firstX < secondX)
                {
                    return -1;
                }
                else if (firstX > secondX)
                {
                    return 1;
                }

                return 0;
            }
        }
    }
}
