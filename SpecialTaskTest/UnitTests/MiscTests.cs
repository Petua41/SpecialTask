using SpecialTask.Drawing;
using SpecialTask.Infrastructure.Collections;
using static SpecialTask.Infrastructure.Extensoins.InternalColorExtensions;
using static SpecialTask.Infrastructure.Extensoins.PointListExtensions;

namespace SpecialTaskTest
{
    public class MiscTest
    {
        [Test]
        public void PointsToStringTest()
        {
            List<Point> points = new() { new(10, 10), new(20, 30), new(40, 40) };
            string expected = "10 10, 20 30, 40 40";
            string actual = points.PointsToString();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void SplitHexValueTest()
        {
            (byte, byte, byte) expected = (255, 0, 130);
            uint hex = 0xFF0082;
            (byte, byte, byte) actual = SplitHexValue(hex);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}