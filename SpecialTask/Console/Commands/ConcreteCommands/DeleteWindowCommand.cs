using SpecialTask.Infrastructure.WindowSystem;

namespace SpecialTask.Console.Commands.ConcreteCommands
{
    /// <summary>
    /// Command to delete window
    /// </summary>
    internal class DeleteWindowCommand : ICommand
    {
        private readonly WindowManager receiver;

        private readonly int number;

        public DeleteWindowCommand(object[] args)
        {
            receiver = WindowManager.Instance;
            number = (int)args[0];
        }

        public void Execute()
        {
            try
            {
                receiver.DestroyWindow(number);
            }
            catch (ArgumentException)
            {
                Logger.Error($"Trying to delete window {number}, but window {number} doesn`t exist");
                HighConsole.DisplayError($"[color:red]Window {number} doesn`t exist![color]");
            }
        }

        public void Unexecute()
        {
            Logger.Warning("Unexecution of window command");
        }
    }
}
