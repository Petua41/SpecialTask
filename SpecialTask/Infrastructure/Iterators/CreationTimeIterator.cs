using SpecialTask.Drawing.Shapes;
using SpecialTask.Infrastructure.WindowSystem;

namespace SpecialTask.Infrastructure.Iterators
{
    internal class CreationTimeIterator : IIterator
    {
        private static CreationTimeIterator? singleton;

        private CreationTimeIterator() { }

        public static IIterator Instance
        {
            get
            {
                singleton ??= new CreationTimeIterator();
                return singleton;
            }
        }

        public IReadOnlyList<Shape> GetCompleteResult()
        {
            return CurrentWindow.Shapes.Where(sh => sh is not SelectionMarker).ToList();	// TODO: это костыль
        }
    }
}
