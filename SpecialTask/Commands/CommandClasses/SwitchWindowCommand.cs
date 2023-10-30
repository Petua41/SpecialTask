﻿using System;

namespace SpecialTask.Commands.CommandClasses
{
    /// <summary>
	/// Command to switch windows
	/// </summary>
	class SwitchWindowCommand : ICommand
    {
        private readonly WindowManager receiver;

        private readonly int numberOfWindow;

        public SwitchWindowCommand(object[] args)
        {
            receiver = WindowManager.Instance;
            numberOfWindow = (int)args[0];
        }

        public void Execute()
        {
            try { receiver.SwitchToWindow(numberOfWindow); }
            catch (ArgumentException)
            {
                Logger.Instance.Error($"Trying to switch to window {numberOfWindow}, but window {numberOfWindow} doesn`t exist");
                MiddleConsole.HighConsole.DisplayError($"Window {numberOfWindow} doesn`t exist!");
            }
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of window command");
        }
    }
}
