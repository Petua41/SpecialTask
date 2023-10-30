using System;
using System.IO;

namespace SpecialTask.Commands.CommandClasses
{
    /// <summary>
	/// "Save as" command
	/// </summary>
	class SaveAsCommand : ICommand
    {
        // no receiver, because SaveLoadFacade is static
        readonly string filename;

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
                    Logger.Instance.Error($"Cannot save to {filename}: invalid characters");
                    MiddleConsole.HighConsole.DisplayError($"Filename cannot contain theese characters: {string.Join(string.Empty, Path.GetInvalidFileNameChars())}");
                }
                else
                {
                    Logger.Instance.Error($"Cannot save to {filename}: directory {dir} doesn`t exists");
                    MiddleConsole.HighConsole.DisplayError($"Directory {dir} doesn`t exist");
                }
            }
            catch (UnauthorizedAccessException)
            {
                Logger.Instance.Error($"Cannot save to: {filename}: no permissions");
                MiddleConsole.HighConsole.DisplayError($"you have no permission to write to {filename}. This incident will be reported");
            }
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of save_as command");
        }
    }
}
