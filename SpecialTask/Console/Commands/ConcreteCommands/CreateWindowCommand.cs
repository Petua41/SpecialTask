using SpecialTask.Infrastructure.WindowSystem;

namespace SpecialTask.Console.Commands.ConcreteCommands
{
    /// <summary>
    /// Command to create window
    /// </summary>
    internal class CreateWindowCommand : ICommand
    {
        private readonly WindowManager receiver;

        public CreateWindowCommand()
        {
            receiver = WindowManager.Instance;
        }

        public void Execute()
        {
            receiver.CreateWindow();
        }

        public void Unexecute()
        {
            Logger.Warning("Unexecution of window command");
        }
    }
}
