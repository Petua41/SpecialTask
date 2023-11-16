namespace SpecialTaskTest.IntegrationTests.CommandsTests
{
    internal class WindowTests : TestsBase
    {
        [Test]
        public void CreateWindowTest()
        {
            EnterCommand("window create");

            Assert.That(Windows, Has.Count.EqualTo(3));     // Console and two DrawingWindow`s
        }

        [Test]
        public void DeleteWindowTest()
        {
            EnterCommand("window create");
            EnterCommand("window delete -n1");      // numbers from zero

            Assert.That(Windows, Has.Count.EqualTo(2));     // Console and DrawingWindow
        }

        [Test]
        public void SwitchWindowTest()
        {   // Draw circle on first window, then switch to second and test that there`s no circle on it
            EnterCommand("new circle -x100 -y100 -r80 -t3 -Cred");
            EnterCommand("window create");
            EnterCommand("window switch -n1");      // numbers from zero
            TestScreenshot("new rectangle -x100 -y100 -X200 -Y200 -t5 -Cbrightgreen", Properties.Resources.Regular_square);
        }
    }
}
