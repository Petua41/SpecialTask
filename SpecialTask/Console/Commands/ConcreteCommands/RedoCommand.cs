namespace SpecialTask.Console.Commands.ConcreteCommands
{
    /// <summary>
    /// Command to redo undone commands
    /// </summary>
    internal class RedoCommand : ICommand
    {
        // There`s no receiver, `cause CommandsFacade is static
        private readonly int number;

        public RedoCommand(object[] args)
        {
            number = (int)args[0];
        }

        public void Execute()
        {
            try { CommandsFacade.RedoCommands(number); }
            catch (InvalidOperationException)
            {
                Logger.Warning("Nothing to redo");
                HighConsole.DisplayWarning("Nothing to redo!");
            }
        }

        public void Unexecute()
        {
            Logger.Warning("Unexecution of redo command");
        }
    }
}
