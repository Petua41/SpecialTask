using System;
using SpecialTask.Helpers;

namespace SpecialTask.Console
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
}
