using SpecialTask.Drawing.Shapes;

namespace SpecialTask.Infrastructure.Iterators
{
    internal enum SortingOrder { Coordinates, CreationTime, ZOrder }

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

        public static void SetConcreteIterator(SortingOrder iteratorType)
        {
            concreteIterator = iteratorType switch
            {
                SortingOrder.Coordinates => CoordinatesIterator.Instance,
                SortingOrder.ZOrder => ZOrderIterator.Instance,
                _ => CreationTimeIterator.Instance
            };
        }
    }
}
