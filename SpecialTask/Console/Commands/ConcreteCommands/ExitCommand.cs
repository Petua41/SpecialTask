using SpecialTask.Infrastructure.CommandHelpers.SaveLoad;
using SpecialTask.Infrastructure.Events;

namespace SpecialTask.Console.Commands.ConcreteCommands
{
    /// <summary>
    /// Command to close application
    /// </summary>
    internal class ExitCommand : ICommand
    {
        private enum YesNoSaveAnswer { None, Yes, No, Save }

        private readonly System.Windows.Application receiver;

        private YesNoSaveAnswer answer = YesNoSaveAnswer.None;
        private readonly Task task;
        private readonly CancellationTokenSource tokenSource;

        public ExitCommand()
        {
            receiver = System.Windows.Application.Current;

            HighConsole.SomethingTranferred += OnSomethingTransferred;
            HighConsole.CtrlCTransferred += OnCtrlCTransferred;

            tokenSource = new();
            task = new Task(EmptyTask, tokenSource.Token);
        }

        public void Execute()
        {
            if (SaveLoadFacade.Instance.NeedsSave)
            {
                HighConsole.TransferringInput = true;
                HighConsole.DisplayQuestion("File is not saved. Exit? [y, s, n] (default=n)");

                GetInputIfNotSaved();
            }
            else
            {
                receiver.Shutdown();
            }
        }

        public void Unexecute()
        {
            Logger.Warning("Unexecution of exit command.");
        }

        private async void GetInputIfNotSaved()
        {
            try { await task; }
            catch (TaskCanceledException) { /* continue */ }

            HighConsole.TransferringInput = false;

            switch (answer)
            {
                case YesNoSaveAnswer.Yes:
                    receiver.Shutdown();
                    break;
                case YesNoSaveAnswer.No:
                    HighConsole.NewLine();
                    HighConsole.DisplayPrompt();
                    break;
                case YesNoSaveAnswer.Save:
                    HighConsole.Display("saving...");
                    CommandsFacade.Execute(new SaveCommand());		// We don`t need to check anything here. SaveLoadFacade will do
                    receiver.Shutdown();
                    break;
                default:
                    Logger.Error("None answer in exit command");
                    return;
            }
        }

        private void OnSomethingTransferred(object? sender, TransferringEventArgs e)
        {
            string trString = e.Input;

            answer = trString.ToLower() switch
            {
                "y" or "yes" => YesNoSaveAnswer.Yes,
                "s" or "save" => YesNoSaveAnswer.Save,
                _ => YesNoSaveAnswer.No
            };

            tokenSource.Cancel(true);
        }

        private void OnCtrlCTransferred(object? sender, EventArgs e)
        {
            answer = YesNoSaveAnswer.No;
            tokenSource.Cancel(true);
        }

        private void EmptyTask()
        {
            while (true)
            {
                ;
            }
        }
    }
}
