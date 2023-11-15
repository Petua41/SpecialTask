using SpecialTask.Console.Commands;
using SpecialTask.Console.CommandsParser;
using SpecialTask.Console.Interfaces;
using SpecialTask.Infrastructure.Collections;
using SpecialTask.Infrastructure.Enums;
using SpecialTask.Infrastructure.Events;
using SpecialTask.Infrastructure.Exceptions;
using System.Windows;
using static SpecialTask.Infrastructure.Extensoins.InternalColorExtensions;
using static SpecialTask.Infrastructure.Extensoins.StringExtensions;

namespace SpecialTask.Console
{
    /// <summary>
    /// Like mediator for the whole system: knows to whom send which query
    /// </summary>
    public class MiddleConsole : IHighConsole, ILowConsole          // double-singleton
    {
        private static readonly object syncLock = new();
        private static volatile MiddleConsole? singleton;
        private readonly MainWindow mainWindowInstance;

        private readonly List<string> prevCommands = new();
        private int pointer = 0;                    // from end

        private MiddleConsole()
        {
            if (Application.Current.MainWindow is not MainWindow mw)
            {
                Logger.Fatal("Cannot get MainWindow");
                Application.Current.Shutdown();                     // it`s bad idea. I should catch FatalError somewhere on the top of hierarchy
                throw new FatalError("Cannot get MainWindow");
            }
            mainWindowInstance = mw;
        }

        public static IHighConsole HighConsole
        {
            get
            {
                if (singleton is not null) return singleton;

                lock (syncLock)
                {
                    singleton ??= new();
                }
                return singleton;
            }
        }

        public static ILowConsole LowConsole
        {
            get
            {
                if (singleton is not null) return singleton;

                lock (syncLock)
                {
                    singleton ??= new();
                }
                return singleton;
            }
        }

        public string Autocomplete(string currentInput)
        {
            return ConsoleCommandsParser.Autocomplete(currentInput);
        }

        public void ChangeUndoStackDepth(int depth)
        {
            CommandsFacade.ChangeUndoStackDepth(depth);
        }

        public void DisplayError(string message)
        {
            NewLine();
            Display(message, InternalColor.Red);
        }

        public void Display(string message)
        {
            Pairs<string, InternalColor> messageSplittedByColors = message.SplitByColors();
            foreach (KeyValuePair<string, InternalColor> kvp in messageSplittedByColors)
            {
                Display(kvp.Key, kvp.Value);
            }
        }

        public void NewLine()
        {
            Display(Environment.NewLine, InternalColor.None);      // no matter, in which color we display \n
        }

        public void DisplayGlobalHelp()
        {
            Display(XMLCommandsParser.GlobalHelp);
        }

        public void DisplayPrompt()
        {
            Display(">> ", InternalColor.Green);
        }

        public void DisplayQuestion(string message)
        {
            NewLine();
            Display(message, InternalColor.Yellow);
        }

        public void DisplayWarning(string message)
        {
            NewLine();
            Display(message, InternalColor.Purple);
        }

        public void ProcessInputString(string input)
        {
            if (TransferringInput)
            {
                SomethingTranferred?.Invoke(this, new(input));
            }
            else
            {
                prevCommands.Add(input);
                pointer = 0;

                ConsoleCommandsParser.ParseCommand(input);
            }
        }

        public string ProcessDownArrow()
        {
            if (TransferringInput)
            {
                return string.Empty;
            }

            if (pointer > 0)
            {
                pointer--;
            }
            else
            {
                return string.Empty;
            }

            return prevCommands[^(pointer + 1)];
        }

        public string ProcessUpArrow()
        {
            if (TransferringInput)
            {
                return string.Empty;
            }

            if (prevCommands.Count == 0)
            {
                return string.Empty;
            }

            string command = prevCommands[^(pointer + 1)];
            if (pointer + 1 < prevCommands.Count)
            {
                pointer++;
            }

            return command;
        }

        public void ProcessCtrlC()
        {
            if (TransferringInput)
            {
                CtrlCTransferred?.Invoke(this, new());
            }
            else
            {
                mainWindowInstance.DisplayInputString("exit");
                ProcessInputString("exit");
            }
        }

        public bool TransferringInput { get; set; }

        private void Display(string message, InternalColor color = InternalColor.White)
        {
            mainWindowInstance.Display(message, color.GetWPFColor());
        }

        public event TransferringEventHandler? SomethingTranferred;

        public event EventHandler? CtrlCTransferred;
    }
}
