using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace SpecialTask
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly WPFConsole wpfConsole;
        private string currentInput = "";
        private bool inputBlocked = false;
        private readonly Brush defaultForegroundBrush = new SolidColorBrush(Colors.White);
        private readonly Logger logger;
        private readonly WindowManager windowManager;

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

            logger = Logger.Instance;       // Можно было бы везде использовать Logger.Instance, но, если мы получаем его здесь, то у него будет правильное время создания
            wpfConsole = WPFConsole.Instance;

            ParseCommandLineArguments();

            windowManager = WindowManager.Instance; // Same as Logger

            Display("\n>> ", Colors.Green);
        }

        public void Display(string message, System.Windows.Media.Color color)
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
            MoveCaretToEnd();
        }

        public void LockInput()
        {
            inputBlocked = true;
        }

        public void UnlockIntput()
        {
            inputBlocked = false;
        }

        private void ConsoleTBKeyDown(object sender, KeyEventArgs e)
        {
            //if (inputBlocked) return;

            Key key = e.Key;

            char? inputChar = ProcessInputChar(e);
            if (inputChar != null)
            {
                Display(inputChar.ToString());
                currentInput += inputChar;
            }
        }

        private char? ProcessInputChar(KeyEventArgs e)      // это прям лютый костыль, но по-другому никак. Спасибо WPF
        {
            Key key = e.Key;
            KeyboardDevice kb = e.KeyboardDevice;
            bool shiftPressed = kb.IsKeyDown(Key.LeftShift) || kb.IsKeyDown(Key.RightShift);

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

        private char ProcessLetterKeys(Key key, bool shiftPressed)
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
                    Display("\n");
                    ConsoleScrollViewer.ScrollToEnd();
                    ProcessInputString();
                    return null;
                case Key.Tab:
                    ProcessTab();
                    return null;
                case Key.Back:
                    ProcessBackspace();
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
            wpfConsole.ProcessInputString(currentInput);
            currentInput = "";
        }

        /// <summary>
        /// При помощи WPFConsole дополняет текущую введённую строку
        /// </summary>
        private void ProcessTab()
        {
            // TODO: здесь нужно получать, что можно дополнить от WPFConsole и дополнять
            // Вот здесь и пригодится цепочка обязанностей: в зависимости от того, что надо дополнять (команда, аргумент, путь...) этот запрос обрабатывают разные классы
        }

        private void ProcessBackspace()
        {
            if (currentInput.Length > 0)
            {
                ConsoleTB.Text = ConsoleTB.Text[..^1];
                currentInput = currentInput[..^1];
                MoveCaretToEnd();
                // TODO: мы теряем цвета
            }
        }

        private void ProcessArrows(Key key)
        {
            switch (key)
            {
                case Key.Up:
                    string prevCommandToDisplay = wpfConsole.ProcessUpArrow();
                    // TODO
                    break;
                case Key.Down:
                    string nextCommandToDisplay = wpfConsole.ProcessDownArrow();
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

        private void MoveCaretToEnd()
        {
            // TODO: у TextBlock нет явной Caret и Background не поддерживается
        }

        private void ConsoleClosed(object sender, EventArgs e)
        {
            // TODO: здесь мы должны закрывать все остальные окна (потому что консоль -- основное окно)

            logger.Dispose();
        }

        private void ParseCommandLineArguments()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 1) return;
            try
            {
                if ((args[1] == "-d" || args[1] == "--undo_stack_depth") && args.Length > 2) wpfConsole.ChangeUndoStackDepth(int.Parse(args[2]));
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
    }
}
