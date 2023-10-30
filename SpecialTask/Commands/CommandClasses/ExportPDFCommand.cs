using SpecialTaskConverter;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SpecialTask.Commands.CommandClasses
{
    /// <summary>
    /// Export PDF
    /// </summary>
    class ExportPDFCommand : ICommand
    {
        private readonly STConverter receiver;

        private readonly string inFilename = "";
        private readonly string outFilename;

        public ExportPDFCommand(object[] args)
        {
            inFilename = (string)args[0];
            outFilename = (string)args[1];

            if (inFilename.Length == 0 || inFilename == "_")
            {
                inFilename = SaveLoadFacade.CorrectFilename(DateTime.Now.ToString().Replace(':', '.'));
                CommandsFacade.ExecuteButDontRegister(new SaveAsCommand(inFilename));
            }
            else inFilename = SaveLoadFacade.CorrectFilename(inFilename);

            receiver = new(inFilename);
        }

        public async void Execute()
        {
            string correctedFilename = SaveLoadFacade.CorrectFilename(outFilename, ".pdf");

            try { await Task.Run(() => { receiver.ToPDF(correctedFilename); }); }
            catch (IOException)
            {
                Logger.Instance.Error($"Cannot export PDF: cannot open {correctedFilename} for writing");
                MiddleConsole.HighConsole.DisplayError($"Cannot open {correctedFilename} for writing");
            }
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of export PDF command");
        }
    }
}
