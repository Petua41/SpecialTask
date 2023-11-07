using SpecialTask.Infrastructure;
using SpecialTaskConverter;
using System.IO;

namespace SpecialTask.Console.Commands.ConcreteCommands
{
    /// <summary>
    /// Export PDF
    /// </summary>
    internal class ExportPDFCommand : ICommand
    {
        private readonly STConverter receiver;

        private readonly string inFilename = string.Empty;
        private readonly string outFilename;

        public ExportPDFCommand(object[] args)
        {
            inFilename = (string)args[0];
            outFilename = (string)args[1];

            if (inFilename.Length == 0)
            {
                inFilename = PathsController.CorrectFilename(PathsController.DateTimeFilename, PathsController.DefaultSaveDirectory, ".std");
                CommandsFacade.Execute(new SaveAsCommand(inFilename));
            }
            else
            {
                inFilename = PathsController.CorrectFilename(inFilename, PathsController.DefaultSaveDirectory, ".std");
            }

            receiver = new(inFilename);
        }

        public async void Execute()
        {
            string correctedFilename = PathsController.CorrectFilename(outFilename, PathsController.DefaultSaveDirectory, ".pdf");

            try { await Task.Run(() => { receiver.ToPDF(correctedFilename); }); }
            catch (IOException)
            {
                Logger.Error($"Cannot export PDF: cannot open {correctedFilename} for writing");
                HighConsole.DisplayError($"Cannot open {correctedFilename} for writing");
            }
        }

        public void Unexecute()
        {
            Logger.Warning("Unexecution of export PDF command");
        }
    }
}
