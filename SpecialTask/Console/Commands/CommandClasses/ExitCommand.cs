using SpecialTask.Helpers;
using SpecialTask.Helpers.CommandHelpers;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SpecialTask.Console.Commands.CommandClasses
{
    /// <summary>
	/// Command to close application
	/// </summary>
	class ExitCommand : ICommand
    {
        enum EYesNoSaveAnswer { None, Yes, No, Save }

        private readonly System.Windows.Application receiver;

        private EYesNoSaveAnswer answer = EYesNoSaveAnswer.None;
        private readonly Task task;
        private readonly CancellationTokenSource tokenSource;

        public ExitCommand()
        {
            receiver = System.Windows.Application.Current;

            MiddleConsole.HighConsole.SomethingTranferred += OnSomethingTransferred;
            MiddleConsole.HighConsole.CtrlCTransferred += OnCtrlCTransferred;

            tokenSource = new();
            task = new Task(EmptyTask, tokenSource.Token);
        }

        public void Execute()
        {
            if (SaveLoadFacade.Instance.NeedsSave)
            {
                MiddleConsole.HighConsole.TransferringInput = true;
                MiddleConsole.HighConsole.DisplayQuestion("File is not saved. Exit? [y, s, n] (default=n)");

                GetInputIfNotSaved();
            }
            else receiver.Shutdown();
        }

        public void Unexecute()
        {
            Logger.Instance.Warning("Unexecution of exit command.");
        }

        private async void GetInputIfNotSaved()
        {
            try { await task; }
            catch (TaskCanceledException) { /* continue */ }

            MiddleConsole.HighConsole.TransferringInput = false;

            switch (answer)
            {
                case EYesNoSaveAnswer.Yes:
                    receiver.Shutdown();
                    break;
                case EYesNoSaveAnswer.No:
                    MiddleConsole.HighConsole.NewLine();
                    MiddleConsole.HighConsole.DisplayPrompt();
                    break;
                case EYesNoSaveAnswer.Save:
                    MiddleConsole.HighConsole.Display("saving...");
                    CommandsFacade.Execute(new SaveCommand());		// We don`t need to check anything here. SaveLoadFacade will do
                    receiver.Shutdown();
                    break;
                default:
                    Logger.Instance.Error("None answer in exit command");
                    return;
            }
        }

        private void OnSomethingTransferred(object? sender, TransferringEventArgs e)
        {
            string trString = e.Input;

            answer = trString.ToLower() switch
            {
                "y" or "yes" => EYesNoSaveAnswer.Yes,
                "s" or "save" => EYesNoSaveAnswer.Save,
                _ => EYesNoSaveAnswer.No
            };

            tokenSource.Cancel(true);
        }

        private void OnCtrlCTransferred(object? sender, EventArgs e)
        {
            answer = EYesNoSaveAnswer.No;
            tokenSource.Cancel(true);
        }

        private void EmptyTask()
        {
            while (true) ;
        }
    }
}
