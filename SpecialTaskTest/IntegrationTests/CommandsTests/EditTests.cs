using static SpecialTaskTest.Properties.Resources;

namespace SpecialTaskTest.IntegrationTests.CommandsTests
{
    internal class EditTests : TestsBase
    {
        // Edit layer:
        [Test]
        public void BringForwardTest()
        {
            EnterCommand("new rectangle -x100 -y100 -X200 -Y200 -Cgreen -t5");  // create circle
            EnterCommand("new circle -x100 -y100 -r80 -t3 -Cred");              // create square
            EnterCommand("edit");
            EnterCommand("0");                                                  // select square
            EnterCommand("0");                                                  // select layer operations
            TestScreenshot("1", Circle_and_square);                             // select "bring forward". Circle should be below square
        }

        [Test]
        public void SendBackwardsTest()
        {
            EnterCommand("new rectangle -x100 -y100 -X200 -Y200 -Cgreen -t5");  // create circle
            EnterCommand("new circle -x100 -y100 -r80 -t3 -Cred");              // create square
            EnterCommand("edit");
            EnterCommand("1");                                                  // select circle
            EnterCommand("0");                                                  // select layer operations
            TestScreenshot("0", Circle_and_square);                             // select "send backwards". Circle should be below square
        }

        [Test]
        public void BringToFrontTest()
        {
            EnterCommand("new circle -x100 -y100 -r80 -t3 -Cred");
            EnterCommand("new rectangle -x100 -y100 -X200 -Y200 -t5 -Cgreen");
            EnterCommand("new line -x10 -y10 -X210 -Y210 -t7 -Cblue");
            EnterCommand("edit");
            EnterCommand("1");      // select square
            EnterCommand("0");      // select layer operations
            TestScreenshot("3", Circle_line_square);      // select "bring to front". Should be: circle, line, square (from back to top)
        }

        [Test]
        public void SendToBackTest()
        {
            EnterCommand("new circle -x100 -y100 -r80 -t3 -Cred");
            EnterCommand("new rectangle -x100 -y100 -X200 -Y200 -t5 -Cgreen");
            EnterCommand("new line -x10 -y10 -X210 -Y210 -t7 -Cblue");
            EnterCommand("edit");
            EnterCommand("1");      // select square
            EnterCommand("0");      // select layer operations
            TestScreenshot("2", Square_circle_line);      // select "send to back". Should be: square, circle, line (from back to top)
        }

        // Edit attributes:
        [Test]
        public void EditShapeAttributesTest()
        {
            EnterCommand("new circle -x100 -y100 -r8 -t3 -Cred");
            EnterCommand("edit");
            EnterCommand("0");  // select circle
            EnterCommand("1");  // select "edit shape attributes"
            EnterCommand("2");  // select "radius"
            TestScreenshot("80", Regular_circle);   // new radius is 80
        }

        // Remove shape:
        [Test]
        public void RemoveShapeTest()
        {
            EnterCommand("new rectangle -x100 -y100 -X200 -Y200 -Cgreen -t5");
            EnterCommand("new circle -x100 -y100 -r80 -t3 -Cred");
            EnterCommand("edit");
            EnterCommand("0");  // select square
            TestScreenshot("2", Regular_circle);  // select "remove shape"
        }

        // Add streak:
        [Test]
        public void AddStreakTest()
        {
            EnterCommand("new circle -x100 -y100 -r80 -t3 -Cyellow");
            EnterCommand("edit");
            EnterCommand("0");  // select circle
            EnterCommand("3");  // select "add streak"
            EnterCommand("yellow"); // streak color
            TestScreenshot("hl", Horizontal_lines);     // streak texture
        }
    }
}
