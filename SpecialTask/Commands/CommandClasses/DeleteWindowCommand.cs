using System;

namespace SpecialTask.Commands.CommandClasses
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
                Logger.Instance.Error($"Trying to delete window {number}, but window {number} doesn`t exist");
                MiddleConsole.HighConsole.DisplayError($"[color:red]Window {number} doesn`t exist![color]");
            }
		}

		public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of window command");
        }
    }
}
