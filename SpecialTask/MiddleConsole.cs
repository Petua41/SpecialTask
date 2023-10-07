using System;
using System.Collections.Generic;
using System.Windows;

namespace SpecialTask
{
    /// <summary>
    /// High-level console interface for business classes
    /// </summary>
    public interface IHighConsole
    {
        public static IHighConsole? HighConsole { get; }

        public void DisplayGlobalHelp();
        public void DisplayError(string message);
        public void DisplayWarning(string message);
        public void DisplayQuestion(string message);
        public void Display(string message);
        public void NewLine();
        public void DisplayPrompt();

        public string TransferredString { get; }
        public char? TransferredChar { get; }
        public ESpecialKeyCombinations TransferredCombination { get; }
        public bool TransferringInput { get; set; }
        public bool InputBlocked { get; set; }

        public event EventHandler? SomethingTranferred;
    }

    /// <summary>
    /// Low-level console interface for WPF classes
    /// </summary>
    public interface ILowConsole
    {
        public static ILowConsole? LowConsole { get; }

        public void DisplayPrompt();
        public string Autocomplete(string currentInput);
        public string ProcessUpArrow();
        public string ProcessDownArrow();
        public void ChangeUndoStackDepth(int depth);
        public void TransferInput(char? character, ESpecialKeyCombinations combination);
        public void ProcessInputString(string input);

    }

    public class MiddleConsole : IHighConsole, ILowConsole          // double-singleton
    {
        private static MiddleConsole? singleton;
        private readonly MainWindow mainWindowInstance;

        private string interceptedString = "";
        private char? lastInterceptedChar = null;
        private ESpecialKeyCombinations lastInterceptedCombination = ESpecialKeyCombinations.None;
        private string lastInterceptedString = "";

        private const EColor defaultColor = EColor.White;

        private MiddleConsole()
        {
            if (singleton != null) throw new SingletonError();

            try { mainWindowInstance = (MainWindow)Application.Current.MainWindow; }
            catch (NullReferenceException ex)
            {
                Logger.Instance.Error(string.Format("{0} exception while trying to get MainWindow instance!", ex.GetType().ToString()));
                throw;
            }
        }

        public static IHighConsole HighConsole
        {
            get
            {
                singleton ??= new();
                return singleton;
            }
        }

        public static ILowConsole LowConsole
        {
            get
            {
                singleton ??= new();
                return singleton;
            }
        }

        public void TransferInput(char? character, ESpecialKeyCombinations combination)
        {
            if (combination != ESpecialKeyCombinations.None) lastInterceptedCombination = combination;

            switch (combination)
            {
                case ESpecialKeyCombinations.Enter:
                    lastInterceptedString = interceptedString;
                    interceptedString = "";
                    break;
                case ESpecialKeyCombinations.Backspace:
                    if (interceptedString.Length > 0) interceptedString = interceptedString[..^1];
                    break;
                default:
                    if (character != null)
                    {
                        interceptedString += character;
                        lastInterceptedChar = character;
                    }
                    break;
            }

            SomethingTranferred?.Invoke(this, new());
        }

        public event EventHandler? SomethingTranferred;

        public string Autocomplete(string currentInput)
        {
            return CommandsParser.Autocomplete(currentInput);
        }

        public void ChangeUndoStackDepth(int depth)
        {
            CommandsFacade.ChangeUndoStackDepth(depth);
        }

        public void DisplayError(string message)
        {
            Display(message, EColor.Red);
            NewLine();
        }

        public void Display(string message)
        {
            MyMap<string, EColor> messageSplittedByColors = SplitMessageByColors(message);
            foreach (KeyValuePair<string, EColor> kvp in messageSplittedByColors)
            {
                Display(kvp.Key, kvp.Value);
            }
        }

        public void NewLine()
        {
            Display(Environment.NewLine, defaultColor);
        }

        public void DisplayGlobalHelp()
        {
            Display(CommandsParser.globalHelp);
        }

        public void DisplayPrompt()
        {
            Display(">> ", EColor.Green);
            InputBlocked = false;
        }

        public void DisplayQuestion(string message)
        {
            Display(message, EColor.Yellow);
        }

        public void DisplayWarning(string message)
        {
            Display(message, EColor.Purple);
            NewLine();
        }

        public void ProcessInputString(string input)
        {
            CommandsParser.ParseCommand(input);
            DisplayPrompt();
        }

        public string ProcessDownArrow()
        {
            // TODO
            throw new NotImplementedException();
        }

        public string ProcessUpArrow()
        {
            // TODO
            throw new NotImplementedException();
        }

        public string TransferredString => lastInterceptedString;

        public char? TransferredChar => lastInterceptedChar;

        public bool TransferringInput
        {
            get => mainWindowInstance.TransferringInput;
            set => mainWindowInstance.TransferringInput = value;
        }

        public bool InputBlocked
        {
            get => mainWindowInstance.InputBlocked;
            set => mainWindowInstance.InputBlocked = value;
        }

        public ESpecialKeyCombinations TransferredCombination => lastInterceptedCombination;


        private void Display(string message, EColor color = EColor.None)
        {
            mainWindowInstance.Display(message, color.GetWPFColor());
        }

        // TODO: this method is TOO long
        public static MyMap<string, EColor> SplitMessageByColors(string message)        // This must be private, but I wanna test it
        {
            MyMap<string, EColor> messageSplittedByColors = new();

            EColor lastColor = defaultColor;
            do
            {
                int indexOfNextColorChange = message.IndexOf("[color");
                if (indexOfNextColorChange == -1)
                {
                    messageSplittedByColors.Add(message, lastColor);
                    message = "";
                }
                else if (indexOfNextColorChange == 0)
                {
                    int endOfColorSequence = message.IndexOf("]");
                    string colorSequence = message[..(endOfColorSequence + 1)];
                    if (colorSequence == "[color]") lastColor = defaultColor;
                    else
                    {
                        string colorName = colorSequence[7..^1];
                        try { lastColor = ColorsController.Parse(colorName); }
                        catch (ColorExcepttion)
                        {
                            Logger.Instance.Error(string.Format("Invalid color name in escape sequence: {0}", colorName));
                            throw new EscapeSequenceParsingError();
                        }
                    }
                    message = message[(endOfColorSequence + 1)..];
                }
                else
                {
                    string currentPartOfMessage = message[..indexOfNextColorChange];
                    message = message[indexOfNextColorChange..];
                    messageSplittedByColors.Add(currentPartOfMessage, lastColor);
                }
            } while (message.Length > 0);

            return messageSplittedByColors;
        }
    }
}
