using SpecialTask.Infrastructure;
using System.IO;
using System.Windows.Media.Imaging;

namespace SpecialTask.Console.Commands.CommandClasses
{
    /// <summary>
    /// Command to capture a screenshot of canvas (for tests)
    /// </summary>
    class ScreenshotCommand : ICommand
    {
        private string filename;

        public ScreenshotCommand(object[] args)
        {
            filename = (string)args[0];
        }

        public void Execute()
        {
            BitmapSource bmp = CurrentWindow.CanvasBitmapSource;

            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(bmp));

            if (!Path.IsPathFullyQualified(filename)) filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), filename);
            filename = Path.ChangeExtension(filename, ".png");

            using Stream stream = File.Create(filename);

            encoder.Save(stream);
        }

        public void Unexecute()
        {
            Logger.Warning("Unexecution of screenshot command");
        }
    }
}
