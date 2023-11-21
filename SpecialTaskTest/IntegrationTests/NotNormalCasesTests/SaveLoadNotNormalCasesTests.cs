namespace SpecialTaskTest.IntegrationTests.NotNormalCasesTests
{
    internal class SaveLoadNotNormalCasesTests : TestsBase
    {
        [Test]
        public void NothingToSaveTest()
        {
            EnterCommand("save");
            Assert.That(ConsoleOutput, Does.Contain("File is already saved"));
        }

        [Test]
        public void SaveDirectoryDoesntExist()
        {
            EnterCommand("new circle -x100 -y100 -r80 -t3 -Cred");  // so that it will be smth to save
            EnterCommand("save_as -f /Some/Directory/That/Surely/Doesn`t/Exist");
            Assert.That(ConsoleOutput, Does.Contain("Directory").And.Contain("doesn`t exist"));
        }

        [Test]
        public void LoadFileDoesntExist()
        {
            EnterCommand("load -f some_filename_that_surely_doesnt_exist.not_existing_extension");
            Assert.That(ConsoleOutput, Does.Contain("File not found"));
        }
    }
}
