﻿using SpecialTask.Helpers.CommandHelpers;
using SpecialTaskConverter;
using System.IO;

namespace SpecialTask.Console.Commands.CommandClasses
{
    /// <summary>
    /// Export PDF
    /// </summary>
    class ExportPDFCommand : ICommand
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
                inFilename = SaveLoadFacade.CorrectFilename(DateTime.Now.ToString().Replace(':', '.'));
                CommandsFacade.Execute(new SaveAsCommand(inFilename));
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
                HighConsole.DisplayError($"Cannot open {correctedFilename} for writing");
            }
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of export PDF command");
        }
    }
}
