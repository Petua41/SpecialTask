using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SpecialTask.Commands.CommandClasses
{
    /// <summary>
    /// Command to capture a screenshot of canvas (for tests)
    /// </summary>
    class ScreenshotCommand : ICommand
    {
        private readonly WindowManager receiver;

        private string filename;

        public ScreenshotCommand(object[] args)
        {
            receiver = WindowManager.Instance;
            filename = (string)args[0];
        }

        public void Execute()
        {
            Canvas canvas = receiver.Canvas;

            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;

            RenderTargetBitmap bmp = new((int)width, (int)height, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(canvas);

            PngBitmapEncoder encoder = new();
            encoder.Frames.Add(BitmapFrame.Create(bmp));

            if (!Path.IsPathFullyQualified(filename)) filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), filename);
            filename = Path.ChangeExtension(filename, ".png");

            using Stream stream = File.Create(filename);

            encoder.Save(stream);
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of screenshot command");
        }
    }
}
