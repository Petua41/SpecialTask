using SpecialTask;

namespace SpecialTaskTest
{
    public class STConsoleTests
    {
        STConsole console;

        [SetUp]
        public void Setup()
        {
            console = STConsole.Instance;
        }

        [Test]
        public void SplitMessageByColorsTest()
        {
            string message = "none[color:Green]green[color:Magenta]magenta[color]none";
            MyMap<string, EColor> expected = new()
            {
                { "none", EColor.None },
                { "green", EColor.Green },
                { "magenta", EColor.Magenta },
                { "none", EColor.None }
            };
            MyMap<string, EColor> actual = STConsole.SplitMessageByColors(message);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void SplitColorlessMessageByColorsTest()
        {
            string message = "this is a colorless message";
            MyMap<string, EColor> expected = new()
            {
                { "this is a colorless message", EColor.None }
            };
            MyMap<string, EColor> actual = STConsole.SplitMessageByColors(message);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}