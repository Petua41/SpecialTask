using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
		private string currentInput = "";
		private bool inputBlocked = false;
		private bool transferringInput = false;
		private readonly Brush defaultForegroundBrush = new SolidColorBrush(Colors.White);
		private readonly Logger logger;
		private readonly WindowManager windowManager;
		List<(TextPointer, TextPointer, Brush)> appliedRanges = new();

		private readonly Dictionary<Key, char> numberKeys = new()
		{
			{ Key.D0, '0' }, { Key.D1, '1' }, { Key.D2, '2' }, { Key.D3, '3' }, { Key.D4, '4' }, { Key.D5, '5' }, { Key.D6, '6' }, { Key.D7, '7' },
			{ Key.D8, '8' }, { Key.D9, '9' }
		};

		private readonly Dictionary<Key, char> shiftedNumberKeys = new()
		{
			{ Key.D0, ')' }, { Key.D1, '!' }, { Key.D2, '@' }, { Key.D3, '#' }, { Key.D4, '$' }, { Key.D5, '%' }, { Key.D6, '^' }, { Key.D7, '&' },
			{ Key.D8, '*' }, { Key.D9, '(' }
		};

		private readonly Dictionary<Key, char> otherKeys = new()
		{
			{ Key.Oem3, '`' }, { Key.OemMinus, '-' }, { Key.OemPlus, '=' }, { Key.OemOpenBrackets, '[' }, { Key.Oem6, ']' }, { Key.Oem1, ';' },
			{ Key.OemQuotes, '\'' }, { Key.OemComma, ',' }, { Key.OemPeriod, '.' }, {Key.OemQuestion, '/'}, {Key.Oem5, '\\'}, {Key.Space, ' '}
		};

		private readonly Dictionary<Key, char> shiftedOtherKeys = new()
		{
			{ Key.Oem3, '~' }, { Key.OemMinus, '_' }, { Key.OemPlus, '+' }, { Key.OemOpenBrackets, '{' }, { Key.Oem6, '}' }, { Key.Oem1, ':' },
			{ Key.OemQuotes, '"' }, { Key.OemComma, '<' }, { Key.OemPeriod, '>' }, {Key.OemQuestion, '?'}, {Key.Oem5, '|'}
		};

		public MainWindow()
		{
			InitializeComponent();

			logger = Logger.Instance;       // so that Logger gets right creation time
			lowConsole = MiddleConsole.LowConsole;

			ParseCommandLineArguments();

			windowManager = WindowManager.Instance; // Same as Logger

			Display("\n");
			lowConsole.DisplayPrompt();
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
			ApplyAndSaveRange(range, brush);
			MoveCaretToEnd();
		}

		/// <summary>
		/// Input blocked: user cannot type any character
		/// </summary>
		public bool InputBlocked
		{
			get => inputBlocked;
			set => inputBlocked = value;
		}

		/// <summary>
		/// Transferring input: every char or special key combination is sended to STConsole
		/// </summary>
		public bool TransferringInput
		{
			get => transferringInput;
			set => transferringInput = value;
		}

		private void ConsoleTBKeyDown(object sender, KeyEventArgs e)
		{
			if (InputBlocked) return;

			char? inputChar = ProcessInputChar(e);
			if (inputChar != null)
			{
				if (TransferringInput) TransferInput(inputChar, ESpecialKeyCombinations.None);			// if we got character, transfer it
				Display(inputChar.ToString() ?? "");
				currentInput += inputChar;
			}
		}

		private char? ProcessInputChar(KeyEventArgs e)      // это прям лютый костыль, но по-другому никак. Спасибо WPF
		{
			Key key = e.Key;
			KeyboardDevice kb = e.KeyboardDevice;
			bool shiftPressed = kb.IsKeyDown(Key.LeftShift) || kb.IsKeyDown(Key.RightShift);
			bool ctrlPressed = kb.IsKeyDown(Key.LeftCtrl) || kb.IsKeyDown(Key.RightCtrl);

			if (ctrlPressed)
			{
				if (ProcessCrtlKeys(key, shiftPressed)) return null;
			}

			// [34, 43] -- цифры
			if (Key.D0 <= key && key <= Key.D9) return ProcessNumberKeys(key, shiftPressed);
			// [44, 69] -- буквы
			if (Key.A <= key && key <= Key.Z) return ProcessLetterKeys(key, shiftPressed);

			return ProcessSpecialKey(key, shiftPressed);
		}

		private char ProcessNumberKeys(Key key, bool shiftPressed)
		{
			if (shiftPressed) return shiftedNumberKeys[key];
			return numberKeys[key];
		}

		private static char ProcessLetterKeys(Key key, bool shiftPressed)
		{
			if (shiftPressed) return key.ToString().Single();
			return key.ToString().ToLower().Single();
		}

		private char? ProcessSpecialKey(Key key, bool shiftPressed)
		{
			if (otherKeys.ContainsKey(key))
			{
				if (shiftPressed && shiftedOtherKeys.ContainsKey(key)) return shiftedOtherKeys[key];
				return otherKeys[key];
			}

			switch (key)
			{
				case Key.Return:
					if (TransferringInput) TransferInput(null, ESpecialKeyCombinations.Enter);
					else EmulateEnter();
					return null;
				case Key.Tab:
					if (TransferringInput) return null;				// same as Ctrl+Z
					else ProcessTab();
					return null;
				case Key.Back:
                    if (TransferringInput) TransferInput(null, ESpecialKeyCombinations.Backspace);
                    else ProcessBackspace();
					return null;
				default:
					return null;
			}
		}

		/// <summary>
		/// Передаёт введённую строку (целиком) в WPFConsole
		/// </summary>
		private void ProcessInputString()
		{
			lowConsole.ProcessInputString(currentInput);
			currentInput = "";
		}

		/// <summary>
		/// При помощи WPFConsole дополняет текущую введённую строку
		/// </summary>
		private void ProcessTab()
		{
			// TODO: здесь нужно получать, что можно дополнить от WPFConsole и дополнять
			// Вот здесь и пригодится цепочка обязанностей: в зависимости от того, что надо дополнять (команда, аргумент, путь...) этот запрос обрабатывают разные классы
			string completion = lowConsole.Autocomplete(currentInput);
			if (completion.Length > 0) EmulateInput(completion);
		}

		private void ProcessBackspace()
		{
			if (currentInput.Length > 0)
			{
				ConsoleTB.Text = ConsoleTB.Text[..^1];
				currentInput = currentInput[..^1];
				MoveCaretToEnd();
				// TODO: мы теряем цвета

				LoadAndApplyRanges();
			}
		}

		/// <summary>
		/// Processes special keys, when Ctrl is pressed
		/// </summary>
		/// <returns>Whether the key was intercepted</returns>
		private bool ProcessCrtlKeys(Key key, bool shiftPressed)
		{
			// If we`re here, Ctrl is already pressed
			switch (key)
			{
				case Key.V:
					string clip = Clipboard.GetText();
					EmulateInput(clip);
					return true;
				case Key.C:
					if (TransferringInput)		// we don`t kill command. we just tell it that Ctrl+C pressed
					{
						TransferInput(null, ESpecialKeyCombinations.CtrlC);
						return true;
					}
					return false;
				case Key.Z:
					if (TransferringInput) return false;		// there`s no point in whether sending Ctrl+Z to command or processing it
					if (shiftPressed) EmulateInput("redo");
					else EmulateInput("undo");
					EmulateEnter();
					return true;
				default:
					return false;
			}
		}

		private void ProcessArrows(Key key)
		{
			switch (key)
			{
				case Key.Up:
					if (TransferringInput) return;					// same as Ctrl+Z
					string prevCommandToDisplay = lowConsole.ProcessUpArrow();
					// TODO
					break;
				case Key.Down:
					if (TransferringInput) return;                  // same as Ctrl+Z
                    string nextCommandToDisplay = lowConsole.ProcessDownArrow();
					if (nextCommandToDisplay == "")
					{
						// TODO: вниз листать нечего. Ничего не делаем (нужно убедиться, что всё осталось как было)
					}
					else
					{
						// TODO
					}
					break;
				case Key.Left:
					// TODO: здесь двигаем Caret
					break;
				case Key.Right:
					// TODO здесь двигаем Caret
					break;
				case Key.End:
					MoveCaretToEnd();
					break;
				case Key.Home:
					// MoveCaretToStartOfString()
					break;
				default:
					logger.Warning(string.Format("{0} is not an arrow, but invoked ProcessArrows", key.ToString()));
					break;
			}
		}

		private static void MoveCaretToEnd()
		{
			// TODO: у TextBlock нет явной Caret и Background не поддерживается
		}

		private void ConsoleClosed(object sender, EventArgs e)
		{
			WindowManager.Instance.CloseAll();

			logger.Dispose();
			SaveLoadFacade.Instance.Dispose();
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
				logger.Error(string.Format("{0} is not valid undo stack depth", args[2]));
				Display(string.Format("{0} is not valid undo stack depth. Setting default (15)\n", args[2]), Colors.Red);
			}
		}

		private void ConsoleWindowGotFocus(object sender, RoutedEventArgs e)
		{
			ConsoleTB.Focus();
		}

		private void EmulateInput(string str)
		{
            Display(str);
            currentInput += str;
        }

		private void EmulateEnter()
		{
            Display("\n");
            ConsoleScrollViewer.ScrollToEnd();
            ProcessInputString();
        }

		private void ApplyAndSaveRange(TextRange range, Brush brush)
		{
            range.ApplyPropertyValue(ForegroundProperty, brush);
			appliedRanges.Add((range.Start, range.End, brush));
        }

		private void LoadAndApplyRanges()
		{
			foreach ((TextPointer, TextPointer, Brush) tp in appliedRanges)
			{
				TextPointer start = tp.Item1;
				TextPointer end = tp.Item2;
				TextRange range = new(start, end);
				string text = range.Text;
				range.ApplyPropertyValue(ForegroundProperty, tp.Item3);
			}
		}

		private void TransferInput(char? character, ESpecialKeyCombinations combination)
		{
			lowConsole.TransferInput(character, combination);
		}
	}
}
