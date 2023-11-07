using SpecialTask.Infrastructure.CommandHelpers.SaveLoad;

namespace SpecialTask.Console.Commands.ConcreteCommands
{
    /// <summary>
    /// "Save" command
    /// </summary>
    internal class SaveCommand : ICommand
    {
        // no receiver, because SaveLoadFacade is static

        public SaveCommand() { }

        public void Execute()
        {
            try { SaveLoadFacade.Instance.Save(); }
            catch (InvalidOperationException)
            {
                Logger.Warning("Nothing to save");
                HighConsole.DisplayWarning("File is already saved");
            }
        }

        public void Unexecute()
        {
            Logger.Warning("Unexecution of save command");
        }
    }
}
