using SpecialTask;
using SpecialTask.Console;
using SpecialTask.Infrastructure.Collections;
using SpecialTask.Infrastructure.Enums;
using static SpecialTask.Infrastructure.Extensoins.StringExtensions;

namespace SpecialTaskTest
{
    public class STConsoleTests
    {
        [Test]
        public void SplitMessageByColorsTest()
        {
            string message = "none[color:Green]green[color:Magenta]magenta[color]none";
            Pairs<string, STColor> expected = new()
            {
                { "none", STColor.None },
                { "green", STColor.Green },
                { "magenta", STColor.Magenta },
                { "none", STColor.None }
            };
            Pairs<string, STColor> actual = message.SplitByColors();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void SplitColorlessMessageByColorsTest()
        {
            string message = "this is a colorless message";
            Pairs<string, STColor> expected = new()
            {
                { "this is a colorless message", STColor.None }
            };
            Pairs<string, STColor> actual = message.SplitByColors();
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}