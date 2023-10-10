using System.Collections.Generic;
using System.Linq;

namespace SpecialTask
{
    enum ESortingOrder { Coordinates, CreationTime }

    static class IteratorsFacade
	{
		private static Iterator concreteIterator;      // Стратегия

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
				ESortingOrder.CreationTime => CreationTimeIterator.Instance,
				_ => throw new StringArgumentNameException(),
			};
		}
	}

	abstract class Iterator
	{
		public static Iterator Instance { get => throw new NotOverridenException(); }
		public abstract List<Shape> GetCompleteResult();
	}

	class CoordinatesIterator : Iterator
	{
		private static CoordinatesIterator? singleton;

		private CoordinatesIterator()
		{
			if (singleton != null) throw new SingletonError();
		}

		public static new Iterator Instance
		{
			get
			{
				singleton ??= new CoordinatesIterator();
				return singleton;
			}
		}

		public override List<Shape> GetCompleteResult()
		{
			List<Shape> rawList = WindowManager.Instance.ShapesOnCurrentWindow;
			return rawList.OrderBy(sh => sh, new CoordinatesComparer()).ToList();	// Наверное, делать это вручную было бы эффективнее
		}

		private class CoordinatesComparer: IComparer<Shape>
		{
			int IComparer<Shape>.Compare(Shape? x, Shape? y)
			{
				if (x == null || y == null) throw new NullComparisonException();

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

	class CreationTimeIterator : Iterator
	{
		private static CreationTimeIterator? singleton;

		private CreationTimeIterator()
		{
			if (singleton != null) throw new SingletonError();
		}

		public static new Iterator Instance
		{
			get
			{
				singleton ??= new CreationTimeIterator();
				return singleton;
			}
		}

		public override List<Shape> GetCompleteResult()
		{
			return WindowManager.Instance.ShapesOnCurrentWindow;
		}
	}
}
