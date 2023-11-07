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
            Pairs<string, EColor> expected = new()
            {
                { "none", EColor.None },
                { "green", EColor.Green },
                { "magenta", EColor.Magenta },
                { "none", EColor.None }
            };
            Pairs<string, EColor> actual = message.SplitByColors();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void SplitColorlessMessageByColorsTest()
        {
            string message = "this is a colorless message";
            Pairs<string, EColor> expected = new()
            {
                { "this is a colorless message", EColor.None }
            };
            Pairs<string, EColor> actual = message.SplitByColors();
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}