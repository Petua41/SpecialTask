using static SpecialTaskTest.Properties.Resources;

namespace SpecialTaskTest.IntegrationTests.CommandsTests
{
    internal class NewTests: TestsBase
    {
        [Test]
        public void RegularNewCircleTest()
        {
            TestScreenshot("new circle -x100 -y100 -r80 -t3 -Cred", Regular_circle);
        }

        [Test]
        public void RegularNewRectangleTest()
        {
            TestScreenshot("new rectangle -x100 -y100 -X200 -Y200 -t5 -Cbrightgreen", Regular_square);
        }

        [Test]
        public void RegularNewPolygonTest()
        {
            TestScreenshot("new polygon -p50 50, 70 80, 90 150, 60 200, 10 100 -t4 -Cmagenta", Regular_polygon);
        }

        [Test]
        public void RegularNewTextTest()
        {
            TestScreenshot("new text -x100 -y100 -f90 -tRegular text -Cblue", Regular_text);
        }

        [Test]
        public void RegularNewLineTest()
        {
            TestScreenshot("new line -x100 -y100 -X300 -Y300 -t3 -Cblack", Regular_line);
        }
    }
}
