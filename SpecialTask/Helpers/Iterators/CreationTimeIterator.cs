using SpecialTask.Drawing;
using SpecialTask.Drawing.Shapes;
using System.Collections.Generic;
using System.Linq;

namespace SpecialTask.Helpers.Iterators
{
    class CreationTimeIterator : IIterator
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

        public List<Shape> GetCompleteResult()
        {
            return CurrentWindow.Shapes.Where(sh => sh is not SelectionMarker).ToList();	// TODO: это костыль
        }
    }
}
