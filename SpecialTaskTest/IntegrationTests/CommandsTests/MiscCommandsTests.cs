using static SpecialTaskTest.Properties.Resources;
using static System.Threading.Thread;

namespace SpecialTaskTest.IntegrationTests.CommandsTests
{
    internal class MiscCommandsTests : TestsBase
    {
        [Test]
        public void ClearTest()
        {
            EnterCommand("new circle -x100 -y100 -r80 -t3 -Cred");
            TestScreenshot("clear", Empty_screen);
        }

        [Test]
        public void SelectPasteTest()
        {
            EnterCommand("new circle -x100 -y100 -r80 -t3 -Cred");
            EnterCommand("select -x10 -y10 -X210 -Y210");
            Sleep(3000);        // wait until selection marker disappears
            TestScreenshot("paste -x250 -y250", Pasted_circle);     // Test that circle duplicates and selection marker disappears
        }

        [Test]
        public void UndoTest()
        {
            EnterCommand("new circle -x100 -y100 -r80 -t3 -Cred");
            TestScreenshot("undo", Empty_screen);
        }

        [Test]
        public void RedoTest()
        {
            EnterCommand("new circle -x100 -y100 -r80 -t3 -Cred");              // create circle
            EnterCommand("undo");                                               // undo circle creation
            EnterCommand("new rectangle -x100 -y100 -X200 -Y200 -Cgreen -t5");  // create square
            TestScreenshot("redo", Circle_and_square);                          // redo circle creation. Should be circle and square
        }
    }
}
