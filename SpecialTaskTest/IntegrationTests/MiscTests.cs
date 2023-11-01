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
        public void CreateWindowTest()
        {
            EnterCommand("window create");

            Assert.That(Windows, Has.Count.EqualTo(3));
        }
    }
}
