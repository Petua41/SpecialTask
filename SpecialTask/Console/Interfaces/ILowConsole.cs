namespace SpecialTask.Console.Interfaces
{
    /// <summary>
    /// Low-level console interface for WPF classes
    /// </summary>
    public interface ILowConsole
    {
        void DisplayPrompt();
        string Autocomplete(string currentInput);
        string ProcessUpArrow();
        string ProcessDownArrow();
        void ChangeUndoStackDepth(int depth);
        void ProcessInputString(string input);
        void NewLine();
        void ProcessCtrlC();
    }
}
