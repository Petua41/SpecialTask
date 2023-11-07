using SpecialTask.Infrastructure.Events;

namespace SpecialTask.Console.Interfaces
{
    /// <summary>
    /// High-level console interface for business classes
    /// </summary>
    public interface IHighConsole
    {
        void DisplayGlobalHelp();
        void DisplayError(string message);
        void DisplayWarning(string message);
        void DisplayQuestion(string message);
        void Display(string message);
        void NewLine();
        void DisplayPrompt();

        bool TransferringInput { get; set; }

        event TransferringEventHandler? SomethingTranferred;
        event EventHandler? CtrlCTransferred;
    }
}
