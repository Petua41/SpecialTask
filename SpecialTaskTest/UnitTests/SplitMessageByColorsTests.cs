using SpecialTask.Infrastructure.Enums;
using static SpecialTask.Infrastructure.Extensoins.KeyValuePairListExtensions;
using static SpecialTask.Infrastructure.Extensoins.StringExtensions;

namespace SpecialTaskTest
{
    public class SplitMessageByColorsTests
    {
        [Test]
        public void SplitMessageByColorsTest()
        {
            string message = "none[color:Green]green[color:Magenta]magenta[color]none";
            List<KeyValuePair<string, InternalColor>> expected = new()
            {
                { "none", InternalColor.White },
                { "green", InternalColor.Green },
                { "magenta", InternalColor.Magenta },
                { "none", InternalColor.White }
            };
            List<KeyValuePair<string, InternalColor>> actual = message.SplitByColors();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void SplitColorlessMessageByColorsTest()
        {
            string message = "this is a colorless message";
            List<KeyValuePair<string, InternalColor>> expected = new()
            {
                { "this is a colorless message", InternalColor.White }
            };
            List<KeyValuePair<string, InternalColor>> actual = message.SplitByColors();
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}