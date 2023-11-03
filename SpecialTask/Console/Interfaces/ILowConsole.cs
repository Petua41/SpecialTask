namespace SpecialTask.Console
{
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
        public void ProcessCtrlC();

    }
}
