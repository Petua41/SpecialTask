using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Input;
using FlaUI.UIA3;
using System.Drawing.Imaging;
using System.Drawing;
using System.Runtime.InteropServices;
using static System.Threading.Thread;

namespace SpecialTaskTest.IntegrationTests
{
    class TestsBase
    {
        private const string EXEC_NAME = "C:\\Users\\Petua\\source\\repos\\SpecialTask\\SpecialTask\\bin\\Debug\\net7.0-windows\\SpecialTask.exe";
        private readonly string DIR_FOR_SCREENSHOTS = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + "\\Temp";

        protected Application app;
        protected UIA3Automation automation;
        protected Window mainWindow;
        protected TextBox consoleEntry;

        [SetUp]
        public void SetUpApplication()
        {
            app = Application.Launch(EXEC_NAME);
            automation = new();

            Sleep(900);

            Window? win = Windows.Find(x => x.Title == "Console");
            Assert.That(win, Is.Not.Null);
            mainWindow = win;

            AutomationElement? consoleEntryElem = FindElementOnMainWindow("ConsoleEntry");
            Assert.That(consoleEntryElem, Is.Not.Null);
            consoleEntry = consoleEntryElem.AsTextBox();
        }

        [TearDown]
        public void TearDownApplication()
        {
            app.Close();
            app.Dispose();
            automation.Dispose();
        }

        protected AutomationElement? FindElementOnMainWindow(string name)
        {
            return mainWindow.FindFirstDescendant(x => x.ByAutomationId(name));
        }

        protected void EnterCommand(string command)
        {
            consoleEntry.Focus();
            
            Keyboard.Type(command);
            Wait.UntilInputIsProcessed();

            Keyboard.Press(FlaUI.Core.WindowsAPI.VirtualKeyShort.ENTER);
            Wait.UntilInputIsProcessed();
            app.WaitWhileBusy();

            Sleep(500);
        }

        protected void TestScreenshot(string command, Bitmap expected)
        {
            EnterCommand(command);

            if (!Directory.Exists(DIR_FOR_SCREENSHOTS)) Directory.CreateDirectory(DIR_FOR_SCREENSHOTS);

            string filename = Path.Combine(DIR_FOR_SCREENSHOTS, DateTime.Now.Ticks.ToString());
            filename = Path.ChangeExtension(filename, ".png");

            EnterCommand($"screenshot -f{filename}");
            Bitmap actual = new(filename);

            bool result = CompareBitmaps(expected, actual);
            Assert.That(result, Is.True);
        }

        protected List<Window> Windows => app.GetAllTopLevelWindows(automation).ToList();

        private static bool CompareBitmaps(Bitmap bmp1, Bitmap bmp2)
        {
            if (bmp1 is null || bmp2 is null)
                return false;
            if (Equals(bmp1, bmp2))
                return true;
            if (!bmp1.Size.Equals(bmp2.Size) || !bmp1.PixelFormat.Equals(bmp2.PixelFormat))
                return false;

            int bytes = bmp1.Width * bmp1.Height * (Image.GetPixelFormatSize(bmp1.PixelFormat) / 8);

            byte[] b1bytes = new byte[bytes];
            byte[] b2bytes = new byte[bytes];

            BitmapData bitmapData1 = bmp1.LockBits(new Rectangle(0, 0, bmp1.Width, bmp1.Height), ImageLockMode.ReadOnly, bmp1.PixelFormat);
            BitmapData bitmapData2 = bmp2.LockBits(new Rectangle(0, 0, bmp2.Width, bmp2.Height), ImageLockMode.ReadOnly, bmp2.PixelFormat);

            Marshal.Copy(bitmapData1.Scan0, b1bytes, 0, bytes);
            Marshal.Copy(bitmapData2.Scan0, b2bytes, 0, bytes);

            var a = (ReadOnlySpan<byte>)b1bytes;
            var b = (ReadOnlySpan<byte>)b2bytes;
            bool result = a.SequenceEqual(b);

            bmp1.UnlockBits(bitmapData1);
            bmp2.UnlockBits(bitmapData2);

            return result;
        }
    }
}
