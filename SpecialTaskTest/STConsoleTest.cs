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

        [Test]
        public void AutocompleteEmptyStringTest()
        {
            string input = "";
            string expected = "";
            string actual = console.Autocomplete(input);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void AutocompletePartOfCommandTest()
        {
            string input = "exi";
            string expected = "t";
            string actual = console.Autocomplete(input);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void AutocompletePartOfInvalidCommandTest()
        {
            string input = "aaab";
            string expected = "";
            string actual = console.Autocomplete(input);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void AutocompleteCompleteCommandTest()
        {
            string input = "exit";
            string expected = "";
            string actual = console.Autocomplete(input);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void AutocompletePartOfArgumentWhenCannotDetermineTest()
        {
            string input = "new circle --center";
            string expected = "";
            string actual = console.Autocomplete(input);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void AutocompletePartOfArgumentWhenCanDetermineTest()
        {
            string input = "edit --creation_";
            string expected = "time";
            string actual = console.Autocomplete(input);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void AutocompletePartOfInvalidArgumentTest()
        {
            string input = "edit --center";
            string expected = "";
            string actual = console.Autocomplete(input);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void AutocompletePseudoBoolParameterTest()
        {
            string input = "new circle -s";
            string expected = "";
            string actual = console.Autocomplete(input);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void AutocompleteParameterWithoutDefaultValueTest()
        {
            string input = "new circle --center_x";
            string expected = "";
            string actual = console.Autocomplete(input);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void AutocompleteParameterWithDefaultValueTest()
        {
            string input = "undo -n";
            string expected = "1";
            string actual = console.Autocomplete(input);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}