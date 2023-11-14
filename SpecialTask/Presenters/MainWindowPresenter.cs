using SpecialTask.Infrastructure.WindowSystem;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace SpecialTask.Presenters
{
    internal class MainWindowPresenter
    {
        private readonly Brush defaultForegroundBrush = new SolidColorBrush(Colors.White);

        private readonly MainWindow mainWindow;

        public MainWindowPresenter(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
        }

        public void Display(string message, Color color)
        {
            Display(message, new SolidColorBrush(color));
        }

        public void Display(string message)
        {
            Display(message, defaultForegroundBrush);
        }

        public void Display(string message, Brush brush)
        {
            TextRange range = new(mainWindow.ConsoleTB.ContentEnd, mainWindow.ConsoleTB.ContentEnd)
            {
                Text = message
            };
            range.ApplyPropertyValue(System.Windows.Controls.Control.ForegroundProperty, brush);
            ScrollToEnd();
        }

        public void DisplayInputString(string text)
        {
            LowConsole.NewLine();
            LowConsole.DisplayPrompt();
            Display(text);
        }

        public void OnConsoleClosed(object sender, EventArgs e)
        {
            WindowManager.Instance.CloseAll();

            Logger.Dispose();
        }

        public void ConsoleWindowGotFocus(object sender, RoutedEventArgs e)
        {
            mainWindow.ConsoleEntry.Focus();
        }

        public void ConsoleEntryKeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;

            if (Keyboard.IsKeyDown(Key.RightCtrl) || Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                ProcessCtrl();
            }
            else if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
            {
                ProcessAlt();
            }

            if (key == Key.Enter)
            {
                string text = mainWindow.ConsoleEntry.Text.Trim();
                DisplayInputString(text);
                LowConsole.ProcessInputString(text);

                ClearInputLine();
            }
            else if (key == Key.Tab)
            {
                Autocomplete(mainWindow.ConsoleEntry.Text);
            }
        }

        private void ScrollToEnd()
        {
            mainWindow.ConsoleScrollViewer.ScrollToEnd();
        }

        private void Autocomplete(string input)
        {
            string strToComplete = LowConsole.Autocomplete(input);
            EmulateInput(strToComplete);
            MoveCaretToEnd();
        }

        private void ProcessCtrl()
        {
            if (Keyboard.IsKeyDown(Key.S))
            {
                DisplayInputString("save");
                LowConsole.ProcessInputString("save");
            }
            else if (Keyboard.IsKeyDown(Key.E))         // 🔼
            {
                ProcessUpArrow();
            }
            else if (Keyboard.IsKeyDown(Key.D))         // 🔽
            {
                ProcessDownArrow();
            }

            // Maybe somethig more
        }

        private void ProcessAlt()
        {
            if (Keyboard.IsKeyDown(Key.Z))              // Ctrl+Z or Ctrl+Shift+Z
            {
                bool shiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
                DisplayInputString(shiftPressed ? "redo" : "undo");
                LowConsole.ProcessInputString(shiftPressed ? "redo" : "undo");
            }
            if (Keyboard.IsKeyDown(Key.C))				// Ctrl+C
            {
                LowConsole.ProcessCtrlC();
            }
        }

        private void ProcessUpArrow()
        {
            string comp = LowConsole.ProcessUpArrow();
            if (comp.Length > 0)
            {
                ClearInputLine();
                EmulateInput(comp);
                MoveCaretToEnd();
            }
        }

        private void ProcessDownArrow()
        {
            ClearInputLine();
            EmulateInput(LowConsole.ProcessDownArrow());
            MoveCaretToEnd();
        }

        private void EmulateInput(string str)
        {
            mainWindow.ConsoleEntry.Text += str;
        }

        private void MoveCaretToEnd()
        {
            mainWindow.ConsoleEntry.CaretIndex = mainWindow.ConsoleEntry.Text.Length;
        }

        private void ClearInputLine()
        {
            mainWindow.ConsoleEntry.Text = string.Empty;
        }
    }
}
