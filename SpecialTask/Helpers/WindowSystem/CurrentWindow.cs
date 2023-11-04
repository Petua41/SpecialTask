using SpecialTask.Drawing;
using SpecialTask.Helpers.CommandHelpers;
using SpecialTask.Helpers.WindowSystem;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SpecialTask.Helpers
{
    static class CurrentWindow
    {
        public static List<Shape> Shapes => Window.ShapesOnThisWindow;

        public static void AddShape(Shape shape)
        {
            Window.AddShape(shape);

            _ = SaveLoadFacade.Instance;		// so that it will be initialized
            SomethingDisplayed?.Invoke(null, new());
        }

        public static void RemoveShape(Shape shape)
        {
            Window.RemoveShape(shape);
        }

        public static int SendBackward(string uniqueName)
        {
            return Window.SendBackwards(uniqueName);
        }

        public static int BringForward(string uniqueName)
        {
            return Window.BringForward(uniqueName);
        }

        public static int SendToBack(string uniqueName)
        {
            return Window.SendToBack(uniqueName);
        }

        public static int BringToFront(string uniqueName)
        {
            return Window.BringToFront(uniqueName);
        }

        public static void MoveToLayer(string uniqueName, int newLayer)
        {
            Window.MoveToLayer(uniqueName, newLayer);
        }

        /// <summary>
        /// Gets <see cref="BitmapSource"/>, containing an image of current canvas` state
        /// </summary>
        public static BitmapSource CanvasBitmapSource
        // I could return Canvas, but this method guarantees, that Canvas won`t be used for evil
        {
            get
            {
                Canvas canvas = Window.Canvas;

                double width = canvas.ActualWidth;
                double height = canvas.ActualHeight;

                RenderTargetBitmap bmp = new((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
                bmp.Render(canvas);

                return bmp;
            }
        }

#pragma warning disable CS0618
        private static Window Window => WindowManager.Instance.CurrentWindow;   // This method is marked obsolete, because it shouldn`t be used everywhere but here
#pragma warning restore

        public static event EventHandler? SomethingDisplayed;
    }
}
