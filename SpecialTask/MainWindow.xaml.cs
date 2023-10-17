using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace SpecialTask
{
	public enum ESpecialKeyCombinations { None, Enter, Backspace, CtrlC }

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly ILowConsole lowConsole;
		private readonly Brush defaultForegroundBrush = new SolidColorBrush(EColor.White.GetWPFColor());
		private readonly Logger logger;
		private readonly WindowManager windowManager;

		public MainWindow()
		{
			InitializeComponent();

			logger = Logger.Instance;					// so that Logger gets right creation time
			lowConsole = MiddleConsole.LowConsole;

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

		/// <summary>
		/// Transferring input: every char or special key combination is sent to STConsole
		/// </summary>
		public bool TransferringInput { get; set; }

		private void ScrollToEnd()
		{
			ConsoleScrollViewer.ScrollToEnd();
		}

		private void OnConsoleClosed(object sender, EventArgs e)
		{
			windowManager.CloseAll();

			logger.Dispose();
		}

		private void ParseCommandLineArguments()
		{
			string[] args = Environment.GetCommandLineArgs();
			if (args.Length == 1) return;
			try
			{
				if ((args[1] == "-d" || args[1] == "--undo_stack_depth") && args.Length > 2) lowConsole.ChangeUndoStackDepth(int.Parse(args[2]));
			}
			catch (FormatException)
			{
				logger.Error($"{args[2]} is not valid undo stack depth");
				Display($"{args[2]} is not valid undo stack depth. Setting to default (15)\n", Colors.Red);
			}
		}

		private void ConsoleWindowGotFocus(object sender, RoutedEventArgs e)
		{
			ConsoleEntry.Focus();
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
				EmulateEnter();
			}
			else if (key == Key.Tab)
			{
                Autocomplete(ConsoleEntry.Text);
			}
        }

        private void EmulateEnter()
        {
            DisplayAndProcessInputString(ConsoleEntry.Text);
            ConsoleEntry.Text = "";
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
				Display("CtrlS");
				DisplayAndProcessInputString("save");
			}
			else if (Keyboard.IsKeyDown(Key.E))			// 🔼
			{
				ProcessUpArrow();
			}
			else if (Keyboard.IsKeyDown(Key.D))			// 🔽
			{
				ProcessDownArrow();
			}

			// Maybe somethig more
		}

		private void ProcessAlt()
		{
			if (Keyboard.IsKeyDown(Key.Z))				// Ctrl+Z or Ctrl+Shift+Z
			{
                bool shiftPressed = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
                if (!TransferringInput) DisplayAndProcessInputString(shiftPressed ? "redo" : "undo");	// when transferring, we don`t either transfer Ctrl+Z or handle it
            }
            if (Keyboard.IsKeyDown(Key.C))				// Ctrl+C
            {
				if (TransferringInput) lowConsole.TransferCtrlC();
				else DisplayAndProcessInputString("exit");
            }
        }

        private void ProcessUpArrow()
        {
            if (TransferringInput) return;                  // same as Ctrl+Z

            ClearInputLine();
            EmulateInput(lowConsole.ProcessUpArrow());
            MoveCaretToEnd();
        }

        private void ProcessDownArrow()
        {
            if (TransferringInput) return;                  // same as Ctrl+Z

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

        private void DisplayAndProcessInputString(string text)
		{
			lowConsole.NewLine();
			lowConsole.DisplayPrompt();
			Display(text);

			if (TransferringInput) lowConsole.TransferInputString(text);
			else lowConsole.ProcessInputString(text);
		}

		private void ClearInputLine()
		{
			ConsoleEntry.Text = "";
        }
    }
}
