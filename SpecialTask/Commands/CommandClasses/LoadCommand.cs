using System.IO;

namespace SpecialTask.Commands.CommandClasses
{
    /// <summary>
	/// "Load" command
	/// </summary>
	class LoadCommand : ICommand
    {
        // no receiver, because SaveLoadFacade is static

        readonly string filename;
        readonly bool clearScreen = false;

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
            catch (LoadXMLError)
            {
                Logger.Instance.Error($"Cannot load {filename}: invalid file format");
                MiddleConsole.HighConsole.DisplayError("Invalid file format");
            }
            catch (FileNotFoundException)
            {
                Logger.Instance.Error($"Cannot load {filename}: file not found");
                MiddleConsole.HighConsole.DisplayError("File not found");
            }
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of load command");
        }
    }
}
