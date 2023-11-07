using SpecialTask.Infrastructure.WindowSystem;

namespace SpecialTask.Console.Commands.ConcreteCommands
{
    /// <summary>
    /// Command to switch windows
    /// </summary>
    internal class SwitchWindowCommand : ICommand
    {
        private readonly WindowManager receiver;

        private readonly int numberOfWindow;

        public SwitchWindowCommand(object[] args)
        {
            receiver = WindowManager.Instance;
            numberOfWindow = (int)args[0];
        }

        [Obsolete]
        public void Execute()
        {
            try { receiver.SwitchToWindow(numberOfWindow); }
            catch (ArgumentException)
            {
                Logger.Error($"Trying to switch to window {numberOfWindow}, but window {numberOfWindow} doesn`t exist");
                HighConsole.DisplayError($"Window {numberOfWindow} doesn`t exist!");
            }
        }

        public void Unexecute()
        {
            Logger.Warning("Unexecution of window command");
        }
    }
}
