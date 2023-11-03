using SpecialTask.Drawing;
using System.Collections.Generic;
using SpecialTask.Helpers.Iterators;

namespace SpecialTask.Helpers
{
    enum ESortingOrder { Coordinates, CreationTime }

    static class IteratorsFacade
    {
        private static IIterator concreteIterator;      // Strategy

        static IteratorsFacade()
        {
            concreteIterator = CreationTimeIterator.Instance;    // По-умолчанию CreationTimeIterator
        }

        public static List<Shape> GetCompleteResult()
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
        public List<Shape> GetCompleteResult();
    }
}
