using System;
using System.Collections.Generic;
using System.Linq;
using SpecialTask.Console;
using SpecialTask.Drawing;
using SpecialTask.Helpers;

namespace SpecialTask.Console.Commands.CommandClasses
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

            receiver.NewLine();

            string output = string.Join(Environment.NewLine, textures.Select(x => $"{x.Key} -- {x.Value}"));

            receiver.Display(output);
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of textures command");
        }
    }
}
