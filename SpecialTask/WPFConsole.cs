using System;
using System.Windows;

namespace SpecialTask
{
    /// <summary>
    /// Класс-обёртка для консоли. Выступает посредником между бизнес-классом STConsole и "низкоуровневой" частью приложения. 
    /// Consider ALWAYS using STConsole`s methods instead of WPFConsole`s
    /// </summary>
    public class WPFConsole
    {
        private static WPFConsole? singleton;
        private readonly MainWindow mainWindowInstance;
        private string interceptedString = "";
        private char? lastInterceptedChar = null;
        private ESpecialKeyCombinations lastInterceptedCombination = ESpecialKeyCombinations.None;
        private string lastInterceptedString = "";

        private WPFConsole()
        {
            if (singleton != null) throw new SingletonError();
            try { mainWindowInstance = (MainWindow)Application.Current.MainWindow; }
            catch (Exception ex)
            {
                Logger.Instance.Error(string.Format("{0} exception while trying to get MainWindow instance!", ex.GetType().ToString()));
                throw;
            }
        }

        public static WPFConsole Instance
        {
            get
            {
                singleton ??= new WPFConsole();
                return singleton;
            }
        }

        public void Display(string message, EColor color = EColor.None)
        {
            mainWindowInstance.Display(message, color.GetWPFColor());
        }

        public void NewLine()
        {
            Display(Environment.NewLine);
        }

        /// <summary>
        /// Выводит приглашение ко вводу и разрешает ввод. ДОЛЖЕН вызываться каждый раз, когда пользователь может что-то ввести
        /// </summary>
        public void DisplayPrompt()
        {
            Display(">> ", EColor.Green);
            InputBlocked = false;
        }

        public void ProcessInputString(string input)
        {
            STConsole.Instance.ProcessInput(input);
            DisplayPrompt();
        }

        public string Autocomplete(string currentInput)
        {
            return STConsole.Instance.Autocomplete(currentInput);
        }

        public string ProcessUpArrow()
        {
            // TODO: я ещё не решил, как будут храниться предыдущие команды
            throw new NotImplementedException();
        }

        public string ProcessDownArrow()
        {
            // TODO: я ещё не решил, как будут храниться предыдущие команды
            throw new NotImplementedException();
        }

        public void ChangeUndoStackDepth(int depth)
        {
            CommandsFacade.ChangeUndoStackDepth(depth);
        }

        public bool InputBlocked
        {
            get => mainWindowInstance.InputBlocked;
            set => mainWindowInstance.InputBlocked = value;
        }

        public bool TransferringInput
        {
            get => mainWindowInstance.TransferringInput;
            set => mainWindowInstance.TransferringInput = value;
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

        public string GetTransferredString()
        {
            return lastInterceptedString;
        }

        public char? GetTransferredChar()
        {
            return lastInterceptedChar;
        }

        public ESpecialKeyCombinations GetTransferredCombination()
        {
            return lastInterceptedCombination;
        }

        public event EventHandler? SomethingTranferred;
    }
}
