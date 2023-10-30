using System;
using System.Collections.Generic;

namespace SpecialTask.Commands.CommandClasses
{
    /// <summary>
	/// "Save" command
	/// </summary>
	class SaveCommand : ICommand
    {
        // no receiver, because SaveLoadFacade is static

        public SaveCommand() { }

        public void Execute()
        {
            try { SaveLoadFacade.Instance.Save(); }
            catch (InvalidOperationException)
            {
                Logger.Instance.Warning("Nothing to save");
                MiddleConsole.HighConsole.DisplayWarning("File is already saved");
            }
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of save command");
        }
    }
}
