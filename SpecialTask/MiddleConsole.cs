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
        public bool TransferringInput { get; set; }

        public event TransferringEventHandler? SomethingTranferred;
        public event EventHandler? CtrlCTransferred;
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
        public void ProcessInputString(string input);
        public void NewLine();
        public void TransferInputString(string input);
        public void TransferCtrlC();

    }

    public class MiddleConsole : IHighConsole, ILowConsole          // double-singleton
    {
        private static MiddleConsole? singleton;
        private readonly MainWindow mainWindowInstance;

        private readonly List<string> prevCommands = new();
        private int pointer = 0;                    // from end

        private const EColor defaultColor = EColor.White;

        private MiddleConsole()
        {
            try { mainWindowInstance = (MainWindow)Application.Current.MainWindow; }
            catch (NullReferenceException ex)
            {
                Logger.Instance.Error($"{ex.GetType()} exception while trying to get MainWindow instance!");
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

        public void TransferInputString(string input)
        {
            SomethingTranferred?.Invoke(this, new(input));
        }

        public void TransferCtrlC()
        {
            CtrlCTransferred?.Invoke(this, new());
        }

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
            NewLine();
            Display(message, EColor.Red);
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
        }

        public void DisplayQuestion(string message)
        {
            NewLine();
            Display(message, EColor.Yellow);
        }

        public void DisplayWarning(string message)
        {
            NewLine();
            Display(message, EColor.Purple);
        }

        public void ProcessInputString(string input)
        {
            prevCommands.Add(input);
            pointer = 0;

            CommandsParser.ParseCommand(input);
        }

        public string ProcessDownArrow()
        {
            if (pointer > 0) pointer--;
            else return "";
            return prevCommands[^(pointer + 1)];
        }

        public string ProcessUpArrow()
        {
            string command = prevCommands[^(pointer + 1)];
            if (pointer + 1 < prevCommands.Count) pointer++;
            return command;
        }

        public bool TransferringInput
        {
            get => mainWindowInstance.TransferringInput;
            set => mainWindowInstance.TransferringInput = value;
        }

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
                        lastColor = ColorsController.Parse(colorName);
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

        public event TransferringEventHandler? SomethingTranferred;

        public event EventHandler? CtrlCTransferred;
    }

    public class TransferringEventArgs : EventArgs
    {
        public TransferringEventArgs(string input)
        {
            Input = input;
        }

        public string Input { get; set; }
    }

    public delegate void TransferringEventHandler(object sender, TransferringEventArgs args);
}
