using SpecialTask.Infrastructure.CommandHelpers.SaveLoad;
using System.IO;

namespace SpecialTask.Console.Commands.ConcreteCommands
{
    /// <summary>
    /// "Save as" command
    /// </summary>
    internal class SaveAsCommand : ICommand
    {
        // no receiver, because SaveLoadFacade is static
        private readonly string filename;

        public SaveAsCommand(string filename)
        {
            this.filename = filename;
        }

        public SaveAsCommand(object[] args) : this((string)args[0]) { }

        public void Execute()
        {
            try { SaveLoadFacade.Instance.SaveAs(filename); }
            catch (IOException)
            {
                string? dir = Path.GetDirectoryName(filename);

                if (Directory.Exists(dir))
                {
                    Logger.Error($"Cannot save to {filename}: invalid characters");
                    HighConsole.DisplayError($"Filename cannot contain theese characters: {string.Join(string.Empty, Path.GetInvalidFileNameChars())}");
                }
                else
                {
                    Logger.Error($"Cannot save to {filename}: directory {dir} doesn`t exists");
                    HighConsole.DisplayError($"Directory {dir} doesn`t exist");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Logger.Error($"Cannot save to: {filename}: no permissions");
                HighConsole.DisplayError($"you have no permission to write to {filename}. This incident will be reported");
            }
        }

        public void Unexecute()
        {
            Logger.Warning("Unexecution of save_as command");
        }
    }
}
