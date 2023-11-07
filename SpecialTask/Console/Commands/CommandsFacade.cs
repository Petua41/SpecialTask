using SpecialTask.Infrastructure;
using SpecialTask.Infrastructure.Collections;
using SpecialTask.Infrastructure.Events;
using SpecialTask.Infrastructure.WindowSystem;

namespace SpecialTask.Console.Commands
{
    /// <summary>
    /// Controlls execution, undo and redo of all Commands
    /// </summary>
    internal static class CommandsFacade         // Facade and Mediator at the same time.
    {
        private static int undoStackDepth = 15;
        private static int currentWindowNumber = 0;
        private static readonly Dictionary<int, LimitedStack<ICommand>> stacks = new();
        private static readonly Dictionary<int, Stack<ICommand>> undoneStacks = new();

        static CommandsFacade()
        {
            WindowManager.Instance.WindowSwitchedEvent += OnWindowSwitched;
        }

        public static void Register(ICommand command)
        {
            Stack.Push(command);
        }

        public static void Execute(ICommand command)
        {
            try { command.Execute(); }
            catch (KeyboardInterruptException) { HighConsole.DisplayError("Keyboar interrupt"); }
        }

        public static void UndoCommands(int numberOfCommands = 1)
        {
            for (int i = 0; i < numberOfCommands; i++)
            {
                Undo();
            }
        }

        public static void RedoCommands(int numberOfCommands = 1)
        {
            if (numberOfCommands > UndoneStack.Count)
            {
                throw new InvalidOperationException();
            }

            for (int i = 0; i < numberOfCommands; i++)
            {
                Redo();
            }
        }

        public static void ChangeUndoStackDepth(int depth)
        {
            undoStackDepth = depth;
            Stack = new(Stack, undoStackDepth);
        }

        private static void Undo()
        {
            ICommand command = Stack.Pop();
            command.Unexecute();
            UndoneStack.Push(command);
        }

        private static void Redo()
        {
            if (UndoneStack.Count > 0)
            {
                ICommand command = UndoneStack.Pop();
                Register(command);
                Execute(command);
            }
        }

        private static LimitedStack<ICommand> Stack
        {
            get
            {
                if (stacks.ContainsKey(currentWindowNumber))
                {
                    return stacks[currentWindowNumber];
                }

                LimitedStack<ICommand> newStack = new(undoStackDepth);
                stacks.Add(currentWindowNumber, newStack);
                return newStack;
            }
            set => stacks[currentWindowNumber] = value;
        }

        private static Stack<ICommand> UndoneStack
        {
            get
            {
                if (undoneStacks.ContainsKey(currentWindowNumber))
                {
                    return undoneStacks[currentWindowNumber];
                }

                Stack<ICommand> newStack = new();
                undoneStacks.Add(currentWindowNumber, newStack);
                return newStack;
            }
        }

        private static void OnWindowSwitched(object sender, WindowSwitchedEventArgs e)
        {
            currentWindowNumber = e.NewNumber;
        }
    }
}
