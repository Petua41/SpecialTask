using SpecialTask.Infrastructure.CommandHelpers.SaveLoad;
using SpecialTask.Infrastructure.Exceptions;
using System.IO;

namespace SpecialTask.Console.Commands.ConcreteCommands
{
    /// <summary>
    /// "Load" command
    /// </summary>
    internal class LoadCommand : ICommand
    {
        // no receiver, because SaveLoadFacade is static

        private readonly string filename;
        private readonly bool clearScreen = false;

        public LoadCommand(object[] args)
        {
            filename = (string)args[0];
            clearScreen = (bool)args[1];
        }

        public void Execute()
        {
            if (clearScreen)
            {
                ICommand command = new ClearCommand();
                CommandsFacade.Register(command);
                CommandsFacade.Execute(command);
            }

            try { SaveLoadFacade.Instance.Load(filename); }
            catch (LoadXMLException)
            {
                Logger.Error($"Cannot load {filename}: invalid file format");
                HighConsole.DisplayError("Invalid file format");
            }
            catch (FileNotFoundException)
            {
                Logger.Error($"Cannot load {filename}: file not found");
                HighConsole.DisplayError("File not found");
            }
        }

        public void Unexecute()
        {
            Logger.Warning("Unexecution of load command");
        }
    }
}
