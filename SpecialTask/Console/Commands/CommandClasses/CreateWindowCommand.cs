﻿using SpecialTask.Infrastructure;

namespace SpecialTask.Console.Commands.CommandClasses
{
    /// <summary>
    /// Command to create window
    /// </summary>
    class CreateWindowCommand : ICommand
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
