using SpecialTask.Console.Interfaces;
using SpecialTask.Infrastructure.Enums;

namespace SpecialTask.Console.Commands.ConcreteCommands
{
    /// <summary>
    /// Displays list of colors (with examples)
    /// </summary>
    internal class ColorsCommand : ICommand
    {
        private readonly IHighConsole receiver;

        public ColorsCommand()
        {
            receiver = HighConsole;
        }

        public void Execute()
        {
            receiver.NewLine();

            string output = string.Join(' ', ColorsController.ColorsList.Select(c => $"[color:{c}]{c}[color]"));

            receiver.Display(output);
            receiver.NewLine();
        }

        public void Unexecute()
        {
            Logger.Warning("Unexecution of colors command");
        }
    }
}
