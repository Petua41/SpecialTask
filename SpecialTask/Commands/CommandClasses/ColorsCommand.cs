using System.Linq;

namespace SpecialTask.Commands.CommandClasses
{
    /// <summary>
	/// Displays list of colors (with examples)
	/// </summary>
	class ColorsCommand : ICommand
    {
        private readonly IHighConsole receiver;

        public ColorsCommand()
        {
            receiver = MiddleConsole.HighConsole;
        }

        public void Execute()
        {
            receiver.NewLine();

            string output = string.Join(' ', from color in ColorsController.ColorsList select $"[color:{color}]{color}[color]");

            receiver.Display(output);
            receiver.NewLine();
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of colors command");
        }
    }
}
