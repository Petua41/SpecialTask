using SpecialTask.Drawing.Shapes;

namespace SpecialTask.Infrastructure.Iterators
{
    internal enum ESortingOrder { Coordinates, CreationTime }

    internal static class IteratorsFacade
    {
        private static IIterator concreteIterator;      // Strategy

        static IteratorsFacade()
        {
            concreteIterator = CreationTimeIterator.Instance;    // Default is CreationTimeIterator
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
}
