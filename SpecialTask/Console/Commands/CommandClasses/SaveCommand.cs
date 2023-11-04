using SpecialTask.Helpers.CommandHelpers;

namespace SpecialTask.Console.Commands.CommandClasses
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
                HighConsole.DisplayWarning("File is already saved");
            }
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of save command");
        }
    }
}
