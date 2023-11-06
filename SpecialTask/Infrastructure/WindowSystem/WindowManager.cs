using SpecialTask.Infrastructure.WindowSystem;

namespace SpecialTask.Infrastructure
{
    /// <summary>
    /// Controls creation, deletion and switching windows
    /// </summary>
    class WindowManager
    {
        private static WindowManager? singleton;

        private Window currentWindow;
        private readonly List<Window> existingWindows;

        private WindowManager()
        {
            currentWindow = new(0);
            existingWindows = new List<Window> { currentWindow };
        }

        public static WindowManager Instance
        {
            get
            {
                singleton ??= new WindowManager();
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
            currentWindow = existingWindows[numberOfWindow];
            WindowSwitchedEvent?.Invoke(this, new WindowSwitchedEventArgs(numberOfWindow));
        }

        public void CloseAll()
        {
            for (int i = existingWindows.Count - 1; i >= 0; i--) DestroyWindow(i);
        }

        [Obsolete("Please don`t call WindowSystem.Window directly. Use CurrentWindow instead")]
        public Window CurrentWindow => currentWindow;

        public void OnSomeAssotiatedWindowClosed(Window winToDraw)
        {
            int idx = existingWindows.IndexOf(winToDraw);
            if (idx >= 0) RemoveWindowFromLists(existingWindows.IndexOf(winToDraw));
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
            if (numberOfWindow < 0 || numberOfWindow >= existingWindows.Count) throw new ArgumentException($"Window {numberOfWindow} doesn`t exist");
        }

        public event WindowSwitchedEventHandler? WindowSwitchedEvent;
    }
}
