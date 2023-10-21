using System;
using System.Collections.Generic;
using System.Linq;

namespace SpecialTask
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
			List<Shape> rawList = WindowManager.Instance.ShapesOnCurrentWindow;
			return rawList.OrderBy(sh => sh, new CoordinatesComparer()).Where(sh => sh is not SelectionMarker).ToList();	// Наверное, делать это вручную было бы эффективнее
		}

		private class CoordinatesComparer: IComparer<Shape>
		{
			int IComparer<Shape>.Compare(Shape? x, Shape? y)
			{
				if (x == null && y == null) return 0;
				if (x == null) return -1;
				if (y == null) return 1;

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
			return WindowManager.Instance.ShapesOnCurrentWindow.Where(sh => sh is not SelectionMarker).ToList();	// TODO: это костыль
        }
	}
}
