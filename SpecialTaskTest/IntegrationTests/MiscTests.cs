using FlaUI.Core.AutomationElements;

namespace SpecialTaskTest.IntegrationTests
{
    internal class MiscTests: TestsBase
    {
        [Test]
        public void OpenApplicationTest()
        {
            List<Window> windows = Windows;
            Assert.That(windows, Has.Count.EqualTo(2));

            Window? drawingWindow1 = windows.Find(x => x.Title == "Drawing window 0");
            Assert.That(mainWindow, Is.Not.Null);
        }

        [Test]
        public void CommandHelpTest()
        {
            EnterCommand("new circle --help");
            Assert.That(ConsoleOutput, Does.Contain("Adds circle to the screen"));
        }
    }
}
