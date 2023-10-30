using System;
using System.Collections.Generic;

namespace SpecialTask.Commands.CommandClasses
{
    /// <summary>
	/// Command to redo undone commands
	/// </summary>
	class RedoCommand : ICommand
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
                Logger.Instance.Warning("Nothing to redo");
                MiddleConsole.HighConsole.DisplayWarning("Nothing to redo!");
            }
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of redo command");
        }
    }
}
