using SpecialTask.Infrastructure;
using SpecialTask.Infrastructure.CommandInfrastructure;
using SpecialTaskConverter;
using System.IO;

namespace SpecialTask.Console.Commands.CommandClasses
{
    /// <summary>
	/// Export SVG
	/// </summary>
	internal class ExportSVGCommand : ICommand
    {
        private readonly STConverter? receiver;

        private readonly string inFilename = string.Empty;
        private readonly string outFilename;

        private readonly bool createdTempFile = false;

        public ExportSVGCommand(object[] args)
        {
            inFilename = (string)args[0];
            outFilename = (string)args[1];

            if (inFilename.Length == 0)
            {
                inFilename = PathsController.CorrectFilename(PathsController.DateTimeFilename, PathsController.DefaultSaveDirectory, ".std");
                CommandsFacade.Execute(new SaveAsCommand(inFilename));
                createdTempFile = true;
            }
            else inFilename = PathsController.CorrectFilename(inFilename, PathsController.DefaultSaveDirectory, ".std");

            try { receiver = new(inFilename); }
            catch (FileNotFoundException)
            {
                Logger.Error($"Cannot export SVG: File {inFilename} not found");
                HighConsole.DisplayError($"File {inFilename} not found");
            }
        }

        public void Execute()
        {
            receiver?.ToSVG(PathsController.CorrectFilename(outFilename, PathsController.DefaultSaveDirectory, ".svg"));

            if (createdTempFile)        // FIXME: Stream не успевает закрыться до этого момента
            {
                try { File.Delete(inFilename); }
                catch (Exception) { /* ignore */ }
            }
        }

        public void Unexecute()
        {
            Logger.Warning("Unexecution of export SVG command");
        }
    }
}
