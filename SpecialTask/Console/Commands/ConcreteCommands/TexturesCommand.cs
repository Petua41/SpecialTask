using SpecialTask.Console.Interfaces;
using SpecialTask.Infrastructure.Enums;

namespace SpecialTask.Console.Commands.ConcreteCommands
{
    /// <summary>
    /// Display list of textures (with descriptions)
    /// </summary>
    internal class TexturesCommand : ICommand
    {
        private readonly IHighConsole receiver;

        public TexturesCommand()
        {
            receiver = HighConsole;
        }

        public void Execute()
        {
            Dictionary<string, string> textures = TextureController.TexturesWithDescriptions;

            receiver.NewLine();

            string output = string.Join(Environment.NewLine, textures.Select(x => $"{x.Key} -- {x.Value}"));

            receiver.Display(output);
        }

        public void Unexecute()
        {
            Logger.Warning("Unexecution of textures command");
        }
    }
}
