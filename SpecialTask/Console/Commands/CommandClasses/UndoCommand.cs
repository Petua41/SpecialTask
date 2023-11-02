using System;
using SpecialTask.Console;
using SpecialTask.Helpers;

namespace SpecialTask.Console.Commands.CommandClasses
{
    /// <summary>
    /// Command to undo commands
    /// </summary>
    class UndoCommand : ICommand
    {
        // There`s no receiver, `cause CommandsFacade is static
        private readonly int number;

        public UndoCommand(object[] args)
        {
            number = (int)args[0];
        }

        public void Execute()
        {
            try { CommandsFacade.UndoCommands(number); }
            catch (InvalidOperationException)
            {
                Logger.Instance.Error("Noting to undo!");
                MiddleConsole.HighConsole.DisplayWarning("Nothung to undo!");
            }
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of undo command");
        }
    }
}
