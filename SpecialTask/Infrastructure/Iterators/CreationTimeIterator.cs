using SpecialTask.Drawing.Shapes;
using SpecialTask.Infrastructure.WindowSystem;

namespace SpecialTask.Infrastructure.Iterators
{
    internal class CreationTimeIterator : IIterator
    {

        private static readonly object syncLock = new();
        private static volatile CreationTimeIterator? singleton;

        private CreationTimeIterator() { }

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
            return WindowManager.CurrentWindow.Shapes.Where(sh => sh is not SelectionMarker).ToList();	// TODO: это костыль
        }
    }
}
