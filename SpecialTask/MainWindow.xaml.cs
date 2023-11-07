using SpecialTask.Console.CommandsParser;
using SpecialTask.Console.Interfaces;
using SpecialTask.Infrastructure;
using SpecialTask.Infrastructure.Enums;
using SpecialTask.Infrastructure.Loggers;
using SpecialTask.Infrastructure.WindowSystem;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace SpecialTask
{
    public enum SpecialKeyCombinations { None, Enter, Backspace, CtrlC }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private readonly ILowConsole lowConsole;
        private readonly Brush defaultForegroundBrush = new SolidColorBrush(EColor.White.GetWPFColor());
        private readonly WindowManager windowManager;

        public MainWindow()
        {
            InitializeComponent();
            PathsController.InitPaths();                    // we must call it before any other calls. It`s not good
            InitializeLogger(ConcreteLoggers.SimpleLogger); // so that it gets right creation time
            try
            {
                ConsoleCommandsParser.InitializeCommands();
            }
            catch (FatalError)
            {
                Logger.Fatal($"Invalid XML file with commands!{Environment.NewLine}Please, contact us");
                Application.Current.Shutdown();
            }

            lowConsole = LowConsole;

            ParseCommandLineArguments();

            windowManager = WindowManager.Instance;     // Same as Logger
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
            TextRange range = new(ConsoleTB.ContentEnd, ConsoleTB.ContentEnd)
            {
                Text = message
            };
            range.ApplyPropertyValue(ForegroundProperty, brush);
            ScrollToEnd();
        }

        private void ScrollToEnd()
        {
            ConsoleScrollViewer.ScrollToEnd();
        }

        private void OnConsoleClosed(object sender, EventArgs e)
        {
            windowManager.CloseAll();

            Logger.Dispose();
        }

        private void ParseCommandLineArguments()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 1)
            {
                return;
            }

            if ((args[1] == "-d" || args[1] == "--undo_stack_depth") && args.Length > 2)
            {
                if (int.TryParse(args[2], out int newDepth)) lowConsole.ChangeUndoStackDepth(newDepth);
                else
                {
                    Logger.Error($"{args[2]} is not valid undo stack depth");
                    Display($"{args[2]} is not valid undo stack depth. Setting to default (15){Environment.NewLine}", Colors.Red);
                }
            }
        }

        private void ConsoleWindowGotFocus(object sender, RoutedEventArgs e)
        {
            _ = ConsoleEntry.Focus();
        }

        private void ConsoleEntryKeyDown(object sender, KeyEventArgs e)
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
                DisplayAndProcessInputString(ConsoleEntry.Text);
                ClearInputLine();
            }
            else if (key == Key.Tab)
            {
                Autocomplete(ConsoleEntry.Text);
            }
        }

        private void Autocomplete(string input)
        {
            string strToComplete = lowConsole.Autocomplete(input);
            EmulateInput(strToComplete);
            MoveCaretToEnd();
        }

        private void ProcessCtrl()
        {
            if (Keyboard.IsKeyDown(Key.S))
            {
                DisplayAndProcessInputString("save");
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
                DisplayAndProcessInputString(shiftPressed ? "redo" : "undo");
            }
            if (Keyboard.IsKeyDown(Key.C))				// Ctrl+C
            {
                lowConsole.ProcessCtrlC();
            }
        }

        private void ProcessUpArrow()
        {
            string comp = lowConsole.ProcessUpArrow();
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
            EmulateInput(lowConsole.ProcessDownArrow());
            MoveCaretToEnd();
        }

        private void EmulateInput(string str)
        {
            ConsoleEntry.Text += str;
        }

        private void MoveCaretToEnd()
        {
            ConsoleEntry.CaretIndex = ConsoleEntry.Text.Length;
        }

        public void DisplayAndProcessInputString(string text)
        {
            lowConsole.NewLine();
            lowConsole.DisplayPrompt();
            Display(text);

            lowConsole.ProcessInputString(text);
        }

        private void ClearInputLine()
        {
            ConsoleEntry.Text = string.Empty;
        }
    }
}
