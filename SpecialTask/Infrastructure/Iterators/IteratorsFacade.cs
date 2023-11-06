using SpecialTask.Drawing;
using SpecialTask.Infrastructure.Iterators;

namespace SpecialTask.Infrastructure
{
    enum ESortingOrder { Coordinates, CreationTime }

    static class IteratorsFacade
    {
        private static IIterator concreteIterator;      // Strategy

        static IteratorsFacade()
        {
            concreteIterator = CreationTimeIterator.Instance;    // По-умолчанию CreationTimeIterator
        }

        public static IReadOnlyList<Shape> GetCompleteResult()
        {
            return concreteIterator.GetCompleteResult();
        }

        public static void SetConcreteIterator(ESortingOrder iteratorType)
        {
            concreteIterator = iteratorType switch
            {
                ESortingOrder.Coordinates => CoordinatesIterator.Instance,
                _ => CreationTimeIterator.Instance
            };
        }
    }

    interface IIterator
    {
        public IReadOnlyList<Shape> GetCompleteResult();
    }
}
