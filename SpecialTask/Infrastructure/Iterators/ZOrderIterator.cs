using SpecialTask.Drawing.Shapes;
using SpecialTask.Infrastructure.WindowSystem;

namespace SpecialTask.Infrastructure.Iterators
{
    internal class ZOrderIterator : IIterator
    {
        private static readonly object syncLock = new();
        private static volatile ZOrderIterator? singleton;

        private ZOrderIterator() { }

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
            List<Shape> shapes = WindowManager.CurrentWindow.Shapes;
            List<int> zOrder = WindowManager.CurrentWindow.ZOrder;

            return shapes.Zip(zOrder).OrderBy(tp => tp.Second).Select(tp => tp.First).ToList();     // I could do this by inserting shapes to final list, but I don`t sure if it will be readable
        }
    }
}
