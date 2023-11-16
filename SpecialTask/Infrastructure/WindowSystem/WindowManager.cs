using SpecialTask.Drawing.Shapes;
using SpecialTask.Infrastructure.CommandHelpers.SaveLoad;
using SpecialTask.Infrastructure.Events;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SpecialTask.Infrastructure.WindowSystem
{
    /// <summary>
    /// Controls creation, deletion and switching windows
    /// </summary>
    internal class WindowManager
    {
        internal static class CurrentWindow
        {
            public static void AddShape(Shape shape)
            {
                Window.AddShape(shape);

                _ = SaveLoadFacade.Instance;        // so that it will be initialized
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
            // It`s still not perfect solution, but much better
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

            public static List<Shape> Shapes => Window.ShapesOnThisWindow;

            private static Window Window => Instance.CurrentDrawingWindow;   // This method is marked obsolete, because it shouldn`t be used everywhere but here

            public static event EventHandler? SomethingDisplayed;
        }

        private static readonly object syncLock = new();
        private static volatile WindowManager? singleton;
        private readonly List<Window> existingWindows;

        private WindowManager()
        {
            CurrentDrawingWindow = new(0);
            existingWindows = new List<Window> { CurrentDrawingWindow };
#pragma warning restore
        }

        public static WindowManager Instance
        {
            get
            {
                if (singleton is not null)
                {
                    return singleton;
                }

                lock (syncLock)
                {
                    singleton ??= new();
                }
                return singleton;
            }
        }

        /// <summary>
        /// Creates new Window
        /// </summary>
        public void CreateWindow()
        {
            existingWindows.Add(new(existingWindows.Count));
        }

        public void DestroyWindow(int numberOfWindow)
        {
            existingWindows[numberOfWindow].Destroy();
            RemoveWindowFromLists(numberOfWindow);
        }

        public void SwitchToWindow(int numberOfWindow)
        {
            ValidateWindowNumber(numberOfWindow);                   // here we pass exception on
            CurrentDrawingWindow = existingWindows[numberOfWindow];
            WindowSwitchedEvent?.Invoke(this, new WindowSwitchedEventArgs(numberOfWindow));
        }

        public void CloseAll()
        {
            for (int i = existingWindows.Count - 1; i >= 0; i--)
            {
                DestroyWindow(i);
            }
        }

        public void OnSomeAssotiatedWindowClosed(Window winToDraw)
        {
            int idx = existingWindows.IndexOf(winToDraw);
            if (idx >= 0)
            {
                RemoveWindowFromLists(existingWindows.IndexOf(winToDraw));
            }
        }

        private void RemoveWindowFromLists(int windowNumber)
        {
            try { ValidateWindowNumber(windowNumber); }
            catch (ArgumentException) { return; }			// if window doesn`t exist, don`t remove it

            for (int i = windowNumber + 1; i < existingWindows.Count; i++)
            {
                existingWindows[i].ChangeTitle(i - 1);
            }
            existingWindows.RemoveAt(windowNumber);
        }

        /// <exception cref="ArgumentException"></exception>
        private void ValidateWindowNumber(int numberOfWindow)
        {
            if (numberOfWindow < 0 || numberOfWindow >= existingWindows.Count)
            {
                throw new ArgumentException($"Window {numberOfWindow} doesn`t exist");
            }
        }

        private Window CurrentDrawingWindow { get; set; }

        public event WindowSwitchedEventHandler? WindowSwitchedEvent;
    }
}
