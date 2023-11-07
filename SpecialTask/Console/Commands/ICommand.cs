namespace SpecialTask.Console.Commands
{
    internal interface ICommand
    {
        void Execute();
        void Unexecute();
    }
}
