using System.Collections.Generic;
using System.Linq;

namespace SpecialTask
{
	static class SelectPasteHandler
	{
		private static List<Shape> savedShapes = new();
		private static int savedLeftTopX = 0;
		private static int savedLeftTopY = 0;

		public static void SaveArea(int leftTopX, int leftTopY, int rightBottomX, int rightBottomY)
		{
			savedShapes = (from shape in WindowManager.Instance.ShapesOnCurrentWindow
						   where (shape is not SelectionMarker &&
						   leftTopX <= shape.Center.X && shape.Center.X <= rightBottomX &&
						   leftTopY <= shape.Center.Y && shape.Center.Y <= rightBottomY) select shape).ToList();

			savedLeftTopX = leftTopX;
			savedLeftTopY = leftTopY;
		}

		public static List<Shape> PasteArea(int leftTopX, int leftTopY)
		{
			int xOffset = leftTopX - savedLeftTopX;
			int yOffset = leftTopY - savedLeftTopY;

			foreach (Shape shape in savedShapes)
			{
				Shape sh = shape.Clone();

				sh.MoveXBy(xOffset);
				sh.MoveYBy(yOffset);
			}

			return savedShapes;
		}
	}
}
