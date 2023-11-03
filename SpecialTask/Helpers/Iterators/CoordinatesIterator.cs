using SpecialTask.Drawing;
using SpecialTask.Drawing.Shapes;
using System.Collections.Generic;
using System.Linq;

namespace SpecialTask.Helpers.Iterators
{
    class CoordinatesIterator : IIterator
    {
        private static CoordinatesIterator? singleton;

        private CoordinatesIterator() { }

        public static IIterator Instance
        {
            get
            {
                singleton ??= new CoordinatesIterator();
                return singleton;
            }
        }

        public List<Shape> GetCompleteResult()
        {
            List<Shape> rawList = CurrentWindow.Shapes;
            return rawList.OrderBy(sh => sh, new CoordinatesComparer()).Where(sh => sh is not SelectionMarker).ToList();    // Наверное, делать это вручную было бы эффективнее
        }

        private class CoordinatesComparer : IComparer<Shape>
        {
            int IComparer<Shape>.Compare(Shape? x, Shape? y)
            {
                if (x is null && y is null) return 0;
                if (x is null) return -1;
                if (y is null) return 1;

                (int firstX, int firstY) = x.Center;
                (int secondX, int secondY) = y.Center;

                if (firstY < secondY) return -1;
                if (firstY > secondY) return 1;

                if (firstX < secondX) return -1;
                else if (firstX > secondX) return 1;

                return 0;
            }
        }
    }
}
