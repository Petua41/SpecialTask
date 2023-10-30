using System.Collections.Generic;
using System.Linq;

namespace SpecialTask.Commands.CommandClasses
{
    /// <summary>
	/// Display list of textures (with descriptions)
	/// </summary>
	class TexturesCommand : ICommand
    {
        private readonly IHighConsole receiver;

        public TexturesCommand()
        {
            receiver = MiddleConsole.HighConsole;
        }

        public void Execute()
        {
            Dictionary<string, string> textures = TextureController.TexturesWithDescriptions;

            string output = string.Join('\n', from kvp in textures select $"{kvp.Key} -- {kvp.Value}");

            receiver.Display(output);
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of textures command");
        }
    }
}
