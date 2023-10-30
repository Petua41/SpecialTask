using System;

namespace SpecialTask.Commands.CommandClasses
{
    /// <summary>
	/// Command to delete window
	/// </summary>
	class DeleteWindowCommand : ICommand
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
            try { receiver.DestroyWindow(number); }
            catch (ArgumentException)
            {
                Logger.Instance.Error(string.Format("Trying to delete window {0}, but window {0} doesn`t exist", number));
                MiddleConsole.HighConsole.DisplayError(string.Format("[color:red]Window {0} doesn`t exist![color]", number));
            }
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of window command");
        }
    }
}
