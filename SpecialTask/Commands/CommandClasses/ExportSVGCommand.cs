using SpecialTaskConverter;
using System;
using System.IO;

namespace SpecialTask.Commands.CommandClasses
{
    /// <summary>
	/// Export SVG
	/// </summary>
	class ExportSVGCommand : ICommand
    {
        private readonly STConverter? receiver;

        private readonly string inFilename = "";
        private readonly string outFilename;

        private readonly bool createdTempFile = false;

        public ExportSVGCommand(object[] args)
        {
            inFilename = (string)args[0];
            outFilename = (string)args[1];

            if (inFilename.Length == 0)
            {
                inFilename = SaveLoadFacade.CorrectFilename(DateTime.Now.ToString().Replace(':', '.'));
                CommandsFacade.Execute(new SaveAsCommand(inFilename));
                createdTempFile = true;
            }
            else inFilename = SaveLoadFacade.CorrectFilename(inFilename);

            try { receiver = new(inFilename); }
            catch (FileNotFoundException)
            {
                Logger.Instance.Error($"Cannot export SVG: File {inFilename} not found");
                MiddleConsole.HighConsole.DisplayError($"File {inFilename} not found");
            }
        }

        public void Execute()
        {
            receiver?.ToSVG(SaveLoadFacade.CorrectFilename(outFilename, ".svg"));

            if (createdTempFile)        // FIXME: Stream не успевает закрыться до этого момента
            {
                try { File.Delete(inFilename); }
                catch (Exception) { /* ignore */ }
            }
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of export SVG command");
        }
    }
}
