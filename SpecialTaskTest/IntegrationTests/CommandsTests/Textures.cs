using static SpecialTaskTest.Properties.Resources;

namespace SpecialTaskTest.IntegrationTests.CommandsTests
{
    internal class Textures : TestsBase
    {
        [Test]
        public void SolidColorTest()
        {
            TestScreenshot("new circle -x100 -y100 -r80 -t3 -Cred -s -cbrightred -Tsolid", Solid_color);
        }

        [Test]
        public void HorizontalLinesTest() 
        {
            TestScreenshot("new circle -x100 -y100 -r80 -t3 -Cyellow -s -cyellow -Thl", Horizontal_lines);
        }

        [Test]
        public void VerticalLinesTest()
        {
            TestScreenshot("new rectangle -x100 -y100 -X300 -Y300 -t10 -Cbrightmagenta -s -cbrightmagenta -Tvl", Vertical_lines);
        }

        [Test]
        public void HorizontalTranspGradTest()
        {
            TestScreenshot("new circle -x100 -y100 -r80 -t0 -Cgreen -s -cgreen -Thtg", Horiz_transp_grad);
        }

        [Test]
        public void RainbowGradTest()
        {
            TestScreenshot("new circle -x200 -y200 -r180 -t1 -Cblack -s -cblack -Trainbow", Rainbow);
        }

        [Test]
        public void RadialTranspGradTest()
        {
            TestScreenshot("new polygon -p100 10, 40 300, 20 60, 90 150, 150 200, 40 50 -Cbrightblue -s -cbrightblue -t2 -Trtg", Radial_transp_grad);
        }

        [Test]
        public void WaterTest()
        {
            TestScreenshot("new rectangle -x100 -y100 -X500 -Y200 -Cred -t3 -s -cred -TWater", Water);
        }

        [Test]
        public void DotsTest()
        {
            TestScreenshot("new circle -x100 -y100 -r80 -t3 -Cbrightyellow -cbrightyellow -s -Tdots", Dots);
        }

        [Test]
        public void HolesTest()
        {
            TestScreenshot("new rectangle -x100 -y100 -X200 -Y200 -Cblack -s -cblack -t1 -Tholes", Holes);
        }
    }
}
