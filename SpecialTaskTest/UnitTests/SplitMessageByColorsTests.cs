using SpecialTask.Infrastructure.Collections;
using SpecialTask.Infrastructure.Enums;
using static SpecialTask.Infrastructure.Extensoins.StringExtensions;

namespace SpecialTaskTest
{
    public class SplitMessageByColorsTests
    {
        [Test]
        public void SplitMessageByColorsTest()
        {
            string message = "none[color:Green]green[color:Magenta]magenta[color]none";
            Pairs<string, InternalColor> expected = new()
            {
                { "none", InternalColor.None },
                { "green", InternalColor.Green },
                { "magenta", InternalColor.Magenta },
                { "none", InternalColor.None }
            };
            Pairs<string, InternalColor> actual = message.SplitByColors();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void SplitColorlessMessageByColorsTest()
        {
            string message = "this is a colorless message";
            Pairs<string, InternalColor> expected = new()
            {
                { "this is a colorless message", InternalColor.None }
            };
            Pairs<string, InternalColor> actual = message.SplitByColors();
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}