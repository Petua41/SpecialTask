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
            /*savedShapes = (from shape in WindowManager.Instance.ShapesOnCurrentWindow() where (shape is not SelectionMarker &&
                           leftTopX <= shape.Center.Item1 && shape.Center.Item1 <= rightBottomX && 
                           leftTopY <= shape.Center.Item2 && shape.Center.Item2 <= rightBottomY) 
                           select shape).ToList();*/
            foreach (Shape shape in WindowManager.Instance.ShapesOnCurrentWindow())
            {
                if (shape is not SelectionMarker)
                {
                    int centerX = shape.Center.Item1;
                    int centerY = shape.Center.Item2;
                    if (leftTopX <= centerX && centerX <= rightBottomX)
                    {
                        if (leftTopY <= centerY && centerY <= rightBottomY)
                        {
                            savedShapes.Add(shape);
                        }
                    }
                }
            }

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
