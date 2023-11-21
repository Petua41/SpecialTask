namespace SpecialTaskTest.IntegrationTests.NotNormalCasesTests
{
    internal class WrongInputTests : TestsBase
    {
        [Test]
        public void HelpTest()
        {   // all invalid commands are considered as help
            EnterCommand("abcabc");
            Assert.That(ConsoleOutput, Does.Contain("Special Task 1 ver."));
        }

        [Test]
        public void MissingArgumentTest()
        {
            EnterCommand("new circle -x100 -y100 -r80 -t3");        // --color is missing
            Assert.That(ConsoleOutput, Does.Contain("color is necessary"));
        }

        [Test]
        public void DuplicatedArgumentTest()
        {   // duplicated arguments should be ignored and get first value
            TestScreenshot("new circle -x100 -y100 -r80 -t3 -Cred -t25", Properties.Resources.Regular_circle);  // --line_thickness is duplicated (should be 3)
        }

        [Test]
        public void InvalidArgumentTest()
        {
            EnterCommand("new circle -x100 -y100 -r80 -t3 -Cred --arg");    // --arg is invalid
            Assert.That(ConsoleOutput, Does.Contain("Invalid argument: --arg"));
        }

        [Test]
        public void InvalidValueTest()      // missing value is invalid value too (next argument is parsed as value)
        {
            EnterCommand("new circle -xaaa -y100 -r80 -t3 -Cred");      // aaa is not integer
            Assert.That(ConsoleOutput, Does.Contain("aaa is not Int"));
        }

        [Test]
        public void NegativeIntegerTest()
        {   // all numbers in this app are considered as positive integers. If number is negative, it should be set to 0
            TestScreenshot("new circle -x-100 -y100 -r80 -t3 -Cred", Properties.Resources.Circle_with_zero_centerX);   // --center_x is negative
        }

        [Test]
        public void MissingValueTest()
        {   // value is missing only if argument is on the end of command-line. Otherwise next argument is considered as value
            EnterCommand("new circle -x100 -y100 -r80 -t3 -C");     // value for --color is missing
            Assert.That(ConsoleOutput, Does.Contain("requires value"));
        }

        [Test]
        public void CallOfFictionalCommandTest()
        {   // "first-level" commands without "second-level" commands are fictional: they don`t support execution
            EnterCommand("new");
            Assert.That(ConsoleOutput, Does.Contain("You cannot call"));
        }
    }
}
