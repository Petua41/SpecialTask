using SpecialTask.Drawing.Shapes;
using SpecialTask.Infrastructure.WindowSystem;

namespace SpecialTask.Infrastructure.CommandHelpers
{
    internal static class SelectionMemento
    {
        private static List<Shape> savedShapes = new();
        private static int savedLeftTopX = 0;
        private static int savedLeftTopY = 0;

        public static void SaveArea(int leftTopX, int leftTopY, int rightBottomX, int rightBottomY)
        {
            savedShapes = WindowManager.CurrentWindow.Shapes.Where(sh => sh is not SelectionMarker &&
                leftTopX <= sh.Center.X && sh.Center.X <= rightBottomX &&
                leftTopY < sh.Center.Y && sh.Center.Y <= rightBottomY).ToList();

            savedLeftTopX = leftTopX;
            savedLeftTopY = leftTopY;
        }

        /// <returns>List of new (pasted) shapes</returns>
        public static List<Shape> PasteArea(int leftTopX, int leftTopY)
        {
            int xOffset = leftTopX - savedLeftTopX;
            int yOffset = leftTopY - savedLeftTopY;

            List<Shape> pastedShapes = new();

            foreach (Shape shape in savedShapes)
            {
                Shape sh = shape.Clone();

                sh.Display();

                sh.MoveXBy(xOffset);
                sh.MoveYBy(yOffset);

                pastedShapes.Add(sh);
            }

            return pastedShapes;
        }
    }
}
