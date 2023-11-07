using SpecialTask.Drawing.Shapes;

namespace SpecialTask.Infrastructure.Iterators
{
    internal interface IIterator
    {
        IReadOnlyList<Shape> GetCompleteResult();
    }
}
